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

using SubLib;

namespace GnomeSubtitles {

public class ChangeTextCommand : FixedSingleSelectionCommand {
	private static string description = "Editing Text";
	private Subtitle subtitle;
	string storedText;

	public ChangeTextCommand (string text) : base(description, true, false) {
		this.subtitle = Global.Subtitles[Path];
		this.storedText = text;
	}
	
	//TODO: only group when it's the text of the same word
	public override bool CanGroupWith (Command command) {
		return (Path.Compare((command as ChangeTextCommand).Path) == 0);	
	}
	
	protected override bool ChangeValues () {
		string previousText = subtitle.Text.Get();
		subtitle.Text.Set(storedText);
		storedText = previousText;
		return true;
	}
	
	protected override void PostProcess () {
		Global.GUI.View.RedrawPath(Path);
	}

}

}