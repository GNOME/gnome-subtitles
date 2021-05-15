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
using Gtk;

namespace GnomeSubtitles.Ui.VideoPreview {

	/* Enums */
	public enum MediaStatus { Unloaded, Loading, Loaded, Paused, Playing };

	/* Delegates */
	public delegate void ErrorFoundHandler(string errorMessage);
	public delegate void StatusChangedHandler(MediaStatus newStatus);
	
	public abstract class MediaBackend {
	
		private MediaStatus status = MediaStatus.Unloaded;
		
	
		/* Events */
		public event ErrorFoundHandler ErrorFound;
		public event StatusChangedHandler StatusChanged;
		public event BasicEventHandler EndOfStreamReached; 
	
	
		/* Public properties */
	
		public MediaStatus Status {
			get { return status; }
		}

		public abstract string Name { get; }
		
		///Returns the current media position in ms, or -1 if unable to get the value						
		public abstract long CurrentPosition { get; }
		public abstract long Duration { get; }
		public abstract bool HasVideo { get; }
		public abstract float AspectRatio { get; }
		public abstract float FrameRate { get; }
		public abstract bool HasAudio { get; }
		

		/* Public methods */
		
		public abstract void Initialize();
		public abstract bool Load(string uri);
		
		public abstract void Unload();
		public abstract void Dispose();
		
		public abstract void Play();
		public abstract void Pause();
		public abstract void SetSpeed(float speed);
		public abstract void Seek(long time, bool isAbsolute);
		public abstract Widget CreateVideoWidget();
	
	
		/* Protected members */
		
		protected void SetStatus(MediaStatus status) {
			this.status = status;
			
			if (StatusChanged != null) {
				StatusChanged(status);
			}
		}
		
		protected void TriggerErrorFound(string error) {
			if (ErrorFound != null) {
				ErrorFound(error);
			}
		}
		
		protected void TriggerEndOfStreamReached() {
			if (EndOfStreamReached != null) {
				EndOfStreamReached();
			}
		}

	}

}