/*
 * This file is part of Gnome Subtitles.
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

namespace GnomeSubtitles {

public class AdjustTimingsCommand : FixedMultipleSelectionCommand {
	private static string description = Cat.Get("Adjusting timings");
	private TimeSpan firstTime, lastTime;
	private int firstFrame, lastFrame;
	private bool useTimes = true;

	public AdjustTimingsCommand (TimeSpan firstTime, TimeSpan lastTime, SelectionIntended selectionIntended) : base(description, false, selectionIntended, true) {
		this.firstTime = firstTime;
		this.lastTime = lastTime;
		useTimes = true;
	}
	
	public AdjustTimingsCommand (int firstFrame, int lastFrame, SelectionIntended selectionIntended) : base(description, false, selectionIntended, true) {
		this.firstFrame = firstFrame;
		this.lastFrame = lastFrame;
		useTimes = false;
	}

	protected override bool ChangeValues () {
		if (useTimes) {
			if (ApplyToAll)
				AdjustAllSubtitlesTime();
			else
				AdjustSubtitlesTime();	
		}
		else {
			if (ApplyToAll)
				AdjustAllSubtitlesFrames();
			else
				AdjustSubtitlesFrames();
		}
		return true;
	}

	private void AdjustAllSubtitlesTime () {
		Subtitles subtitles = Global.Document.Subtitles;
		
		TimeSpan oldFirstTime = subtitles[0].Times.Start;
		TimeSpan oldLastTime = subtitles[subtitles.Count - 1].Times.Start;
		
		subtitles.AdjustTimings(firstTime, lastTime);
		
		firstTime = oldFirstTime;
		lastTime = oldLastTime;
	}
	
	private void AdjustAllSubtitlesFrames () {
		Subtitles subtitles = Global.Document.Subtitles;
		
		int oldFirstFrame = subtitles[0].Frames.Start;
		int oldLastFrame = subtitles[subtitles.Count - 1].Frames.Start;
		
		subtitles.AdjustTimings(firstFrame, lastFrame);
		
		firstFrame = oldFirstFrame;
		lastFrame = oldLastFrame;
	}
	
	private void AdjustSubtitlesTime () {
		Subtitles subtitles = Global.Document.Subtitles;
		
		int firstSubtitle = Util.PathToInt(FirstPath);
		int lastSubtitle = Util.PathToInt(LastPath);
		
		TimeSpan oldFirstTime = subtitles[firstSubtitle].Times.Start;
		TimeSpan oldLastTime = subtitles[lastSubtitle].Times.Start;
		
		subtitles.AdjustTimings(firstSubtitle, firstTime, lastSubtitle, lastTime);
		
		firstTime = oldFirstTime;
		lastTime = oldLastTime;
	}
	
	private void AdjustSubtitlesFrames () {
		Subtitles subtitles = Global.Document.Subtitles;
		
		int firstSubtitle = Util.PathToInt(Paths[0]);
		int lastSubtitle = Util.PathToInt(Paths[Paths.Length - 1]);
		
		int oldFirstFrame = subtitles[firstSubtitle].Frames.Start;
		int oldLastFrame = subtitles[lastSubtitle].Frames.Start;
		
		subtitles.AdjustTimings(firstSubtitle, firstFrame, lastSubtitle, lastFrame);
		
		firstFrame = oldFirstFrame;
		lastFrame = oldLastFrame;
	}
}

}
