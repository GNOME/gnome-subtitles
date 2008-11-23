/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2008 Pedro Castro
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
using System;
using System.IO;

namespace GnomeSubtitles.Ui.VideoPreview {

/* Delegates */
public delegate TimeSpan PlayerGetTimeFunc (); //Represents a function that gets a time from the player
public delegate void PositionChangedEventHandler (TimeSpan position); //Represents a function that handles changes in the position

public class PlayerPositionWatcher {
	private uint timeoutId = 0;
	
	/* Events */
	public event PositionChangedEventHandler Changed;
	
	/* Delegate functions */
	private PlayerGetTimeFunc PlayerGetPosition;

	
	/* Constants */
	private const int timeout = 100; //milliseconds

	public PlayerPositionWatcher (PlayerGetTimeFunc playerGetPositionFunc) {
		PlayerGetPosition = playerGetPositionFunc;
	}

	
	/* Public methods */
	
	/// <summary>Starts watching for changes on the player position.</summary>
	public void Start () {
		RemoveCheckPositionTimeout();
		AddCheckPositionTimeout();
	}
	
	public void Stop () {
		RemoveCheckPositionTimeout();
	}
	
	/* Event members */

	private void RemoveCheckPositionTimeout () {
		if (timeoutId != 0) {
			GLib.Source.Remove(timeoutId);
			timeoutId = 0;
		}	
	}
	
	private void AddCheckPositionTimeout () {
		timeoutId = GLib.Timeout.Add(timeout, CheckPosition);
	}

	private bool CheckPosition () {
		TimeSpan position = PlayerGetPosition();
		EmitPositionChanged(position);
		return true;
	}
		
	private void EmitPositionChanged (TimeSpan position) {
		if (Changed != null)
			Changed(position);
	}

}

}
