/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2019 Pedro Castro
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

public class VideoOpenDialog : BaseDialog {
	private Uri chosenUri;
	
	private static readonly string[] VideoMimeTypes = {
		"video/*",
		"application/vnd.rn-realmedia", //support for real media files (currently includes rmvb)
		"application/vnd.rn-realmedia-vbr" //this should be the real mime type for rmvb files
	};
	
	private static readonly string[] AudioMimeTypes = {
		"audio/*",
		"application/ogg"
	};

	public VideoOpenDialog () : base() {
		base.Init(BuildDialog());
	}

	/* Public properties */

	public Uri Uri {
		get { return chosenUri; }
	}

	/* Private methods */

	private FileChooserDialog BuildDialog() {
		FileChooserDialog dialog = new FileChooserDialog(Catalog.GetString("Open Video"), Base.Ui.Window, FileChooserAction.Open,
			Util.GetStockLabel("gtk-cancel"), ResponseType.Cancel, Util.GetStockLabel("gtk-open"), ResponseType.Ok);

		dialog.DefaultResponse = ResponseType.Ok;

		if (Base.IsDocumentLoaded && Base.Document.TextFile.IsPathRooted) {
			dialog.SetCurrentFolder(Base.Document.TextFile.Directory);
		} else {
			dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
		}

		SetFilters(dialog);

		return dialog;
	}

	private void SetFilters (FileChooserDialog dialog) {
	
		/* Media files (video+audio) */
		FileFilter mediaFilesFilter = new FileFilter();
		mediaFilesFilter.Name = Catalog.GetString("All Media Files");
		foreach (string mimeType in VideoMimeTypes) {
			mediaFilesFilter.AddMimeType(mimeType);
		}
		foreach (string mimeType in AudioMimeTypes) {
			mediaFilesFilter.AddMimeType(mimeType);
		}
		dialog.AddFilter(mediaFilesFilter);

		/* Video files */
		FileFilter videoFilesFilter = new FileFilter();
		videoFilesFilter.Name = Catalog.GetString("Video Files");
		foreach (string mimeType in VideoMimeTypes) {
			videoFilesFilter.AddMimeType(mimeType);
		}
		dialog.AddFilter(videoFilesFilter);

		/* Audio files */
		FileFilter audioFilesFilter = new FileFilter();
		audioFilesFilter.Name = Catalog.GetString("Audio Files");
		foreach (string mimeType in AudioMimeTypes) {
			audioFilesFilter.AddMimeType(mimeType);
		}
		dialog.AddFilter(audioFilesFilter);


		/* All files */
		FileFilter allFilesFilter = new FileFilter();
		allFilesFilter.Name = Catalog.GetString("All Files");
		allFilesFilter.AddPattern("*");
		dialog.AddFilter(allFilesFilter);

		/* Set active filter */
		dialog.Filter = mediaFilesFilter;
	}

	/* Event members */

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			string uri = (Dialog as FileChooserDialog).Uri;
			if (uri != null) {
				chosenUri = new Uri(uri);
			}
			SetReturnValue(true);
		}
		return false;
	}

}

}
