/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2022 Pedro Castro
 *
 * Gnome Subtitles is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * Gnome Subtitles is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using GnomeSubtitles.Ui.VideoPreview;
using Gst;
using Gst.PbUtils;
using Mono.Unix;
using SubLib.Util;
using System;

namespace External.GStreamer {

	public class GstBackend : MediaBackend {

		private Element playbin = null;
		private Element gtkSink = null;
		private GstMediaInfo mediaInfo = null;
		private float speed = 1f; //Keeping it here as we need to pass it to every 'seek' call

		public override string Name {
			get { return "GStreamer"; }
		}
		
		public override long CurrentPosition {
			get { return GetCurrentPosition(); }
		}
		
		public override long Duration {
			get { return mediaInfo.Duration; }
		}
		
		public override bool HasVideo {
			get { return mediaInfo.HasVideo; }
		}
		
		public override float AspectRatio {
			get { return mediaInfo.AspectRatio; }
		}

		public override float FrameRate {
			get { return mediaInfo.FrameRate; }
		}

		public override bool HasAudio {
			get { return mediaInfo.HasAudio; }
		}

		public override Gtk.Widget CreateVideoWidget () {
			return gtkSink["widget"] as Gtk.Widget;
		}
		
		
		/* Public methods */

		public override void Dispose () {
			//Do nothing
		}

		public override void Initialize () {
			//Disable LIBVA by default as it's causing lots of problems. This is done by setting an invalid driver name.
			if (!"FALSE".Equals(Environment.GetEnvironmentVariable("GS_DISABLE_LIBVA"))) {
				Logger.Info("[GstBackend] Disabling libva");
				Environment.SetEnvironmentVariable("LIBVA_DRIVER_NAME", "GNOME_SUBTITLES");
			}

			Gst.Application.Init();

			playbin = ElementFactory.Make("playbin", "playbin");
			gtkSink = ElementFactory.Make("gtksink", "gtksink");

			playbin["video-sink"] = gtkSink;

			playbin.Bus.AddSignalWatch();
			playbin.Bus.Message += OnGstMessage;
		}

		public override bool Load (string uri) {
			SetStatus(MediaStatus.Loading);
			
			playbin["uri"]  = uri;
			return playbin.SetState(State.Paused) != StateChangeReturn.Failure;
		}

		public override void Pause () {
			playbin.SetState(State.Paused);
			SetStatus(MediaStatus.Paused);
		}

		public override void Play () {
			playbin.SetState(State.Playing);
			SetStatus(MediaStatus.Playing);
		}

		public override void Seek (long time, bool isAbsolute) {
			if ((Status == MediaStatus.Unloaded) || (Status == MediaStatus.Loading)) {
				return;
			}
			
			long newPosition = isAbsolute ? time : GetCurrentPosition() + time;

			/* Note: gst_element_seek_simple can't be used here because it always resets speed to 1, 
			 * and we need to keep the current speed.
			 */
			playbin.Seek(speed, Format.Time, SeekFlags.Flush, SeekType.Set, newPosition * Gst.Constants.MSECOND, SeekType.None, 0);
		}

		public override void SetSpeed (float speed) {
			if ((Status == MediaStatus.Unloaded) || (Status == MediaStatus.Loading)) {
				return;
			}
			
			this.speed = speed;
			playbin.Seek(speed, Format.Time, SeekFlags.Flush, SeekType.Set, GetCurrentPosition() * Gst.Constants.MSECOND, SeekType.None, 0);
		}

		public override void Unload () {
			if (playbin != null) {
				playbin.SetState(State.Null);
				playbin.Unref();
				playbin = null;
			}

			SetStatus(MediaStatus.Unloaded);
						
			mediaInfo = null;
			speed = 1f;
		}
				

		/* Private members */

		private void OnGstMessage (object o, MessageArgs args) {

			switch (args.Message.Type) {

				/* ASYNC_DONE can be emitted many times (example: when returning back from
				 * pause to play). If the info is already loaded, don't do anything.
				 */
				case MessageType.AsyncDone:
					if (mediaInfo == null) {
						LoadMediaInfo();
					}
					break;

				/*case GST_MESSAGE_STATE_CHANGED: {
					GstState old_state, new_state, pending_state;
					gst_message_parse_state_changed(message, &old_state, &new_state, &pending_state);
					g_print("on_gst_message: STATE CHANGED: old=%s, new(current)=%s, pending(target)=%s\n",
						gst_element_state_get_name(old_state), gst_element_state_get_name(new_state), gst_element_state_get_name(pending_state));
					break;
				}*/

				case MessageType.Error:
					GLib.GException gerror;
					string debug;
					args.Message.ParseError(out gerror, out debug);

					Logger.Error("[GstBackend] Gst error: '{0}'; debug message: '{1}'", gerror.Message, debug);
					TriggerErrorFound(gerror.Message);
					break;

				// Playback finished
				case MessageType.Eos:
					TriggerEndOfStreamReached();
					break;
			}
		}

		private void LoadMediaInfo () {
			mediaInfo = new GstMediaInfo();

			/* Check for video and audio */
			mediaInfo.HasVideo = CheckIfMediaHasVideo();
			mediaInfo.HasAudio = CheckIfMediaHasAudio();

			if (!mediaInfo.HasVideo && !mediaInfo.HasAudio) {
				TriggerErrorFound(Catalog.GetString("The file contains no video or audio."));
				return;
			}

			if (mediaInfo.HasVideo) {
				if (!LoadVideoInfo(mediaInfo)) {
					TriggerNoMediaInfoError();
					return;
				}
			}

			LoadMediaDuration();
		}
		
		private bool CheckIfMediaHasVideo() {
			return (int)playbin["current-video"] != -1;
		}
		
		private bool CheckIfMediaHasAudio() {
			return (int)playbin["current-audio"] != -1;
		}
		
		private bool LoadVideoInfo(GstMediaInfo info) {
			Pad videoPad = gtkSink.GetStaticPad("sink");
			if (videoPad == null) {
				Logger.Error("[GstBackend] Unable to obtain the video pad");
				return false;
			}
			
			Caps caps = videoPad.CurrentCaps;
			if (caps == null) {
				Logger.Error("[GstBackend] Unable to obtain the video caps");
				return false;
			}
			
			bool gotWidth = false, gotHeight = false, gotFrameRate = false;
			foreach (Structure structure in caps) {
				string name = structure.Name;
				if (String.IsNullOrEmpty(name) || !name.StartsWith("video", StringComparison.Ordinal)) {
					continue;
				}
				
				/* Get the values */
				
				if (!gotWidth) {
					int width = -1;
					if (structure.GetInt("width", out width)) {
						info.Width = width;
						gotWidth = true;
					}
				}
				
				if (!gotHeight) {
					int height = -1;
					if (structure.GetInt("height", out height)) {
						info.Height = height;
						gotHeight = true;
					}
				}
				
				if (!gotFrameRate) {
					int numerator = -1, denominator = -1;
					if (structure.GetFraction("framerate", out numerator, out denominator)) {
						info.FrameRate = (float)numerator / (float)denominator;
						gotFrameRate = true;
					}
				}
				
				if (gotWidth && gotHeight && gotFrameRate) {
					break;
				}
			}


			/* Check if we got everything we needed */
			
			if (!gotWidth || !gotHeight || !gotFrameRate) {
				Logger.Error("[GstBackend] Unable to obtain the video width, height or frame rate. GotWidth={0}, GotHeight={1}, GotFrameRate={2}", gotWidth, gotHeight, gotFrameRate);
				return false;
			}
			
			info.AspectRatio = (float)mediaInfo.Width / (float)mediaInfo.Height;
			
			return true;
		}
		
		/**
		 * Performs an async query when the default method for obtaining the duration fails.
		 */
		private void LoadMediaDuration() {
			long duration = -1;	
			if (playbin.QueryDuration(Format.Time, out duration)) {
				mediaInfo.Duration = duration / Gst.Constants.MSECOND;
				TriggerLoadComplete();
				return;
			}
	
			/* Usually we can query the duration above. However, there are
			 * some files where this doesn't happen (for example, some audio files
			 * where the duration is only computed when starting to play the file).
			 * In those cases, the GstDiscoverer is used below.
			 */
			Logger.Info("[GstBackend] Unable to query the media duration. Using the discoverer.");
			
			
			string uri = playbin["current-uri"] as String;
			Discoverer discoverer = new Discoverer(Gst.Constants.SECOND);
			if (discoverer == null) {
				Logger.Error("[GstBackend] Unable to get the gstreamer discoverer");
				TriggerNoMediaInfoError();
				return;
			}

			discoverer.Discovered += OnDiscovered;
			discoverer.Start();
			
			if (!discoverer.DiscoverUriAsync(uri)) {
				Logger.Error("[GstBackend] Failed to start the discoverer");
				TriggerNoMediaInfoError();
				return;
			}
		}
		
		private long GetCurrentPosition() {
			long position = -1;
			
			/* Try to query the position */
			if (playbin.QueryPosition(Format.Time, out position)) {
				return position / Gst.Constants.MSECOND;
			}
			
			/* Query position wasn't successful. This is usually due to a pending state change,
			 * which happens for example after seeking, so we wait for the state change to complete
			 * and try again.
			 */
			WaitForStateChangeToComplete();
			if (playbin.QueryPosition(Format.Time, out position)) {
				return position / Gst.Constants.MSECOND;
			}
			
			/* If we're here, we're still unable to return the position after waiting for the state
	 		 * change to complete. Return -1. */
	 		Logger.Error("[GstBackend] Unable to query the current position");
			return -1;
		}

		private void WaitForStateChangeToComplete () {
			State state;
			State pending;
			playbin.GetState(out state, out pending, Gst.Constants.CLOCK_TIME_NONE);
		}

		private void TriggerLoadComplete () {
			SetStatus(MediaStatus.Loaded);
		}
		
		private void OnDiscovered (object o, DiscoveredArgs args) {
			mediaInfo.Duration = (long)args.Info.Duration / Gst.Constants.MSECOND;
			TriggerLoadComplete();			
		}

		private void TriggerNoMediaInfoError() {
			TriggerErrorFound(Catalog.GetString("Unable to obtain the complete media information."));
		}
		

	}

}