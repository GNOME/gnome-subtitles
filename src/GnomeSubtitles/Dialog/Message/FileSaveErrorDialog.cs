/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2019 Pedro Castro
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
using System.IO;
using System.Security;
using SubLib.Util;

namespace GnomeSubtitles.Dialog.Message {

public class FileSaveErrorDialog : ErrorDialog {

	/* Strings */
	private string primaryTextStart = Catalog.GetString("Could not save the file");
	private string actionLabel = Catalog.GetString("Save to another file");

	public FileSaveErrorDialog (string filename, Exception exception) {
		Logger.Error(exception, "File save error");

		string primaryText = GetPrimaryText(filename);
		string secondaryText = GetSecondaryText(exception);
		SetText(primaryText, secondaryText);
	}

	/* Protected methods */

	protected override void AddButtons () {
		Button actionButton = dialog.AddButton(actionLabel, ResponseType.Accept) as Button;
		actionButton.Image = new Image(Stock.Save, IconSize.Button);
		dialog.AddButton(Stock.Ok, ResponseType.Ok);
		
		dialog.DefaultResponse = ResponseType.Accept;
	}

	/* Private methods */

	private string GetPrimaryText (string filename) {
		return primaryTextStart + " " + filename + ".";
	}

	private string GetSecondaryText (Exception exception) {
		if (exception is OutOfMemoryException)
			return Catalog.GetString("You have run out of memory. Please close some programs and try again.");
		else if (exception is IOException)
			return Catalog.GetString("An I/O error has occurred.");
		else if ((exception is UnauthorizedAccessException) || (exception is SecurityException))
			return Catalog.GetString("You do not have the permissions necessary to save the file.");
		else if ((exception is ArgumentNullException) || (exception is ArgumentException) || (exception is PathTooLongException))
			return Catalog.GetString("The specified file is invalid.");
		else
			return GetGeneralExceptionErrorMessage(exception);
	}

}

}
