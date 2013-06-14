/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2011 Pedro Castro
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

using GnomeSubtitles.Core;
using Gtk;
using Mono.Unix;
using System;

namespace GnomeSubtitles.Dialog {

public class VideoOpenDialog : GladeDialog {
	protected FileChooserDialog dialog = null;
	private Uri chosenUri = null;

	/* Constant strings */
	private const string gladeFilename = "VideoOpenDialog.glade";


	public VideoOpenDialog () : base(gladeFilename) {
		dialog = GetDialog() as FileChooserDialog;

		if (Base.IsDocumentLoaded && Base.Document.TextFile.IsPathRooted)
			dialog.SetCurrentFolder(Base.Document.TextFile.Directory);
		else
			dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));

		SetFilters();
	}

	/* Public properties */

	public Uri Uri {
		get { return chosenUri; }
	}

	/* Private methods */

	private void SetFilters () {

		/* Video files */
		FileFilter videoFilesFilter = new FileFilter();
		videoFilesFilter.Name = Catalog.GetString("All Video Files");
		videoFilesFilter.AddMimeType("video/*");
		videoFilesFilter.AddMimeType("application/vnd.rn-realmedia"); //support for real media files (currently includes rmvb)
		videoFilesFilter.AddMimeType("application/vnd.rn-realmedia-vbr"); //this should be the real mime type for rmvb files
		dialog.AddFilter(videoFilesFilter);

		/* Audio files */
		FileFilter audioFilesFilter = new FileFilter();
		audioFilesFilter.Name = Catalog.GetString("All Audio Files");
		audioFilesFilter.AddMimeType("audio/*");
		audioFilesFilter.AddMimeType("application/ogg");
		dialog.AddFilter(audioFilesFilter);


		/* All files */
		FileFilter allFilesFilter = new FileFilter();
		allFilesFilter.Name = Catalog.GetString("All Files");
		allFilesFilter.AddPattern("*");
		dialog.AddFilter(allFilesFilter);

		/* Set active filter */
		dialog.Filter = videoFilesFilter;
	}

	/* Event members */

	#pragma warning disable 169		//Disables warning about handlers not being used

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			if (dialog.Uri != null) {
				chosenUri = new Uri(dialog.Uri);
			}
			SetReturnValue(true);
		}
		return false;
	}

}

}
