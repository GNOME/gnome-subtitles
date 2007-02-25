/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
 * Copyright (C) 2006-2007 Pedro Castro
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

using System;
using System.IO;

namespace GnomeSubtitles {

public class PlayerPositionWatcher {
	private uint timeoutId = 0;
	private float position = 0;
	private float length = 0;
	private bool endReached = false;
	
	/* Delegate functions */
	private PlayerGetPositionFunc PlayerGetPosition;
	private PlayerEndReachedFunc PlayerEndReached;
	private PlayerPositionChangedFunc PlayerPositionChanged;
	
	/* Constants */
	private const int timeout = 100; //milliseconds

	public PlayerPositionWatcher (PlayerGetPositionFunc playerGetPositionFunc, PlayerEndReachedFunc playerEndReachedFunc) {
		PlayerGetPosition = playerGetPositionFunc;
		PlayerEndReached = playerEndReachedFunc;
	}
	
	/* Public properties */
	
	public bool Paused {
		get { return (timeoutId == 0); }
	}
	
	public float Length {
		get { return length; }
	}
	
	public bool EndReached {
		get { return endReached; }
	}
	
	public PlayerPositionChangedFunc OnPlayerPositionChanged {
		set { PlayerPositionChanged = value; }
	}

	
	/* Public methods */
	
	public void Enable (float length) {
		position = 0;
		this.length = length;
	}
	
	public void Disable () {
		Stop();
		position = 0;
		length = 0;
	}
	
	
	/// <summary>Starts watching for changes on the player position.</summary>
	/// <remarks>This should only be used after opening a new video. If issued right after a <see cref="Stop" />,
	/// unpredictable results may arise. Use <see cref="Enabled" /> to toggle enable on the same video.</remarks>
	public void Start () {
		endReached = false;
		RemoveCheckPositionTimeout();
		AddCheckPositionTimeout();
	}
	
	public void Stop () {
		RemoveCheckPositionTimeout();
	}
	
	public void Check () {
		if (Paused) { //Only check if paused, otherwise automatic updates are used
			CheckPosition();
		}	
	}
	
	/* Event members */

	private bool CheckPosition () {
		bool caughtException = false;
		
		try {
			position = PlayerGetPosition();
		}
		catch (IOException) {
			caughtException = true;
		}

		if ((position == -1) || (caughtException)) { //The end has been reached
			endReached = true;
			Stop();
			PlayerEndReached();
			EmitPositionChanged(length);
		}
		else {
			if (position < length)
				endReached = false;
			
			EmitPositionChanged(position);
		}

		return true;
	}
	
	private void AddCheckPositionTimeout () {
		timeoutId = GLib.Timeout.Add(timeout, CheckPosition);
	}
	
	private void RemoveCheckPositionTimeout () {
		if (timeoutId != 0) {
			GLib.Source.Remove(timeoutId);
			timeoutId = 0;
		}	
	}
	
	private void EmitPositionChanged (float position) {
		if (PlayerPositionChanged != null)
			PlayerPositionChanged(position);
	}


}

}