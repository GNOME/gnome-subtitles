/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2008 Pedro Castro
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

using GnomeSubtitles.Ui.VideoPreview.Exceptions;
using GStreamer;
using Gtk;
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

	public Player (AspectFrame aspectFrame) {
		this.frame = aspectFrame;

		InitializeSocket();
		InitializePositionWatcher();
		InitializePlaybin();
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
		position.Changed += OnPositionWatcherChanged;		
	}
	
	/* Events */
	public event PlayerErrorEventHandler Error;
	public event EndOfStreamEventHandler EndOfStream;
	public event StateEventHandler StateChanged;
	public event PositionChangedEventHandler PositionChanged;
	public event VideoInfoEventHandler FoundVideoInfo;
	public event VideoDurationEventHandler FoundDuration;
	

	/* Properties */

	public TimeSpan Duration {
		get { return playbin.Duration; }
	}
	
	public float AspectRatio {
		get { return playbin.VideoInfo.AspectRatio; }
	}
	
	public float FrameRate {
		get { return playbin.VideoInfo.FrameRate; }
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
	}

	public void Play () {
		playbin.Play();
	}
	
	public void Pause () {
		playbin.Pause();
	}
	
	public void Rewind (TimeSpan dec) {
		Seek(playbin.CurrentPosition - dec);
	}
	
	public void Forward (TimeSpan inc) {
		Seek(playbin.CurrentPosition + inc);
	}

	public void Seek (TimeSpan newPosition) {
		playbin.Seek(newPosition);
	}
	
	public void Seek (double newPosition) {
		playbin.Seek(newPosition); // newPosition in milliseconds
	}
	
	public void Dispose () {
		Close();
		playbin.Dispose();
	}


	/* Private members */
	
	private void InitializeSocket () {
		socket = new Socket();
		socket.ModifyBg(StateType.Normal, socket.Style.Black);

		frame.Child = socket;

		socket.Realize();
		socket.Show();
	}
	
	/// <summary>Gets the current player position.</summary>
	private TimeSpan GetPosition () {
		return playbin.CurrentPosition;
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
	
	private void OnPositionWatcherChanged (TimeSpan time) {
		if (PositionChanged != null)
			PositionChanged(time);
	}

	private void OnPlaybinFoundVideoInfo (VideoInfoEventArgs args) {
		Console.Error.WriteLine("Got video info: " + args.VideoInfo.ToString());
		frame.Ratio = args.VideoInfo.AspectRatio;

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