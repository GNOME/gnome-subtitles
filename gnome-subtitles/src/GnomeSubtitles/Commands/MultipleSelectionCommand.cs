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

public abstract class MultipleSelectionCommand : Command {
	private TreePath[] paths = null;
	private TreePath focus = null;
	private SelectionType selectionType;
	
	/// <summary>Base constructor for classes that inherit <see cref="MultipleSelectionCommand" />.</summary>
	/// <param name="description">The description of the command.</param>
	/// <param name="canGroup">Whether to group the command with similar commands.</param>
	/// <param name="type">The type of selection.</param>
	public MultipleSelectionCommand (string description, bool canGroup, SelectionType selectionType) : base(description, canGroup) {
		this.selectionType = selectionType;
		switch (selectionType) {
			case SelectionType.Simple:
				this.paths = Global.GUI.View.Selection.Paths;
				this.focus = Global.GUI.View.Selection.Focus;
				break;
			case SelectionType.Range:
				this.paths = Global.GUI.View.Selection.Range;
				this.focus = Global.GUI.View.Selection.Focus;
				break;
		}
	}
	
	/* Protected properties */
	
	protected TreePath[] Paths {
		get { return paths; }
	}
	
	protected TreePath Focus {
		get { return focus; }
	}

	protected SelectionType SelectionType {
		get { return selectionType; }
	}

	/// <summary>Whether to apply the command to all subtitles.</summary>
	protected bool ApplyToAll {
		get { return selectionType == SelectionType.All; }
	}

	/// <summary>Whether to apply the command to a range of subtitles.</summary>
	protected bool ApplyToRange {
		get { return selectionType == SelectionType.Range; }
	}

	/// <summary>Whether to apply the command to a simple selection of subtitles.</summary>
	protected bool ApplyToSimple {
		get { return selectionType == SelectionType.Simple; }
	}
	

}

}