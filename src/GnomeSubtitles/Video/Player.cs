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

/* Delegates */
public delegate void PlayerErrorCaughtFunc (string message); //Represents a function that handles a caught error

public class Player {
	private Socket socket = null;
	private Playbin playbin = null;
	private PlayerPositionWatcher position = null;
	private bool playbinLoadError= false; //used to indicate whether an error has occured while loading the playbin

	/* Delegate functions */
	private EventHandler EndReached;
	private PlayerErrorCaughtFunc ErrorCaught;

	public Player () {
		CreateSocket();
		position = new PlayerPositionWatcher(GetPosition);
	}

	public Socket Widget {
		get { return socket; }
	}
	
	public void Open (Uri videoUri) {
		playbin = new Playbin();
	
		/* Initialize the playbin */
		if (!playbin.Initiate(socket.Id))
			throw new PlayerCouldNotOpenVideoException();
		
		/* Handle errors during playbin loading */
		playbin.EventChanged += OnPlaybinLoadEventChanged;
		
		/* Load the playbin */
		playbin.Load(videoUri.AbsoluteUri);
		
		/* Wait for the playbin to be ready (have video information) */
		bool isReady = WaitForPlaybinReady();
		
		playbin.EventChanged -= OnPlaybinLoadEventChanged;
		playbinLoadError = false;
		if (!isReady) {
			/* An error has occurred, returning */
			throw new PlayerCouldNotOpenVideoException();
		}

		/* Load was successful, connecting the normal handlers */
		playbin.StateChanged += OnStateChanged;
		playbin.EventChanged += OnEventChanged;
		
		/* Start position watcher */
		position.Start();
	}
	
	public void Close () {
		if (playbin != null) {
			playbin.StateChanged -= OnStateChanged;
			playbin.EventChanged -= OnEventChanged;
			DestroyPlaybin();
			
			position.Stop();
		}
	}
	
	public PlayerTimeChangedFunc OnPositionChanged {
		set { position.OnPlayerPositionChanged = value; }
	}
	
	public PlayerErrorCaughtFunc OnErrorCaught {
		set { this.ErrorCaught = value; }
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

	private void DestroyPlaybin () {
		playbinLoadError = false;
		playbin.Dispose();
		playbin = null;
	}
	
	/// <summary>Waits for the playbin to be ready.</summary>
	/// <param name=""></param>
	/// <remarks>The playbin is ready when it is able to access both the duration and video information.</remarks>
	/// <returns>Whether the playbin is ready.</returns>
	private bool WaitForPlaybinReady () {
		bool gotVideoInfo = false;
		bool gotDuration = false;

		DateTime endTime = DateTime.Now.AddSeconds(5); //Total time to wait for either successful load or error
		while (DateTime.Now < endTime) {
			if (playbinLoadError)
				return false;
		
			/* Check for duration if it hasn't been accessed yet */
			if (!gotDuration) {
				TimeSpan duration = playbin.Duration;
				if (duration == TimeSpan.Zero) {
					GLib.MainContext.Iteration(); //Because an error event may be triggered and we have to catch it
					System.Threading.Thread.Sleep(15); //milliseconds
					continue;
				}
				else {
					gotDuration = true;
					Console.Error.WriteLine("Got duration: " + duration);
				}
			}
		
			/* Check for video information if it hasn't been accessed yet */
			if (!gotVideoInfo) {
				EngineVideoInfo info = playbin.VideoInfo;
				if (info == null) {
					GLib.MainContext.Iteration(); //Because an error event may be triggered and we have to catch it
					System.Threading.Thread.Sleep(15); //milliseconds
					continue;
				}
				else {
					gotVideoInfo = true;
					
					Console.Error.WriteLine("Got video info: " + info + " (took " + (DateTime.Now - endTime.AddSeconds(-5)).TotalSeconds + " seconds)");
				}
			}

			/* Was able to access all info, returning */
			return true;
		}
		
		/* Was not able to access any info */
		return false;
	}
	
	/// <summary>Gets the current player position.</summary>
	private TimeSpan GetPosition () {
		if (playbin == null)
			return TimeSpan.Zero;
		else
			return playbin.CurrentPosition;
	}

	
	/* Event members */

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
		else if (args.Event == MediaEvents.Error) {
			ErrorCaught(args.Message);
		}
	}
	
	private void OnPlaybinLoadEventChanged (object o, EngineEventArgs args) {
		if (args.Event == MediaEvents.Error) {
			Console.Error.WriteLine("Caught an error while loading the playbin: " + args.Message);
			playbinLoadError = true;
		}	
	}
	
}

}