/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2009 Pedro Castro
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

using GnomeSubtitles.Ui.View;
using Gtk;

namespace GnomeSubtitles.Core.Command {

public abstract class MultipleSelectionCommand : Command {
	private TreePath[] paths = null;
	private TreePath focus = null;
	private SelectionType selectionType;


	public MultipleSelectionCommand (string description, bool canGroup, SelectionIntended selectionIntended, TreePath[] paths) : this(description, canGroup, selectionIntended, paths, true) {
	}

	/// <summary>Base constructor for classes that inherit <see cref="MultipleSelectionCommand" />.</summary>
	/// <param name="description">The description of the command.</param>
	/// <param name="canGroup">Whether to possibly group the command with the previous command.</param>
	/// <param name="selectionIntended">The intended selection.</param>
	/// <param name="paths">The paths to select, or null to use auto selection if setPaths is enabled.</param>
	/// <param name="setPaths">Whether to set the paths based on the current selection and the selectionType</param>
	public MultipleSelectionCommand (string description, bool canGroup, SelectionIntended selectionIntended, TreePath[] paths, bool setPaths) : base(description, canGroup) {
		if (setPaths) {
			switch (selectionIntended) {
				case SelectionIntended.Simple:
					this.paths = (paths != null ? paths : Base.Ui.View.Selection.Paths);
					this.focus = Base.Ui.View.Selection.Focus;
					break;
				case SelectionIntended.Range:
					this.paths = (paths != null ? paths : Base.Ui.View.Selection.Range);
					this.focus = Base.Ui.View.Selection.Focus;
					break;
				case SelectionIntended.SimpleToFirst:
					this.paths = (paths != null ? paths : Base.Ui.View.Selection.PathsToFirst);
					this.focus = Base.Ui.View.Selection.Focus;
					break;
				case SelectionIntended.SimpleToLast:
					this.paths = (paths != null ? paths : Base.Ui.View.Selection.PathsToLast);
					this.focus = Base.Ui.View.Selection.Focus;
					break;
				default:
					if (paths != null)
						this.paths = paths;

					break;
			}
		}

		this.selectionType = GetSelectionType(selectionIntended);
	}

	/* Protected properties */

	protected TreePath[] Paths {
		get { return paths; }
		set { paths = value; }
	}

	protected TreePath FirstPath {
		get { return paths[0]; }
	}

	protected TreePath LastPath {
		get { return paths[paths.Length - 1]; }
	}

	protected TreePath Focus {
		get { return focus; }
		set { focus = value; }
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

	/* Private methods */

	private SelectionType GetSelectionType (SelectionIntended selectionIntended) {
		switch (selectionIntended) {
			case SelectionIntended.All:
				return SelectionType.All;
			case SelectionIntended.Simple:
				return SelectionType.Simple;
			case SelectionIntended.SimpleToFirst:
				return SelectionType.Range;
			case SelectionIntended.SimpleToLast:
				return SelectionType.Range;
			case SelectionIntended.Range:
				return SelectionType.Range;
			default:
				return SelectionType.Simple;
		}
	}

}

}
