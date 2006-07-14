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
using System;
using System.Collections;
using SubLib;

namespace GnomeSubtitles {


public abstract class ChangeFrameRateCommand : Command {
	private float storedFrameRate = 0;

	public ChangeFrameRateCommand (GUI gui, string description, float frameRate) : base(gui, description, false) {
		this.storedFrameRate = frameRate;
	}
	
	public override void Execute () {
		float previousFrameRate = GetFrameRate();
		SetFrameRate(storedFrameRate);
		storedFrameRate = previousFrameRate;
		
		GUI.RefreshAndReselect();
	}
	
	protected abstract float GetFrameRate ();
	protected abstract void SetFrameRate (float frameRate);
}

public class ChangeInputFrameRateCommand : ChangeFrameRateCommand {
	private	static string description = "Changing Input Frame Rate";

	public ChangeInputFrameRateCommand (GUI gui, float frameRate) : base(gui, description, frameRate) {
	}
	
	protected override float GetFrameRate () {
		return GUI.Core.Subtitles.Properties.OriginalFrameRate;
	}
	
	protected override void SetFrameRate (float frameRate) {
		GUI.Core.Subtitles.ChangeOriginalFrameRate(frameRate);
	}
}

public class ChangeMovieFrameRateCommand : ChangeFrameRateCommand {
	private	static string description = "Changing Movie Frame Rate";

	public ChangeMovieFrameRateCommand (GUI gui, float frameRate) : base(gui, description, frameRate) {
	}
	
	protected override float GetFrameRate () {
		return GUI.Core.Subtitles.Properties.CurrentFrameRate;
	}
	
	protected override void SetFrameRate (float frameRate) {
		GUI.Core.Subtitles.ChangeFrameRate(frameRate);
	}
}

/* Commands that use Single Selection */

public abstract class ChangeTimingCommand : SingleSelectionCommand {
	private Subtitle subtitle;
	private TimeSpan storedTime;
	private int storedFrames = -1;

	
	public ChangeTimingCommand (GUI gui, int frames, string description): base(gui, description, true) {
		this.subtitle = gui.Core.Subtitles.Get(Path);
		this.storedFrames = frames;
	}
	
	public ChangeTimingCommand (GUI gui, TimeSpan time, string description): base(gui, description, true) {
		this.subtitle = gui.Core.Subtitles.Get(Path);
		this.storedTime = time;
	}
	
	protected Subtitle Subtitle {
		get { return subtitle; }
	}

	public override bool CanGroupWith (Command command) {
		return (Path.Compare((command as ChangeTimingCommand).Path) == 0);	
	}

	protected override void ChangeValues () {
		TimeSpan previousTime = GetPreviousTime();
		if (storedFrames == -1)
			SetTime(storedTime);
		else {
			SetFrames(storedFrames);
			storedFrames = -1;
		}
			
		storedTime = previousTime;	
	}
	
	protected override void AfterSelected () {
		UpdateDependentTimings();
	}

	protected abstract TimeSpan GetPreviousTime ();
	protected abstract void SetTime (TimeSpan storedTime);
	protected abstract void SetFrames (int storedFrames);
	protected abstract void UpdateDependentTimings ();
}

public class ChangeStartCommand : ChangeTimingCommand {
	private static string description = "Editing From";

	public ChangeStartCommand (GUI gui, int frames): base(gui, frames, description) {
	}
	
	public ChangeStartCommand (GUI gui, TimeSpan time): base(gui, time, description) {
	}

	protected override TimeSpan GetPreviousTime () {
		return Subtitle.Times.Start;
	}
	
	protected override void SetTime (TimeSpan storedTime) {
		Subtitle.Times.Start = storedTime;
	}
	
	protected override void SetFrames (int storedFrames) {
		Subtitle.Frames.Start = storedFrames;
	}
	
	protected override void UpdateDependentTimings () {
		GUI.SubtitleEdit.LoadDurationTiming();
	} 

}

public class ChangeEndCommand : ChangeTimingCommand {
	private static string description = "Editing To";

	public ChangeEndCommand (GUI gui, int frames): base(gui, frames, description) {
	}
	
	public ChangeEndCommand (GUI gui, TimeSpan time): base(gui, time, description) {
	}

	protected override TimeSpan GetPreviousTime () {
		return Subtitle.Times.End;
	}
	
	protected override void SetTime (TimeSpan storedTime) {
		Subtitle.Times.End = storedTime;
	}
	
	protected override void SetFrames (int storedFrames) {
		Subtitle.Frames.End = storedFrames;
	}
	
	protected override void UpdateDependentTimings () {
		GUI.SubtitleEdit.LoadDurationTiming();
	} 

}

public class ChangeDurationCommand : ChangeTimingCommand {
	private static string description = "Editing During";

	public ChangeDurationCommand (GUI gui, int frames): base(gui, frames, description) {
	}
	
	public ChangeDurationCommand (GUI gui, TimeSpan time): base(gui, time, description) {
	}

	protected override TimeSpan GetPreviousTime () {
		return Subtitle.Times.Duration;
	}
	
	protected override void SetTime (TimeSpan storedTime) {
		Subtitle.Times.Duration = storedTime;
	}
	
	protected override void SetFrames (int storedFrames) {
		Subtitle.Frames.Duration = storedFrames;
	}
	
	protected override void UpdateDependentTimings () {
		GUI.SubtitleEdit.LoadEndTiming();
	} 

}


public class ChangeTextCommand : SingleSelectionCommand {
	private static string description = "Editing Text";
	private Subtitle subtitle;
	string storedText;

	public ChangeTextCommand (GUI gui, string text) : base(gui, description, true) {
		this.subtitle = gui.Core.Subtitles.Get(Path);
		this.storedText = text;
	}
	
	//TODO: only group when it's the text of the same word
	public override bool CanGroupWith (Command command) {
		return (Path.Compare((command as ChangeTextCommand).Path) == 0);	
	}
	
	protected override void ChangeValues () {
		string previousText = subtitle.Text.Get();
		subtitle.Text.Set(storedText);
		storedText = previousText;		
	}
	
	protected override void AfterSelected () {
		GUI.SubtitleView.RedrawSelectedRow();
	}

}

public class InsertSubtitleCommand : SingleSelectionCommand {
	private static string description = "Inserting Subtitle";
	private Subtitle subtitle = null;
	
	public InsertSubtitleCommand (GUI gui) : base(gui, description, false) {
		TreePath lastPath = gui.SubtitleView.LastSelectedPath;
		int pathIndex = lastPath.Indices[0] + 1;
		Path = new TreePath(pathIndex.ToString());
	}

	public override void Execute () {
		SubtitleView subtitleView = GUI.SubtitleView;
		subtitleView.UnselectAll();

		if (subtitle == null)
			GUI.Core.Subtitles.AddAfter(PathIndex - 1);
		else
			GUI.Core.Subtitles.Add(subtitle, PathIndex);

		SelectPath();
		ScrollToSelection();
	}

	public override void Undo () {
		SubtitleView subtitleView = GUI.SubtitleView;
		subtitleView.UnselectAll();
		Subtitles subtitles = GUI.Core.Subtitles; 
		subtitle = subtitles.Get(PathIndex);
		subtitles.Remove(Path);
		SelectPath();
		ScrollToSelection();
	}
	
	public override void Redo () {
		Execute();
	}

}


/* Commands that use Multiple Selection */

public abstract class ChangeStyleCommand : MultipleSelectionCommand {
	private bool styleValue;

	public ChangeStyleCommand (GUI gui, string description, bool newStyleValue) : base(gui, description, false) {
		this.styleValue = newStyleValue;
	}
	
	protected override void ChangeValues () {
		Subtitles subtitles = GUI.Core.Subtitles;
		foreach (TreePath path in Paths) {
			Subtitle subtitle = subtitles.Get(path);
			SetStyle(subtitle, styleValue);
		}
		ToggleStyleValue();
	}

	protected abstract void SetStyle (Subtitle subtitle, bool style);
	
	private void ToggleStyleValue () {
		styleValue = !styleValue;
	}
}

public class ChangeBoldStyleCommand : ChangeStyleCommand {
	private static string description = "Toggling Bold";

	public ChangeBoldStyleCommand (GUI gui, bool newStyleValue) : base(gui, description, newStyleValue) {
	}

	protected override void SetStyle (Subtitle subtitle, bool styleValue) {
		subtitle.Style.Bold = styleValue;
	}
}

public class ChangeItalicStyleCommand : ChangeStyleCommand {
	private static string description = "Toggling Italic";

	public ChangeItalicStyleCommand (GUI gui, bool newStyleValue) : base(gui, description, newStyleValue) {
	}

	protected override void SetStyle (Subtitle subtitle, bool styleValue) {
		subtitle.Style.Italic = styleValue;
	}
}

public class ChangeUnderlineStyleCommand : ChangeStyleCommand {
	private static string description = "Toggling Underline";

	public ChangeUnderlineStyleCommand (GUI gui, bool newStyleValue) : base(gui, description, newStyleValue) {
	}

	protected override void SetStyle (Subtitle subtitle, bool styleValue) {
		subtitle.Style.Underline = styleValue;
	}
}

public class DeleteSubtitlesCommand : MultipleSelectionCommand {
	private static string description = "Deleting Subtitles";
	private Subtitle[] deletedSubtitles = null;
	
	public DeleteSubtitlesCommand (GUI gui) : base(gui, description, false) {
		deletedSubtitles = new Subtitle[Paths.Length];
	}
	
	public override void Execute () {
		DeleteSubtitles(true);
	}
	
	public override void Undo () {
		SubtitleView subtitleView = GUI.SubtitleView;
		subtitleView.DisconnectSelectionChangedSignals();
		subtitleView.UnselectAll();
		for (int pathIndex = 0 ; pathIndex < Paths.Length ; pathIndex++) {
			Subtitle subtitle = deletedSubtitles[pathIndex];
			int index = Paths[pathIndex].Indices[0];
			GUI.Core.Subtitles.Add(subtitle, index);
		}
		subtitleView.ConnectSelectionChangedSignals();
		SelectPaths();
		ScrollToSelection();
	}
	
	public override void Redo () {
		DeleteSubtitles(false);
	}
	
	private void DeleteSubtitles (bool toStoreSubtitles) {
		Subtitles subtitles = GUI.Core.Subtitles;
		GUI.SubtitleView.DisconnectSelectionChangedSignals();
		for (int pathIndex = 0 ; pathIndex < Paths.Length ; pathIndex++) {
			TreePath path = Paths[pathIndex];
			
			//Subtract pathIndex because indexes decrement as subtitles are removed. Paths must be sorted.
			int index = path.Indices[0] - pathIndex; 
			
			Subtitle subtitle = subtitles.Get(index);

			if (toStoreSubtitles)
				deletedSubtitles[pathIndex] = subtitle;

			subtitles.Remove(index);
		}
		GUI.SubtitleView.ConnectSelectionChangedSignals();
		SelectSubtitleAferDeletion();
	}
	
	private void SelectSubtitleAferDeletion () {
		TreePath firstDeleted = Paths[0];
		int firstIndex = firstDeleted.Indices[0];
		int subtitleCount = GUI.Core.Subtitles.Count;
		if (subtitleCount == 0) {
			GUI.SubtitleView.Reselect();
			return;
		}
		int indexToSelect = Math.Min(firstIndex, subtitleCount - 1);
		TreePath pathToSelect = new TreePath(indexToSelect.ToString());
		GUI.SubtitleView.Widget.Selection.SelectPath(pathToSelect);
		GUI.SubtitleView.Refresh();
		GUI.SubtitleView.ScrollToPath(pathToSelect);		
	}
}

}