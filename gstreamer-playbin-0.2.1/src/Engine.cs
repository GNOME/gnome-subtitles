/*

	Copyright (c)  Goran Sterjov

    This file is part of the GStreamer Playbin Wrapper.
    Derived from the Dissent Project.

    GStreamer Playbin Wrapper is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    The Dissent Project is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with the Dissent Project; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/



using System;
using System.Runtime.InteropServices;


namespace GStreamer
{
	
	
	// media engine enumerations
	public enum MediaStatus { Playing, Stopped, Paused, Loaded, Unloaded }
	public enum MediaEvents { EndOfStream, Error, Buffering, Volume, Seek }
	
	// media engine event handlers
	public delegate void EngineEventHandler (object o, EngineEventArgs args);
	public delegate void EngineStateHandler (object o, EngineStateArgs args);
	
	
	
	/// <summary>
	/// The GStreamer Playbin.
	/// </summary>
	public class Playbin
	{
		
		// engine callbacks from the C wrapper
		delegate void eosCallback (IntPtr engine);
		delegate void errorCallback (IntPtr engine, string error, string debug);
		delegate void bufferCallback (IntPtr engine, int prog);
		
		eosCallback eos_cb;
		errorCallback error_cb;
		bufferCallback buffer_cb;
		
		
		// declarations
		HandleRef engine;
		MediaStatus status = MediaStatus.Unloaded;
		bool buffer_fin;
		EngineVideoInfo videoInfo = null;
		
		/// <summary>Raised when an event occurs. ie: EndOfStream, Error, etc.</summary>
		public event EngineEventHandler EventChanged;
		
		/// <summary>Raised when the playbin state changes. ie: Playing, Paused, etc.</summary>
		public event EngineStateHandler StateChanged;
		
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
				throwError ("Failed to load the Gstreamer library");
				return false;
			}
			else engine = new HandleRef (this, ptr);
			
			
			// set up callbacks
			eos_cb = new eosCallback (onEos);
			error_cb = new errorCallback (onError);
			buffer_cb = new bufferCallback (onBuffer);
			
			gst_binding_set_eos_cb (engine, eos_cb);
			gst_binding_set_error_cb (engine, error_cb);
			gst_binding_set_buffer_cb (engine, buffer_cb);
			
			
			status = MediaStatus.Stopped;
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
			Stop ();
			gst_binding_deinit (engine);
			ChangeState (MediaStatus.Unloaded);
		}
		
		/// <summary>
		/// Loads the specified path into the GStreamer library.
		/// </summary>
		public bool Load (string uri)
		{
			if (!isStopped) Stop ();
			gst_binding_load (engine, uri);
			ChangeState (MediaStatus.Loaded);
			return true;
		}
		
		/// <summary>
		/// Plays the loaded media file.
		/// </summary>
		public void Play ()
		{
			if (!isPlaying && !isUnloaded) {
				gst_binding_play (engine);
				ChangeState (MediaStatus.Playing);
			}
		}
		
		/// <summary>
		/// Pauses the loaded media file.
		/// </summary>
		public void Pause ()
		{
			if (isPlaying) {
				gst_binding_pause (engine);
				ChangeState (MediaStatus.Paused);
			}
		}
		
		/// <summary>
		/// Stops the loaded media file and unloads it.
		/// </summary>
		public void Stop ()
		{
			if (isPlaying || isPaused) {
				gst_binding_unload (engine);
				Seek (0);
				ChangeState (MediaStatus.Stopped);
				videoInfo = null;
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
			if (isUnloaded) return;
			gst_binding_set_position (engine, (ulong) time.TotalMilliseconds);
			ChangeEvent (MediaEvents.Seek, null, 0);
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
		/// Returns the current position that the media file is on.
		/// </summary>
		public TimeSpan CurrentPosition
		{
			get
			{
				if (!isPlaying && !isPaused)
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
		/// Returns information on the video stream, or null if it's not available
		/// </summary>
		public EngineVideoInfo VideoInfo
		{
			get{
				if (isUnloaded)
					return null;
					
				if (videoInfo == null) {
					IntPtr ptr = gst_binding_get_video_info (engine);
					if (ptr == IntPtr.Zero)
						return null;
					else {
						videoInfo = new EngineVideoInfo();
						Marshal.PtrToStructure(ptr, videoInfo);
					}
				}
				return videoInfo;
			}
		}

		/// <summary>
		/// Returns the current volume of the GStreamer library.
		/// </summary>
		public double Volume
		{
			get{ return (double) gst_binding_get_volume (engine); }
			set
			{
				gst_binding_set_volume (engine, (int) value);
				ChangeEvent (MediaEvents.Volume, null, 0);
			}
		}
		
		
		
		/// <summary>
		/// Returns a value determining if the media file is a video file.
		/// </summary>
		public bool HasVideo
		{
			get{ return !isUnloaded ? gst_binding_has_video (engine) : false; }
		}
		
		/// <summary>
		/// Returns the current status of the media engine.
		/// </summary>
		public MediaStatus CurrentStatus
		{
			get { return status; }
		}
		
		
		
		
		// throws an error to the global error handler
		void throwError (string message)
		{
			EngineEventArgs args = new EngineEventArgs (MediaEvents.Error, message, 0);
        	if(EventChanged != null) EventChanged(this, args);
		}
		
		
		// the stream has ended
		void onEos (IntPtr engine)
		{
			ChangeEvent (MediaEvents.EndOfStream, null, 0);
		}
		
		
		// an error in the gstreamer pipeline has occured
		void onError (IntPtr engine, string error, string debug)
		{
			throwError (error + "\n\nDebug Message:\n" + debug);
			ChangeState (MediaStatus.Stopped);
			ChangeEvent (MediaEvents.Error, error, 0);
		}
		
		
		// the gstreamer pipeline is being buffered
		void onBuffer (IntPtr engine, int prog)
		{
			if(!buffer_fin) {
				buffer_fin = prog >= 100;
				ChangeEvent (MediaEvents.Buffering, "Buffering", (double)prog);
			}
		}
		
		
		// throw the media state changed event
		void ChangeState(MediaStatus state)
		{
			status = state;
        	EngineStateArgs args = new EngineStateArgs (state);
        	if(StateChanged != null) StateChanged(this, args);
        }
        
        // throw the media event changed event
        void ChangeEvent(MediaEvents engineEvent, string message, double percent)
        {
        	EngineEventArgs args = new EngineEventArgs (engineEvent, message, percent);
        	if(EventChanged != null) EventChanged(this, args);
        }
		
		
		// private convenience properties
		bool isPlaying { get{ return status == MediaStatus.Playing; } }
		bool isPaused { get{ return status == MediaStatus.Paused; } }
		bool isStopped { get{ return status == MediaStatus.Stopped; } }
		bool isUnloaded { get{ return status == MediaStatus.Unloaded; } }
		
		
		
		// core engine functions
		[DllImport("gstreamer_playbin")]
		static extern IntPtr gst_binding_init (ulong xwin);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_deinit (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_load (HandleRef play, string uri);
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
		static extern IntPtr gst_binding_get_video_info (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern ulong gst_binding_get_position (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern int gst_binding_get_volume (HandleRef play);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_position (HandleRef play, ulong pos);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_volume (HandleRef play, int vol);
		[DllImport("gstreamer_playbin")]
		static extern bool gst_binding_has_video (HandleRef play);
		
		
		// engine callbacks
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_eos_cb (HandleRef play, eosCallback cb);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_error_cb (HandleRef play, errorCallback cb);
		[DllImport("gstreamer_playbin")]
		static extern void gst_binding_set_buffer_cb (HandleRef play, bufferCallback cb);
	}
	
	
	
	
	/// <summary>
	/// Arguments for a raised event.
	/// </summary>
	public sealed class EngineEventArgs
	{
		MediaEvents engine_event;
		string message;
		double buffer;
		
		public EngineEventArgs (MediaEvents engine_event, string message, double buffer)
		{
        	this.engine_event = engine_event;
        	this.message = message;
        	this.buffer = buffer;
        }
        
        public MediaEvents Event { get{ return engine_event; } }
        public string Message { get{ return message; } }
        public double BufferingPercent { get{ return buffer; } }
    }
    
    
	
	
	/// <summary>
	/// Arguments for a raised state.
	/// </summary>
    public sealed class EngineStateArgs
    {
    	MediaStatus state;
    	
    	public EngineStateArgs (MediaStatus state)
    	{
    		this.state = state;
    	}
    	
    	public MediaStatus State { get{ return state; } }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public class EngineVideoInfo
    {
    	int width;
		int height;
    	float frame_rate;
    	
    	public int Width { get{ return width; } }
       	public int Height { get{ return height; } }
       	public float AspectRatio { get { return (float)width/height; } }
    	public float FrameRate { get{ return frame_rate; } }
    	
    	public override string ToString () { return "Width=" + width + ", Height=" + height + ", FrameRate=" + frame_rate; }
    }
	
	
}
