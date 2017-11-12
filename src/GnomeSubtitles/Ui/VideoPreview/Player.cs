/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2017 Pedro Castro
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

using Gdk;
using GnomeSubtitles.Ui.VideoPreview.Exceptions;
using GStreamer;
using Gtk;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Ui.VideoPreview {

/* Delegates */
public delegate void PlayerErrorEventHandler (Uri videoUri, Exception e);
public delegate void VideoDurationEventHandler (TimeSpan duration);

public class Player {

	private AspectFrame frame = null;
	private Socket socket = null;
	private Playbin playbin = null;
	private PlayerPositionWatcher position = null;
	private bool hasFoundDuration = false;
	private Uri videoUri = null;
	private VideoInfo videoInfo = null;
	private float speed = 1;

	/* Constants */
	public const float DefaultAspectRatio = 1.67f;
	public const float DefaultMinSpeed = 0.1f;
	public const float DefaultSpeedStep = 0.1f;
	public const float DefaultMaxSpeed = 2;

	public Player (AspectFrame aspectFrame) {
		this.frame = aspectFrame;

		InitializeSocket();
		InitializePositionWatcher();
		InitializePlaybin();
	}


	/* Events */
	public event PlayerErrorEventHandler Error;
	public event EndOfStreamEventHandler EndOfStream;
	public event StateEventHandler StateChanged;
	public event PositionPulseEventHandler PositionPulse;
	public event VideoInfoEventHandler FoundVideoInfo;
	public event VideoDurationEventHandler FoundDuration;

	/* Properties */

	public MediaStatus State {
		get { return playbin.CurrentStatus; }
	}

	public bool HasDuration {
		get { return playbin.Duration != TimeSpan.Zero; }
	}

	public TimeSpan Duration {
		get { return playbin.Duration; }
	}

	public bool HasVideoInfo {
		get { return videoInfo != null; }
	}

	public float AspectRatio {
		get { return videoInfo.AspectRatio; }
	}

	public float FrameRate {
		get { return videoInfo.FrameRate; }
	}

	public bool HasAudio {
		get { return videoInfo.HasAudio; }
	}

	public bool HasVideo {
		get { return videoInfo.HasVideo; }
	}

	public Uri VideoUri {
		get { return videoUri; }
	}

	public float Speed {
		get { return speed; }
	}


	/* Public methods */

	public void Open (Uri videoUri) {
		this.videoUri = videoUri;

		/* Load the playbin */
		playbin.Load(videoUri.AbsoluteUri);
	}

	public void Close () {
		position.Stop();
		playbin.Unload();

		videoUri = null;
		hasFoundDuration = false;
		videoInfo = null;
		speed = 1;
	}

	public void Play () {
		playbin.Play();
	}

	public void Pause () {
		playbin.Pause();
	}

    public void SpeedUp () {
        if (this.speed >= DefaultMaxSpeed)
	        return;

		this.speed += DefaultSpeedStep;
		ChangeSpeed(this.speed);
	}

	public void SpeedDown () {
	    if (this.speed <= DefaultMinSpeed)
	        return;

	    this.speed -= DefaultSpeedStep;
		ChangeSpeed(this.speed);
	}

	public void SpeedReset () {
		this.speed = 1;
		ChangeSpeed(this.speed);
	}

	public void Rewind (TimeSpan dec) {
		Seek(playbin.CurrentPosition - dec);
	}

	public void Forward (TimeSpan inc) {
		Seek(playbin.CurrentPosition + inc);
	}

	public void Seek (TimeSpan newPosition) {
		playbin.Seek(newPosition, speed);
	}

	public void Seek (double newPosition) {
		playbin.Seek(newPosition, speed); // newPosition in milliseconds
	}

	public void Dispose () {
		Close();
		playbin.Dispose();
	}


	/* Private members */

	private void InitializeSocket () {
		socket = new Socket();

//		RGBA black = new RGBA();
//		black.Red = 0;
//		black.Green = 0;
//		black.Blue = 0;
//		black.Alpha = 1;
//		socket.OverrideBackgroundColor(StateFlags.Normal, black);

		frame.Child = socket;

		socket.Realize();
		socket.Show();
	}

	private void InitializePlaybin () {
		playbin = new Playbin();

		if (!playbin.Initiate(socket.Id))
			throw new PlayerCouldNotInitiateEngineException();

		playbin.Error += OnPlaybinError;
		playbin.EndOfStream += OnPlaybinEndOfStream;
		playbin.StateChanged += OnPlaybinStateChanged;
		playbin.FoundVideoInfo += OnPlaybinFoundVideoInfo;
		playbin.FoundTag += OnPlaybinFoundTag;
	}

	private void InitializePositionWatcher () {
		position = new PlayerPositionWatcher(GetPosition);
		position.PositionPulse += OnPositionWatcherPulse;
	}

	/// <summary>Gets the current player position.</summary>
	private TimeSpan GetPosition () {
		return playbin.CurrentPosition;
	}

	private void ChangeSpeed (float newSpeed) {
	    playbin.Seek(playbin.CurrentPosition, newSpeed);
	}


	/* Event members */

	private void OnPlaybinError (ErrorEventArgs args) {
		if (Error != null)
			Error(videoUri, new PlayerEngineException(args.Error, args.Debug));
	}

	private void OnPlaybinEndOfStream () {
		position.Stop();
		if (EndOfStream != null)
			EndOfStream();
	}

	private void OnPlaybinStateChanged (StateEventArgs args) {
		if (args.State == MediaStatus.Unloaded)
			position.Stop();
		else
			position.Start();

		if (StateChanged != null)
			StateChanged(args);
	}

	private void OnPositionWatcherPulse (TimeSpan time) {
		if (PositionPulse != null)
			PositionPulse(time);
	}

	private void OnPlaybinFoundVideoInfo (VideoInfoEventArgs args) {
		Console.Error.WriteLine("Got video info: " + args.VideoInfo.ToString());
		this.videoInfo = args.VideoInfo;

		/* Set defaults if there is no video */
		if (!videoInfo.HasVideo) {
			videoInfo.FrameRate = SubtitleConstants.DefaultFrameRate;
			videoInfo.AspectRatio = DefaultAspectRatio;
		}

		frame.Ratio = videoInfo.AspectRatio;

		if (FoundVideoInfo != null)
			FoundVideoInfo(args);
	}

	private void OnPlaybinFoundTag (TagEventArgs args) {
		if ((!hasFoundDuration) && (FoundDuration != null) && (playbin.Duration != TimeSpan.Zero)) {
			TimeSpan duration = playbin.Duration;
			Console.Error.WriteLine("Got video duration: " + duration);

			hasFoundDuration = true;
			FoundDuration(duration);
		}
	}

}

}
