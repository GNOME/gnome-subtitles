/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2021 Pedro Castro
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

using GnomeSubtitles.Core;
using GnomeSubtitles.Core.Command;
using Gtk;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Ui.Edit {

public class SubtitleEditTranslation : SubtitleEditTextView {

	public SubtitleEditTranslation (TextView textView) : base(textView) {
		Base.InitFinished += OnBaseInitFinished;
	}

	protected override SubtitleTextType GetTextType () {
		return SubtitleTextType.Translation;
	}

	protected override void ChangeSubtitleTextContent (Subtitle subtitle, string text) {
		subtitle.Translation.Set(text);
	}

	protected override string GetSubtitleTextContent (Subtitle subtitle) {
		return subtitle.Translation.Get();
	}

	protected override void ExecuteInsertCommand (int index, string text) {
		Base.CommandManager.Execute(new InsertTranslationCommand(index, text));
	}

	protected override void ExecuteDeleteCommand (int index, string text, int cursor) {
		Base.CommandManager.Execute(new DeleteTranslationCommand(index, text, cursor));
	}

	protected override SpellLanguage GetSpellActiveLanguage () {
		return Base.SpellLanguages.ActiveTranslationLanguage;
	}

	/* Event members */

	private void OnBaseInitFinished () {
		Base.Ui.Edit.TextEdit.ToggleOverwrite += OnTextEditToggleOverwrite;

		Base.TranslationLoaded += OnBaseTranslationLoaded;
		Base.TranslationUnloaded += OnBaseTranslationUnloaded;
	}

	private void OnBaseTranslationLoaded () {
		Base.Ui.View.Selection.Changed += OnSubtitleSelectionChanged;

    	Base.SpellLanguages.EnabledToggled += OnSpellEnabledToggled;
		Base.SpellLanguages.TranslationLanguageChanged += OnSpellLanguageChanged;
		SetSpellLanguage();

		SetVisibility(true);
	}

	private void OnBaseTranslationUnloaded () {
		Base.Ui.View.Selection.Changed -= OnSubtitleSelectionChanged;

		Base.SpellLanguages.EnabledToggled -= OnSpellEnabledToggled;
		Base.SpellLanguages.TranslationLanguageChanged -= OnSpellLanguageChanged;
		SetSpellLanguage();

    	SetVisibility(false);
	}

	private void OnTextEditToggleOverwrite (object o, EventArgs args) {
		ToggleOverwriteSilent();
	}


}

}