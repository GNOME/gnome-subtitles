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
using GnomeSubtitles.Ui;
using GnomeSubtitles.Ui.Component;
using GnomeSubtitles.Ui.VideoPreview;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;
using System.Collections;
using System.IO;
using SubLib.Util;

namespace GnomeSubtitles.Dialog {

public class FileOpenDialog : BaseDialog {
	
	private string chosenFilename = String.Empty;
	private EncodingDescription chosenEncoding = EncodingDescription.Empty;
	
	private ArrayList videoFiles = null; //The full paths of the video files in the current dir
	private ArrayList videoFilenames = null; //The filenames of videoFiles, without extensions
	private Uri chosenVideoUri = null;
	private bool autoChooseVideoFile = true;

	/* Components */
	private EncodingComboBox encodingComboBox = null;

	/* Widgets */
	private ComboBoxText videoComboBox = null;
	private Label videoLabel = null;


	public FileOpenDialog () : this(true, Catalog.GetString("Open File")) {
	}

	protected FileOpenDialog (bool showVideo, string title) : base() {
		base.Init(BuildDialog(showVideo, title));
	}


	/* Overridden members */

	public override DialogScope Scope {
		get { return DialogScope.Singleton; }
	}


	/* Public properties */

	public EncodingDescription Encoding {
		get { return chosenEncoding; }
	}

	public string Filename {
		get { return chosenFilename; }
	}

	public bool HasVideoFilename {
		get { return chosenVideoUri != null; }
	}

	public Uri VideoUri {
		get { return chosenVideoUri; }
	}


	/* Protected members */

	protected virtual string GetStartFolder () {
		if (Base.IsDocumentLoaded && Base.Document.TextFile.IsPathRooted) {
			return Base.Document.TextFile.Directory;
		} else {
			return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		}
	}


	/* Private members */

	private FileChooserDialog BuildDialog(bool showVideo, string title) {
		FileChooserDialog dialog = new FileChooserDialog(title, Base.Ui.Window, FileChooserAction.Open,
			Util.GetStockLabel("gtk-cancel"), ResponseType.Cancel, Util.GetStockLabel("gtk-open"), ResponseType.Ok);

		dialog.DefaultResponse = ResponseType.Ok;

		//Build content area

		Box box = new Box(Orientation.Horizontal, WidgetStyles.BoxSpacingMedium);
		box.BorderWidth = WidgetStyles.BorderWidthMedium;
		
		if (showVideo) {
			videoLabel = new Label(Catalog.GetString("Video to Open:"));
			box.Add(videoLabel);
		
			videoComboBox = new ComboBoxText();
			videoComboBox.Hexpand = true;

			CellRendererText videoComboBoxRenderer = (videoComboBox.Cells[0] as CellRendererText);
			videoComboBoxRenderer.WidthChars = 20;
			videoComboBoxRenderer.Ellipsize = Pango.EllipsizeMode.End;
			videoComboBox.RowSeparatorFunc = ComboBoxUtil.SeparatorFunc;
			box.Add(videoComboBox);
			
			autoChooseVideoFile = Base.Config.VideoAutoChooseFile;
		}

		box.Add(new Label(Catalog.GetString("Character Encoding:")));

		encodingComboBox = BuildEncodingComboBox();
		encodingComboBox.Widget.Hexpand = !showVideo;
		box.Add(encodingComboBox.Widget);

		dialog.ContentArea.Add(box);
		dialog.ContentArea.ShowAll();

		//Other stuff

		SetFilters(dialog);
		dialog.SetCurrentFolder(GetStartFolder());

		if (showVideo) {
			dialog.CurrentFolderChanged += OnCurrentFolderChanged;
			dialog.SelectionChanged += OnSelectionChanged;
		}

		return dialog;
	}

	private EncodingComboBox BuildEncodingComboBox () {
		int fixedEncoding = -1;
		ConfigFileOpenEncoding encodingConfig = Base.Config.FileOpenEncoding;
		if (encodingConfig == ConfigFileOpenEncoding.Fixed) {
			string encodingCode = Base.Config.FileOpenEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingCode, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		EncodingComboBox comboBox = new EncodingComboBox(true, null, fixedEncoding);

		/* Only need to handle the case of currentLocale, as Fixed is handled before and AutoDetect is the default behaviour */
		if (encodingConfig == ConfigFileOpenEncoding.CurrentLocale) {
			comboBox.ActiveSelection = (int)encodingConfig;
		}

		return comboBox;
	}

	private void FillVideoComboBoxBasedOnCurrentFolder () {
		videoFiles = null;
		videoFilenames = null;
		videoComboBox.RemoveAll();

		string folder = String.Empty;
		try {
			folder = (Dialog as FileChooserDialog).CurrentFolder;
		}
		catch (Exception e) {
			Logger.Error(e, "Caught exception when trying to get the current folder");
			SetVideoSelectionSensitivity(false);
			return;
		}

		if ((folder == null) || (folder == String.Empty)) {
			Logger.Error("Error when trying to get the current folder.");
			SetVideoSelectionSensitivity(false);
			return;
		}

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
		videoComboBox.PrependText(Catalog.GetString("None"));
		videoComboBox.Active = 0;
	}

	private void SetActiveVideoFile () {
		if ((videoFiles == null) || (videoFiles.Count == 0))
			return;

		string filePath = String.Empty;
		try {
			filePath = (Dialog as FileChooserDialog).Filename;
		}
		catch (Exception e) {
			Logger.Error(e, "Caught exception when trying to get the current filename");
			SetActiveComboBoxItem(0);
			return;
		}

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

	/* Note: It would be nice show a separator after "All Subtitle Files" but filters
	 * don't allow to set a separator function like we do in a normal combo box.
	 */
	private void SetFilters (FileChooserDialog dialog) {
		SubtitleTypeInfo[] types = Subtitles.AvailableTypesSorted;
		FileFilter[] filters = new FileFilter[types.Length + 2];
		int filterPosition = 0;

		/* First filter corresponds to all files */
		FileFilter allFilesFilter = new FileFilter();
		allFilesFilter.Name = Catalog.GetString("All Files");
		allFilesFilter.AddPattern("*");
		filters[filterPosition] = allFilesFilter;
		filterPosition++;

		/* Second filter corresponds to all subtitle files */
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

		foreach (FileFilter filter in filters) {
			dialog.AddFilter(filter);
		}

		dialog.Filter = subtitleFilesFilter;
	}


	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			chosenFilename = (Dialog as FileChooserDialog).Filename;
			chosenEncoding = encodingComboBox.ChosenEncoding;

			if (Base.Config.FileOpenEncodingOption == ConfigFileOpenEncodingOption.RememberLastUsed) {
				int activeAction = encodingComboBox.ActiveSelection;
				ConfigFileOpenEncoding activeOption = (ConfigFileOpenEncoding)Enum.ToObject(typeof(ConfigFileOpenEncoding), activeAction);
				if (((int)activeOption) >= ((int)ConfigFileOpenEncoding.Fixed))
					Base.Config.FileOpenEncodingFixed = chosenEncoding.Code;
				else
					Base.Config.FileOpenEncoding = activeOption;
			}

			if ((videoComboBox != null) && (videoComboBox.Active > 0)) {
				int videoFileIndex = videoComboBox.Active - 2;
				chosenVideoUri = new Uri("file://" + videoFiles[videoFileIndex] as string);
			}
			SetReturnValue(true);
		}
		return false;
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