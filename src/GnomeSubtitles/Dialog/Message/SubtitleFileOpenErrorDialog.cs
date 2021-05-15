/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2021 Pedro Castro
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
using SubLib.Exceptions;
using SubLib.Util;
using System;
using System.IO;
using System.Security;

namespace GnomeSubtitles.Dialog.Message {

public class SubtitleFileOpenErrorDialog : ErrorDialog {
	
	private string primaryTextStart = Catalog.GetString("Could not open the file {0}.");
	private string actionLabel = Catalog.GetString("Open another file");

	public SubtitleFileOpenErrorDialog (string filename, Exception exception) {
		Logger.Error(exception, "Subtitle file open error");

		string primaryText = GetPrimaryText(filename);
		string secondaryText = GetSecondaryText(exception);
		SetText(primaryText, secondaryText);
	}

	/* Overridden members */
	
	protected override void AddButtons () {
		Button actionButton = dialog.AddButton(actionLabel, ResponseType.Accept) as Button;
		actionButton.Image = new Image(Stock.Open, IconSize.Button);
		dialog.AddButton(Stock.Ok, ResponseType.Ok);
		
		dialog.DefaultResponse = ResponseType.Accept;
	}
	
	
	/* Private members */

	private string GetPrimaryText (string filename) {
		return string.Format(primaryTextStart, filename);
	}

	private string GetSecondaryText (Exception exception) {
		string text = GetSecondaryTextFromException(exception);
		if (text != null) {
			return text;
		} 

		return GetGeneralExceptionErrorMessage(exception);
	}


	private string GetSecondaryTextFromException (Exception exception) {
		if (exception is UnknownSubtitleFormatException)
			return Catalog.GetString("Unable to detect the subtitle format. Please check that the file type is supported.");
		else if (exception is EncodingNotSupportedException)
			return Catalog.GetString("The encoding used is not supported by your system. Please choose another encoding.");
		else if (exception is OutOfMemoryException)
			return Catalog.GetString("You have run out of memory. Please close some programs and try again.");
		else if (exception is IOException)
			return Catalog.GetString("An I/O error has occurred.");
		else if ((exception is UnauthorizedAccessException) || (exception is SecurityException))
			return Catalog.GetString("You do not have the permissions necessary to open the file.");
		else if ((exception is ArgumentNullException) || (exception is ArgumentException) || (exception is NotSupportedException) || (exception is PathTooLongException))
			return Catalog.GetString("The specified file is invalid.");
		else if (exception is FileNotFoundException)
			return Catalog.GetString("The file could not be found.");
		else if (exception is FileTooLargeException)
			return Catalog.GetString("The file appears to be too large for a text-based subtitle file.");
		else if (exception is UriFormatException)
			return Catalog.GetString("The file path appears to be invalid.");
		else
			return null;
	}

}

}
