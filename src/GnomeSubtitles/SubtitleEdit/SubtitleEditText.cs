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

using Gtk;
using SubLib;

namespace GnomeSubtitles {

public class SubtitleEditText : SubtitleEditTextView {

	public SubtitleEditText (TextView textView) : base(textView) {
	}
	
	protected override SubtitleTextType GetTextType () {
		return SubtitleTextType.Text;
	}
	
	protected override void ChangeSubtitleTextContent (Subtitle subtitle, string text) {
		subtitle.Text.Set(text);
	}
	
	protected override string GetSubtitleTextContent (Subtitle subtitle) {
		return subtitle.Text.Get();
	}
	
	protected override void ExecuteInsertCommand (int index, string text) {
		Global.CommandManager.Execute(new InsertTextCommand(index, text));
	}
	
	protected override void ExecuteDeleteCommand (int index, string text, int cursor) {
		Global.CommandManager.Execute(new DeleteTextCommand(index, text, cursor));
	}
	
	protected override SpellLanguage GetSpellActiveLanguage () {
		return Global.SpellLanguages.ActiveTextLanguage;
	}
	
	protected override void ConnectLanguageChangedSignal () {
		Global.SpellLanguages.TextLanguageChanged += OnSpellLanguageChanged;
	}
}

}