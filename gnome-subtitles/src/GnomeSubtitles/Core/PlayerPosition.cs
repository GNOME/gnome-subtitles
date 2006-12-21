/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
 * Copyright (C) 2006 Pedro Castro
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


namespace GnomeSubtitles {

public class PlayerPosition {
	private Player player = null;
	private bool paused = true;
	private uint timeoutId = 0;
	
	/* Constants */
	private const int timeout = 500; //milliseconds
	
	/* Events */	
	private event PlayerPositionHandler positionChanged;

	public PlayerPosition (Player player, PlayerPositionHandler handler) {
		this.player = player;
		positionChanged += handler;
	}
	
	public bool Paused {
		get { return paused; }
		set { paused = value; }	
	}
	
	/// <summary>Starts watching for changes on the player position.</summary>
	/// <remarks>This should only be used after opening a new video. If issued right after a <see cref="Stop" />,
	/// unpredictable results may arise. Use <see cref="Enabled" /> to toggle enable on the same video.</remarks>
	public void Start () {
		paused = true;
		
		if (timeoutId != 0)
			Stop();

		timeoutId = GLib.Timeout.Add(timeout, CheckPosition);
	}
	
	public void Stop () {
		paused = true;
		
		if (timeoutId != 0) {
			GLib.Source.Remove(timeoutId);
			timeoutId = 0;
		}
	}
	
	/* Event members */
		
	private bool CheckPosition () {
		if (!paused) {
			float position = player.TimePosition;
			System.Console.WriteLine("Position is " + position);
			positionChanged(position);
		}
		return true;
	}



}


}