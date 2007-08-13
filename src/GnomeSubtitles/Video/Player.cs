/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007 Pedro Castro
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

using GStreamer;
using Gtk;
using System;

namespace GnomeSubtitles {

public class Player {
	private Socket socket = null;
	private Playbin playbin = null;
	private PlayerPositionWatcher position = null;

	/* Delegate functions */
	private EventHandler EndReached;

	public Player () {
		CreateSocket();
		position = new PlayerPositionWatcher(GetPosition);
	}

	public Socket Widget {
		get { return socket; }
	}

	public void Open (string videoUri) {
		CreatePlaybin();
		
		playbin.EventChanged += OnEventChanged; //TODO delete
		
		playbin.Load(videoUri);
		WaitForPlaybinReady();
	}

	
	public void Close () {
		if (playbin != null)
			DestroyPlaybin();
	}
	
	public PlayerTimeChangedFunc OnPositionChanged {
		set { position.OnPlayerPositionChanged = value; }
	}
	
	public EventHandler OnEndReached {
		set { this.EndReached = value; }
	}
	
	public float AspectRatio {
		get { return playbin.VideoInfo.AspectRatio; }
	}
	
	public TimeSpan Length {
		get { return playbin.Duration; }
	}
	
	public float FrameRate {
		get { return playbin.VideoInfo.FrameRate; }
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
	
	// newPosition in milliseconds
	public void Seek (double newPosition) {
		playbin.Seek(newPosition);
	}

	/* Private members */
	
	private void CreateSocket () {
		socket = new Socket();
		socket.ModifyBg(StateType.Normal, socket.Style.Black);	
	}
	
	private void CreatePlaybin () {
		playbin = new Playbin();
		ConnectSignals();
		playbin.Initiate(socket.Id);
	}
	
	private void DestroyPlaybin () {
		playbin.Dispose();
		DisconnectSignals();
		playbin = null;
	}
	
	// TODO use different method do determine if file type is valid
	private void WaitForPlaybinReady () {
		bool gotVideoInfo = false;
		bool gotDuration = false;

		DateTime endTime = DateTime.Now.AddSeconds(5);
		while (DateTime.Now < endTime) {
			if (!gotDuration) {
				TimeSpan duration = playbin.Duration;
				if (duration == TimeSpan.Zero) {
					System.Threading.Thread.Sleep(15); //milliseconds
					continue;
				}
				else {
					gotDuration = true;
					Console.WriteLine("Got duration: " + duration);
				}
			}
		
			if (!gotVideoInfo) {
				EngineVideoInfo info = playbin.VideoInfo;
				if (info == null) {
					System.Threading.Thread.Sleep(15); //milliseconds
					continue;
				}
				else {
					gotVideoInfo = true;
					Console.WriteLine("Got video info: " + info + " (took " + (DateTime.Now - endTime.AddSeconds(-5)).TotalSeconds + " seconds)");
				}
			}
			return;
		}
		throw new PlayerCouldNotOpenVideoException();
	}
	
	/// <summary>Gets the current player position.</summary>
	private TimeSpan GetPosition () {
		if (playbin == null)
			return TimeSpan.Zero;
		else
			return playbin.CurrentPosition;
	}

	
	/* Event members */
	
	private void ConnectSignals () {
		playbin.StateChanged += OnStateChanged;
		playbin.EventChanged += OnEventChanged;
	}
	
	private void DisconnectSignals () {
		playbin.StateChanged -= OnStateChanged;
		playbin.EventChanged -= OnEventChanged;
	}
	
	private void OnStateChanged (object o, EngineStateArgs args) {
		if ((args.State == MediaStatus.Playing) || (args.State == MediaStatus.Paused))
			position.Start();
		else
			position.Stop();
	}
	
	private void OnEventChanged (object o, EngineEventArgs args) {
		if (args.Event == MediaEvents.EndOfStream) {
			EndReached(this, EventArgs.Empty);		
		}
	}
	
}

}