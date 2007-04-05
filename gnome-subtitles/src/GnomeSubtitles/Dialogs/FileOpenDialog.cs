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

using Glade;
using Gtk;
using SubLib;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GnomeSubtitles {

public class FileOpenDialog : SubtitleFileChooserDialog {
	private ArrayList videoFiles = null; //The full paths of the video files in the current fir
	private ArrayList videoFilenames = null; //The filenames of videoFiles, without extensions
	
	private string chosenVideoFilename = String.Empty;
	private bool autoChooseVideoFile = true;

	/* Constant strings */
	private const string gladeFilename = "FileOpenDialog.glade";

	/* Widgets */
	[WidgetAttribute] private ComboBox videoComboBox;
	[WidgetAttribute] private Label videoLabel;
	
	
	public FileOpenDialog () : base(gladeFilename) {
		autoChooseVideoFile = Global.Config.PrefsVideoAutoChooseFile;

		videoComboBox.RowSeparatorFunc = SeparatorFunc;	
		
		dialog.CurrentFolderChanged += OnCurrentFolderChanged; //Only needed because setting it in the Glade file is not working
		dialog.SelectionChanged += OnSelectionChanged; //Only needed because setting it in the Glade file is not working
	
		if (Global.IsDocumentLoaded && Global.Document.FileProperties.IsPathRooted)
			dialog.SetCurrentFolder(Global.Document.FileProperties.Directory);
		else
			dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			
		SetFilters();
		
		ShowDialog();
	}
	
	/* Public properties */
	
	public bool HasVideoFilename {
		get { return chosenVideoFilename != String.Empty; }
	}
	
	public string VideoFilename {
		get { return chosenVideoFilename; }
	}
	
	/* Protected members */
	
	protected override void AddInitialEncodingComboBoxItems (ComboBox comboBox) {
		comboBox.AppendText("Auto Detected");
		comboBox.AppendText("-");
	}
	
	/* Private members */
	
	private void FillVideoComboBoxBasedOnCurrentFolder () {
		videoFiles = null;
		videoFilenames = null;
		(videoComboBox.Model as ListStore).Clear();
	
		string folder = dialog.CurrentFolder;
		if ((folder == null) || (folder == String.Empty))
			return;

		videoFiles = VideoFiles.GetVideoFilesAtPath(folder);

		if ((videoFiles.Count == 0) || (videoFiles == null)) {
			SetVideoSelectionSensitivity(false);
			return;
		}
		else
			SetVideoSelectionSensitivity(true);
				
		videoFiles.Sort();
		videoFilenames = new ArrayList();
		foreach (string file in videoFiles) {
			string filename = Path.GetFileName(file);
			videoComboBox.AppendText(filename);
			
			videoFilenames.Add(FilenameWithoutExtension(filename));
		}
		
		videoComboBox.PrependText("-");
		videoComboBox.PrependText("None");
		videoComboBox.Active = 0;
	}
	
	private void SetActiveVideoFile () {
		if ((videoFiles == null) || (videoFiles.Count == 0))
			return;
	
		string filePath = dialog.Filename;
		if ((filePath == null) || (filePath == String.Empty) || (!File.Exists(filePath))) {
			SetActiveComboBoxItem(0);
			return;
		}
		
		string filename = Path.GetFileNameWithoutExtension(filePath);
		if ((filename == String.Empty) || (filename == null)) {
			SetActiveComboBoxItem(0);
			return;
		}
		
		int activeVideoFile = 0;
		
		for (int count = 0 ; count < videoFilenames.Count ; count++) {
			string videoFilename = videoFilenames[count] as string;
			if (filename.Equals(videoFilename)) {
				activeVideoFile = count + 2; //Add 2 because of prepended text
				break;
			}
		}
		SetActiveComboBoxItem(activeVideoFile);		
	}
	
	private void SetActiveComboBoxItem (int item) {
		videoComboBox.Active = item;
	}
	
	private void SetVideoSelectionSensitivity (bool sensitivity) {
		videoComboBox.Sensitive = sensitivity;
		videoLabel.Sensitive = sensitivity;
	}
	
	private string FilenameWithoutExtension (string filename) {
		int index = filename.LastIndexOf('.');
		if (index != -1)
			return filename.Substring(0, index);
		else
			return filename;
	}
	
	private void SetFilters () {
		SubtitleTypeInfo[] types = Subtitles.AvailableTypesSorted;
		FileFilter[] filters = new FileFilter[types.Length + 2];
		int filterPosition = 0;
		
		/* First filter corresponds to all files */
		FileFilter allFilesFilter = new FileFilter();
		allFilesFilter.Name = "All Files";
		allFilesFilter.AddPattern("*");
		filters[filterPosition] = allFilesFilter;
		filterPosition++;
		
		/* Second filter corresponds to all subtitle files */
		FileFilter subtitleFilesFilter = new FileFilter();
		subtitleFilesFilter.Name = "All Subtitle Files";
		subtitleFilesFilter.AddPattern("*.txt");
		filters[filterPosition] = subtitleFilesFilter;
		filterPosition++;
		
		/* Remaining filters correspond to the subtitle types */
		foreach (SubtitleTypeInfo type in types) {
			FileFilter filter = new FileFilter();
			foreach (string extension in type.Extensions) {
				string pattern = "*." + extension;
				filter.AddPattern(pattern);
				subtitleFilesFilter.AddPattern(pattern);
			}
			filter.Name = type.Name;
			filters[filterPosition] = filter;
			filterPosition++;
		}
		
		foreach (FileFilter filter in filters)
			dialog.AddFilter(filter);
		
		dialog.Filter = subtitleFilesFilter;
	}
	
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok) {
			chosenFilename = dialog.Filename;
			int activeEncodingComboBoxItem = GetActiveEncodingComboBoxItem();
			if (activeEncodingComboBoxItem > 0) {
				int encodingIndex = activeEncodingComboBoxItem - 2;
				chosenEncoding = encodings[encodingIndex];
				hasChosenEncoding = true;
			}
			if (videoComboBox.Active > 0) {
				int videoFileIndex = videoComboBox.Active - 2;
				chosenVideoFilename = videoFiles[videoFileIndex] as string;
			}			
			actionDone = true;
		}
		CloseDialog();
	}
	
	private void OnCurrentFolderChanged (object o, EventArgs args) {
		FillVideoComboBoxBasedOnCurrentFolder();
	}
	
	private void OnSelectionChanged (object o, EventArgs args) {
		if (autoChooseVideoFile)
			SetActiveVideoFile();
	}
	
}

}
