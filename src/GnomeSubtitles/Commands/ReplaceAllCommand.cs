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

using Gtk;
using SubLib;
using System;
using System.Text.RegularExpressions;

namespace GnomeSubtitles {

public class ReplaceAllCommand : MultipleSelectionCommand {
	private static string description = "Replacing All";
	
	private int[] subtitles = null;
	private string[] texts  = null;
	
	private Regex regex = null;
	private string replacement = String.Empty;

	public ReplaceAllCommand (Regex regex, string replacement) : base(description, false, SelectionIntended.Simple, false) {
		this.regex = regex;
		this.replacement = replacement;
	}


	public override bool Execute () {
		int count = Global.Document.Subtitles.ReplaceAll(regex, replacement, out subtitles, out texts);
		if (count == 0)
			return false;
		
		Paths = Util.IntsToPaths(subtitles);
		if (Paths.Length > 0)
			Focus = Paths[0];
		
		Global.GUI.View.Selection.Select(Paths, Focus, true);
		return true;
	}
	
	public override void Undo () {
		for (int position = 0 ; position < subtitles.Length ; position++) {
			int index = subtitles[position];
			Subtitle subtitle = Global.Document.Subtitles[index];
			string oldText = subtitle.Text.Get();
			string newText = texts[position];
			subtitle.Text.Set(newText);
			texts[position] = oldText;
		}
	
		Global.GUI.View.Selection.Select(Paths, Focus, true);
	}
	
	public override void Redo () {
		Undo();
	}
	




}

}
