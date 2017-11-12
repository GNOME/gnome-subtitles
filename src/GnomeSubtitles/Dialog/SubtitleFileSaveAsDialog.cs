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

//using Glade;
using GnomeSubtitles.Core;
using GnomeSubtitles.Ui.Component;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;
using System.IO;
using System.Text;

namespace GnomeSubtitles.Dialog {

public abstract class SubtitleFileSaveAsDialog : BuilderDialog {
	protected FileChooserDialog dialog = null;

	private string chosenFilename = String.Empty;
	private EncodingDescription chosenEncoding = EncodingDescription.Empty;
	private SubtitleTextType textType;
	private SubtitleType chosenSubtitleType;
	private NewlineType chosenNewlineType;

	/* Constant strings */
	private const string gladeFilename = "FileSaveAsDialog.glade";

	/* Components */
	private EncodingComboBox encodingComboBoxComponent = null;
	private SubtitleFormatComboBox formatComboBoxComponent = null;
	private NewlineTypeComboBox newlineComboBoxComponent = null;

	/* Widgets */
	[Builder.Object] private ComboBoxText fileEncodingComboBox = null;
	[Builder.Object] private ComboBoxText subtitleFormatComboBox = null;
	[Builder.Object] private ComboBoxText newlineTypeComboBox = null;


	protected SubtitleFileSaveAsDialog (SubtitleTextType textType) : base(gladeFilename) {
		dialog = GetDialog() as FileChooserDialog;

		this.textType = textType;
		SetTitle();

		InitEncodingComboBox();
		InitFormatComboBox();
		InitNewlineComboBox();

		SetDialogFromFileProperties();
		ConnectHandlers();
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


	/* Private members */

	private void InitEncodingComboBox () {
		int fixedEncoding = GetFixedEncoding();
		ConfigFileSaveEncoding encodingConfig = Base.Config.FileSaveEncoding;
		if (encodingConfig == ConfigFileSaveEncoding.Fixed) {
			string encodingName = Base.Config.FileSaveEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingName, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		this.encodingComboBoxComponent = new EncodingComboBox(fileEncodingComboBox, false, null, fixedEncoding);

		/* Only need to handle the case of currentLocale, as Fixed and Keep Existent is handled before */
		if (encodingConfig == ConfigFileSaveEncoding.CurrentLocale)
			encodingComboBoxComponent.ActiveSelection = (int)encodingConfig;
	}

	private void InitFormatComboBox () {
		SubtitleType fixedSubtitleType = GetFixedSubtitleType();
		ConfigFileSaveFormat formatConfig = Base.Config.FileSaveFormat;
		if (formatConfig == ConfigFileSaveFormat.Fixed) {
			fixedSubtitleType = Base.Config.FileSaveFormatFixed;
		}
		/* Check if fixed subtitle type has been correctly identified */
		if (fixedSubtitleType == SubtitleType.Unknown) {
			fixedSubtitleType = SubtitleType.SubRip;
		}

		this.formatComboBoxComponent = new SubtitleFormatComboBox(subtitleFormatComboBox, fixedSubtitleType, null);
	}

	private void InitNewlineComboBox () {
		NewlineType newlineTypeToSelect = Base.Config.FileSaveNewline;
		/* If no newline type set, or system default unknown, use Unix */
		if (newlineTypeToSelect == NewlineType.Unknown)
			newlineTypeToSelect = NewlineType.Unix;

		this.newlineComboBoxComponent = new NewlineTypeComboBox(newlineTypeComboBox, newlineTypeToSelect, null);
	}

	private void SetTitle () {
		if (textType == SubtitleTextType.Text)
			dialog.Title = Catalog.GetString("Save As");
		else
			dialog.Title = Catalog.GetString("Save Translation As");
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

	private void SetDialogFromFileProperties () {
		/* Set folder */
		if ((textType == SubtitleTextType.Translation) && Base.Document.HasTranslationFileProperties && Base.Document.TranslationFile.IsPathRooted)
			dialog.SetCurrentFolder(Base.Document.TranslationFile.Directory);
		else if (Base.Document.HasTextFileProperties && Base.Document.TextFile.IsPathRooted)
			dialog.SetCurrentFolder(Base.Document.TextFile.Directory);
		else
			dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));

		/* Set filename */
		FileProperties fileProperties = (textType == SubtitleTextType.Text ? Base.Document.TextFile : Base.Document.TranslationFile);
		if ((fileProperties != null) && (fileProperties.Filename != String.Empty))
			dialog.CurrentName = fileProperties.Filename;
		else
			dialog.CurrentName = (textType == SubtitleTextType.Text ? Base.Document.UnsavedTextFilename : Base.Document.UnsavedTranslationFilename);
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

	private string AddExtensionIfNeeded (SubtitleType type) {
		string filename = dialog.Filename;
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

	#pragma warning disable 169		//Disables warning about handlers not being used

	private void ConnectHandlers () {
		this.formatComboBoxComponent.SelectionChanged += OnFormatChanged;
	}

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {

			/* Check chosen encoding */
			chosenEncoding = encodingComboBoxComponent.ChosenEncoding;
			if (Base.Config.FileSaveEncodingOption == ConfigFileSaveEncodingOption.RememberLastUsed) {
				int activeAction = encodingComboBoxComponent.ActiveSelection;
				ConfigFileSaveEncoding activeOption = (ConfigFileSaveEncoding)Enum.ToObject(typeof(ConfigFileSaveEncoding), activeAction);
				if (((int)activeOption) >= ((int)ConfigFileSaveEncoding.Fixed)) {
					Base.Config.FileSaveEncodingFixed = chosenEncoding.Name;
				}
				else {
					Base.Config.FileSaveEncoding = activeOption;
				}
			}

			/* Check chosen subtitle format */
			chosenSubtitleType = formatComboBoxComponent.ChosenSubtitleType;
			if (Base.Config.FileSaveFormatOption == ConfigFileSaveFormatOption.RememberLastUsed) {
				Base.Config.FileSaveFormatFixed = chosenSubtitleType;
			}

			/* Check chosen newline type */
			chosenNewlineType = newlineComboBoxComponent.ChosenNewlineType;
			if (Base.Config.FileSaveNewlineOption == ConfigFileSaveNewlineOption.RememberLastUsed) {
				Base.Config.FileSaveNewline = chosenNewlineType;
			}

			/* Check chosen filename */
			chosenFilename = AddExtensionIfNeeded(chosenSubtitleType);

			SetReturnValue(true);
		}
		return false;
	}

	private void OnFormatChanged (object o, EventArgs args) {
		string filename = dialog.Filename;
		if ((filename == null) || (filename == String.Empty))
			return;

		string folder = dialog.CurrentFolder;
		if ((folder != null) && (folder != String.Empty)) {
			filename = filename.Substring(folder.Length + 1);
		}

		SubtitleType subtitleType = formatComboBoxComponent.ChosenSubtitleType;
		filename = UpdateFilenameExtension(filename, subtitleType);
		dialog.CurrentName = filename;
	}

}

}
