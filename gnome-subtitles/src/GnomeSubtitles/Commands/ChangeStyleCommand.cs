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
using SubLib;

namespace GnomeSubtitles {

public abstract class ChangeStyleCommand : FixedMultipleSelectionCommand {
	private bool styleValue;

	public ChangeStyleCommand (string description, bool newStyleValue) : base(description, false, SelectionType.Simple, true) {
		this.styleValue = newStyleValue;
	}
	
	protected override void ChangeValues () {
		foreach (TreePath path in Paths) {
			Subtitle subtitle = Global.Subtitles[path];
			SetStyle(subtitle, styleValue);
		}
		ToggleStyleValue();
	}

	/* Methods to be extended */
	
	protected abstract void SetStyle (Subtitle subtitle, bool style);

	/* Private members */

	private void ToggleStyleValue () {
		styleValue = !styleValue;
	}

}

public class ChangeBoldStyleCommand : ChangeStyleCommand {
	private static string description = "Toggling Bold";

	public ChangeBoldStyleCommand (bool newStyleValue) : base(description, newStyleValue) {
	}

	protected override void SetStyle (Subtitle subtitle, bool styleValue) {
		subtitle.Style.Bold = styleValue;
	}
}

public class ChangeItalicStyleCommand : ChangeStyleCommand {
	private static string description = "Toggling Italic";

	public ChangeItalicStyleCommand (bool newStyleValue) : base(description, newStyleValue) {
	}

	protected override void SetStyle (Subtitle subtitle, bool styleValue) {
		subtitle.Style.Italic = styleValue;
	}
}

public class ChangeUnderlineStyleCommand : ChangeStyleCommand {
	private static string description = "Toggling Underline";

	public ChangeUnderlineStyleCommand (bool newStyleValue) : base(description, newStyleValue) {
	}

	protected override void SetStyle (Subtitle subtitle, bool styleValue) {
		subtitle.Style.Underline = styleValue;
	}
}

}