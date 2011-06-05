/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2009,2011 Pedro Castro
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

using GnomeSubtitles.Ui.View;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using SubLib.Core.Timing;
using System;

namespace GnomeSubtitles.Core.Command {

public class ShiftTimingsCommand : FixedMultipleSelectionCommand {
	private static string description = Catalog.GetString("Shifting timings");
	private TimeSpan time;
	private int frames;
	private bool useTimes = true;

	public ShiftTimingsCommand (TimeSpan time, SelectionIntended selectionIntended) : base(description, false, selectionIntended, null, true) {
		this.time = time;
		useTimes = true;
	}
	
	public ShiftTimingsCommand (int frames, SelectionIntended selectionIntended) : base(description, false, selectionIntended, null, true) {
		this.frames = frames;
		useTimes = false;
	}

	protected override bool ChangeValues () {
		if (useTimes) {
			if (ApplyToAll)
				ShiftTimesAll();
			else if (ApplyToSimple)
				ShiftTimesSimple();
			else
				ShiftTimesRange();
		}
		else {
			if (ApplyToAll)
				ShiftFramesAll();
			else if (ApplyToSimple)
				ShiftFramesSimple();
			else
				ShiftFramesRange();
		}
		return true;
	}
	
	protected override void PostProcess () {
		Base.Ui.Video.SeekToSelection(true);
	}
	
	/* Private Members */

	private void ShiftTimesAll () {
		ShiftOperator shiftOp = new ShiftOperator(Base.Document.Subtitles);
		shiftOp.Shift(time);
		time = time.Negate();
	}
	
	private void ShiftFramesAll () {
		ShiftOperator shiftOp = new ShiftOperator(Base.Document.Subtitles);
		shiftOp.Shift(frames);
		frames = -frames;
	}
	
	private void ShiftTimesSimple () {
		foreach (TreePath path in Paths) {
			Subtitle subtitle = Base.Document.Subtitles[path];
			subtitle.Times.Shift(time);
		}
		time = time.Negate();
	}

	private void ShiftFramesSimple () {
		foreach (TreePath path in Paths) {
			Subtitle subtitle = Base.Document.Subtitles[path];
			subtitle.Frames.Shift(frames);
		}
		frames = -frames;
	}
	
	private void ShiftTimesRange () {
		int firstSubtitle = Util.PathToInt(FirstPath);
		int lastSubtitle = Util.PathToInt(LastPath);
		
		ShiftOperator shiftOp = new ShiftOperator(Base.Document.Subtitles);
		shiftOp.Shift(time, firstSubtitle, lastSubtitle);
		time = time.Negate();
	}
	
	private void ShiftFramesRange () {
		int firstSubtitle = Util.PathToInt(FirstPath);
		int lastSubtitle = Util.PathToInt(LastPath);
		
		ShiftOperator shiftOp = new ShiftOperator(Base.Document.Subtitles);
		shiftOp.Shift(frames, firstSubtitle, lastSubtitle);
		frames = -frames;
	}
}

}
