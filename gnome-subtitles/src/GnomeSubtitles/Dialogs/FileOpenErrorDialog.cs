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
using System.IO;
using Gtk;
using SubLib;

namespace GnomeSubtitles {

//TODO: use MessageDialog from Glade?
public class OpenErrorDialog {
	private MessageDialog dialog = null;
	private const string primaryText = "<span weight=\"bold\" size=\"larger\">Could not open the subtitles \"{0}\".</span>";
	private const string actionLabel = "Open another file";
	private bool toRunAction = false;
	
	public OpenErrorDialog (string fileName, Exception exception) {
		string secondaryText = SecondaryTextFromException(exception);
		string message = primaryText + "\n\n" + secondaryText;
		dialog = new MessageDialog(Global.GUI.Window, DialogFlags.Modal, MessageType.Error,
			ButtonsType.None, message, fileName);
	
		Button actionButton = dialog.AddButton(actionLabel, ResponseType.Accept) as Button;
		actionButton.Image = new Image(Stock.Open, IconSize.Button);
		dialog.AddButton(Stock.Ok, ResponseType.Ok);

		dialog.Title = String.Empty;
		dialog.Response += OnResponse;
	}
	
	public bool WaitForResponse () {
		dialog.Run();
		return toRunAction;
	}
	
	/* Event handlers */
	
	private void OnResponse (object o, ResponseArgs args) {
		ResponseType response = args.ResponseId;
		if (response == ResponseType.Accept) {
			toRunAction = true;
		}
		CloseDialog();
	}
	
	/* Private members */
	
	private void CloseDialog() {
		dialog.Destroy();
	}

	private string SecondaryTextFromException (Exception exception) {
		if (exception is UnknownSubtitleFormatException)
			return "Unable to detect the subtitle format. Please check that the file type is supported.";
		else if (exception is OutOfMemoryException)
			return "You have run out of memory. Please close some programs and try again.";
		else if (exception is IOException)
			return "An I/O error has occured.";
		else
			return "An unknown error has occured. Please report a bug and include this exception name: \"" + exception.GetType() + "\".";
	}
	
}

}