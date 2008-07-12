/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2008 Pedro Castro
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
using SubLib;
using System;

namespace GnomeSubtitles.Dialog {

public abstract class SaveConfirmationDialog : WarningDialog {
	private SubtitleTextType textType;
	
	#region Strings
	private string secondaryText = Catalog.GetString("If you don't save, all your changes will be permanently lost.");
	#endregion


	public SaveConfirmationDialog (string primaryText, SubtitleTextType textType) : base() {
		this.textType = textType;
	
		string fileName = (textType == SubtitleTextType.Text ? Base.Document.TextFile.Filename : Base.Document.TranslationFile.Filename);
		SetText(primaryText, secondaryText, fileName);
	}
	
	
	#region Abstract methods
	
	protected abstract string GetRejectLabel ();
	
	#endregion
	

	#region Protected methods
	
	protected override void AddButtons () {
		string rejectLabel = GetRejectLabel();
		dialog.AddButton(rejectLabel, ResponseType.Reject);
		dialog.AddButton(Stock.Cancel, ResponseType.Cancel);
		dialog.AddButton(Stock.Save, ResponseType.Accept);
	}
	
	#endregion
	
	
	#region Events
	
	protected override void OnResponse (object o, ResponseArgs args) {
		Close();

		ResponseType response = args.ResponseId;
		if (response == ResponseType.Reject)
			returnValue = true;
		else if (response == ResponseType.Accept) {
			if (textType == SubtitleTextType.Text)
				returnValue = Core.Base.Ui.Save();
			else
				returnValue = Core.Base.Ui.TranslationSave();
		}
	}
	
	#endregion

}

/* Confirmation dialogs for New operations */

public class SaveSubtitlesOnNewFileConfirmationDialog : SaveConfirmationDialog {

	#region Strings
	private static string primaryText = Catalog.GetString("Save the changes to subtitles \"{0}\" before creating new subtitles?");
	private static string rejectLabel = Catalog.GetString("Create without Saving");
	#endregion

	public SaveSubtitlesOnNewFileConfirmationDialog () : base(primaryText, SubtitleTextType.Text) {
	}
	
	public SaveSubtitlesOnNewFileConfirmationDialog (string primaryText, SubtitleTextType textType) : base(primaryText, textType) {
	}
	
	
	#region Protected methods
	
	protected override string GetRejectLabel ()
	{
		return rejectLabel;
	}
	
	#endregion
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
	
	#region Protected methods
	
	protected override string GetRejectLabel ()
	{
		return rejectLabel;
	}
	
	#endregion
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
	
	#region Protected methods
	
	protected override string GetRejectLabel ()
	{
		return rejectLabel;
	}
	
	#endregion
}

public class SaveTranslationOnCloseConfirmationDialog : SaveSubtitlesOnCloseFileConfirmationDialog {
	private static string primaryText = Catalog.GetString("Save the changes to translation \"{0}\" before closing?");

	public SaveTranslationOnCloseConfirmationDialog () : base(primaryText, SubtitleTextType.Translation) {
	}
}

}
