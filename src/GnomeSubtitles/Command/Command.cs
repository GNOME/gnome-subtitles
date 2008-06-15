/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2008 Pedro Castro
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

namespace GnomeSubtitles.Command {

public abstract class Command {
	private string description;
	private bool canGroup = false; //Whether this command can possibly be grouped with the previous command, if similar
	private bool stopsGrouping = false; //Whether this command stops further grouping
	private CommandTarget target = CommandTarget.Normal; //Normal is the typical type of command

	public Command (string description, bool canGroup) {
		this.description = description;
		this.canGroup = canGroup;
	}
	
	public string Description {
		get { return description; }
	}
	
	public bool CanGroup {
		get { return canGroup; }
	}
	
	public bool StopsGrouping {
		get { return stopsGrouping; }
	}
	
	public CommandTarget Target {
		get { return target; }
	}

	public abstract bool Execute ();

	public virtual void Undo () {
		Execute();
	}
	
	public virtual void Redo () {
		Undo();
	}
	
	/// <summary>Whether this command can be grouped with the last command.</summary>
	public virtual bool CanGroupWith (Command command) {
		return false;
	}

	/// <summary>Merges a command with an existing command.</summary>
	public virtual Command MergeWith (Command command) {
		return command;
	}
	
	/* Protected members */
	
	protected void SetDescription (string description) {
		this.description = description;
	}
	
	protected void SetStopsGrouping (bool stopsGrouping) {
		this.stopsGrouping = stopsGrouping;
	}
	
	protected void SetCanGroup (bool canGroup) {
		this.canGroup = canGroup;
	}
	
	protected virtual void SetCommandTarget (CommandTarget target) {
		this.target = target;
	}

}

}
