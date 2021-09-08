/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2021 Pedro Castro
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

using GnomeSubtitles.Core;
using GnomeSubtitles.Ui.VideoPreview;
using Gtk;
using Mono.Unix;
using SubLib.Util;
using System;
using System.Runtime.InteropServices;

/*
 * There are many ways to integrate gstreamer into an application. This implementation uses a gstreamer
 * playbin (which is a gstreamer pipeline with a number of additional features) with a video sink set to a 
 * gtksink (instead of the default "auto" video sink). Once initialized, this video sink allows returning
 * its GtkWidget, which is then added to the application UI. Most of the hard work is done in the auxiliary
 * gst_backend C library, whereas here we use bindings to that library.
 * 
 * Previously, this implementation relied on gst_video_overlay_set_window_handle which received the X11 id
 * of the Gdk.Window to which we wanted gstreamer to attach to, however this implementation presented many
 * glitches which weren't simple to solve (ex: screen went black when switching between apps, and also when
 * updating the current subtitle while the video was paused).
 *
 * Reference to other ways to implement gstreamer into a GTK application:
 *   - https://github.com/GNOME/clutter-gst (does a lot more than a simple integration; used in Totem)
 *   - https://developer.gnome.org/totem/stable/BaconVideoWidget.html (Totem bacon video widget)
 */

namespace External.GStreamer {

	/* Enums */
	
	public class GstBackend : MediaBackend {
		private HandleRef backend;
		private GstMediaInfo mediaInfo = null;
		private float speed = 1f; //Keeping it here as we need to pass it to every 'seek' call

		/* Delegates */
		private delegate void ErrorFoundHandler(ErrorType type, string errorMessage, string debugMessage);
		private delegate void LoadCompleteHandler(GstMediaInfo mediaInfo);

		/* Enums */
		private enum ErrorType { GStreamer, NoMediaInfo, NoVideoOrAudio};

		/* Error messages */
		private readonly string ErrorMessageNoMediaInfo = Catalog.GetString("Unable to obtain the complete media information.");
		private readonly string ErrorMessageNoVideoOrAudio = Catalog.GetString("The file contains no video or audio.");
		
		/* Public properties */
	
		public override string Name {
			get { return "GStreamer"; }
		}
		
		public override long CurrentPosition {
			get { return gst_backend_get_position(backend); }
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


		/* Public methods */

		public override void Initialize() {
		
			//Disable LIBVA by default as it's causing lots of problems. This is done by setting an invalid driver name.
			if (!"FALSE".Equals(Environment.GetEnvironmentVariable("GS_DISABLE_LIBVA"))) {
				Logger.Info("[GstBackend] Disabling libva");
				Environment.SetEnvironmentVariable("LIBVA_DRIVER_NAME", "GNOME_SUBTITLES");
			}
			
			IntPtr ptr = gst_backend_init(OnErrorFound, OnLoadComplete, OnEndOfStreamReached);
			if (ptr == IntPtr.Zero) {
				throw new Exception("Unable to initialize gstreamer: gst_backend_init returned no pointer.");
			}
			
			backend = new HandleRef(this, ptr);
		}
		
		public override bool Load(string uri) {
			SetStatus(MediaStatus.Loading);
	
			return gst_backend_load(backend, uri);
		}
		public override void Unload() {
			gst_backend_unload(backend);
			SetStatus(MediaStatus.Unloaded);
						
			mediaInfo = null;
			speed = 1f;
		}

		public override void Dispose() {
			//Do nothing
		}
		
		public override void Play() {
			gst_backend_play(backend);
			SetStatus(MediaStatus.Playing);
		}
	
		public override void Pause() {
			gst_backend_pause(backend);
			SetStatus(MediaStatus.Paused);
		}
		
		public override void Seek(long time, bool isAbsolute) {
			if ((Status == MediaStatus.Unloaded) || (Status == MediaStatus.Loading)) {
				return;
			}
			gst_backend_seek(backend, time, isAbsolute, speed);
		}

		public override void SetSpeed (float speed) {
			if ((Status == MediaStatus.Unloaded) || (Status == MediaStatus.Loading)) {
				return;
			}
			
			this.speed = speed;
			gst_backend_set_speed(backend, speed);
		}

		public override Widget CreateVideoWidget() {
			return new Widget(gst_backend_get_video_widget(backend));
		}
	
	
		/* Event handlers */
	
		private void OnLoadComplete(GstMediaInfo mediaInfo) {
			this.mediaInfo = mediaInfo;
			SetStatus(MediaStatus.Loaded);
		}
		
		private void OnErrorFound(ErrorType code, string userMessage, string debugMessage) {
			//If we have a debug message, output it to the logs
			if (!String.IsNullOrEmpty(debugMessage)) {
				Logger.Error("[GstBackend] OnErrorFound debug message: {0}", debugMessage);
			}

			//If we have a user message, use it. Otherwise, show an error message according to the specified code.
			string errorMessage = userMessage;
			if (String.IsNullOrEmpty(userMessage)) {
				if (code == ErrorType.NoMediaInfo) {
					errorMessage = ErrorMessageNoMediaInfo;
				} else if (code == ErrorType.NoVideoOrAudio) {
					errorMessage = ErrorMessageNoVideoOrAudio;
				}
			}
			if (String.IsNullOrEmpty(errorMessage)) {
				Logger.Error("[GstBackend] Got an empty error message for code '{0}'", code);
			}
			TriggerErrorFound(errorMessage);
		}
		
		private void OnEndOfStreamReached() {
			TriggerEndOfStreamReached();
		}


		/* Imports */
		
		[DllImport("gst_backend")]
		private static extern IntPtr gst_backend_init(ErrorFoundHandler onErrorFound, LoadCompleteHandler onLoadComplete, BasicEventHandler onEndOfStreamReached);
		[DllImport("gst_backend")]
		private static extern bool gst_backend_load(HandleRef backend, string uri);
		[DllImport("gst_backend")]
		private static extern void gst_backend_unload(HandleRef backend);
		[DllImport("gst_backend")]
		private static extern void gst_backend_play(HandleRef backend);
		[DllImport("gst_backend")]
		private static extern void gst_backend_pause(HandleRef backend);
		[DllImport("gst_backend")]
		private static extern long gst_backend_get_position(HandleRef backend);
		[DllImport("gst_backend")]
		private static extern void gst_backend_seek(HandleRef backend, long time, bool isAbsolute, float speed);
		[DllImport("gst_backend")]
		private static extern void gst_backend_set_speed(HandleRef backend, float speed);
		[DllImport("gst_backend")]
		private static extern IntPtr gst_backend_get_video_widget(HandleRef backend);
	}

}
