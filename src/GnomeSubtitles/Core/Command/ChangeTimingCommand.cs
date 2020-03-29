/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2011 Pedro Castro
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

using Mono.Unix;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Core.Command {

public abstract class ChangeTimingCommand : FixedSingleSelectionCommand {
	private TimeSpan storedTime;
	private int storedFrames = -1;
	private bool seekAfterChange = false; //TODO put this in upper classes

	/* Protected variables */
	protected Subtitle subtitle = null;

	public ChangeTimingCommand (int frames, string description, bool seekAfterChange): base(description, true, true) {
		this.subtitle = Base.Document.Subtitles[Path];
		this.storedFrames = frames;
		this.seekAfterChange = seekAfterChange;
	}

	public ChangeTimingCommand (TimeSpan time, string description, bool seekAfterChange): base(description, true, true) {
		this.subtitle = Base.Document.Subtitles[Path];
		this.storedTime = time;
		this.seekAfterChange = seekAfterChange;
	}

	public override bool CanGroupWith (Command command) {
		return (Util.PathsAreEqual(Path, (command as ChangeTimingCommand).Path));
	}

	protected override bool ChangeValues () {
		TimeSpan previousTime = GetPreviousTime();
		if (storedFrames == -1)
			SetTime(storedTime);
		else {
			SetFrames(storedFrames);
			storedFrames = -1; //Set to -1 because times will be used on the next times
		}
		storedTime = previousTime;
		return true;
	}

	protected override void PostProcess () {
		if (this.seekAfterChange) {
			Base.Ui.Video.SeekToSelection(true);
		}
	}

	protected abstract TimeSpan GetPreviousTime ();
	protected abstract void SetTime (TimeSpan storedTime);
	protected abstract void SetFrames (int storedFrames);
}

public class ChangeStartCommand : ChangeTimingCommand {
	private static string description = Catalog.GetString("Editing Start");

	public ChangeStartCommand (int frames, bool seekAfterChange): base(frames, description, seekAfterChange) {
	}

	public ChangeStartCommand (TimeSpan time, bool seekAfterChange): base(time, description, seekAfterChange) {
	}

	/* Overridden methods */

	protected override TimeSpan GetPreviousTime () {
		return subtitle.Times.Start;
	}

	protected override void SetTime (TimeSpan storedTime) {
		subtitle.Times.Start = storedTime;
	}

	protected override void SetFrames (int storedFrames) {
		subtitle.Frames.Start = storedFrames;
	}
}

public class ChangeEndCommand : ChangeTimingCommand {
	private static string description = Catalog.GetString("Editing End");

	public ChangeEndCommand (int frames, bool seekAfterChange): base(frames, description, seekAfterChange) {
	}

	public ChangeEndCommand (TimeSpan time, bool seekAfterChange): base(time, description, seekAfterChange) {
	}

	protected override TimeSpan GetPreviousTime () {
		return subtitle.Times.End;
	}

	protected override void SetTime (TimeSpan storedTime) {
		subtitle.Times.End = storedTime;
	}

	protected override void SetFrames (int storedFrames) {
		subtitle.Frames.End = storedFrames;
	}
}

public class ChangeDurationCommand : ChangeTimingCommand {
	private static string description = Catalog.GetString("Editing Duration");

	public ChangeDurationCommand (int frames, bool seekAfterChange): base(frames, description, seekAfterChange) {
	}

	public ChangeDurationCommand (TimeSpan time, bool seekAfterChange): base(time, description, seekAfterChange) {
	}

	protected override TimeSpan GetPreviousTime () {
		return subtitle.Times.Duration;
	}

	protected override void SetTime (TimeSpan storedTime) {
		subtitle.Times.Duration = storedTime;
	}

	protected override void SetFrames (int storedFrames) {
		subtitle.Frames.Duration = storedFrames;
	}
}

}
