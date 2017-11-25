/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2017 Pedro Castro
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
using GnomeSubtitles.Ui.Component;
using GnomeSubtitles.Ui.VideoPreview;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GnomeSubtitles.Dialog {

public class FileOpenDialog : BaseDialog {
	protected FileChooserDialog dialog = null;

	private string chosenFilename = String.Empty;
	private EncodingDescription chosenEncoding = EncodingDescription.Empty;
	private ArrayList videoFiles = null; //The full paths of the video files in the current dir
	private ArrayList videoFilenames = null; //The filenames of videoFiles, without extensions
	private Uri chosenVideoUri = null;
	private bool autoChooseVideoFile = true;

	/* Components */
	private EncodingComboBox encodingComboBox = null;

	/* Widgets */
	private ComboBoxText fileEncodingComboBox = null;
	private ComboBoxText videoComboBox = null;
	private Label videoLabel = null;


	public FileOpenDialog () : this(true, Catalog.GetString("Open File")) {
	}

	protected FileOpenDialog (bool toEnableVideo, string title) : base() {
		BuildDialog(title);

		if (toEnableVideo) {
			EnableVideo();
		}

		base.Init(dialog);
	}

	private void BuildDialog(string title) {
		dialog = new FileChooserDialog(title, Base.Ui.Window, FileChooserAction.Open,
			Util.GetStockLabel("gtk-cancel"), ResponseType.Cancel, Util.GetStockLabel("gtk-open"), ResponseType.Ok);

		dialog.DefaultResponse = ResponseType.Ok;

		/* Build content area */

		Grid grid = new Grid();
		grid.RowSpacing = 6;
		grid.ColumnSpacing = 6;
		grid.BorderWidth = 6;

		Label encodingLabel = new Label(Catalog.GetString("Character Encoding:"));
		encodingLabel.Xalign = 0;
		grid.Attach(encodingLabel, 0, 0, 1, 1);

		this.videoLabel = new Label(Catalog.GetString("Video To Open:"));
		this.videoLabel.Xalign = 0;
		grid.Attach(this.videoLabel, 0, 1, 1, 1);

		this.fileEncodingComboBox = new ComboBoxText();
		this.fileEncodingComboBox.Hexpand = true;
		grid.Attach(this.fileEncodingComboBox, 1, 0, 1, 1);
		this.encodingComboBox = BuildEncodingComboBox();

		this.videoComboBox = new ComboBoxText();
		this.videoComboBox.Hexpand = true;
		grid.Attach(this.videoComboBox, 1, 1, 1, 1);

		dialog.ContentArea.Add(grid);
		dialog.ContentArea.ShowAll();

		/* Other stuff */
		SetFilters();
		dialog.SetCurrentFolder(GetStartFolder());
	}

	private EncodingComboBox BuildEncodingComboBox () {
		int fixedEncoding = -1;
		ConfigFileOpenEncoding encodingConfig = Base.Config.FileOpenEncoding;
		if (encodingConfig == ConfigFileOpenEncoding.Fixed) {
			string encodingName = Base.Config.FileOpenEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingName, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		EncodingComboBox comboBox = new EncodingComboBox(this.fileEncodingComboBox, true, null, fixedEncoding);

		/* Only need to handle the case of currentLocale, as Fixed is handled before and AutoDetect is the default behaviour */
		if (encodingConfig == ConfigFileOpenEncoding.CurrentLocale) {
			comboBox.ActiveSelection = (int)encodingConfig;
		}

		return comboBox;
	}

	/* Overriden members */

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

	private void FillVideoComboBoxBasedOnCurrentFolder () {
		videoFiles = null;
		videoFilenames = null;
		(videoComboBox.Model as ListStore).Clear();

		string folder = String.Empty;
		try {
			folder = dialog.CurrentFolder;
		}
		catch (Exception e) {
			System.Console.Error.WriteLine("Caught exception when trying to get the current folder:");
			System.Console.Error.WriteLine(e);
			SetVideoSelectionSensitivity(false);
			return;
		}

		if ((folder == null) || (folder == String.Empty)) {
			System.Console.Error.WriteLine("Error when trying to get the current folder.");
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
			filePath = dialog.Filename;
		}
		catch (Exception e) {
			System.Console.Error.WriteLine("Caught exception when trying to get the current filename:");
			System.Console.Error.WriteLine(e);
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

	private void EnableVideo () {
		videoLabel.Visible = true;
		videoComboBox.Visible = true;

		autoChooseVideoFile = Base.Config.VideoAutoChooseFile;
		videoComboBox.RowSeparatorFunc = ComboBoxUtil.SeparatorFunc;

		dialog.CurrentFolderChanged += OnCurrentFolderChanged;
		dialog.SelectionChanged += OnSelectionChanged;
	}

	private void SetFilters () {
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
			chosenFilename = dialog.Filename;
			chosenEncoding = encodingComboBox.ChosenEncoding;

			if (Base.Config.FileOpenEncodingOption == ConfigFileOpenEncodingOption.RememberLastUsed) {
				int activeAction = encodingComboBox.ActiveSelection;
				ConfigFileOpenEncoding activeOption = (ConfigFileOpenEncoding)Enum.ToObject(typeof(ConfigFileOpenEncoding), activeAction);
				if (((int)activeOption) >= ((int)ConfigFileOpenEncoding.Fixed))
					Base.Config.FileOpenEncodingFixed = chosenEncoding.Name;
				else
					Base.Config.FileOpenEncoding = activeOption;
			}

			if (videoComboBox.Active > 0) {
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
