/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2007 Pedro Castro
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
using Mono.Unix;
using SubLib;
using System;

namespace GnomeSubtitles {

/* TODO: Use MessageDialog from Glade? */
public class SaveConfirmationDialog {
	private MessageDialog dialog = null;
	private bool toClose = false;
	private SubtitleTextType textType;
	
	/* Strings */
	private string secondaryText = Catalog.GetString("If you don't save, all your changes will be permanently lost.");


	public SaveConfirmationDialog (string primaryText, string rejectLabel, SubtitleTextType textType) {
		this.textType = textType;
	
		string message = "<span weight=\"bold\" size=\"larger\">" + primaryText + "</span>\n\n" + secondaryText;
		string fileName = (textType == SubtitleTextType.Text ? Global.Document.TextFile.Filename : Global.Document.TranslationFile.Filename);
		dialog = new MessageDialog(Global.GUI.Window, DialogFlags.Modal, MessageType.Warning,
			ButtonsType.None, message, fileName);
	
		dialog.AddButton(rejectLabel, ResponseType.Reject);
		dialog.AddButton(Stock.Cancel, ResponseType.Cancel);
		dialog.AddButton(Stock.Save, ResponseType.Accept);
		dialog.Title = String.Empty;
		
		dialog.Response += OnResponse;
	}
	
	public bool WaitForResponse () {
		dialog.Run();
		return toClose;
	}
	
	/* Event handlers */
	
	private void OnResponse (object o, ResponseArgs args) {
		CloseDialog();

		ResponseType response = args.ResponseId;
		if (response == ResponseType.Reject)
			toClose = true;
		else if (response == ResponseType.Accept) {
			if (textType == SubtitleTextType.Text)
				toClose = Global.GUI.Save();
			else
				toClose = Global.GUI.TranslationSave();
		}
	}
	
	/* Private members */
	
	private void CloseDialog() {
		dialog.Destroy();
	}
	
}

/* Confirmation dialogs for New operations */

public class SaveSubtitlesOnNewFileConfirmationDialog : SaveConfirmationDialog {
	private static string primaryText = Catalog.GetString("Save the changes to subtitles \"{0}\" before creating new subtitles?");
	private static string rejectLabel = Catalog.GetString("Create without Saving");

	public SaveSubtitlesOnNewFileConfirmationDialog () : base(primaryText, rejectLabel, SubtitleTextType.Text) {
	}
	
	public SaveSubtitlesOnNewFileConfirmationDialog (string primaryText, SubtitleTextType textType) : base(primaryText, rejectLabel, textType) {
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

	public SaveSubtitlesOnOpenFileConfirmationDialog () : base(primaryText, rejectLabel, SubtitleTextType.Text) {
	}
	
	public SaveSubtitlesOnOpenFileConfirmationDialog (string primaryText, SubtitleTextType textType) : base (primaryText, rejectLabel, textType) {
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

	public SaveSubtitlesOnCloseFileConfirmationDialog () : base(primaryText, rejectLabel, SubtitleTextType.Text) {
	}
	
	public SaveSubtitlesOnCloseFileConfirmationDialog (string primaryText, SubtitleTextType textType) : base(primaryText, rejectLabel, textType) {
	}
}

public class SaveTranslationOnCloseConfirmationDialog : SaveSubtitlesOnCloseFileConfirmationDialog {
	private static string primaryText = Catalog.GetString("Save the changes to translation \"{0}\" before closing?");

	public SaveTranslationOnCloseConfirmationDialog () : base(primaryText, SubtitleTextType.Translation) {
	}
}

}
