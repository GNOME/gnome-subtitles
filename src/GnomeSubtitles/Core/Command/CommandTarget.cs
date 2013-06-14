/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2009 Pedro Castro
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

namespace GnomeSubtitles.Core.Command {

/// <summary>
/// Represents the target of a command.
/// </summary>
public enum CommandTarget {

	/// <summary>The command target is the normal document.</summary>
	Normal,

	/// <summary>The command target is the translation.</summary>
	Translation,

	/// <summary>The command target is both the normal document and the translation.</summary>
	NormalAndTranslation
}

}
