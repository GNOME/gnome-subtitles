/*

	Copyright (c)  Goran Sterjov, Pedro Castro

    This file is part of the GStreamer Playbin Wrapper.
    Derived from Fuse.

    GStreamer Playbin Wrapper is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    GStreamer Playbin Wrapper is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with GStreamer Playbin Wrapper; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/



using System;
using System.Runtime.InteropServices;


namespace GStreamer
{
	
	#pragma warning disable 649		//Disables warning about fields not being assigned to
	
	// media engine enumerations
	public enum MediaStatus { Playing, Paused, Loaded, Unloaded }
	
	// media engine event handlers
	public delegate void ErrorEventHandler (ErrorEventArgs args);
	public delegate void BufferEventHandler (BufferEventArgs args);
	public delegate void EndOfStreamEventHandler ();
	
	public delegate void StateEventHandler (StateEventArgs args);
	public delegate void VideoInfoEventHandler (VideoInfoEventArgs args);
	public delegate void TagEventHandler (TagEventArgs args);
	
	
	
	/// <summary>
	/// The GStreamer Playbin.
	/// </summary>
	public class Playbin
	{
		
		// engine callbacks from the C wrapper
		delegate void eosCallback ();
		delegate void errorCallback (string error, string debug);
		delegate void bufferCallback (int progress);
		delegate void infoCallback (IntPtr ptr);
		delegate void tagCallback (IntPtr ptr);
		
		eosCallback eos_cb;
		errorCallback error_cb;
		bufferCallback buffer_cb;
		infoCallback info_cb;
		tagCallback tag_cb;
		
		
		// declarations
		HandleRef engine;
		MediaStatus status = MediaStatus.Unloaded;
		
		
		/// <summary>Raised when an error occurs.</summary>
		public event ErrorEventHandler Error;
		
		/// <summary>Raised when the buffer status has changed.</summary>
		public event BufferEventHandler Buffer;
		
		/// <summary>Raised when the end of the stream is reached.</summary>
		public event EndOfStreamEventHandler EndOfStream;
		
		/// <summary>Raised when the playbin state changes. ie: Playing, Paused, etc.</summary>
		public event StateEventHandler StateChanged;
		
		/// <summary>Raised when video information is found.</summary>
		public event VideoInfoEventHandler FoundVideoInfo;
		
		/// <summary>Raised when a media tag is found.</summary>
		public event TagEventHandler FoundTag;
		
		
		
		/// <summary>
		/// Load the GStreamer library and attach it
		/// to the specified window.
		/// </summary>
		public bool Initiate (ulong x_window_id)
		{
			
			// load the gstreamer library
			IntPtr ptr = gst_binding_init (x_window_id);
			
			if(ptr == IntPtr.Zero)
			{
				throwError ("Failed to load the Gstreamer library", "");
				return false;
			}
			else engine = new HandleRef (this, ptr);
			
			
			// set up callbacks
			eos_cb = new eosCallback (onEos);
			error_cb = new errorCallback (onError);
			buffer_cb = new bufferCallback (onBuffer);
			info_cb = new infoCallback (onInfo);
			tag_cb = new tagCallback (onTag);
			
			gst_binding_set_eos_cb (engine, eos_cb);
			gst_binding_set_error_cb (engine, error_cb);
			gst_binding_set_buffer_cb (engine, buffer_cb);
			gst_binding_set_info_cb (engine, info_cb);
			gst_binding_set_tag_cb (engine, tag_cb);
			
			
			status = MediaStatus.Unloaded;
			return true;
		}
		
		
		/// <summary>
		/// Load the GStreamer library.
		/// </summary>
		public bool Initiate ()
		{
			return Initiate (0);
		}
		
		
		
		/// <summary>
		/// Disposes the GStreamer library.
		/// </summary>
		public void Dispose ()
		{
			Unload ();
			gst_binding_deinit (engine);
			changeState (MediaStatus.Unloaded);
		}
		
		/// <summary>
		/// Loads the specified path into the GStreamer library.
		/// </summary>
		public bool Load (string uri)
		{
			if (!isUnloaded)
				Unload ();
			
			bool loaded = gst_binding_load (engine, uri);
			
            if (loaded)
                changeState (MediaStatus.Loaded);
            
            return loaded;
		}
		
		/// <summary>
		/// Plays the loaded media file.
		/// </summary>
		public void Play ()
		{
			if (!isPlaying && !isUnloaded)
			{
				gst_binding_play (engine);
				changeState (MediaStatus.Playing);
			}
		}
		
		/// <summary>
		/// Pauses the loaded media file.
		/// </summary>
		public void Pause ()
		{
			if (isPlaying)
			{
				gst_binding_pause (engine);
				changeState (MediaStatus.Paused);
			}
		}
		
		/// <summary>
		/// Unloads the media file.
		/// </summary>
		public void Unload ()
		{
			if (!isUnloaded)
			{
				gst_binding_unload (engine);
				changeState (MediaStatus.Unloaded);
			}
		}
		
		/// <summary>
		/// Changes the window to which video playback is attached to.
		/// </summary>
		public void SetWindow (ulong window_id)
		{
			gst_binding_set_xid (engine, window_id);
		}
		
		
		
		/// <summary>
		/// Seeks to the nearest millisecond on the media file.
		/// </summary>
		public void Seek (TimeSpan time)
		{
			if (isUnloaded)
				return;
			
			gst_binding_set_position (engine, (ulong) time.TotalMilliseconds);
		}
		
		
		/// <summary>
		/// Seeks to the nearest millisecond on the media file.
		/// </summary>
		public void Seek (double milliseconds)
		{
			TimeSpan time = TimeSpan.FromMilliseconds (milliseconds);
			Seek (time);
		}
		
		
		/// <summary>
		/// Seeks to the specified track number.
		/// </summary>
		public void SeekToTrack (int track_number)
		{
			if (isUnloaded)
				return;
			gst_binding_set_track (engine, (ulong) track_number);
		}
		
		
		
		
		/// <summary>
		/// Returns the current position that the media file is on.
		/// </summary>
		public TimeSpan CurrentPosition
		{
			get
			{
				if (isUnloaded)
					return TimeSpan.Zero;
				
				double pos = (double) gst_binding_get_position (engine);
				return TimeSpan.FromMilliseconds (pos);
			}
		}
		
		
		/// <summary>
		/// Returns the total duration of the media file.
		/// </summary>
		public TimeSpan Duration
		{
			get{
				if (isUnloaded)
					return TimeSpan.Zero;
				
				double dur = (double) gst_binding_get_duration (engine);
				return TimeSpan.FromMilliseconds (dur);
			}
		}
		
		
		
		/// <summary>
		/// Returns the current volume of the GStreamer library.
		/// </summary>
		public double Volume
		{
			get{ return (double) gst_binding_get_volume (engine); }
			set{ gst_binding_set_volume (engine, (int) value); }
		}
		
		
		
		/// <summary>
		/// Returns a value determining if the media file is a video file.
		/// </summary>
		public bool HasVideo
		{
			get{ return !isUnloaded ? gst_binding_has_video (engine) : false; }
		}
		
		
		
		/// <summary>
		/// Returns a string array of all the visualisations available
		/// </summary>
		public string[] VisualisationList
		{
			get
			{
				IntPtr ptr = gst_binding_get_visuals_list (engine);
				GLib.List list = new GLib.List (ptr, typeof (string));
				
				string[] array = new string[list.Count];
				
				for (int i=0; i<list.Count; i++)
					array[i] = (list[i] as string);
				
				list.Dispose ();
				list = null;
				
				return array;
			}
		}
		
		
		/// <summary>
		/// Sets the visualisation
		/// </summary>
		public string Visualisation
		{
			set{ gst_binding_set_visual (engine, value); }
		}

		
		
		
		/// <summary>
		/// Returns information on the video stream, or null if it's not available
		/// </summary>
		public VideoInfo VideoInfo
		{
			get
			{
				IntPtr ptr = gst_binding_get_video_info (engine);
				if (ptr != IntPtr.Zero)
					return new VideoInfo (ptr);
				else
					return null;
			}
		}
		
		
		
		/// <summary>
		/// Returns the tag of the current media file, or null if it's not available
		/// </summary>
		public Tag Tag
		{
			get
			{
				IntPtr ptr = gst_binding_get_tag (engine);
				if (ptr != IntPtr.Zero)
					return new Tag (ptr);
				else
					return null;
			}
		}
		
		
		
		
		
		/// <summary>
		/// Returns the current status of the media engine.
		/// </summary>
		public MediaStatus CurrentStatus
		{
			get { return status; }
		}
		
		
		
		void changeState (MediaStatus state)
		{
			status = state;
			if (StateChanged != null)
				StateChanged (new StateEventArgs (state));
		}
		
		
		
		// throws an error to the global error handler
		void throwError (string error, string debug)
		{
        	if(Error != null)
				Error (new ErrorEventArgs (error, debug));
		}
		
		
		// an error in the gstreamer pipeline has occured
		void onError (string error, string debug)
		{
			throwError (error, debug);
		}
		
		
		// the stream has ended
		void onEos ()
		{
			if (EndOfStream != null)
				EndOfStream ();
		}
		
		
		// the gstreamer pipeline is being buffered
		void onBuffer (int progress)
		{
			if (Buffer != null)
				Buffer (new BufferEventArgs (progress));
		}
		
		
		// media information is available
		void onInfo (IntPtr ptr)
		{
			if (FoundVideoInfo != null)
			{
				VideoInfo video_info = getVideoInfo (ptr);
				if (video_info != null)
					FoundVideoInfo (new VideoInfoEventArgs (video_info));
			}
		}
		
		
		// a media tag is available
		void onTag (IntPtr ptr)
		{
			if (FoundTag != null)
			{
				Tag tag = getTag (ptr);
				if (tag != null)
					FoundTag (new TagEventArgs (tag));
			}
		}
		
		
		
		Tag getTag (IntPtr ptr)
		{
			if (ptr != IntPtr.Zero)
				return new Tag (ptr);
			else
				return null;
		}
		
		
		VideoInfo getVideoInfo (IntPtr ptr)
		{
			if (ptr != IntPtr.Zero)
				return new VideoInfo (ptr);
			else
				return null;
		}
        
		
		
		// private convenience properties
		bool isPlaying { get{ return status == MediaStatus.Playing; } }
		bool isUnloaded { get{ return status == MediaStatus.Unloaded; } }


		// core engine functions
		[DllImport("gstreamer_playbin")]
		static extern IntPtr gst_binding_init (ulong xwin);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_deinit (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern bool gst_binding_load (HandleRef play, string uri);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_play (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_pause (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_unload (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_xid (HandleRef play, ulong xid);
		
		
		// engine property functions
		[DllImport("gstreamer_playbin")]
		static extern ulong gst_binding_get_duration (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern ulong gst_binding_get_position (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern int gst_binding_get_volume (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_position (HandleRef play, ulong pos);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_track (HandleRef play, ulong track_number);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_volume (HandleRef play, int vol);
		[DllImport("gstreamer_playbin")]
		static extern bool gst_binding_has_video (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern IntPtr gst_binding_get_video_info (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern IntPtr gst_binding_get_tag (HandleRef play);
		
		
		// engine callbacks
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_eos_cb (HandleRef play, eosCallback cb);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_error_cb (HandleRef play, errorCallback cb);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_buffer_cb (HandleRef play, bufferCallback cb);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_info_cb (HandleRef play, infoCallback cb);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_tag_cb (HandleRef play, tagCallback cb);
		
		
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_visual (HandleRef play, string vis_name);
		
		
		[DllImport("gstreamer_playbin")]
		static extern IntPtr gst_binding_get_visuals_list (HandleRef play);
	}
	
	
	
	[StructLayout(LayoutKind.Sequential)]
    public class VideoInfo
    {
    	int width;
		int height;
    	float frame_rate;
		bool has_audio;
		bool has_video;
		
		public VideoInfo (IntPtr ptr)
		{
			if (ptr != IntPtr.Zero)
				Marshal.PtrToStructure (ptr, this);
		}
    	
    	public int Width { get{ return width; } }
       	public int Height { get{ return height; } }
       	public float AspectRatio { get { return (float)width/height; } }
    	public float FrameRate { get{ return frame_rate; } }
		public bool HasAudio { get{ return has_audio; } }
		public bool HasVideo { get{ return has_video; } }
    	
    	public override string ToString ()
    	{
    		return "width=" + width + ", height=" + height + ", frame_rate=" + frame_rate + ", has_audio=" + has_audio + ", has_video=" + has_video;
    	}

    }
	
	
	[StructLayout(LayoutKind.Sequential)]
    public class Tag
    {
    	string disc_id;
		string music_brainz_id;
    	int current_track;
		int track_count;
		int duration;
		
		public Tag (IntPtr ptr)
		{
			if (ptr != IntPtr.Zero)
				Marshal.PtrToStructure (ptr, this);
		}
    	
    	public string DiscID { get{ return disc_id; } }
       	public string MusicBrainzID { get{ return music_brainz_id; } }
       	public int CurrentTrack { get { return current_track; } }
    	public int TrackCount { get{ return track_count; } }
		public int Duration { get{ return duration; } }
    }
	
	
}