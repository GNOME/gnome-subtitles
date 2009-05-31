/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2009 Pedro Castro
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
using GnomeSubtitles.Core;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;
using System.IO;
using System.Text;

namespace GnomeSubtitles.Dialog {

public abstract class SubtitleFileSaveAsDialog : SubtitleFileChooserDialog {
	private SubtitleTextType textType;
	private SubtitleType chosenSubtitleType;
	private SubtitleTypeInfo[] subtitleTypes = null;
	private NewlineType chosenNewlineType;

	/* Constant strings */
	private const string gladeFilename = "FileSaveAsDialog.glade";
	
	/* Widgets */
	
	[WidgetAttribute] private ComboBox encodingComboBox = null;
	[WidgetAttribute] private ComboBox formatComboBox = null;
	[WidgetAttribute] private ComboBox newlineTypeComboBox = null;

	protected SubtitleFileSaveAsDialog (SubtitleTextType textType) : base(gladeFilename) {
		this.textType = textType;
		SetTitle();
		FillFormatComboBox();
		FillNewlineTypeComboBox();
	}

	/* Overriden members */

	public override DialogScope Scope {
		get { return DialogScope.Document; }
	}

	/* Public properties */
	
	public SubtitleType SubtitleType {
		get { return chosenSubtitleType; }
	}
	
	public NewlineType NewlineType {
		get { return chosenNewlineType; }
	}
	
	/* Public methods */
	
	public override void Show () {
		UpdateContents();
		base.Show();		
	}
	
	/* Protected methods */
	
	protected override int GetFixedEncoding () {
		try {
			return Base.Document.TextFile.Encoding.CodePage;
		}
		catch (NullReferenceException) {
			return -1;
		}
	}
	
	protected override ComboBox GetEncodingComboBox () {
		return encodingComboBox;
	}
	
	/* Private members */
	
	private void SetTitle () {
		if (textType == SubtitleTextType.Text)
			dialog.Title = Catalog.GetString("Save As");
		else
			dialog.Title = Catalog.GetString("Save Translation As");
	
	}
	
	private void UpdateContents () {
		FileProperties fileProperties = (textType == SubtitleTextType.Text ? Base.Document.TextFile : Base.Document.TranslationFile);
	
		if (fileProperties.IsPathRooted)
			dialog.SetCurrentFolder(fileProperties.Directory);
		else
			dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			
		dialog.CurrentName = fileProperties.Filename;

		/* There seems to be a bug in GTK that makes the dialog return null for currentFolder and currentFilename
		   while in the constructor. After constructing it works fine. */

		SetActiveFormat();
		SetActiveNewlineType();
	}

	private void FillFormatComboBox () {
		subtitleTypes = Subtitles.AvailableTypesSorted;
		
		foreach (SubtitleTypeInfo typeInfo in subtitleTypes) {
			formatComboBox.AppendText(typeInfo.Name + " (" + typeInfo.ExtensionsAsText + ")");
		}	
	}

	private void SetActiveFormat () {
		SubtitleType subtitleType = Base.Document.TextFile.SubtitleType; //The type of the subtitle file
		int position = FindSubtitleTypePosition(subtitleType);
		if (position != -1) {
			formatComboBox.Active = position;
			return;
		}
		
		/* The current subtitle type was not found, trying the most common based on the TimingMode */
		TimingMode timingMode = Base.TimingMode;
		
		/* If timing mode is Frames, set to MicroDVD */
		if (timingMode == TimingMode.Frames) {
			position = FindSubtitleTypePosition(SubtitleType.MicroDVD);
			if (position != -1) {
				formatComboBox.Active = position;
				return;
			}
		}
		
		/* If SubRip subtitle type is found, use it */
		position = FindSubtitleTypePosition(SubtitleType.SubRip);
		if (position != -1) {
			formatComboBox.Active = position;
			return;
		}
		
		/* All options tried to no aval, selecting the first */
		formatComboBox.Active = 0;
	}
	
	private int FindSubtitleTypePosition (SubtitleType type) {
		for (int position = 0 ; position < subtitleTypes.Length ; position++) {
			SubtitleType current = subtitleTypes[position].Type;
			if (current == type)
				return position;
		}
		return -1;
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
		else if (IsSubtitleExtension(extensionDotted))  { //filename's extension is a subtitle extension
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
	
	private bool IsSubtitleExtension (string dottedExtension) {
		string extension = dottedExtension.Substring(1); //Remove the starting dot
		foreach (SubtitleTypeInfo type in subtitleTypes) {
			if (type.HasExtension(extension))
				return true;
		}
		return false;
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
	
	private void FillNewlineTypeComboBox () {
		string mac = "Macintosh";
		string unix = "Unix";
		string windows = "Windows";
		string systemDefault = " (" + Catalog.GetString("System Default") + ")";
		
		NewlineType systemNewline = GetSystemNewlineType();
		SetSystemNewlineSuffix(systemNewline, ref mac, ref unix, ref windows, systemDefault);
		
		newlineTypeComboBox.AppendText(mac);
		newlineTypeComboBox.AppendText(unix);
		newlineTypeComboBox.AppendText(windows);
	}
	
	private void SetActiveNewlineType () {
		NewlineType systemNewline = GetSystemNewlineType();
		NewlineType documentNewline = Base.Document.TextFile.NewlineType;
		NewlineType newlineToMakeActive = (documentNewline != NewlineType.Unknown ? documentNewline : systemNewline);
		int item = GetNewlineTypePosition(newlineToMakeActive);
		newlineTypeComboBox.Active = item;	
	}
	
	private NewlineType GetSystemNewlineType () {
		switch (Environment.NewLine) {
			case "\n":
				return NewlineType.Unix;
			case "\r":
				return NewlineType.Macintosh;
			case "\r\n":
				return NewlineType.Windows;
			default:
				return NewlineType.Unknown;
		}
	}
	
	private void SetSystemNewlineSuffix (NewlineType newline, ref string mac, ref string unix, ref string windows, string suffix) {
		switch (newline) {
			case NewlineType.Macintosh:
				mac += suffix;
				break;
			case NewlineType.Unix:
				unix += suffix;
				break;
			case NewlineType.Windows:
				windows += suffix;
				break;
		}
	}
	
	private int GetNewlineTypePosition (NewlineType newline) {
		switch (newline) {
			case NewlineType.Macintosh:
				return 0;
			case NewlineType.Unix:
				return 1;
			case NewlineType.Windows:
				return 2;
			default:
				return 1;
		}	
	}
	
	private NewlineType GetChosenNewlineType () {
		switch (newlineTypeComboBox.Active) {
			case 0:
				return NewlineType.Macintosh;
			case 1:
				return NewlineType.Unix;
			case 2:
				return NewlineType.Windows;
			default:
				return NewlineType.Unix;
		}
	}

	/* Event members */

	#pragma warning disable 169		//Disables warning about handlers not being used

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			int formatIndex = formatComboBox.Active;
			chosenSubtitleType = subtitleTypes[formatIndex].Type;
			chosenFilename = AddExtensionIfNeeded(chosenSubtitleType);
			
			int encodingIndex = GetActiveEncodingComboBoxItem();
			chosenEncoding = encodings[encodingIndex];
			SetReturnValue(true);
			
			chosenNewlineType = GetChosenNewlineType();
		}
		return false;
	}

	private void OnFormatChanged (object o, EventArgs args) {
		SubtitleType type = subtitleTypes[formatComboBox.Active].Type;
		string filename = dialog.Filename;
		if ((filename == null) || (filename == String.Empty))
			return;

		string folder = dialog.CurrentFolder;
		if ((folder != null) && (folder != String.Empty)) {
			filename = filename.Substring(folder.Length + 1);
		}

		filename = UpdateFilenameExtension(filename, type);
		dialog.CurrentName = filename;
	}

}

}
