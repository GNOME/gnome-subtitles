/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
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

using System;
using System.IO;
using Gtk;
using SubLib;

namespace GnomeSubtitles {

public abstract class FileOpenErrorDialog : GladeDialog {
	
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

	/* Abstract methods */
	
	protected abstract string SecondaryTextFromException (Exception exception);
	
	/* Protected methods */
	
	protected string GetGeneralExceptionErrorMessage (Exception exception) {
		return "An unknown error has occured. Please report a bug and include this error name: \"" + exception.GetType() + "\".";
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