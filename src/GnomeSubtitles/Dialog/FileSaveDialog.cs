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
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Dialog {

public class FileSaveDialog : BaseDialog {
	private string chosenFilename = String.Empty;
	private EncodingDescription chosenEncoding = EncodingDescription.Empty;
	private SubtitleType chosenSubtitleType;
	private NewlineType chosenNewlineType;

	/* Components */
	private SubtitleFormatComboBox formatComboBox = null;
	private EncodingComboBox encodingComboBox = null;
	private NewlineTypeComboBox newlineComboBox = null;

	public FileSaveDialog () : this(Catalog.GetString("Save As")) {
	}

	protected FileSaveDialog (string title) : base() {
		base.Init(BuildDialog(title));
	}


	/* Public properties */

	public EncodingDescription Encoding {
		get { return chosenEncoding; }
	}

	public string Filename {
		get { return chosenFilename; }
	}

	public SubtitleType SubtitleType {
		get { return chosenSubtitleType; }
	}

	public NewlineType NewlineType {
		get { return chosenNewlineType; }
	}

	/* Protected members */

	protected virtual string GetStartFolder () {
		if (Base.Document.HasTextFileProperties && Base.Document.TextFile.IsPathRooted)
			return Base.Document.TextFile.Directory;
		else
			return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
	}

	protected virtual string GetStartFilename () {
		if (Base.Document.HasTextFileProperties && !String.IsNullOrEmpty(Base.Document.TextFile.Filename)) {
			return Base.Document.TextFile.Filename;
		}

		return Base.Document.UnsavedTextFilename;
	}

	/* Private members */

	private FileChooserDialog BuildDialog (string title) {
		FileChooserDialog dialog = new FileChooserDialog(title, Base.Ui.Window, FileChooserAction.Save,
			Util.GetStockLabel("gtk-cancel"), ResponseType.Cancel, Util.GetStockLabel("gtk-save"), ResponseType.Ok);

		dialog.DefaultResponse = ResponseType.Ok;
		dialog.DoOverwriteConfirmation = true;

		//Build content area

		Grid grid = new Grid();
		grid.RowSpacing = WidgetStyles.RowSpacingMedium;
		grid.ColumnSpacing = WidgetStyles.ColumnSpacingMedium;
		grid.BorderWidth = WidgetStyles.BorderWidthMedium;

		Label formatLabel = new Label(Catalog.GetString("Subtitle Format:"));
		formatLabel.Xalign = 0;
		grid.Attach(formatLabel, 0, 0, 1, 1);
		BuildFormatComboBox();
		formatComboBox.Widget.Hexpand = true;
		grid.Attach(formatComboBox.Widget, 1, 0, 3, 1);

		Label encodingLabel = new Label(Catalog.GetString("Character Encoding:"));
		encodingLabel.Xalign = 0;
		grid.Attach(encodingLabel, 0, 1, 1, 1);
		BuildEncodingComboBox();
		encodingComboBox.Widget.Hexpand = true;
		grid.Attach(encodingComboBox.Widget, 1, 1, 1, 1);

		Label newlineLabel = new Label(Catalog.GetString("Line Ending:"));
		newlineLabel.Xalign = 0;
		grid.Attach(newlineLabel, 2, 1, 1, 1);
		BuildNewlineComboBox();
		newlineComboBox.Widget.Hexpand = true;
		grid.Attach(newlineComboBox.Widget, 3, 1, 1, 1);

		dialog.ContentArea.Add(grid);
		dialog.ContentArea.ShowAll();

		// Other stuff
		dialog.SetCurrentFolder(GetStartFolder());
		dialog.CurrentName = AddExtensionIfNeeded(GetStartFilename(), formatComboBox.ChosenSubtitleType);

		return dialog;
	}

	private void BuildFormatComboBox () {
		SubtitleType fixedSubtitleType = GetFixedSubtitleType();
		ConfigFileSaveFormat formatConfig = Base.Config.FileSaveFormat;
		if (formatConfig == ConfigFileSaveFormat.Fixed) {
			fixedSubtitleType = Base.Config.FileSaveFormatFixed;
		}

		/* Check if fixed subtitle type has been correctly identified */
		if (fixedSubtitleType == SubtitleType.Unknown) {
			fixedSubtitleType = SubtitleType.SubRip;
		}

		formatComboBox = new SubtitleFormatComboBox(fixedSubtitleType, null);
		formatComboBox.SelectionChanged += OnFormatChanged;
	}

	private void BuildEncodingComboBox () {
		int fixedEncoding = GetFixedEncoding();
		ConfigFileSaveEncoding encodingConfig = Base.Config.FileSaveEncoding;
		if (encodingConfig == ConfigFileSaveEncoding.Fixed) {
			string encodingCode = Base.Config.FileSaveEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingCode, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		encodingComboBox = new EncodingComboBox(false, null, fixedEncoding);

		/* Only need to handle the case of currentLocale, as Fixed and Keep Existent is handled before */
		if (encodingConfig == ConfigFileSaveEncoding.CurrentLocale)
			encodingComboBox.ActiveSelection = (int)encodingConfig;
	}

	private void BuildNewlineComboBox () {
		NewlineType newlineTypeToSelect = Base.Config.FileSaveNewline;
		/* If no newline type set, or system default unknown, use Unix */
		if (newlineTypeToSelect == NewlineType.Unknown)
			newlineTypeToSelect = NewlineType.Unix;

		newlineComboBox = new NewlineTypeComboBox(newlineTypeToSelect, null);
	}

	private int GetFixedEncoding () {
		try {
			return Base.Document.TextFile.Encoding.CodePage;
		}
		catch (NullReferenceException) {
			return -1;
		}
	}

	private SubtitleType GetFixedSubtitleType () {
		try {
			return Base.Document.TextFile.SubtitleType;
		}
		catch (NullReferenceException) {
			return SubtitleType.Unknown;
		}
	}

	private string UpdateFilenameExtension (string filename, SubtitleType type) {
		SubtitleTypeInfo typeInfo = Subtitles.GetAvailableType(type);
		string newExtensionDotted = "." + typeInfo.PreferredExtension;

		int index = -1;
		string extensionDotted = "." + GetFilenameExtension(filename, out index);

		if (extensionDotted == newExtensionDotted) //filename already has the correct extension
			return filename;
		else if (index == -1) //filename doesn't have an extension, appending
			return filename + newExtensionDotted;
		else if (Subtitles.IsSubtitleExtension(extensionDotted))  { //filename's extension is a subtitle extension
			int dotIndex = index - 1;
			return filename.Substring(0, dotIndex) + newExtensionDotted;
		}
		else //filename's extension is not a subtitle extension
			return filename + newExtensionDotted;
	}

	private string AddExtensionIfNeeded (string filename, SubtitleType type) {
		int index = 0;
		string extension = GetFilenameExtension(filename, out index);

		SubtitleTypeInfo typeInfo = Subtitles.GetAvailableType(type);
		if (typeInfo.HasExtension(extension))
			return filename;
		else
			return filename + "." + typeInfo.PreferredExtension;
	}

	/// <summary>Returns the extension for the specified filename.</summary>
	private string GetFilenameExtension (string filename, out int index) {
		int dotIndex = filename.LastIndexOf('.');
		if ((dotIndex != -1) && (dotIndex != (filename.Length - 1))) {
			index = dotIndex + 1;
			return filename.Substring(index);
		}
		else {
			index = -1;
			return String.Empty;
		}
	}


	/* Event members */

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {

			/* Check chosen encoding */
			chosenEncoding = encodingComboBox.ChosenEncoding;
			if (Base.Config.FileSaveEncodingOption == ConfigFileSaveEncodingOption.RememberLastUsed) {
				int activeAction = encodingComboBox.ActiveSelection;
				ConfigFileSaveEncoding activeOption = (ConfigFileSaveEncoding)Enum.ToObject(typeof(ConfigFileSaveEncoding), activeAction);
				if (((int)activeOption) >= ((int)ConfigFileSaveEncoding.Fixed)) {
					Base.Config.FileSaveEncodingFixed = chosenEncoding.Code;
				}
				else {
					Base.Config.FileSaveEncoding = activeOption;
				}
			}

			/* Check chosen subtitle format */
			chosenSubtitleType = formatComboBox.ChosenSubtitleType;
			if (Base.Config.FileSaveFormatOption == ConfigFileSaveFormatOption.RememberLastUsed) {
				Base.Config.FileSaveFormatFixed = chosenSubtitleType;
			}

			/* Check chosen newline type */
			chosenNewlineType = newlineComboBox.ChosenNewlineType;
			if (Base.Config.FileSaveNewlineOption == ConfigFileSaveNewlineOption.RememberLastUsed) {
				Base.Config.FileSaveNewline = chosenNewlineType;
			}

			chosenFilename = (Dialog as FileChooserDialog).Filename;
			SetReturnValue(true);
		}
		return false;
	}
	
	private void OnFormatChanged (object o, EventArgs args) {
		FileChooserDialog dialog = Dialog as FileChooserDialog;

		string filename = dialog.Filename;
		if ((filename == null) || (filename == String.Empty))
			return;

		string folder = dialog.CurrentFolder;
		if ((folder != null) && (folder != String.Empty)) {
			filename = filename.Substring(folder.Length + 1);
		}

		SubtitleType subtitleType = formatComboBox.ChosenSubtitleType;
		filename = UpdateFilenameExtension(filename, subtitleType);
		dialog.CurrentName = filename;
	}

}

}
