/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2021 Pedro Castro
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
using GnomeSubtitles.Dialog;
using GnomeSubtitles.Dialog.Message;
using GnomeSubtitles.Ui.Edit;
using GnomeSubtitles.Ui.VideoPreview;
using GnomeSubtitles.Ui.View;
using Gtk;
using SubLib.Core.Domain;
using SubLib.Exceptions;
using SubLib.Util;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace GnomeSubtitles.Ui {

public class MainUi {
	private static Builder builder = null;

	private Window window = null;
	private WindowState windowState = null;
	private Menus menus = null;
	private Video video = null;
	private SubtitleView view = null;
	private SubtitleEdit edit = null;
	private Status status = null;

	/* Constant strings */
	private const string uiResourceName = "MainWindow.ui";

	public MainUi (EventHandlers handlers) {
		builder = ReadUIContent(uiResourceName, Base.ExecutionContext.TranslationDomain);
		
		window = builder.GetObject("window") as Window;
		
		/* Setting the iconName and wmClass (app name) name is not necessary if a standard desktop environment is
		 * executing the application, in which case this information is obtained from the .desktop file. This is
		 * here just in case a non-standard environment is in place.
		 */
		window.IconName = Base.ExecutionContext.IconName;
		window.SetWmclass(Base.ExecutionContext.ApplicationName, Base.ExecutionContext.ApplicationName);
		
		windowState = new WindowState(Base.Config.ViewWindowWidth, Base.Config.ViewWindowHeight);
		window.SetDefaultSize(windowState.Width, windowState.Height);
		
		Base.ExecutionContext.Application.AddWindow(window);

		video = new Video();
		view = new SubtitleView();
		edit = new SubtitleEdit();
		menus = new Menus();
		status = new Status();

		builder.Autoconnect(handlers);
		Base.InitFinished += OnBaseInitFinished;
    }


	/* Public properties */

	public Window Window {
		get { return window; }
	}
	
	public WindowState WindowState {
		get { return windowState; }
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

	public static Widget GetWidget (string name) {
		return builder.GetObject(name) as Widget;
	}

	public void Show () {
		window.Visible = true;
	}

	/// <summary>Starts the GUI</summary>
	/// <remarks>A file is opened if it was specified as argument. If it wasn't, a blank start is performed.</summary>
	public void Start () {
		string subtitleFilePath = GetSubtitleFileArg(Base.ExecutionContext.Args);
		if (subtitleFilePath != null) {
			Uri videoUri = Base.Config.VideoAutoChooseFile ? VideoFiles.FindMatchingVideo(subtitleFilePath) : null;
			int codePage = GetFileOpenCodePageFromConfig();
			Open(subtitleFilePath, codePage, videoUri);
		}
    }

    /// <summary>Closes the open file.</summary>
    public void Close () {
		if (ToCloseAfterWarning())
			Base.CloseDocument();
    }

    /// <summary>Quits the application.</summary>
    public bool Quit () {
		if (ToCloseAfterWarning()) {
			Video.Quit();
			window.Destroy();
		}
		
		return false;
    }

    /// <summary>Creates a new subtitles document for the specified path.</summary>
    /// <param name="path">The subtitles' filename. If it's an empty string, 'Unsaved Subtitles' will be used instead.</param>
    /// <remarks>If there's a document open with unsaved changes, a warning dialog is shown.</remarks>
    public void New () {
    		if (ToCreateNewAfterWarning()) {
			Base.NewDocument();
		}
    }

    /// <summary>Shows the open dialog and possibly opens a subtitle.</summary>
    /// <remarks>If there's a document currently open with unsaved changes, a warning dialog
    /// is shown before opening the new file.</remarks>
    public void Open () {
		FileOpenDialog dialog = Base.Dialogs.Get(typeof(FileOpenDialog)) as FileOpenDialog;
    	dialog.Show();
    	bool gotOpenResponse = dialog.WaitForResponse();
    	if (gotOpenResponse && ToOpenAfterWarning()) {
    		string filename = dialog.Filename;
    		int codePage = (dialog.Encoding.Equals(EncodingDescription.Empty) ? -1 : dialog.Encoding.CodePage);
    		Uri videoUri = dialog.VideoUri;
    		Open(filename, codePage, videoUri);
    	}
    }

	/// <summary>Opens a subtitle.</summary>
	/// <param name="filename">The path of the subtitles file to open.</param>
    /// <remarks>If there's a document currently open with unsaved changes, a warning dialog
    /// is shown before opening the new file.</remarks>
    public void Open (string filename) {
		if (ToOpenAfterWarning()) {
			Open(filename, -1, null);
		}
    }

    public void OpenVideo () {
    	VideoOpenDialog dialog = Base.Dialogs.Get(typeof(VideoOpenDialog)) as VideoOpenDialog;
    	dialog.Show();
		bool toOpen = dialog.WaitForResponse();
		if (toOpen) {
			Base.OpenVideo(dialog.Uri);
		}
    }

    /// <summary>Executes a Save operation.</summary>
    /// <remarks>If the document hasn't been saved before, a SaveAs is executed.</remarks>
    /// <returns>Whether the file was saved.</returns>
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
    /// <returns>Whether the file was saved.</returns>
    public bool SaveAs () {
		FileSaveDialog dialog = Base.Dialogs.Get(typeof(FileSaveDialog)) as FileSaveDialog;
		FileProperties properties = ShowSaveAsDialog(dialog);
		if (properties != null) {
			Save(properties);
			return true;
		}
		
		return false;
	}

	/// <summary>Starts a new translation.</summary>
	/// <remarks>If there's a translation open with unsaved changes, a warning dialog is shown.</remarks>
    public void TranslationNew () {
    	if (!ToCreateNewTranslationAfterWarning())
    		return;

		Base.NewTranslation();
    }

    /// <summary>Shows the open translation dialog and possibly opens a file.</summary>
    /// <remarks>If there's a translation currently open with unsaved changes, a warning dialog
    /// is shown before opening the new file.</remarks>
    public void TranslationOpen () {
    	FileOpenDialog dialog = Base.Dialogs.Get(typeof(FileTranslationOpenDialog)) as FileOpenDialog;
    	dialog.Show();
    	bool toOpen = dialog.WaitForResponse();
    	if (toOpen && ToOpenTranslationAfterWarning()) {
    		string filename = dialog.Filename;
    		int codePage = (dialog.Encoding.Equals(EncodingDescription.Empty) ? -1 : dialog.Encoding.CodePage);
    		OpenTranslation(filename, codePage);
    	}
    }

	/// <summary>Reloads the current translation file.</summary>
	/// <remarks>If there's a translation currently open with unsaved changes, a warning dialog
	/// is shown before opening the new file.</remarks>
	public void TranslationReload () {
		if (ToOpenTranslationAfterWarning()) {
			OpenTranslation(Base.Document.TranslationFile.Path, Base.Document.TranslationFile.Encoding.CodePage);
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
		FileTranslationSaveDialog dialog = Base.Dialogs.Get(typeof(FileTranslationSaveDialog)) as FileTranslationSaveDialog;
		FileProperties properties = ShowSaveAsDialog(dialog);
		if (properties != null) {
			SaveTranslation(properties);
			return true;
		}

		return false;
	}

    /// <summary>Closes a translation.</summary>
	/// <remarks>If there's a translation open with unsaved changes, a warning dialog is shown.</remarks>
    public void TranslationClose () {
    	if (!ToCloseTranslationAfterWarning())
    		return;

		Base.CloseTranslation();
    }


	/* Private members */

	/// <summary>Opens a subtitle file, given its filename, code page and video filename.</summary>
	/// <param name="path">The path of the subtitles file to open.</param>
	/// <param name="codePage">The code page of the filename. To use autodetection, set it to -1.</param>
	/// <param name="videoUri">The URI of the video to open. If null, no video will be opened.</param>
	/// <remarks>An error dialog is presented if an exception is caught during open.</remarks>
    private void Open (string path, int codePage, Uri videoUri) {
    	try {
    		Encoding encoding = CodePageToEncoding(codePage);
    		Base.Open(path, encoding, videoUri);
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
    //TODO move to util
    private Encoding CodePageToEncoding (int codePage) {
    	if (codePage == -1)
    		return null;

    	try {
    		return Encodings.GetEncoding(codePage);
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
    		Encoding encoding = (codePage == -1 ? null : Encodings.GetEncoding(codePage));
    		Base.OpenTranslation(path, encoding);
		}
		catch (Exception exception) {
			SubtitleFileOpenErrorDialog errorDialog = new SubtitleFileOpenErrorDialog(path, exception);
			errorDialog.Show();
			bool toOpenAgain = errorDialog.WaitForResponse();
			if (toOpenAgain)
				Open();
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
	    	if (toSaveAgain) {
	    		SaveAs();
				return;
			}
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
	private FileProperties ShowSaveAsDialog (FileSaveDialog dialog) {
		dialog.Show();
		bool toSaveAs = dialog.WaitForResponse();
		if (!toSaveAs)
			return null;

		string path = dialog.Filename;
		Encoding encoding = Encodings.GetEncoding(dialog.Encoding.CodePage);
		SubtitleType subtitleType = dialog.SubtitleType;
		NewlineType newlineType = dialog.NewlineType;
		TimingMode timingMode = Base.TimingMode;
		return new FileProperties(path, encoding, subtitleType, timingMode, newlineType);
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

		return true;
	}

	/// <summary>Whether a translation should be opened, after choosing the respective confirmation dialog.</summary>
	private bool ToOpenTranslationAfterWarning () {
   		if (ExistTranslationUnsavedChanges()) {
    		SaveConfirmationDialog dialog = new SaveTranslationOnOpenConfirmationDialog();
   			return dialog.WaitForResponse();
   		}

		return true;
	}

	/// <summary>Whether a translation should be closed, after choosing the respective confirmation dialog.</summary>
    private bool ToCloseTranslationAfterWarning () {
   		if (ExistTranslationUnsavedChanges()) {
    		SaveConfirmationDialog dialog = new SaveTranslationOnCloseConfirmationDialog();
   			return dialog.WaitForResponse();
   		}

		return true;
	}

	//TODO use a header bar to show the title and subtitle
	private void UpdateTitleModificationStatus (bool showFilename, bool modified) {
		if (showFilename) {
			window.Title = (modified ? "*" : "") + Base.Document.TextFile.Filename;
		} else {
			window.Title = Base.ExecutionContext.ApplicationName;
		}
	}

	private int GetFileOpenCodePageFromConfig () {
		switch (Base.Config.FileOpenEncoding) {
			case ConfigFileOpenEncoding.CurrentLocale: return Encodings.SystemDefault.CodePage;
			case ConfigFileOpenEncoding.Fixed:
				string encodingCode = Base.Config.FileOpenEncodingFixed;
				EncodingDescription encodingDescription = EncodingDescription.Empty;
				Encodings.Find(encodingCode, ref encodingDescription);
				return encodingDescription.CodePage;
			default: return -1; //Also accounts for Auto Detect
		}
	}

	private string GetSubtitleFileArg (string[] args) {
		if (args.Length == 0) {
			return null;
		}

		string file = args[0];
		if (file == null) {
			return null;
		}

		if (!Path.IsPathRooted(file)) {
			try {
				file = Path.GetFullPath(file);
			}
			catch (Exception e) {
				Logger.Error(e, "Unable to read subtitle file path from argument 0");
				return null;
			}
		}

		return file;
	}
	
	private Builder ReadUIContent (string uiResourceName, string translationDomain) {
		string content;
	
		using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(uiResourceName)) {
			using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
				content = reader.ReadToEnd();
			}
		}
		
		if (String.IsNullOrEmpty(content)) {
			throw new Exception("Unable to read UI content from resource " + uiResourceName);
		}

		Builder builder = new Builder();
		builder.TranslationDomain = translationDomain;
		builder.AddFromString(content);

		//Note: this doesn't work anymore. The translation domain needs to be set *before* loading the UI, but
		//this gtk-sharp method is doing it *afterwards*.
		//builder = new Builder(uiResourceName, Base.ExecutionContext.TranslationDomain);

		return builder;
	}


	/* Event members */

	private void OnBaseInitFinished () {
		Base.DocumentLoaded += OnBaseDocumentLoaded;
		Base.DocumentUnloaded += OnBaseDocumentUnloaded;
	}

	private void OnBaseDocumentLoaded (Document document) {
		UpdateTitleModificationStatus(true, false);
   		document.ModificationStatusChanged += OnBaseDocumentModificationStatusChanged;
    }

    private void OnBaseDocumentUnloaded (Document document) {
    	if (document != null) {
    		document.ModificationStatusChanged -= OnBaseDocumentModificationStatusChanged;
    	}
    	UpdateTitleModificationStatus(false, false);
    }

    private void OnBaseDocumentModificationStatusChanged (bool modified) {
    	UpdateTitleModificationStatus(true, modified);
	}


}

}
