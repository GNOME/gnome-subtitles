/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2008 Pedro Castro
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

namespace GnomeSubtitles.Ui.View {

/// <summary>
/// Represents an intended selection.
/// </summary>
public enum SelectionIntended {

	/// <summary>Use the current selection.</summary>
	Simple,

	/// <summary>Use the current selection to the first subtitle.</summary>
	SimpleToFirst,

	/// <summary>Use the current selection to the last subtitle.</summary>
	SimpleToLast,

	/// <summary>A range of subtitles is selected.</summary>
	Range,

	/// <summary>All subtitles are selected.</summary>
	All
}

}
