/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2008 Pedro Castro
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
using GnomeSubtitles.Core.Command;
using GnomeSubtitles.Dialog;
using GnomeSubtitles.Ui.Edit;
using GnomeSubtitles.Ui.VideoPreview;
using GnomeSubtitles.Ui.View;
using Gtk;
using Mono.Unix;
using SubLib.Exceptions;
using SubLib.Core.Domain;
using System;
using System.IO;
using System.Text;

namespace GnomeSubtitles.Ui {

public class MainUi {
	private Window window = null;
	private Menus menus = null;
	private Video video = null;
	private SubtitleView view = null;
	private SubtitleEdit edit = null;
	private Status status = null;
	
	/* Constant strings */
	private const string gladeFilename = "MainWindow.glade";
	private const string iconFilename = "gnome-subtitles.png";
	
	public MainUi (EventHandlers handlers, out Glade.XML glade) {
		glade = new Glade.XML(null, gladeFilename, null, Base.ExecutionContext.TranslationDomain);
		
		window = glade.GetWidget("window") as Window;
		window.Icon = new Gdk.Pixbuf(null, iconFilename);
		window.SetDefaultSize(Base.Config.PrefsWindowWidth, Base.Config.PrefsWindowHeight);
		
		video = new Video();
		view = new SubtitleView();
		edit = new SubtitleEdit();
		menus = new Menus();
		status = new Status();

		glade.Autoconnect(handlers);
		
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
    	string[] args = Base.ExecutionContext.Args;
    	if (args.Length > 0) {
    		string subtitleFile = args[0];
    		string videoFile = Base.Config.PrefsVideoAutoChooseFile ? VideoFiles.FindMatchingVideo(subtitleFile) : String.Empty;
			Open(subtitleFile, -1, videoFile);
		}
		else
			BlankStartUp();
    }
    
    /// <summary>Quits the application.</summary>
    public void Quit () {
		if (ToCloseAfterWarning()) {
			video.Quit();
			Base.Quit();
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
    	if (!ToCreateNewAfterWarning())
    		return;

		if (path == String.Empty) {
			//To translators: this is the filename for new files (before being saved)
			path = Catalog.GetString("Unsaved Subtitles");
		}

		Base.CreateDocumentNew(path);

		if (Base.Document.Subtitles.Count == 0) {
			Base.CommandManager.Execute(new InsertFirstSubtitleCommand());
		}
    }
    
    /// <summary>Shows the open dialog and possibly opens a subtitle.</summary>
    /// <remarks>If there's a document currently open with unsaved changes, a warning dialog
    /// is shown before opening the new file.</remarks>
    public void Open () {
    	FileOpenDialog dialog = new FileOpenDialog();
    	dialog.Show();
    	bool toOpen = dialog.WaitForResponse();
    	if (toOpen && ToOpenAfterWarning()) {
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
			string videoUri = dialog.Uri;
			OpenVideo(videoUri);
		}
    }
    
    public void CloseVideo () {
    	Video.Close();
    	UpdateFromCloseVideo();
    }
    
    /// <summary>Executes a Save operation.</summary>
    /// <remarks>If the document hasn't been saved before, a SaveAs is executed.</remarks>
    /// <returns>Whether the file was saved or not.</returns>
    public bool Save () {
    	if (Base.Document.CanTextBeSaved) { //Check if document can be saved or needs a SaveAs
			Save(Base.Document.TextFile);
			return true;
		}
		else
			return SaveAs();
    }

    /// <summary>Executes a SaveAs operation.</summary>
    /// <remarks>After saving, the timing mode is set to the timing mode of the subtitle format using when saving.</remarks>
    /// <returns>Whether the file was saved or not.</returns>
    public bool SaveAs () {
		FileSaveAsDialog dialog = Base.Dialogs.FileSaveAsDialog;
		FileProperties properties = ShowSaveAsDialog(dialog);
		if (properties != null) {
			Save(properties);
			return true;
		}
		else
			return false;
	}
	
	/// <summary>Starts a new translation.</summary>
	/// <remarks>If there's a translation open with unsaved changes, a warning dialog is shown.</remarks>
    public void TranslationNew () {
    	if (!ToCreateNewTranslationAfterWarning())
    		return;

		Base.Document.NewTranslation();
    }
    
    /// <summary>Shows the open translation dialog and possibly opens a file.</summary>
    /// <remarks>If there's a translation currently open with unsaved changes, a warning dialog
    /// is shown before opening the new file.</remarks>
    public void TranslationOpen () {
    	FileOpenDialog dialog = new FileTranslationOpenDialog();
    	dialog.Show();
    	bool toOpen = dialog.WaitForResponse();
    	if (toOpen && ToOpenTranslationAfterWarning()) {
    		string filename = dialog.Filename;
    		int codePage = (dialog.HasChosenEncoding ? dialog.ChosenEncoding.CodePage : -1);
    		OpenTranslation(filename, codePage);
    	}
    }
    
    /// <summary>Executes a Save operation regarding the translation.</summary>
    /// <remarks>If the translation hasn't been saved before, a TranslationSaveAs is executed.</remarks>
    /// <returns>Whether the translation file was saved or not.</returns>
    public bool TranslationSave () {
    	if (Base.Document.CanTranslationBeSaved) { //Check if translation can be saved or needs a SaveAs
			SaveTranslation(Base.Document.TranslationFile);
			return true;
		}
		else
			return TranslationSaveAs();
    }
    
    /// <summary>Executes a translation SaveAs operation.</summary>
    /// <returns>Whether the translation file was saved or not.</returns>
    public bool TranslationSaveAs () {
		FileSaveAsDialog dialog = Base.Dialogs.TranslationSaveAsDialog;
		FileProperties properties = ShowSaveAsDialog(dialog);
		if (properties != null) {
			SaveTranslation(properties);
			return true;
		}
		else
			return false;
	}

    /// <summary>Closes a translation.</summary>
	/// <remarks>If there's a translation open with unsaved changes, a warning dialog is shown.</remarks>
    public void TranslationClose () {
    	if (!ToCloseTranslationAfterWarning())
    		return;

		Base.Document.CloseTranslation();
    }

	public void UpdateFromDocumentModified (bool modified) {
		string prefix = (modified ? "*" : String.Empty);
		window.Title = prefix + Base.Document.TextFile.Filename +
			" - " + Base.ExecutionContext.ApplicationName;
	}
	
	public void UpdateFromNewDocument (bool wasLoaded) {
		view.UpdateFromNewDocument(wasLoaded);
		edit.UpdateFromNewDocument(wasLoaded);
		menus.UpdateFromNewDocument(wasLoaded);
		video.UpdateFromNewDocument(wasLoaded);
	}
	
	public void UpdateFromNewTranslationDocument () {
		view.UpdateFromNewTranslationDocument();
		edit.UpdateFromNewTranslationDocument();
		menus.UpdateFromNewTranslationDocument();
	}
	
	public void UpdateFromCloseTranslation () {
		view.UpdateFromCloseTranslation();
		edit.UpdateFromCloseTranslation();
		menus.UpdateFromCloseTranslation();
	}
	
	public void UpdateFromTimingMode (TimingMode mode) {
		view.UpdateFromTimingMode(mode);
		edit.UpdateFromTimingMode(mode);
		video.UpdateFromTimingMode(mode);
	}
	
	public void UpdateFromOpenVideo () {
		menus.UpdateFromOpenVideo();
	}
	
	public void UpdateFromCloseVideo () {
		menus.UpdateFromCloseVideo();
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
		int count = Base.Document.Subtitles.Collection.Count;
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
    		Encoding encoding =  CodePageToEncoding(codePage);
    		Base.CreateDocumentOpen(path, encoding);
			view.Selection.SelectFirst();
		
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
    
    /// <summary>Creates an <see cref="Encoding" /> from a code page.</summary>
    /// <param name="codePage">The code page.</param>
    /// <returns>The respective <see cref="Encoding" />, or null if codePage == -1.</returns>
    /// <exception cref="EncodingNotSupportedException">Thrown if a detected encoding is not supported by the platform.</exception>
    private Encoding CodePageToEncoding (int codePage) {
    	if (codePage == -1)
    		return null;

    	try {
    		return Encoding.GetEncoding(codePage);
    	}
    	catch (NotSupportedException) {
    		throw new EncodingNotSupportedException();
    	}
    }

    /// <summary>Opens a translation file, given its filename and code page.</summary>
	/// <param name="path">The path of the translation file to open.</param>
	/// <param name="codePage">The code page of the filename. To use autodetection, set it to -1.</param>
	/// <remarks>An error dialog is presented if an exception is caught during open.</remarks>
    private void OpenTranslation (string path, int codePage) {
    	try {
    		Encoding encoding = (codePage == -1 ? null : Encoding.GetEncoding(codePage));
    		Base.Document.OpenTranslation(path, encoding);
		}
		catch (Exception exception) {
			SubtitleFileOpenErrorDialog errorDialog = new SubtitleFileOpenErrorDialog(path, exception);
			errorDialog.Show();
			bool toOpenAgain = errorDialog.WaitForResponse();
			if (toOpenAgain)
				Open();
		}
    }
    
    private void OpenVideo (string videoUriString) {
    	Menus.SetViewVideoActivity(true);
    	Uri videoUri = null;
		try {
			videoUri = new Uri(videoUriString);
			Video.Open(videoUri);
			UpdateFromOpenVideo();
		}
		catch (Exception exception) {
			Video.Close();
			VideoFileOpenErrorDialog errorDialog = new VideoFileOpenErrorDialog(videoUri, exception);
			errorDialog.Show();
			bool toOpenAgain = errorDialog.WaitForResponse();
	    	if (toOpenAgain)
	    		OpenVideo();
		}
    }
    
    private void Save (FileProperties fileProperties) {
		try {
			Base.Document.Save(fileProperties);
		}
		catch (Exception exception) {
			FileSaveErrorDialog errorDialog = new FileSaveErrorDialog(fileProperties.Path, exception);
			errorDialog.Show();
			bool toSaveAgain = errorDialog.WaitForResponse();
	    	if (toSaveAgain)
	    		SaveAs();			
		}
	}
	
	private void SaveTranslation (FileProperties fileProperties) {
		try {
			Base.Document.SaveTranslation(fileProperties);
		}
		catch (Exception exception) {
			FileSaveErrorDialog errorDialog = new FileSaveErrorDialog(fileProperties.Path, exception);
			errorDialog.Show();
			bool toSaveAgain = errorDialog.WaitForResponse();
	    	if (toSaveAgain)
	    		TranslationSaveAs();			
		}
	}
	
	/// <summary>Displays a SaveAs dialog and gets the chosen options as <cref="FileProperties" />.</summary>
	/// <param name="dialog">The dialog to display.</param>
	/// <returns>The chosen file properties, or null in case SaveAs was canceled.</returns>
	private FileProperties ShowSaveAsDialog (FileSaveAsDialog dialog) {
		dialog.Show();
		bool toSaveAs = dialog.WaitForResponse();
		if (!toSaveAs)
			return null;
		
		string path = dialog.Filename;
		Encoding encoding = Encoding.GetEncoding(dialog.ChosenEncoding.CodePage);
		SubtitleType subtitleType = dialog.SubtitleType;			
		NewlineType newlineType = dialog.NewlineType;
		TimingMode timingMode = Base.TimingMode;
		return new FileProperties(path, encoding, subtitleType, timingMode, newlineType);
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

	/// <summary>Whether there are unsaved normal changes.</summary>
	private bool ExistTextUnsavedChanges () {
		return Base.IsDocumentLoaded && Base.Document.WasTextModified;
	}
	
	/// <summary>Whether there are unsaved translation changes.</summary>
	private bool ExistTranslationUnsavedChanges () {
		return Base.IsDocumentLoaded && Base.Document.IsTranslationLoaded && Base.Document.WasTranslationModified;
	}

	/// <summary>Whether the program should be closed, after choosing the respective confirmation dialog.</summary>
    private bool ToCloseAfterWarning () {
    	bool toCreate = true;
    	if (ExistTextUnsavedChanges()) {
	    	SaveConfirmationDialog subtitleDialog = new SaveSubtitlesOnCloseFileConfirmationDialog();
    		toCreate = subtitleDialog.WaitForResponse();
    	}
    	if (toCreate && ExistTranslationUnsavedChanges()) {
   			SaveConfirmationDialog translationDialog = new SaveTranslationOnCloseConfirmationDialog();
   			toCreate = translationDialog.WaitForResponse();
   		}
    	return toCreate; 
	}

    /// <summary>Whether a new document should be created, after choosing the respective confirmation dialog.</summary>
    private bool ToCreateNewAfterWarning () {
    	bool toCreate = true;
   		if (ExistTextUnsavedChanges()) {
    		SaveConfirmationDialog subtitleDialog = new SaveSubtitlesOnNewFileConfirmationDialog();
   			toCreate = subtitleDialog.WaitForResponse();
   		}
   		if (toCreate && ExistTranslationUnsavedChanges()) {
   			SaveConfirmationDialog translationDialog = new SaveTranslationOnNewFileConfirmationDialog();
   			toCreate = translationDialog.WaitForResponse();
   		}
   		return toCreate;
	}
	
	/// <summary>Whether a document should be opened, after choosing the respective confirmation dialog.</summary>
	private bool ToOpenAfterWarning () {
   		bool toCreate = true;
   		if (ExistTextUnsavedChanges()) {
    		SaveConfirmationDialog subtitleDialog = new SaveSubtitlesOnOpenFileConfirmationDialog();
   			toCreate = subtitleDialog.WaitForResponse();
   		}
   		if (toCreate && ExistTranslationUnsavedChanges()) {
   			SaveConfirmationDialog translationDialog = new SaveTranslationOnOpenConfirmationDialog();
   			toCreate = translationDialog.WaitForResponse();
   		}
   		return toCreate;
	}

	/// <summary>Whether a new translation should be created, after choosing the respective confirmation dialog.</summary>
    private bool ToCreateNewTranslationAfterWarning () {
   		if (ExistTranslationUnsavedChanges()) {
    		SaveConfirmationDialog dialog = new SaveTranslationOnNewTranslationConfirmationDialog();
   			return dialog.WaitForResponse();
   		}
   		else
    		return true; 
	}
	
	/// <summary>Whether a translation should be opened, after choosing the respective confirmation dialog.</summary>
	private bool ToOpenTranslationAfterWarning () {
   		if (ExistTranslationUnsavedChanges()) {
    		SaveConfirmationDialog dialog = new SaveTranslationOnOpenConfirmationDialog();
   			return dialog.WaitForResponse();
   		}
   		else
    		return true; 
	}
	
	/// <summary>Whether a translation should be closed, after choosing the respective confirmation dialog.</summary>
    private bool ToCloseTranslationAfterWarning () {
   		if (ExistTranslationUnsavedChanges()) {
    		SaveConfirmationDialog dialog = new SaveTranslationOnCloseConfirmationDialog();
   			return dialog.WaitForResponse();
   		}
   		else
    		return true; 
	}

}

}
