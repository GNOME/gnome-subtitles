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

public class FileOpenErrorDialog : GladeDialog {
	
	/* Constant strings */
	private const string gladeFilename = "FileOpenErrorDialog.glade";
	private const string actionLabel = "Open another file";

	public FileOpenErrorDialog (string filename, Exception exception) : base(gladeFilename) {
		MessageDialog messageDialog = dialog as MessageDialog;
		messageDialog.Text += " " + filename + ".";
		messageDialog.SecondaryText = SecondaryTextFromException(exception);

		Button actionButton = messageDialog.AddButton(actionLabel, ResponseType.Accept) as Button;
		actionButton.Image = new Image(Stock.Open, IconSize.Button);
		messageDialog.AddButton(Stock.Ok, ResponseType.Ok);
	}

	/* Private members */
	
	private string SecondaryTextFromException (Exception exception) {
		if (exception is UnknownSubtitleFormatException)
			return "Unable to detect the subtitle format. Please check that the file type is supported.";
		else if (exception is OutOfMemoryException)
			return "You have run out of memory. Please close some programs and try again.";
		else if (exception is IOException)
			return "An I/O error has occured.";
		else
			return "An unknown error has occured. Please report a bug and include this name: \"" + exception.GetType() + "\".";
	}
	
	/* Event members */
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		ResponseType response = args.ResponseId;
		if (response == ResponseType.Accept) {
			actionDone = true;
		}
		CloseDialog();
	}

}

}