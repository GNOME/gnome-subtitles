/*
 * This file is part of Gnome Subtitles.
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

namespace GnomeSubtitles {

public abstract class Command {
	private string description;
	private bool canGroup;
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
	
	public virtual bool CanGroupWith (Command command) {
		return false;
	}
	
	public virtual void SetCommandTarget (CommandTarget target) {
		this.target = target;
	}
	
	/* Protected members */
	
	protected void SetDescription (string description) {
		this.description = description;
	}
	
	protected void SetCanGroup (bool canGroup) {
		this.canGroup = canGroup;
	}
}

}
