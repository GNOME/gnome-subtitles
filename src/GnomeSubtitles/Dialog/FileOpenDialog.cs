/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2010 Pedro Castro
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
using System;
using System.Collections.Generic;
using System.IO;
using Glade;
using GnomeSubtitles.Core;
using GnomeSubtitles.Ui.Component;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;

namespace GnomeSubtitles.Dialog {

public class FileOpenDialog : GladeDialog {
	protected FileChooserDialog dialog = null;
		
	
	/* Preferences */
	protected bool autoChooseVideoFile = true;
	protected bool autoChooseTranslationFile = true;
	
	protected List<string> subtitleFiles = new List<string>();
	protected List<string> videoFiles = new List<string>();
		
	protected List<FilexEncodingCombo> ActiveSelectionCombos = new List<FilexEncodingCombo>();
		
	/* Constant strings */
	private const string gladeFilename = "FileOpenDialog.glade";

	/* Components */
	protected FilexEncodingCombo selectedSubtitle = null;
	protected FilexEncodingCombo selectedTranslation = null;
	protected FileCombo selectedVideo = null;

	/* Widgets */
	
	[WidgetAttribute] protected Label subtitleFileLabel = null;
	[WidgetAttribute] protected ComboBox subtitleFileComboBox = null;
	[WidgetAttribute] protected ComboBox subtitleEncodingComboBox = null;
	[WidgetAttribute] protected Label translationFileLabel = null;
	[WidgetAttribute] protected ComboBox translationFileComboBox = null;
	[WidgetAttribute] protected ComboBox translationEncodingComboBox = null;
	[WidgetAttribute] protected Label videoFileLabel = null;
	[WidgetAttribute] protected ComboBox videoFileComboBox = null;

	
	
	public FileOpenDialog () : this(true, true ,Catalog.GetString("Open Files")) {
	}
	
	protected FileOpenDialog (bool toEnableVideo, bool toEnableTranslation, string title) : base(gladeFilename) {
		dialog = GetDialog() as FileChooserDialog;
		dialog.Title = title;
		dialog.CurrentFolderChanged += OnCurrentFolderChangedGetTextFiles;
		dialog.SelectionChanged += OnSelectionGetTextFiles;
		
		InitSelectedSubtitleCombo (); // this is overriden in TranslationFileOpen
		
		if (toEnableTranslation) {
			InitSelectedTranslationCombo ();
		}
		if (toEnableVideo) // This must be enabled last to allow autoselection to funtion
			EnableVideo();
	
		string startFolder = GetStartFolder();
		dialog.SetCurrentFolder(startFolder);
		
		SetFilters();		
	}

	/* Overriden members */

	public override DialogScope Scope {
		get { return DialogScope.Singleton; }
	}

	/* Public properties */
		
	public string SelectedSubtitle {
		get { return selectedSubtitle != null ? selectedSubtitle.ActiveSelection : null;}		
	}
		
	public EncodingDescription SelectedSubtitleEncoding {
		get { return selectedSubtitle != null ?  selectedSubtitle.SelectedEncoding : EncodingDescription.Empty;}		
	}
	
	public string SelectedTranslation {
		get { return selectedTranslation != null ? selectedTranslation.ActiveSelection : null;}		
	}
		
	public EncodingDescription SelectedTranslationEncoding {
		get { return selectedTranslation != null ? selectedTranslation.SelectedEncoding : EncodingDescription.Empty;}		
	}
		
	public Uri SelectedVideo {
		get { return selectedVideo.ActiveSelection != null ? FileTools.GetUriFromFilePath(selectedVideo.ActiveSelection) : null;}		
	}

	/* Protected members */
	
	protected virtual string GetStartFolder () {
		if (Base.IsDocumentLoaded && Base.Document.TextFile.IsPathRooted)
			return Base.Document.TextFile.Directory;
		else
			return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
	}

	protected virtual void InitSelectedSubtitleCombo () {
		selectedSubtitle = new FilexEncodingCombo(subtitleFileLabel,subtitleFileComboBox, subtitleEncodingComboBox);
		ActiveSelectionCombos.Add(selectedSubtitle);
	}
	
	protected virtual void InitSelectedTranslationCombo () {
		autoChooseTranslationFile = true; //Base.Config.PrefsTranslationAutoChooseFile;
		selectedTranslation = new FilexEncodingCombo(translationFileLabel, translationFileComboBox, translationEncodingComboBox);	
		ActiveSelectionCombos.Add(selectedTranslation);
	}
	
	protected void AutoChooseVideoFile () {	
		if (ActiveSelectionCombos.Count > 0)
		 if (ActiveSelectionCombos[0].ActiveSelection != null)
			AutoChooseVideoFile(ActiveSelectionCombos[0].ActiveSelection);
				
	}	
		
	protected void AutoChooseVideoFile (string filetomatch) {
		string matchingvideo = FileTools.FindMatchingFile(filetomatch, videoFiles);
		if (videoFiles.Contains(matchingvideo))
			selectedVideo.Active = videoFiles.IndexOf(matchingvideo);
	}	
		
	protected void AutoChooseTranslationFile (string filetomatch) {
		string matchingtranslation = FileTools.FindMatchingFile(filetomatch, subtitleFiles);
		if(subtitleFiles.Contains(matchingtranslation)){
			selectedTranslation.Active = subtitleFiles.IndexOf(matchingtranslation);
		}
	} 	
		
	/* Private members */
		
	private void EnableVideo () {
		autoChooseVideoFile = Base.Config.PrefsVideoAutoChooseFile;
		videoFileComboBox.RowSeparatorFunc = ComboBoxUtil.SeparatorFunc;
		selectedVideo = new FileCombo(videoFileLabel,videoFileComboBox);
		dialog.CurrentFolderChanged += OnCurrentFolderChangedGetVideoFiles;
		dialog.SelectionChanged += OnSelectionGetVideoFiles;
	}
	
	private void SetFilters () {
		SubtitleTypeInfo[] types = Subtitles.AvailableTypesSorted;
		FileFilter[] filters = new FileFilter[types.Length + 3];
		int filterPosition = 0;
		
		/* First filter corresponds to all files */
		FileFilter allFilesFilter = new FileFilter();
		allFilesFilter.Name = Catalog.GetString("All Files");
		allFilesFilter.AddPattern("*");
		filters[filterPosition] = allFilesFilter;
		filterPosition++;
		
		/* Second filter corresponds to all video files*/
		FileFilter videoFilesFilter = new FileFilter();
		videoFilesFilter.Name = Catalog.GetString("Video Files");
		videoFilesFilter.AddMimeType("video/*");
		filters[filterPosition] = videoFilesFilter;
		filterPosition++;
		
		/* Third filter corresponds to all subtitle files */
		FileFilter subtitleFilesFilter = new FileFilter();
		subtitleFilesFilter.Name = Catalog.GetString("All Subtitle Files");
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
		
		dialog.Filter = allFilesFilter;
	}	
		
	private void DisplayVideoFiles (List<string> selectedfiles) {
		if (selectedfiles.Count == 1) {
			selectedVideo.Active = videoFiles.IndexOf(selectedfiles[0]);
			return;
		} 
		if ((autoChooseVideoFile)) {
			AutoChooseVideoFile();
			return;
		}
		selectedVideo.Active = -1;
	}
		
	private void DisplayTextFiles (List<string> selectedfiles) {
		ConnectFileSelectionGroupChanged(false);
			
		if (selectedfiles.Count == 1) {
			ResetTextSelectionComboGroups();	
			ActiveSelectionCombos[0].Active = subtitleFiles.IndexOf(selectedfiles[0]);
			if (autoChooseTranslationFile) 
				AutoChooseTranslationFile(selectedfiles[0]);		
		}
		else if (selectedfiles.Count == 2) {
			selectedfiles.Sort(delegate(string f1, string f2) {return f1.Length.CompareTo(f2.Length);});
			List<string> reversedlist = new List<string>(selectedfiles);
			reversedlist.Reverse();
				
			ActiveSelectionCombos[0].FillComboBox(selectedfiles);
			ActiveSelectionCombos[1].FillComboBox(reversedlist);
			ActiveSelectionCombos[0].Active = 0;
			ActiveSelectionCombos[1].Active = 0;
				
			ConnectFileSelectionGroupChanged(true);
		} else {
			ResetTextSelectionComboGroups();
		}
	}
		
	private void ResetTextSelectionComboGroups () {
		foreach(FilexEncodingCombo combogroup in ActiveSelectionCombos) {
			combogroup.FillComboBox(subtitleFiles);		
		}		
	}	

	#pragma warning disable 169		//Disables warning about handlers not being used

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			SetReturnValue(true);
		}
		return false;
	}
	
	private void OnCurrentFolderChangedGetTextFiles (object o, EventArgs args) {
		subtitleFiles.Clear();
		if (Directory.Exists(dialog.CurrentFolder)){
			subtitleFiles = new List<string>(FileTools.GetFilesOfType(dialog.CurrentFolder, ValidFileTypes.Subtitle));
			ResetTextSelectionComboGroups();
		}
	}
		
	private void OnCurrentFolderChangedGetVideoFiles (object o, EventArgs args) {
		videoFiles.Clear();
		if (Directory.Exists(dialog.CurrentFolder)) {
			videoFiles = new List<string>(FileTools.GetFilesOfType(dialog.CurrentFolder, ValidFileTypes.Video));
			selectedVideo.FillComboBox(videoFiles);
		}
	}
	
	private void OnSelectionGetTextFiles (object o, EventArgs args) {
		List<string> selectedfiles = new List<string>(FileTools.GetFilesOfType(dialog.Filenames, ValidFileTypes.Subtitle));
		if (selectedfiles.Count > ActiveSelectionCombos.Count)
			selectedfiles.Clear();
			
		DisplayTextFiles(selectedfiles);
	}
		
	private void OnSelectionGetVideoFiles (object o, EventArgs args) {
		List<string> selectedfiles = new List<string>(FileTools.GetFilesOfType(dialog.Filenames, ValidFileTypes.Video));
		if (selectedfiles.Count > 1)
			selectedfiles.Clear();
			
		DisplayVideoFiles(selectedfiles);
	}
		
	private void ConnectFileSelectionGroupChanged (bool toconnect) {
		if (selectedSubtitle == null || selectedTranslation == null) 
			return;
			
		if (toconnect) {
			selectedSubtitle.FileSelectionChanged += OnSelectedSubtitleChanged;
			selectedTranslation.FileSelectionChanged += OnSelectedTranslationChanged;
		} else {
			selectedSubtitle.FileSelectionChanged -= OnSelectedSubtitleChanged;
			selectedTranslation.FileSelectionChanged -= OnSelectedTranslationChanged;		
		}
	}
	
	private void OnSelectedSubtitleChanged (object o, EventArgs args) {
		if (selectedSubtitle.Active > -1 && selectedTranslation.Active > -1) {
			selectedTranslation.Active = selectedSubtitle.Active;
		}	
	}
	
	private void OnSelectedTranslationChanged (object o, EventArgs args) {
		if (selectedTranslation.Active > -1 && selectedSubtitle.Active > -1) {
			selectedSubtitle.Active = selectedTranslation.Active;	
		}
	}
}
}