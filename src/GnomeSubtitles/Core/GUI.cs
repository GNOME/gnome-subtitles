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
using SubLib;
using System;
using System.IO;
using System.Text;

namespace GnomeSubtitles {

public class GUI {
	private Window window = null;
	private Menus menus = null;
	private Video video = null;
	private SubtitleView view = null;
	private SubtitleEdit edit = null;
	private Status status = null;
	
	/* Constant strings */
	private const string gladeFilename = "MainWindow.glade";
	private const string iconFilename = "gnome-subtitles.png";
	
	public GUI (EventHandlers handlers, out Glade.XML glade) {
		glade = new Glade.XML(null, gladeFilename, null, Global.Execution.TranslationDomain);
		
		window = glade.GetWidget("window") as Window;
		window.Icon = new Gdk.Pixbuf(null, iconFilename);
		window.SetDefaultSize(Global.Config.PrefsWindowWidth, Global.Config.PrefsWindowHeight);
		
		video = new Video();
		view = new SubtitleView();
		edit = new SubtitleEdit();
		menus = new Menus();
		status = new Status();

		glade.Autoconnect(handlers); //TODO think about using separate connections for different parts of the GUI
		
		window.Visible = true;
    }

	/* Public properties */

	public Window Window {
		get { return window; }
	}

	public Menus Menus {
		get { return menus; }
	}
	
	public Video Video {
		get { return video; }
	}
	
	public SubtitleView View {
		get { return view; }
	}
	
	public SubtitleEdit Edit {
		get { return edit; }
	}
	
	public Status Status {
		get { return status; }
	}

    
    /* Public Methods */
    
    /// <summary>Starts the GUI</summary>
    /// <remarks>A file is opened if it was specified as argument. If it wasn't, a blank start is performed.</summary>
    public void Start () {
    	string[] args = Global.Execution.Args;
    	if (args.Length > 0) {
    		string subtitleFile = args[0];
    		string videoFile = Global.Config.PrefsVideoAutoChooseFile ? VideoFiles.FindMatchingVideo(subtitleFile) : String.Empty;
			Open(subtitleFile, -1, videoFile);
		}
		else
			BlankStartUp();
    }
    
    /// <summary>Quits the application.</summary>
    public void Quit () {
		if (ToCloseAfterWarning) {
			video.Quit();
			Global.Quit();
		}
    }
    
	/// <summary>Kills the window in the most quick and unfriendly way.</summary>
    public void Kill () {
		window.Destroy();
    }

	/// <summary>Creates a new subtitles document for the specified path.</summary>
	/// <param name="path">The subtitles' filename. If it's an empty string, 'Unsaved Subtitles' will be used instead.</param>
	/// <remarks>If there's a document opened with unsaved changes, a warning dialog is shown.</remarks>
    public void New (string path) {
    	if (!ToCreateNewAfterWarning)
    		return;

		if (path == String.Empty)
			path = Catalog.GetString("Unsaved Subtitles");

		Document document = new Document(path);
		Global.Document = document;

		if (document.Subtitles.Count == 0) {
			Global.CommandManager.Execute(new InsertFirstSubtitleCommand());
			Global.GUI.View.Selection.ActivatePath(); //TODO is this necessary?
		}
    }
    
    /// <summary>Shows the open dialog and possibly opens a subtitle.</summary>
    /// <remarks>If there's a document currently opened with unsaved changes, a warning dialog
    /// is shown before opening the new file.</remarks>
    public void Open () {
    	FileOpenDialog dialog = new FileOpenDialog();
    	dialog.Show();
    	bool toOpen = dialog.WaitForResponse();
    	if (toOpen && ToOpenAfterWarning) {
    		string filename = dialog.Filename;
    		int codePage = (dialog.HasChosenEncoding ? dialog.ChosenEncoding.CodePage : -1);
    		string videoFilename = dialog.VideoFilename;
    		Open(filename, codePage, videoFilename);
    	}
    }
        
    public void OpenVideo () {
    	VideoOpenDialog dialog = new VideoOpenDialog();
    	dialog.Show();
		bool toOpen = dialog.WaitForResponse();
		if (toOpen) {
			string filename = dialog.Filename;
			OpenVideo(filename);
		}
    }
    
    /// <summary>Executes a Save operation.</summary>
    /// <remarks>If the document hasn't been saved before, a SaveAs is executed.</remarks>
    /// <returns>Whether the file was saved or not.</returns>
    public bool Save () {
    	if (Global.Document.CanBeSaved) { //Check if document can be saved or needs a SaveAs
			Save(Global.Document.FileProperties);
			return true;
		}
		else
			return SaveAs();
    }

    /// <summary>Executes a SaveAs operation.</summary>
    /// <remarks>After saving, the timing mode is set to the timing mode of the subtitle format using when saving.</remarks>
    /// <returns>Whether the file was saved or not.</returns>
    public bool SaveAs () {
		FileSaveAsDialog dialog = Global.Dialogs.FileSaveAsDialog;
		dialog.Show();
		bool toSaveAs = dialog.WaitForResponse();
		if (toSaveAs) {
			string path = dialog.Filename;
			Encoding encoding = Encoding.GetEncoding(dialog.ChosenEncoding.CodePage);
			SubtitleType subtitleType = dialog.SubtitleType;			
			NewlineType newlineType = dialog.NewlineType;
			TimingMode timingMode = Global.TimingMode;

			FileProperties fileProperties = new FileProperties(path, encoding, subtitleType, timingMode, newlineType);
			Save(fileProperties);
			return true;
		}
		else
			return false;
	}

	public void UpdateWindowTitle (bool modified) {
		string prefix = (modified ? "*" : String.Empty);
		window.Title = prefix + Global.Document.FileProperties.Filename +
			" - " + Global.Execution.ApplicationName;
	}
	
	public void UpdateFromNewDocument (bool wasLoaded) {
	   	Global.CommandManager.Clear();

		UpdateWindowTitle(false);
		view.UpdateFromNewDocument(wasLoaded);
		edit.UpdateFromNewDocument(wasLoaded);
		menus.UpdateFromNewDocument(wasLoaded);
	}
	
	public void UpdateFromTimingMode (TimingMode mode) {
		view.UpdateFromTimingMode(mode);
		edit.UpdateFromTimingMode(mode);
		video.UpdateFromTimingMode(mode);
	}
	
	/// <summary>Updates the various parts of the GUI based on the current selection.</summary>
	public void UpdateFromSelection () {
		if (view.Selection.Count == 1)
			UpdateFromSelection(view.Selection.Subtitle);
		else
			UpdateFromSelection(view.Selection.Paths);
	}
	
	/// <summary>Updates the various parts of the GUI based on the current subtitle count.</summary>
	public void UpdateFromSubtitleCount () {
		int count = Global.Document.Subtitles.Collection.Count;
		menus.UpdateFromSubtitleCount(count);
	}

	
	/* Private members */
	
	/// <summary>Opens a subtitle file, given its filename, code page and video filename.</summary>
	/// <param name="path">The path of the subtitles file to open.</param>
	/// <param name="codePage">The code page of the filename. To use autodetection, set it to -1.</param>
	/// <param name="videoFilename">The videoFilename to open. If <see cref="String.Empty" />, no video will be opened.</param>
	/// <remarks>An error dialog is presented if an exception is caught during open.</remarks>
    private void Open (string path, int codePage, string videoFilename) {
    	try {
    		Encoding encoding = (codePage == -1 ? null : Encoding.GetEncoding(codePage));
    		
    		Global.Document = new Document(path, encoding);
    		Global.Document.UpdateTimingModeFromFileProperties();

			view.Selection.SelectFirst(); //TODO is this needed?
		
			if (videoFilename != String.Empty)
				OpenVideo(videoFilename);
		}
		catch (Exception exception) {
			SubtitleFileOpenErrorDialog errorDialog = new SubtitleFileOpenErrorDialog(path, exception);
			errorDialog.Show();
			bool toOpenAgain = errorDialog.WaitForResponse();
			if (toOpenAgain)
				Open();
		}
    }
    
    private void OpenVideo (string path) {
    	Menus.SetViewVideoActivity(true);
		try {
			Video.Open(path);
			Menus.SetVideoSensitivity(true);
		}
		catch (Exception exception) {
			Video.Close();
			VideoFileOpenErrorDialog errorDialog = new VideoFileOpenErrorDialog(path, exception);
			errorDialog.Show();
			bool toOpenAgain = errorDialog.WaitForResponse();
	    	if (toOpenAgain)
	    		OpenVideo();
		}
    }
    
    private void Save (FileProperties fileProperties) {
		try {
			Global.Document.Save(fileProperties);
			Global.CommandManager.WasModified = false;
			UpdateWindowTitle(false);
		}
		catch (Exception exception) {
			FileSaveErrorDialog errorDialog = new FileSaveErrorDialog(fileProperties.Path, exception);
			errorDialog.Show();
			bool toSaveAgain = errorDialog.WaitForResponse();
	    	if (toSaveAgain)
	    		SaveAs();			
		}
	}
	
	/// <summary>Executes a blank startup operation.</summary>
	/// <remarks>This is used when no document is loaded.</remarks>
	private void BlankStartUp () {
    	menus.BlankStartUp();
    	view.BlankStartUp();
    	edit.BlankStartUp();
    }
		
	/// <summary>Updates the GUI from the specified selected Subtitle.</summary>
	/// <param name="subtitle">The subtitle that is currently selected.</param>
	/// <remarks>This is only used when there is only one selected path. When there are zero or more than one
	/// paths selected, <see cref="UpdateFromSelection(TreePath[])" /> must be used.</remarks>
	private void UpdateFromSelection (Subtitle subtitle) {
		menus.UpdateFromSelection(subtitle);
		edit.UpdateFromSelection(subtitle);
		video.UpdateFromSelection(true);
	}

	/// <summary>Updates the GUI from the specified selected paths.</summary>
	/// <param name="paths">The paths from which the GUI should be updated.</param>
	/// <remarks>This is only used when there are either zero or more than one selected paths. When there is only
	/// one path selected, <see cref="UpdateFromSelection(Subtitle)" /> must be used.</remarks>
	private void UpdateFromSelection (TreePath[] paths) {
		menus.UpdateFromSelection(paths);
		edit.Enabled = false;
		video.UpdateFromSelection(false);
	}    

	/* Private properties */
	
	/// <summary>Whether there are unsaved changes.</summary>
	private bool ExistUnsavedChanges {
		get { return Global.CommandManager.WasModified; }
	}

	/// <summary>Whether the program should be closed, after choosing the respective confirmation dialog.</summary>
    private bool ToCloseAfterWarning {
    	get {
    		if (ExistUnsavedChanges) {
	    		SaveConfirmationDialog dialog = new SaveOnCloseConfirmationDialog();
    			return dialog.WaitForResponse();
    		}
    		else
	    		return true; 
		}
	}
    
    /// <summary>Whether a new document should be created, after choosing the respective confirmation dialog.</summary>
    private bool ToCreateNewAfterWarning {
    	get {
    		if (ExistUnsavedChanges) {
	    		SaveConfirmationDialog dialog = new SaveOnNewConfirmationDialog();
    			return dialog.WaitForResponse();
    		}
    		else
	    		return true; 
		}
	}
	
	/// <summary>Whether a document should be opened, after choosing the respective confirmation dialog.</summary>
	private bool ToOpenAfterWarning { //TODO turn this into a function
    	get {
    		if (ExistUnsavedChanges) {
	    		SaveConfirmationDialog dialog = new SaveOnOpenConfirmationDialog();
    			return dialog.WaitForResponse();
    		}
    		else
	    		return true; 
		}
	}

}

}
