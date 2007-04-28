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
using System;

namespace GnomeSubtitles {

/* TODO: Use MessageDialog from Glade? */
public class SaveConfirmationDialog {
	private MessageDialog dialog = null;
	private bool toClose = false;
	
	/* Strings */
	private string secondaryText = Catalog.GetString("If you don't save, all your changes will be permanently lost.");


	public SaveConfirmationDialog (string primaryText, string rejectLabel) {
		string message = "<span weight=\"bold\" size=\"larger\">" + primaryText + "</span>\n\n" + secondaryText;
		string fileName = Global.Document.FileProperties.Filename;
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
		ResponseType response = args.ResponseId;
		if (response == ResponseType.Reject)
			toClose = true;
		else if (response == ResponseType.Accept)
			toClose = Global.GUI.Save();
		
		CloseDialog();
	}
	
	/* Private members */
	
	private void CloseDialog() {
		dialog.Destroy();
	}
	
}

public class SaveOnCloseConfirmationDialog : SaveConfirmationDialog {
	private static string primaryText = Catalog.GetString("Save the changes to subtitles \"{0}\" before closing?");
	private static string rejectLabel = Catalog.GetString("Close without Saving");

	public SaveOnCloseConfirmationDialog () : base(primaryText, rejectLabel) {
	}
}

public class SaveOnNewConfirmationDialog : SaveConfirmationDialog {
	private static string primaryText = Catalog.GetString("Save the changes to subtitles \"{0}\" before creating new subtitles?");
	private static string rejectLabel = Catalog.GetString("Create without Saving");

	public SaveOnNewConfirmationDialog () : base(primaryText, rejectLabel) {
	}
}

public class SaveOnOpenConfirmationDialog : SaveConfirmationDialog {
	private static string primaryText = Catalog.GetString("Save the changes to subtitles \"{0}\" before opening?");
	private static string rejectLabel = Catalog.GetString("Open without Saving");

	public SaveOnOpenConfirmationDialog () : base(primaryText, rejectLabel) {
	}
}

}
