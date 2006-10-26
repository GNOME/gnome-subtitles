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

using System;
using Gtk;

namespace GnomeSubtitles {

public class ConfirmationDialog {
	private GUI gui = null;
	private MessageDialog dialog = null;
	private string secondaryText = "If you don't save, all your changes will be permanently lost.";
	private bool toClose = false;
	
	public ConfirmationDialog (GUI gui, string primaryText, string rejectLabel) {
		this.gui = gui;
		string message = "<span weight=\"bold\" size=\"larger\">" + primaryText + "</span>\n\n" + secondaryText;
		string fileName = gui.Core.Subtitles.Properties.FileName;
		dialog = new MessageDialog(gui.Window, DialogFlags.Modal, MessageType.Warning,
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
			toClose = gui.Save();
		
		CloseDialog();
	}
	
	/* Private members */
	
	private void CloseDialog() {
		dialog.Destroy();
	}
	
}

public class CloseConfirmationDialog : ConfirmationDialog {
	private static string primaryText = "Save the changes to subtitles \"{0}\" before closing?";
	private static string rejectLabel = "Close without Saving";

	public CloseConfirmationDialog (GUI gui) : base(gui, primaryText, rejectLabel) {
	}
}

public class NewConfirmationDialog : ConfirmationDialog {
	private static string primaryText = "Save the changes to subtitles \"{0}\" before creating new subtitles?";
	private static string rejectLabel = "Create without Saving";

	public NewConfirmationDialog (GUI gui) : base(gui, primaryText, rejectLabel) {
	}
}

public class OpenConfirmationDialog : ConfirmationDialog {
	private static string primaryText = "Save the changes to subtitles \"{0}\" before opening?";
	private static string rejectLabel = "Open without Saving";

	public OpenConfirmationDialog (GUI gui) : base(gui, primaryText, rejectLabel) {
	}
}

}