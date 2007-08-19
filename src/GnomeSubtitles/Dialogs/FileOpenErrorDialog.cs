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

public abstract class FileOpenErrorDialog : ErrorDialog {
	
	/* Strings */
	private string primaryTextStart = Catalog.GetString("Could not open the file");
	private string actionLabel = Catalog.GetString("Open another file"); 

	public FileOpenErrorDialog (Uri uri, Exception exception) : this(uri.LocalPath, exception) {
	}

	public FileOpenErrorDialog (string filename, Exception exception) {
		Console.Error.WriteLine("File open error:\n" + exception);

		string primaryText = GetPrimaryText(filename);
		string secondaryText = GetSecondaryText(exception);
		SetText(primaryText, secondaryText);
	}
	
	/* Protected methods */
	
	protected override void AddButtons () {
		Button actionButton = dialog.AddButton(actionLabel, ResponseType.Accept) as Button;
		actionButton.Image = new Image(Stock.Open, IconSize.Button);
		dialog.AddButton(Stock.Ok, ResponseType.Ok);
	}


	/* Abstract methods */
	
	protected abstract string SecondaryTextFromException (Exception exception);

	
	/* Private methods */
	
	private string GetPrimaryText (string filename) {
		return primaryTextStart + " " + filename + ".";
	}
	
	private string GetSecondaryText (Exception exception) {
		string text = SecondaryTextFromException(exception);
		if (text != String.Empty)
			return text;
		else if (exception is UriFormatException)
			return Catalog.GetString("The file path appears to be invalid.");
		else
			return GetGeneralExceptionErrorMessage(exception);
	}

}

}
