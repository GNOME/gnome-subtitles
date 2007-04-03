/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007 Pedro Castro
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

namespace GnomeSubtitles {

public class BugReporter {

	public static void Report (Exception exception) {
	
	}
	
	/* Private members */
	
	private static string GetBugInfo (Exception exception) {
		return "Gnome Subtitles version: " + ExecutionInfo.Version + "\n"
			+ "SubLib version: " + ExecutionInfo.SubLibVersion + "\n"
			+ "GtkSharp version: " + ExecutionInfo.GtkSharpVersion + "\n"
			+ "GnomeSharp version: " + ExecutionInfo.GnomeSharpVersion + "\n"
			+ "GladeSharp version: " + ExecutionInfo.GladeSharpVersion + "\n"
			+ "GConfSharp version: " + ExecutionInfo.GConfSharpVersion + "\n\n"
			+ "Stack trace:" + "\n"
			+ exception.ToString();
	}

}

}