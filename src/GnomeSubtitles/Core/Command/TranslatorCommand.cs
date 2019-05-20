///*
// * This file is part of Gnome Subtitles.
// * Copyright (C) 2011-2017 Pedro Castro
// *
// * Gnome Subtitles is free software; you can redistribute it and/or modify
// * it under the terms of the GNU General Public License as published by
// * the Free Software Foundation; either version 2 of the License, or
// * (at your option) any later version.
// *
// * Gnome Subtitles is distributed in the hope that it will be useful,
// * but WITHOUT ANY WARRANTY; without even the implied warranty of
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// * GNU General Public License for more details.
// *
// * You should have received a copy of the GNU General Public License
// * along with this program; if not, write to the Free Software
// * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
// */

//using GnomeSubtitles.Dialog;
//using GnomeSubtitles.Dialog.Message;
//using GnomeSubtitles.Ui.Edit;
//using Gtk;
//using Mono.Unix;
//using SubLib;
//using SubLib.Core;
//using SubLib.Core.Domain;
//using SubLib.Exceptions;
//using SubLib.Util;
//using System;

//namespace GnomeSubtitles.Core.Command {

//public class TranslatorCommand : FixedSingleSelectionCommand {
//	private string previousValue = null; // initially, it's the former text replaced by translation
//	private bool leftToRight = true; // direction of translation
//	private static string description = Catalog.GetString("Translating");

//	public TranslatorCommand (bool leftToRight) : base(description, false, true) {
//		this.leftToRight = leftToRight;
//	}

//	/* Public members */

//	protected override bool ChangeValues () {
//		/* If previousText is null, it's the first time the command is executed, so do translation.
//		   Otherwise, it's undo or redo, so we only need to swap the values */
//		if (previousValue == null)
//			return DoTranslation();
//		else {
//			Subtitle subtitle = Base.Document.Subtitles[Path];
//			if (leftToRight) {
//				string currentValue = subtitle.Translation.Get();
//				subtitle.Translation.Set(this.previousValue);
//				this.previousValue = currentValue;
//			}
//			else {
//				string currentValue = subtitle.Text.Get();
//				subtitle.Text.Set(this.previousValue);
//				this.previousValue = currentValue;
//			}
//			return true;
//		}
//	}

//	private bool DoTranslation () {
//		if (Base.Ui.View.Selection.Count != 1) { //TODO: for now, only works if 1 subtitle is selected
//			return false;
//		}

//		/* Show language selection dialog if one of the languages isn't selected */
//		if (!Base.SpellLanguages.HasActiveTextLanguage || !Base.SpellLanguages.HasActiveTranslationLanguage) {
//			SetLanguagesDialog dialog = Base.Dialogs.Get(typeof(SetLanguagesDialog)) as SetLanguagesDialog;
//			dialog.Show();
//			dialog.WaitForResponse();
//			if (!Base.SpellLanguages.HasActiveTextLanguage || !Base.SpellLanguages.HasActiveTranslationLanguage) {
//				return false; //Both must be selected, if they aren't we just don't do anything here
//			}
//		}

//		try {
//			Subtitle subtitle = Base.Document.Subtitles[Path];
//			if (leftToRight) {
//				this.previousValue = subtitle.Translation.Get();
//				string translatedText = Translator.TranslateText(subtitle.Text.Get(), Base.SpellLanguages.ActiveTextLanguage.ID, Base.SpellLanguages.ActiveTranslationLanguage.ID, Translator.TIMEOUT);
//				subtitle.Translation.Set(translatedText);
//			}
//			else {
//				this.previousValue = subtitle.Text.Get();
//				string translatedText = Translator.TranslateText(subtitle.Translation.Get(), Base.SpellLanguages.ActiveTranslationLanguage.ID, Base.SpellLanguages.ActiveTextLanguage.ID, Translator.TIMEOUT);
//				subtitle.Text.Set(translatedText);
//			}

//			//TODO: if only one subtitle is selected, set the cursor on the translated text box and select its text. If multiple subtitles translated, select those subtitles.
//			return true;
//		}
//		catch (TranslatorException e) { //TODO know which exceptions are originally thrown. Check if it's possible to have the second error message in the application language.
//			Logger.Error(e);
//			BasicErrorDialog errorDialog = new BasicErrorDialog(Catalog.GetString("Could not translate the chosen subtitle."), e.Message);
//			errorDialog.Show();
//			return false;
//		}
//	}

//}

//}
