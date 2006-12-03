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

using Gtk;
using SubLib;
using System;

namespace GnomeSubtitles {

public class ShiftTimingsCommand : FixedMultipleSelectionCommand {
	private static string description = "Shifting timings";
	private TimeSpan time;
	private int frames;
	private bool useTimes = true;

	public ShiftTimingsCommand (TimeSpan time, SelectionType selectionType) : base(description, false, selectionType, true) {
		this.time = time;
		useTimes = true;
	}
	
	public ShiftTimingsCommand (int frames, SelectionType selectionType) : base(description, false, selectionType, true) {
		this.frames = frames;
		useTimes = false;
	}

	protected override bool ChangeValues () {
		if (useTimes) {
			if (ApplyToAll)
				ShiftAllSubtitlesTime();
			else
				ShiftSubtitlesTime();	
		}
		else {
			if (ApplyToAll)
				ShiftAllSubtitlesFrames();
			else
				ShiftSubtitlesFrames();
		}
		return true;
	}
	
	/* Private Members */

	private void ShiftAllSubtitlesTime () {
		Global.Subtitles.ShiftTimings(time);
		time = time.Negate();
	}
	
	private void ShiftAllSubtitlesFrames () {
		Global.Subtitles.ShiftTimings(frames);
		frames = -frames;
	}
	
	private void ShiftSubtitlesTime () {
		foreach (TreePath path in Paths) {
			Subtitle subtitle = Global.Subtitles[path];
			subtitle.Times.Shift(time);
		}
		time = time.Negate();	
	}

	private void ShiftSubtitlesFrames () {
		foreach (TreePath path in Paths) {
			Subtitle subtitle = Global.Subtitles[path];
			subtitle.Frames.Shift(frames);
		}
		frames = -frames;
	}
}

}