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

namespace GnomeSubtitles {

/// <summary>Represents a <see cref="Command" /> in which the selection never changes during execution/undo/redo.</summary>
public abstract class FixedMultipleSelectionCommand : MultipleSelectionCommand {
	private bool reselect = false;
	
	/// <summary>Creates a new instance of the <see cref="MultipleFixedSelectionCommand" /> class.</summary>
	/// <param name="description">The description of the command.</param>
	/// <param name="canGroup">Whether to group the command with similar commands.</param>
	/// <param name="applyToAll">Whether the command should be executed for all subtitles. See the <see cref="ApplyToAll" /> property.</param>
	/// <param name="reselect">Whether to reselect the command when executing. Note that this doesn't apply to Undo nor to Redo.</param>
	public FixedMultipleSelectionCommand (string description, bool canGroup, SelectionType selectionType, bool reselect) : base(description, canGroup, selectionType) {
		this.reselect = reselect;		
	}
	
	/* Protected properties */
	
	/// <summary>Whether to reselect the subtitles when the command is executed.</summary>
	/// <remarks>Subtitles aren't really reselected, the GUI is called to be updated based on the selection, as if subtitles had been reselected.</remarks>
	protected bool Reselect {
		get { return reselect; }
	}
	
	public override bool Execute () {
		bool completed = ChangeValues();
		if (!completed)
			return false;
		
		switch (SelectionType) {
			case SelectionType.All:
				Global.GUI.View.Selection.SelectAll();
				break;
			case SelectionType.Range:
				Global.GUI.View.Selection.SelectRange(Paths, Focus, true);
				break;
			case SelectionType.Simple:
				Global.GUI.View.Selection.ScrollToFocus(Focus, true);
				break;
		}
		Global.GUI.View.Refresh();
		if (reselect)
			Global.GUI.UpdateFromSelection();
		
		PostProcess();
		return true;
	}
	
	public override void Undo () {
		ChangeValues();

		switch (SelectionType) {
			case SelectionType.All:
				Global.GUI.View.Selection.SelectAll();
				break;
			case SelectionType.Range:
				Global.GUI.View.Selection.SelectRange(Paths, Focus, true);
				break;
			case SelectionType.Simple:
				Global.GUI.View.Selection.Select(Paths, Focus, true);
				break;
		}

		Global.GUI.View.Refresh();
		PostProcess();
	}

	public override void Redo () {
		Undo();
	}
	
	/* Methods to be extended */
	
	protected virtual bool ChangeValues () {
		return true;
	}
	
	protected virtual void PostProcess () {
		return;
	}

}

}