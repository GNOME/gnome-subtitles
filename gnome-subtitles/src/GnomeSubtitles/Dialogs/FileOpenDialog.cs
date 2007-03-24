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
	private Regex regex = new Regex(@"^.*\.((avi)|(mpeg)|(mpg)|(mp4)|(ogm)|(divx)|(xvid)|(mov))$"); //Regex to match video files
	private ArrayList videoFiles = null; //The full paths of the video files in the current fir
	private ArrayList videoFilenames = null; //The filenames of videoFiles, without extensions
	
	private string chosenVideoFilename = String.Empty;
	private bool autoChooseVideoFile = true;

	/* Constant strings */
	private const string gladeFilename = "FileOpenDialog.glade";

	/* Widgets */
	[WidgetAttribute]
	private ComboBox encodingComboBox;
	[WidgetAttribute]
	private ComboBox videoComboBox;
	[WidgetAttribute]
	private Label videoLabel;
	
	
	public FileOpenDialog () : base(gladeFilename) {
		autoChooseVideoFile = Global.Config.PrefsVideoAutoChooseFile;
	
		SetEncodingComboBox();
		videoComboBox.RowSeparatorFunc = SeparatorFunc;	
		
		dialog.CurrentFolderChanged += OnCurrentFolderChanged; //Only needed because setting it in the Glade file is not working
		dialog.SelectionChanged += OnSelectionChanged; //Only needed because setting it in the Glade file is not working
	
		if (Global.AreSubtitlesLoaded && Global.Subtitles.Properties.IsFilePathRooted)
			dialog.SetCurrentFolder(Global.Subtitles.Properties.FileDirectory);
		else
			dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			
		SetFilters();
	}
	
	/* Public properties */
	
	public bool HasVideoFilename {
		get { return chosenVideoFilename != String.Empty; }
	}
	
	public string VideoFilename {
		get { return chosenVideoFilename; }
	}
	
	/* Private members */
	
	private void SetEncodingComboBox () {
		SetEncodingComboBox(encodingComboBox);
		encodingComboBox.PrependText("-");
		encodingComboBox.PrependText("Auto Detected");
		encodingComboBox.Active = 0;
	}
	
	private void FillVideoComboBoxBasedOnCurrentFolder () {
		System.Console.WriteLine("FillVideoComboBoxBasedOnCurrentFolder: FVCBBOCF"); //TODO delete
		videoFiles = null;
		videoFilenames = null;
		(videoComboBox.Model as ListStore).Clear();
	
		string folder = dialog.CurrentFolder;
		System.Console.WriteLine("FVCBBOCF: Current Folder = " + folder + ". Is it null? " + (folder == null)); //TODO delete
		if ((folder == null) || (folder == String.Empty)) {
			System.Console.WriteLine("Selected folder on File Open is null or empty"); //TODO delete
			return;
		}
		
		string[] allFiles = Directory.GetFiles(folder, "*.*");
		
		videoFiles = new ArrayList();
		videoFilenames = new ArrayList();
		
		foreach (string file in allFiles) {
			if (regex.IsMatch(file))
				videoFiles.Add(file);	
		}
		
		if ((videoFiles.Count == 0) || (videoFiles == null)) {
			SetVideoSelectionSensitivity(false);
			return;
		}
		else
			SetVideoSelectionSensitivity(true);
				
		videoFiles.Sort();

		foreach (string file in videoFiles) {
			string filename = Path.GetFileName(file);
			videoComboBox.AppendText(filename);
			
			videoFilenames.Add(FilenameWithoutExtension(filename));
		}
		
		videoComboBox.PrependText("-");
		videoComboBox.PrependText("None");
		videoComboBox.Active = 0;
		
		System.Console.WriteLine("FVCBBOCF: Found " + videoFiles.Count + " video files."); //TODO delete
	}
	
	private void SetActiveVideoFile () {
		System.Console.WriteLine("SetActiveVideoFile: SAVF"); //TODO delete
		System.Console.WriteLine("SAVF: videoFiles null? " + (videoFiles == null)); //TODO delete
		System.Console.WriteLine("SAVF: videoFiles has " + videoFiles.Count + " elements."); //TODO delete
		if ((videoFiles == null) || (videoFiles.Count == 0))
			return;
	
		string filePath = dialog.Filename;
		System.Console.WriteLine("SAVF: Selected file path: " + filePath); //TODO delete
		System.Console.WriteLine("SAVF: Selected file path is null? " + (filePath == null)); //TODO delete
		System.Console.WriteLine("SAVF: Selected file path is empty? " + (filePath == String.Empty)); //TODO delete
		System.Console.WriteLine("SAVF: Selected file path exists? " + File.Exists(filePath)); //TODO delete
		if ((filePath == String.Empty) || (filePath == null) || (!File.Exists(filePath))) {
			SetActiveComboBoxItem(0);
			return;
		}
		
		string filename = Path.GetFileNameWithoutExtension(filePath);
		System.Console.WriteLine("SAVF: Filename related to filePath: " + filename); //TODO delete
		System.Console.WriteLine("SAVF: Filename is empty? " + (filename == String.Empty)); //TODO delete
		System.Console.WriteLine("SAVF: Filename is null? " + (filename == null)); //TODO delete
		if ((filename == String.Empty) || (filename == null)) {
			SetActiveComboBoxItem(0);
			return;
		}
		
		int activeVideoFile = 0;
		
		System.Console.WriteLine("SAVF: Matching file with videos."); //TODO delete
		for (int count = 0 ; count < videoFilenames.Count ; count++) {
			string videoFilename = videoFilenames[count] as string;
			System.Console.WriteLine("SAVF: Count " + count + ", video filename: " + videoFilename + ", video filepath: " + (videoFiles[count] as string)); //TODO delete
			if (filename.Equals(videoFilename)) {
				System.Console.WriteLine("SAVF: It matches! Setting activeVideoFile to " + (count + 2) + " in " + videoFilenames.Count + " total files (plus 2)."); //TODO delete
				activeVideoFile = count + 2; //Add 2 because of prepended text
				break;
			}
		}
		System.Console.WriteLine("SAVF: Setting activeVideoFile to " + activeVideoFile); //TODO delete
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
			if (encodingComboBox.Active > 0) {
				int encodingIndex = encodingComboBox.Active - 2;
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
		System.Console.WriteLine("OnCurrentFolderChanged event"); //TODO delete
		FillVideoComboBoxBasedOnCurrentFolder();
	}
	
	private void OnSelectionChanged (object o, EventArgs args) {
		System.Console.WriteLine("OnSelectionChanged event"); //TODO delete
		if (autoChooseVideoFile)
			SetActiveVideoFile();
	}
	
}

}
