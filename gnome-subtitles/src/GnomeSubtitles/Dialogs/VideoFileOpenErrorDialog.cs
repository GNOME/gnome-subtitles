/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
 * Copyright (C) 2007 Pedro Castro
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

public class VideoFileOpenErrorDialog : FileOpenErrorDialog {

	public VideoFileOpenErrorDialog (string filename, Exception exception) : base(filename, exception) {
	}

	/* Overriden members */
	
	protected override string SecondaryTextFromException (Exception exception) {
		if (exception is PlayerNotFoundException)
			return "Unable to start the video player. Please check that MPlayer is installed.";
		else if (exception is PlayerCouldNotOpenVideoException)
			return "Please check that the video file is supported.";
		else
			return GetGeneralExceptionErrorMessage(exception);
	}

}

}