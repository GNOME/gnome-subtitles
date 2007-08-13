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

public class VideoOpenDialog : GladeDialog {
	protected new FileChooserDialog dialog = null;
	private static string[] extensions = { "avi", "mpeg", "mpg", "mp4", "ogm", "divx", "xvid", "mov", "ogg" };
	private string chosenUri = String.Empty;
	
	/* Constant strings */
	private const string gladeFilename = "VideoOpenDialog.glade";
	
	
	public VideoOpenDialog () : base(gladeFilename) {
		dialog = base.dialog as FileChooserDialog;

		if (Global.IsDocumentLoaded && Global.Document.TextFile.IsPathRooted)
			dialog.SetCurrentFolder(Global.Document.TextFile.Directory);
		else
			dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));

		SetFilters();
	}
	
	/* Public properties */

	public string Uri {
		get { return chosenUri; }
	}
	
	/* Private methods */
	
	/* TODO check how other players are setting up the filters, possibly using MIME types. */
	private void SetFilters () {

		/* First filter corresponds to all files */
		FileFilter allFilesFilter = new FileFilter();
		allFilesFilter.Name = Catalog.GetString("All Files");
		allFilesFilter.AddPattern("*");
		dialog.AddFilter(allFilesFilter);
		
		/* Second filter corresponds to video files */
		FileFilter videoFilesFilter = new FileFilter();
		videoFilesFilter.Name = Catalog.GetString("All Video Files");
		foreach (string extension in extensions) {
			videoFilesFilter.AddPattern("*." + extension);
		}
		dialog.AddFilter(videoFilesFilter);
		
		/* Set active filter */
		dialog.Filter = videoFilesFilter;
	}
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok) {
			chosenUri = (dialog as FileChooserDialog).Uri;
			actionDone = true;
		}
		Close();
	}

}

}
