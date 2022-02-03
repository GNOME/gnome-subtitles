/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2022 Pedro Castro
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

using External.GStreamer;
using GnomeSubtitles.Core;
using Gtk;
using SubLib.Util;
using System;

namespace GnomeSubtitles.Ui.VideoPreview {

/* Delegates */
public delegate void PlayerErrorHandler(string error);

public class Player {

	private AspectFrame frame = null; //the backend video widget is added as a child of this frame
	private MediaBackend backend = null;
	private PlayerPositionWatcher position = null;
	private MediaStatus status = MediaStatus.Unloaded; //We don't always have a backend, but we always should have a status, so that's why it's here
	private int speed = DefaultSpeed; //Speed is divided by 10 to get the actual speed (example: 10->1). Storing as int to avoid floating rounding errors.
	
	/* Constants */
	private const int DefaultSpeed = 100;
	

	public Player (AspectFrame frame) {
		this.frame = frame;
		InitializePositionWatcher();
	}
	

	/* Events */
	public event PlayerErrorHandler ErrorFound; //Note: all errors are considered fatal, meaning the backend must be disposed immediately
	public event BasicEventHandler EndOfStreamReached;
	public event StatusChangedHandler StatusChanged;
	public event PositionPulseEventHandler PositionPulse;


	/* Properties */

	public MediaStatus Status {
		get { return status; }
	}
	
	public long Duration {
		get { return backend.Duration; }
	}

	public bool HasVideo {
		get { return backend.HasVideo; }
	}

	public float AspectRatio {
		get { return backend.AspectRatio; }
	}

	public float FrameRate {
		get { return backend.FrameRate; }
	}

	public bool HasAudio {
		get { return backend.HasAudio; }
	}

	public int Speed {
		get { return speed; }
	}
	

	/* Public methods */

	public void Open(Uri mediaUri) {
		if (status != MediaStatus.Unloaded) {
			throw new Exception("Trying to load a player which is not unloaded.");
		}
		
		backend = InitializeBackend();
	
		if (!backend.Load(mediaUri.AbsoluteUri)) {
			throw new Exception("Unable to load the media file.");
		}
	}

	public void Close() {
		position.Stop();
		
		//Unload
		if ((status != MediaStatus.Unloaded) && (status != MediaStatus.Loading)) {
			backend.Unload();
		}
		status = MediaStatus.Unloaded;
		
		//Dispose
		DisposeBackend();
		
		speed = DefaultSpeed;
	}

	public void Play() {
		if (status == MediaStatus.Playing) {
			return;
		}
		
		if ((status != MediaStatus.Loaded) && (status != MediaStatus.Paused)) {
			throw new Exception(string.Format("Trying to play but the player status is {0}", status));
		}
	
		backend.Play();
	}

	public void Pause () {
		if (status == MediaStatus.Paused) {
			return;
		}
		
		if (status != MediaStatus.Playing) {
			throw new Exception(string.Format("Trying to pause but the player status is {0}", status));
		}

		backend.Pause();
	}

	public void SetSpeed(int speed) {
		this.speed = speed;
		backend.SetSpeed(((float)speed)/100);
	}
	
	public void ResetSpeed() {
		SetSpeed(DefaultSpeed);
	}
	
	/// <summary>
	/// Rewind the specified time.
	/// </summary>
	/// <param name="time">Time in ms.</param>
	public void Rewind(long time) {
		backend.Seek(-time, false);
	}

	/// <summary>
	/// Forward the specified time.
	/// </summary>
	/// <param name="time">Time in ms.</param>
	public void Forward(long time) {
		backend.Seek(time, false);
	}

	/// <summary>
	/// Seek to the specified position.
	/// </summary>
	/// <param name="position">Position in ms.</param>
	public void Seek(long position) {
		backend.Seek(position, true);
	}


	/* Private members */

	private MediaBackend InitializeBackend() {
		MediaBackend backend = new GstBackend();
		backend.Initialize();
		
		Widget videoWidget = backend.CreateVideoWidget();
		if (videoWidget == null) {
			throw new Exception("Unable to create the video widget");
		}

		frame.Child = videoWidget;
		videoWidget.Realize();
		videoWidget.Show();

		backend.ErrorFound += OnBackendErrorFound;
		backend.StatusChanged += OnBackendStatusChanged;
		backend.EndOfStreamReached += OnBackendEndOfStreamReached;
		
		return backend;
	}
	
	private void DisposeBackend() {
		backend.ErrorFound -= OnBackendErrorFound;
		backend.StatusChanged -= OnBackendStatusChanged;
		backend.EndOfStreamReached -= OnBackendEndOfStreamReached;
		
		frame.Remove(frame.Child);

		backend.Dispose();
		backend = null;
	}
	
	private void InitializePositionWatcher() {
		position = new PlayerPositionWatcher(GetPosition);
		position.PositionPulse += OnPositionWatcherPulse;
	}

	private long GetPosition() {
		return backend.CurrentPosition;
	}


	/* Event members */

	private void OnPositionWatcherPulse (long time) {
		if (PositionPulse != null)
			PositionPulse(time);
	}

	private void OnBackendErrorFound(string error) {
		if (ErrorFound != null) {
			ErrorFound(error);
		}
	}
	
	private void OnBackendStatusChanged(MediaStatus newStatus) {
		this.status = newStatus;
		
		//Handle position watcher
		if (newStatus == MediaStatus.Unloaded) {
			position.Stop();
		} else if (newStatus != MediaStatus.Loading) {
			position.Start();
		}
		
		//Print info when loaded
		if (newStatus == MediaStatus.Loaded) {
			Logger.Info("[Player] Media Loaded: Backend={0}, Duration={1}ms, HasVideo={2}, "
				+ "AspectRatio={3}, FrameRate={4}, HasAudio={5}",
				backend.Name, backend.Duration, backend.HasVideo, backend.AspectRatio,
				backend.FrameRate, backend.HasAudio);
		}

		if (StatusChanged != null) {
			StatusChanged(newStatus);
		}

	}
	
	private void OnBackendEndOfStreamReached() {
		position.Stop();
		
		if (EndOfStreamReached != null) {
			EndOfStreamReached();
		}
	}

}

}
