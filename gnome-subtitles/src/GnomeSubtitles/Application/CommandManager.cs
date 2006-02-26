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

public class CommandManager {
	private int limit = 25;
	private Command[] commands = null;

	
	private int undoCount = 0;
	private int redoCount = 0;
	private int iterator = 0;
	
	public event EventHandler UndoToggled;
	public event EventHandler RedoToggled;

	public CommandManager (int undoLimit, EventHandler onUndoToggled, EventHandler onRedoToggled) {
		limit = undoLimit;
		commands = new Command[undoLimit];
		UndoToggled += onUndoToggled;
		RedoToggled += onRedoToggled;
	}
	
	public bool CanUndo {
		get { return undoCount > 0; }
	}
	
	public bool CanRedo {
		get { return redoCount > 0; }
	}
	
	public void Execute (Command command) {
		command.Execute();
		ProcessExecute(command);
	}
	
	public void Undo () {
		if (!CanUndo)
			return;
		
		ProcessUndo();	
		GetCommand().UnExecute();
	}
	
	public void Redo () {
		if (!CanRedo)
			return;
			
		GetCommand().Execute();
		ProcessRedo();	
	}


	private void ProcessExecute (Command command) {
		bool couldUndoBefore = CanUndo;
		bool couldRedoBefore = CanRedo;
		
		bool canGroup = false;
		if (CanUndo && (command is GroupableCommand)) {
			Command lastCommand = commands[iterator - 1];
			if ((lastCommand.GetType() == command.GetType()) && (lastCommand as GroupableCommand).CanGroupWith(command))
				canGroup = true;
		}
		
		if (!canGroup) {
			commands[iterator] = command;
			Next();
			undoCount = IncrementCount(undoCount);
		}
		
		ClearRedo();
		
		if (!couldUndoBefore)
			EmitUndoToggled();
			
		if (couldRedoBefore)
			EmitRedoToggled();
	}
	
	private void ProcessUndo () {
		bool couldRedoBefore = CanRedo;
	
		Previous();
		undoCount = DecrementCount(undoCount);
		redoCount = IncrementCount(redoCount);
		
		if (!CanUndo)
			EmitUndoToggled();
		
		if (!couldRedoBefore)
			EmitRedoToggled();
	}
	
	private void ProcessRedo () {
		bool couldUndoBefore = CanUndo;
	
		Next();
		undoCount = IncrementCount(undoCount);
		redoCount = DecrementCount(redoCount);	
		
		if (!CanRedo)
			EmitRedoToggled();
			
		if (!couldUndoBefore)
			EmitUndoToggled();
	}
	
	private void EmitUndoToggled() {
		if (UndoToggled != null)
			UndoToggled(this, EventArgs.Empty);
	}
	
	private void EmitRedoToggled() {
		if (RedoToggled != null)
			RedoToggled(this, EventArgs.Empty);
	}
	
	private Command GetCommand () {
		return commands[iterator];
	}
	
	private void Next () {
		if (iterator == (limit - 1))
			iterator = 0;
		else
			iterator++;
	}
	
	private void Previous () {
		if (iterator == 0)
			iterator = (limit - 1);
		else
			iterator--;	
	}
	
	private int IncrementCount (int count) {
		if (count < limit)
			return count + 1;
		else
			return count;
	}
	
	private int DecrementCount (int count) {
		return count - 1;
	}
	
	private void ClearRedo () {
		redoCount = 0;
	}

}

public abstract class Command {
	private GUI gui = null;
	private string description;

	public Command (GUI gui, string description) {
		this.gui = gui;
		this.description = description;
	}
	
	public string Description {
		get { return description; }
	}

	protected GUI GUI {
		get { return gui; }
	}

	public abstract void Execute ();
	public abstract void UnExecute ();
}

public abstract class GroupableCommand : Command {

	public GroupableCommand (GUI gui, string description) : base(gui, description) {
	}

	public abstract bool CanGroupWith (Command command);

}

public abstract class ChangeTimingCommand : GroupableCommand {
	private TreePath path;
	private Subtitle subtitle;
	private TimeSpan storedTime;
	private int storedFrames = -1;

	
	public ChangeTimingCommand (GUI gui, Subtitle subtitle, int frames, string description): base(gui, description) {
		this.path = GUI.SubtitleView.Widget.Selection.GetSelectedRows()[0];
		this.subtitle = subtitle;
		this.storedFrames = frames;
	}
	
	public ChangeTimingCommand (GUI gui, Subtitle subtitle, TimeSpan time, string description): base(gui, description) {
		this.path = GUI.SubtitleView.Widget.Selection.GetSelectedRows()[0];
		this.subtitle = subtitle;
		this.storedTime = time;
	}
	
	protected Subtitle Subtitle {
		get { return subtitle; }
	}
	
	public TreePath Path {
		get { return path; }
	}
	
	public override bool CanGroupWith (Command command) {
		return (path.Compare((command as ChangeTimingCommand).Path) == 0);	
	}

	public override void Execute () {
		TimeSpan previousTime = GetPreviousTime();
		if (storedFrames == -1)
			SetTime(storedTime);
		else {
			SetFrames(storedFrames);
			storedFrames = -1;
		}
			
		storedTime = previousTime;
		SubtitleView subtitleView = GUI.SubtitleView;
		TreeSelection selection = subtitleView.Widget.Selection;
		if (!selection.PathIsSelected(path))
			selection.SelectPath(path);
		else
			subtitleView.Refresh();
	}
	
	public override void UnExecute () {
		Execute();		
	}
	
	protected abstract TimeSpan GetPreviousTime ();
	protected abstract void SetTime (TimeSpan storedTime);
	protected abstract void SetFrames (int storedFrames);
	
}

public class ChangeStartCommand : ChangeTimingCommand {
	private static string description = "Change From";

	public ChangeStartCommand (GUI gui, Subtitle subtitle, int frames): base(gui, subtitle, frames, description) {
	}
	
	public ChangeStartCommand (GUI gui, Subtitle subtitle, TimeSpan time): base(gui, subtitle, time, description) {
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

}

public class ChangeEndCommand : ChangeTimingCommand {
	private static string description = "Change To";

	public ChangeEndCommand (GUI gui, Subtitle subtitle, int frames): base(gui, subtitle, frames, description) {
	}
	
	public ChangeEndCommand (GUI gui, Subtitle subtitle, TimeSpan time): base(gui, subtitle, time, description) {
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

}

public class ChangeDurationCommand : ChangeTimingCommand {
	private static string description = "Change During";

	public ChangeDurationCommand (GUI gui, Subtitle subtitle, int frames): base(gui, subtitle, frames, description) {
	}
	
	public ChangeDurationCommand (GUI gui, Subtitle subtitle, TimeSpan time): base(gui, subtitle, time, description) {
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

}

}