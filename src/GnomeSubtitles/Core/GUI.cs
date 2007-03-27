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
	
	/* Constant strings */
	private const string gladeFilename = "MainWindow.glade";
	private const string iconFilename = "gnome-subtitles.png";
	
	public GUI (EventHandlers handlers, out Glade.XML glade) {
		glade = new Glade.XML(gladeFilename, null);
		
		window = glade.GetWidget("window") as Window;
		window.Icon = new Gdk.Pixbuf(null, iconFilename);
		window.SetDefaultSize(Global.Config.PrefsWindowWidth, Global.Config.PrefsWindowHeight);
		
		video = new Video();
		view = new SubtitleView();
		edit = new SubtitleEdit();
		menus = new Menus();

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

    
    /* Public Methods */
    
    /// <summary>Starts the GUI</summary>
    /// <remarks>A file is opened if it was specified as argument. If it wasn't, a blank start is performed.</summary>
    public void Start () {
    	if (ExecutionInfo.Args.Length > 0)
			Open(ExecutionInfo.Args[0], null, String.Empty);
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

	/// <summary>Creates a new subtitles document, corresponding it to the specified filename.</summary>
	/// <param name="filename">The subtitles' filename. If it's an empty string, 'Unsaved Subtitles' will be used instead.</param>
	/// <remarks>If there's a document currently opened with unsaved changes, a warning dialog is shown.</remarks>
    public void New (string filename) {
    	if (!ToCreateNewAfterWarning)
    		return;

		if (filename == String.Empty)
			filename = "Unsaved Subtitles";

   		SubtitleFactory factory = new SubtitleFactory();
		factory.Verbose = true;
		Subtitles subtitles = new Subtitles(factory.NewWithName(filename));
		
   		NewDocument(subtitles, TimingMode.Times);

		if (subtitles.Count == 0) {
			Global.CommandManager.Execute(new InsertFirstSubtitleCommand());
			Global.GUI.View.Selection.ActivatePath();
		}
    }
    
    /// <summary>Shows the open dialog and possibly opens a subtitle.</summary>
    /// <remarks>If there's a document currently opened with unsaved changes, a warning dialog
    /// is shown before opening the new file.</remarks>
    public void Open () {
    	FileOpenDialog dialog = new FileOpenDialog();
    	bool toOpen = dialog.WaitForResponse();
    	if (toOpen && ToOpenAfterWarning) {
    		string filename = dialog.Filename;
    		try {
    			Encoding encoding = null;
    			if (dialog.HasChosenEncoding)
    				encoding = Encoding.GetEncoding(dialog.ChosenEncoding.CodePage);

				Open(filename, encoding, dialog.VideoFilename);
	    	}
	    	catch (Exception exception) {
	    		SubtitleFileOpenErrorDialog errorDialog = new SubtitleFileOpenErrorDialog(filename, exception);
	    		bool toOpenAgain = errorDialog.WaitForResponse();
	    		if (toOpenAgain)
	    			Open(); //Recursive call to open the dialog again
	    	}
    	}
    }
    
    public void OpenVideo () {
    	VideoOpenDialog dialog = new VideoOpenDialog();
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
    	if (Global.Subtitles.CanSave) { //Check if document can be saved or needs a SaveAs
			Global.Subtitles.Save();
			Global.CommandManager.WasModified = false;
			UpdateWindowTitle(false);
			return true;
		}
		else
			return SaveAs();
    }
    
    /// <summary>Executes a SaveAs operation.</summary>
    /// <remarks>After saving, the timing mode is set to the timing mode of the subtitle format using when saving.</remarks>
    /// <returns>Whether the file was saved or not.</returns>
    public bool SaveAs () {
		FileSaveAsDialog dialog = new FileSaveAsDialog();
		bool toSaveAs = dialog.WaitForResponse();
		if (toSaveAs) {
			string filename = dialog.Filename;
			SubtitleType type = dialog.SubtitleType;
			Encoding encoding = Encoding.GetEncoding(dialog.ChosenEncoding.CodePage);
			Global.Subtitles.SaveAs(filename, type, encoding);
			TimingMode newTimingMode = Global.Subtitles.Properties.TimingMode; 
			Global.TimingMode = newTimingMode;
			Global.CommandManager.WasModified = false;
			UpdateWindowTitle(false);
			OnToggleTimingMode(newTimingMode);
			return true;
		}
		else
			return false;
	}
	
	public void UpdateWindowTitle (bool modified) {
		string prefix = (modified ? "*" : String.Empty);
		window.Title = prefix + Global.Subtitles.Properties.FileName +
			" - " + ExecutionInfo.ApplicationName;
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
		int count = Global.Subtitles.Collection.Count;
		menus.UpdateFromSubtitleCount(count);
	}

	
	/* Private members */
	
	/// <summary>Opens a subtitle file, given its filename and encoding.</summary>
	/// <param name="filename">The filename to open.</param>
	/// <param name="encoding">The encoding of the filename. To use autodetection, set it to null.</param>
	/// <param name="videoFilename">The videoFilename to open. If <see cref="String.Empty" />, no video will be opened.</param>
    private void Open (string filename, Encoding encoding, string videoFilename) {
		SubtitleFactory factory = new SubtitleFactory();
		factory.Verbose = true;
		if (encoding != null)
			factory.Encoding = encoding;

		SubLib.Subtitles openedSubtitles = null;
		try {
			openedSubtitles = factory.Open(filename);
		}
		catch (FileNotFoundException) {
			New(filename);
			return;
		}
		Subtitles subtitles = new Subtitles(openedSubtitles);
		TimingMode timingMode = subtitles.Properties.TimingMode;

    	NewDocument(subtitles, timingMode);
		view.Selection.SelectFirst();
		
		if (videoFilename != String.Empty)
			OpenVideo(videoFilename);
    }
    
    private void OpenVideo (string filename) {
    	Menus.SetViewVideoActivity(true);
		try {
			Video.Open(filename);
			Menus.SetVideoSensitivity(true);
		}
		catch (Exception exception) {
			Video.Close();
			VideoFileOpenErrorDialog errorDialog = new VideoFileOpenErrorDialog(filename, exception);
			bool toOpenAgain = errorDialog.WaitForResponse();
	    	if (toOpenAgain)
	    		OpenVideo(); //Recursive call to open the dialog again
		}
    }
	
	/// <summary>Executes a blank startup operation.</summary>
	/// <remarks>This is used when no document is loaded.</remarks>
	private void BlankStartUp () {
    	menus.BlankStartUp();
    	view.BlankStartUp();
    	edit.BlankStartUp();
    }
     
    /// <summary>Sets the window for a new document.</summary>
    /// <remarks>A new document can be the result of Open or New operations.</remarks>
	private void NewDocument (Subtitles subtitles, TimingMode mode) {
   		bool wasLoaded = Global.AreSubtitlesLoaded;
   		
   		Global.CommandManager.Clear();
   		Global.Subtitles = subtitles;
   		Global.TimingMode = mode;

		UpdateWindowTitle(false);
		menus.NewDocument(wasLoaded);
		view.NewDocument(wasLoaded);
		edit.NewDocument(wasLoaded);
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
	private bool ToOpenAfterWarning {
    	get {
    		if (ExistUnsavedChanges) {
	    		SaveConfirmationDialog dialog = new SaveOnOpenConfirmationDialog();
    			return dialog.WaitForResponse();
    		}
    		else
	    		return true; 
		}
	}
	
	
	/* Event members */
	
	public void OnToggleTimingMode (TimingMode newMode) {
		Global.TimingMode = newMode;
		view.ToggleTimingMode(newMode);
		edit.ToggleTimingMode(newMode);
		video.ToggleTimingMode(newMode);
	}

}

}
