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

using System;

namespace GnomeSubtitles {

public class CommandManager {
	private int limit = 25;
	private Command[] commands = null;
	private bool wasModified = false;

	
	private int undoCount = 0;
	private int redoCount = 0;
	private int iterator = 0;
	
	public event EventHandler UndoToggled;
	public event EventHandler RedoToggled;
	public event EventHandler CommandActivated;
	public event EventHandler Modified;

	public CommandManager (int undoLimit, EventHandler onUndoToggled, EventHandler onRedoToggled,
			EventHandler onCommandActivated, EventHandler onModified) {

		limit = undoLimit;
		commands = new Command[undoLimit];
		UndoToggled += onUndoToggled;
		RedoToggled += onRedoToggled;
		CommandActivated += onCommandActivated;
		Modified += onModified;
	}
	
	public void Clear () {
		commands = new Command[limit];
		undoCount = 0;
		redoCount = 0;
		iterator = 0;
		wasModified = false;
	}
	
	public bool CanUndo {
		get { return undoCount > 0; }
	}
	
	public bool CanRedo {
		get { return redoCount > 0; }
	}
	
	public string UndoDescription {
		get {
			if (CanUndo)
				return "Undo " + PreviousCommand().Description;
			else
				return String.Empty;
		}
	}
	
	public string RedoDescription {
		get {
			if (CanRedo)
				return "Redo " + NextCommand().Description;
			else
				return String.Empty;
		}
	}
	
	public bool WasModified {
		get { return wasModified; }
		set { wasModified = value; }	
	}
	
	public bool Execute (Command command) {
		bool completed = command.Execute();
		if (completed) {
			ProcessExecute(command);
			SetModified();
		}
		return completed;
	}
	
	public void Undo () {
		if (CanUndo) {
			PreviousCommand().Undo();
			ProcessUndo();	
			SetModified();
		}
	}
	
	public void Redo () {
		if (CanRedo) {
			NextCommand().Redo();
			ProcessRedo();
			SetModified();
		}
	}


	private void ProcessExecute (Command command) {
		bool couldUndoBefore = CanUndo;
		bool couldRedoBefore = CanRedo;
		
		ClearRedo();
		
		bool canGroup = false;
		if (CanUndo && command.CanGroup) {
			Command lastCommand = PreviousCommand();
			if ((lastCommand.GetType() == command.GetType()) && (lastCommand.CanGroupWith(command)))
				canGroup = true;
		}
		
		if (!canGroup) {
			commands[iterator] = command;
			Next();
			undoCount = IncrementCount(undoCount);
			EmitCommandActivated();
		}

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
			
		EmitCommandActivated();
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
		
		EmitCommandActivated();
	}
	
	private void EmitUndoToggled () {
		if (UndoToggled != null)
			UndoToggled(this, EventArgs.Empty);
	}
	
	private void EmitRedoToggled () {
		if (RedoToggled != null)
			RedoToggled(this, EventArgs.Empty);
	}
	
	private void EmitCommandActivated () {
		CommandActivated(this, EventArgs.Empty);
	}
	
	private void SetModified () {
		if (!wasModified) {
			wasModified = true;
			Modified(this, EventArgs.Empty);		
		}
	}
	
	private Command NextCommand () {
		return commands[iterator];
	}
	
	private Command PreviousCommand () {
		if (iterator == 0)
			return commands[limit - 1];
		else
			return commands[iterator - 1];
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

}