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

using System.Collections;

namespace GnomeSubtitles {

public class CommandManager {
	private int limit = 25;
	private Command[] commands = null;
	
	private int undoCount = 0;
	private int redoCount = 0;
	private int iterator = 0;
	
	public CommandManager (int undoLimit) {
		limit = undoLimit;
		commands = new Command[undoLimit];	
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
		commands[iterator] = command;
		Next();
		undoCount = IncrementCount(undoCount);
		ClearRedo();
	}
	
	private void ProcessUndo () {
		Previous();
		undoCount = DecrementCount(undoCount);
		redoCount = IncrementCount(redoCount);
	}
	
	private void ProcessRedo () {
		Next();
		undoCount = IncrementCount(undoCount);
		redoCount = DecrementCount(redoCount);	
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

	
	public abstract void Execute ();
	public abstract void UnExecute ();
	
	
	
	//SubCommands inside Command

}

public class DisplayCommand : Command {
	string todo = null;
	string undo = null;

	public DisplayCommand (int num) {
		todo = "I have executed " + num;	
		undo = "I have Unexecuted " + num;	
	}

	public override void Execute () {
		System.Console.WriteLine(todo);	
	}
	
	public override void UnExecute () {
		System.Console.WriteLine(undo);
	}

}


}