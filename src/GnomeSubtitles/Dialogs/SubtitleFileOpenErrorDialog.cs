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

using SubLib;
using System;
using System.IO;

namespace GnomeSubtitles {

public class SubtitleFileOpenErrorDialog : FileOpenErrorDialog {

	public SubtitleFileOpenErrorDialog (string filename, Exception exception) : base(filename, exception) {
	}

	/* Overriden members */
	
	protected override string SecondaryTextFromException (Exception exception) {
		if (exception is UnknownSubtitleFormatException)
			return "Unable to detect the subtitle format. Please check that the file type is supported.";
		else if (exception is OutOfMemoryException)
			return "You have run out of memory. Please close some programs and try again.";
		else if (exception is IOException)
			return "An I/O error has occured.";
		else if (exception is NotSupportedException)
			return "The encoding used is not supported by your system.";
		else
			return GetGeneralExceptionErrorMessage(exception);
	}

}

}
