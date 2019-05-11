/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2019 Pedro Castro
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
using System;

namespace GnomeSubtitles.Core.Command {

public class CommandManager {
	private int limit = -1;
	private Command[] commands = null;

	private int undoCount = 0;
	private int redoCount = 0;
	private int iterator = 0;

	public event EventHandler UndoToggled;
	public event EventHandler RedoToggled;
	public event CommandActivatedHandler CommandActivated;

	#region Constants
	private const int defaultLimit = 50;
	#endregion

	public CommandManager () : this(defaultLimit) {
	}

	public CommandManager (int undoLimit) {
		limit = undoLimit;
		commands = new Command[undoLimit];

		Base.InitFinished += OnBaseInitFinished;
	}

	public void Clear () {
		commands = new Command[limit];
		undoCount = 0;
		redoCount = 0;
		iterator = 0;
	}

	/* Public properties */

	public bool CanUndo {
		get { return undoCount > 0; }
	}

	public bool CanRedo {
		get { return redoCount > 0; }
	}

	public string UndoDescription {
		get {
			if (CanUndo)
				return Catalog.GetString("Undo") + " " + GetPreviousCommand().Description;
			else
				return String.Empty;
		}
	}

	public string RedoDescription {
		get {
			if (CanRedo)
				return Catalog.GetString("Redo") + " " + GetNextCommand().Description;
			else
				return String.Empty;
		}
	}

	public bool Execute (Command command) {
		bool completed = command.Execute();
		if (completed) {
			ProcessExecute(command);
			EmitCommandActivated(command);
		}
		return completed;
	}

	public void Undo () {
		if (CanUndo) {
			Command command = GetPreviousCommand();
			command.Undo();
			ProcessUndo();
			EmitCommandActivated(command);
		}
	}

	public void Redo () {
		if (CanRedo) {
			Command command = GetNextCommand();
			command.Redo();
			ProcessRedo();
			EmitCommandActivated(command);
		}
	}

	/* Private methods */

	private void ProcessExecute (Command command) {
		bool couldUndoBefore = CanUndo;
		bool couldRedoBefore = CanRedo;

		ClearRedo();

		bool canGroup = false;
		if (CanUndo && command.CanGroup) {
			Command lastCommand = GetPreviousCommand();
			if ((!lastCommand.StopsGrouping) && (lastCommand.GetType() == command.GetType()) && (command.CanGroupWith(lastCommand))) {
				canGroup = true;
				Command merged = command.MergeWith(lastCommand);
				SetPreviousCommand(merged);
			}
		}

		if (!canGroup) {
			SetNextCommand(command);
			Next();
			undoCount = IncrementCount(undoCount);
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

	private void EmitUndoToggled () {
		if (UndoToggled != null)
			UndoToggled(this, EventArgs.Empty);
	}

	private void EmitRedoToggled () {
		if (RedoToggled != null)
			RedoToggled(this, EventArgs.Empty);
	}

	private void EmitCommandActivated (Command command) {
		if (CommandActivated != null) {
			CommandActivated(this, new CommandActivatedArgs(command.Target));
		}
	}

	private Command GetNextCommand () {
		return commands[iterator];
	}

	private Command GetPreviousCommand () {
		if (iterator == 0)
			return commands[limit - 1];
		else
			return commands[iterator - 1];
	}

	private void SetNextCommand (Command command) {
		commands[iterator] = command;
	}

	private void SetPreviousCommand (Command command) {
		if (iterator == 0)
			commands[limit - 1] = command;
		else
			commands[iterator - 1] = command;
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

	private void ClearTarget (CommandTarget target) {

		/* Create new collection of commands */
		Command[] newCommands = new Command[limit];
		int newIterator = 0;
		int newUndoCount = 0;
		int newRedoCount = 0;

		/* Go through the undo commands */
		if (undoCount > 0) {
			int lastUndoIter = iterator - undoCount;
			if (lastUndoIter < 0) {
				lastUndoIter = limit + lastUndoIter;
			}

			int undoIter = lastUndoIter;
			while (undoIter != iterator) {
				Command undoCommand = commands[undoIter];
				
				//We only keep the command if its target is not the one we're clearing
				if (undoCommand.Target != target) {
					//If the command target is NormalAndTranslation, it means at least part of it may need to be cleared
					if (undoCommand.Target == CommandTarget.NormalAndTranslation) {
						undoCommand.ClearTarget(target);
					}
				
					newCommands[newIterator] = undoCommand;
					newIterator++;
					newUndoCount++;
				}
				undoIter = (undoIter == limit - 1 ? 0 : undoIter + 1);
			}
		}

		/* Go through the redo commands */
		if (redoCount > 0) {
			int redoIter = iterator;
			int newRedoIterator = newIterator; //Because newIterator cannot be changed now
			for (int redoNum = 0 ; redoNum < redoCount ; redoNum++) {
				Command redoCommand = commands[redoIter];
				
				//We only keep the command if its target is not the one we're clearing
				if (redoCommand.Target != target) {
					//If the command target is NormalAndTranslation, it means at least part of it may need to be cleared
					if (redoCommand.Target == CommandTarget.NormalAndTranslation) {
						redoCommand.ClearTarget(target);
					}
				
					newCommands[newRedoIterator] = redoCommand;
					newRedoIterator++;
					newRedoCount++;
				}
				redoIter = (redoIter == limit - 1 ? 0 : redoIter + 1);
			}
		}

		/* Check whether to toggle undo and redo */
		bool toToggleUndo = ((undoCount > 0) && (newUndoCount == 0));
		bool toToggleRedo = ((redoCount > 0) && (newRedoCount == 0));

		/* Update state */
		commands = newCommands;
		undoCount = newUndoCount;
		redoCount = newRedoCount;
		iterator = newIterator;

		/* Issue possible events */
		if (toToggleUndo) {
			EmitUndoToggled();
		}
		
		if (toToggleRedo) {
			EmitRedoToggled();
		}
	}


	/* Event members */

	private void OnBaseInitFinished () {
		Base.TranslationUnloaded += OnBaseTranslationUnloaded;
	}

	private void OnBaseTranslationUnloaded () {
		ClearTarget(CommandTarget.Translation);
	}

}

}
