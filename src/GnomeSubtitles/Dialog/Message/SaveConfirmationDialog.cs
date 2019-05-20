/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2019 Pedro Castro
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
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Dialog.Message {

public abstract class SaveConfirmationDialog : WarningDialog {
	private SubtitleTextType textType;

	/* Strings */
	private string secondaryText = Catalog.GetString("If you don't save, all your changes will be permanently lost.");

	public SaveConfirmationDialog (string primaryText, SubtitleTextType textType) : base() {
		this.textType = textType;

		string filename = String.Empty;
		if (textType == SubtitleTextType.Text)
			filename = Base.Document.TextFile.Filename;
		else if (Base.Document.HasTranslationFileProperties)
			filename = Base.Document.TranslationFile.Filename;
		else
			filename = Base.Document.UnsavedTranslationFilename;

		SetText(primaryText, secondaryText, filename);
	}


	/* Abstract methods */

	protected abstract string GetRejectLabel ();


	/* Protected methods */

	protected override void AddButtons () {
		string rejectLabel = GetRejectLabel();
		dialog.AddButton(rejectLabel, ResponseType.Reject);
		dialog.AddButton(Stock.Cancel, ResponseType.Cancel);
		dialog.AddButton(Stock.Save, ResponseType.Accept);
		
		dialog.DefaultResponse = ResponseType.Accept;
	}


	/* Event members */

	protected override bool ProcessResponse (ResponseType response) {
		Hide();

		if (response == ResponseType.Reject)
			SetReturnValue(true);
		else if (response == ResponseType.Accept) {
			if (textType == SubtitleTextType.Text)
				SetReturnValue(Core.Base.Ui.Save());
			else
				SetReturnValue(Core.Base.Ui.TranslationSave());
		}

		return false;
	}

}

/* Confirmation dialogs for New operations */

public class SaveSubtitlesOnNewFileConfirmationDialog : SaveConfirmationDialog {

	/* Strings */
	private static string primaryText = Catalog.GetString("Save the changes to subtitles \"{0}\" before creating new subtitles?");
	private static string rejectLabel = Catalog.GetString("Create without Saving");

	public SaveSubtitlesOnNewFileConfirmationDialog () : base(primaryText, SubtitleTextType.Text) {
	}

	public SaveSubtitlesOnNewFileConfirmationDialog (string primaryText, SubtitleTextType textType) : base(primaryText, textType) {
	}


	/* Protected methods */

	protected override string GetRejectLabel ()
	{
		return rejectLabel;
	}

}

public class SaveTranslationOnNewFileConfirmationDialog : SaveSubtitlesOnNewFileConfirmationDialog {
	private static string primaryText = Catalog.GetString("Save the changes to translation \"{0}\" before creating new subtitles?");

	public SaveTranslationOnNewFileConfirmationDialog () : base(primaryText, SubtitleTextType.Translation) {
	}
}

public class SaveTranslationOnNewTranslationConfirmationDialog : SaveSubtitlesOnNewFileConfirmationDialog {
	private static string primaryText = Catalog.GetString("Save the changes to translation \"{0}\" before creating a new translation?");

	public SaveTranslationOnNewTranslationConfirmationDialog () : base(primaryText, SubtitleTextType.Translation) {
	}
}


/* Confirmation dialogs for Open operations */

public class SaveSubtitlesOnOpenFileConfirmationDialog : SaveConfirmationDialog {
	private static string primaryText = Catalog.GetString("Save the changes to subtitles \"{0}\" before opening?");
	private static string rejectLabel = Catalog.GetString("Open without Saving");

	public SaveSubtitlesOnOpenFileConfirmationDialog () : base(primaryText, SubtitleTextType.Text) {
	}

	public SaveSubtitlesOnOpenFileConfirmationDialog (string primaryText, SubtitleTextType textType) : base (primaryText, textType) {
	}

	/* Protected methods */

	protected override string GetRejectLabel ()
	{
		return rejectLabel;
	}

}

//This works both for file open and translation open
public class SaveTranslationOnOpenConfirmationDialog : SaveSubtitlesOnOpenFileConfirmationDialog {
	private static string primaryText = Catalog.GetString("Save the changes to translation \"{0}\" before opening?");

	public SaveTranslationOnOpenConfirmationDialog () : base(primaryText, SubtitleTextType.Translation) {
	}
}

/* Confirmation dialogs for Close operations */

public class SaveSubtitlesOnCloseFileConfirmationDialog : SaveConfirmationDialog {
	private static string primaryText = Catalog.GetString("Save the changes to subtitles \"{0}\" before closing?");
	private static string rejectLabel = Catalog.GetString("Close without Saving");

	public SaveSubtitlesOnCloseFileConfirmationDialog () : base(primaryText, SubtitleTextType.Text) {
	}

	public SaveSubtitlesOnCloseFileConfirmationDialog (string primaryText, SubtitleTextType textType) : base(primaryText, textType) {
	}

	/* Protected methods */

	protected override string GetRejectLabel ()
	{
		return rejectLabel;
	}

}

public class SaveTranslationOnCloseConfirmationDialog : SaveSubtitlesOnCloseFileConfirmationDialog {
	private static string primaryText = Catalog.GetString("Save the changes to translation \"{0}\" before closing?");

	public SaveTranslationOnCloseConfirmationDialog () : base(primaryText, SubtitleTextType.Translation) {
	}
}

}
