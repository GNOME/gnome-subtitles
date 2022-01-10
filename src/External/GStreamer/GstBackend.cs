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

using GnomeSubtitles.Core;
using GnomeSubtitles.Ui.VideoPreview;
using Gst;
using Mono.Unix;
using SubLib.Util;
using System;
using System.Runtime.InteropServices;

namespace External.GStreamer {

	public class GstBackend : MediaBackend {

		private Element playbin = null;
		private Element gtkSink = null;
		private GstMediaInfo mediaInfo = null;

		/* Enums */
		private enum ErrorType { GStreamer, NoMediaInfo, NoVideoOrAudio };

		/* Error messages */
		private readonly string ErrorMessageNoMediaInfo = Catalog.GetString("Unable to obtain the complete media information.");

		public override string Name => throw new NotImplementedException();

		public override long CurrentPosition => throw new NotImplementedException();

		public override long Duration => throw new NotImplementedException();

		public override bool HasVideo => throw new NotImplementedException();

		public override float AspectRatio => throw new NotImplementedException();

		public override float FrameRate => throw new NotImplementedException();

		public override bool HasAudio => throw new NotImplementedException();

		public override Gtk.Widget CreateVideoWidget () {
			throw new NotImplementedException();
		}

		public override void Dispose () {
			throw new NotImplementedException();
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

			playbin.Data ["video-sink"] = gtkSink; //FIXME funciona?
												   //g_object_set(G_OBJECT(backend->element), "video-sink", backend->video_element, NULL);

			playbin.Bus.AddSignalWatch();
			playbin.Bus.Message += OnGstMessage;
		}

		public override bool Load (string uri) {
			throw new NotImplementedException();
		}

		public override void Pause () {
			throw new NotImplementedException();
		}

		public override void Play () {
			throw new NotImplementedException();
		}

		public override void Seek (long time, bool isAbsolute) {
			throw new NotImplementedException();
		}

		public override void SetSpeed (float speed) {
			throw new NotImplementedException();
		}

		public override void Unload () {
			throw new NotImplementedException();
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
				LoadVideoInfo();
				//load_media_info_video(backend);
			}

			LoadMediaDuration();
			//load_media_info_duration(backend);
		}
		
		private bool CheckIfMediaHasVideo() {
			throw new NotImplementedException();
		}
		
		private bool CheckIfMediaHasAudio() {
			throw new NotImplementedException();
		}
		
		private bool LoadVideoInfo() {
			throw new NotImplementedException();
		}
		
		private bool LoadMediaDuration() {
			throw new NotImplementedException();
		}

	}

}