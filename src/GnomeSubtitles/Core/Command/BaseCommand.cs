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

using System;

namespace GnomeSubtitles.Core.Command {

/*
 * Note: commands affecting text *and* translation need to take into account that:
 * - A translation may not be present when the command is executed, but may have been loaded (Translation > Open) afterwards.
 *   When undoing, that translation needs to be taken into account. Example: Subtitles (with text only) were split. Then translation
 *   was opened (which does not generate undo/redo because it's a file open). Undoing the split command will revert the original
 *   text (which means the subtitles that were split will now be merged). In this case, the translation needs to be merged for those
 *   subtitles, and not just set to their previous values (because there were none). Additionally, Redo should always apply the Split
 *   command again in order to do a new split that takes into account the existing translation text.
 * - A translation may be present when the command is executed, but may have been changed / loaded with a new file afterwards. This
 *   means a ClearTarget event will be triggered in order to clear the translation data inside this command. However, this needs to be
 *   taken into account later when Undoing / Redoing.
 */
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
	
	/// <summary>Clears a target within an existing command. When a command only has one target (normal or translation), this is
	/// not necessary as the whole command will be deleted if that target (whole document or translation, respectively) becomes
	/// unavailable. This is used when clearing a translation target in a command that includes NormalAndTranslation, which
	/// means that only its translation part needs to be cleared (and that needs to be done by the command itself).</summary>
	public virtual void ClearTarget (CommandTarget target) {
		//If this command has NormalAndTranslation target, make sure it overrides this method
		if (this.target == CommandTarget.NormalAndTranslation) {
			throw new NotImplementedException("ClearTarget is required for commands with NormalAndTranslation target");
		}
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
