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

using Gtk;

namespace GnomeSubtitles.Core.Command {

public abstract class SingleSelectionCommand : Command {
	private TreePath path = null;

	public SingleSelectionCommand (string description, bool canGroup) : this(description, canGroup, Base.Ui.View.Selection.Path) {
	}

	public SingleSelectionCommand (string description, bool canGroup, TreePath path) : base(description, canGroup) {
		this.path = path;
	}

	protected TreePath Path {
		get { return path; }
	}

}

}
