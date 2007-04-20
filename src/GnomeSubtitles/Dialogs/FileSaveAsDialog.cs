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

using Glade;
using Gtk;
using SubLib;
using System;
using System.IO;
using System.Text;

namespace GnomeSubtitles {

public class FileSaveAsDialog : SubtitleFileChooserDialog {
	private SubtitleType chosenSubtitleType;
	private SubtitleTypeInfo[] subtitleTypes = null;

	/* Constant strings */
	private const string gladeFilename = "FileSaveAsDialog.glade";
	
	/* Widgets */
	
	[WidgetAttribute] private ComboBox formatComboBox;

	public FileSaveAsDialog () : base(gladeFilename) {
		if (Global.Document.FileProperties.IsPathRooted)
			dialog.SetCurrentFolder(Global.Document.FileProperties.Directory);
		else
			dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			
		dialog.CurrentName = Global.Document.FileProperties.Filename;

		/* There seems to be a bug in GTK that makes the dialog return null for currentFolder and currentFilename
		   while in this constructor. After constructing it works fine. */

		FillFormatComboBox();
	}
	
	public SubtitleType SubtitleType {
		get { return chosenSubtitleType; }
	}
	
	/* Protected methods */
	
	protected override int GetFixedEncoding () {
		try {
			return Global.Document.FileProperties.Encoding.CodePage;
		}
		catch (NullReferenceException) {
			return -1;
		}
	}
	
	/* Private members */

	private void FillFormatComboBox () {
		subtitleTypes = Subtitles.AvailableTypesSorted;
		SubtitleType subtitleType = Global.Document.FileProperties.SubtitleType; //The type of the subtitle file
		int activeType = -1; //The position of the combobox to make active
		int microDVDTypeNumber = -1; //The position of the MicroDVD format in the combobox
		int subRipTypeNumber = -1; //The position of the SubRip format in the combobox
		
		int currentTypeNumber = 0;
		foreach (SubtitleTypeInfo typeInfo in subtitleTypes) {
			SubtitleType type = typeInfo.Type;
			
			if (type == subtitleType)
				activeType = currentTypeNumber;
			
			if (type == SubtitleType.MicroDVD)
				microDVDTypeNumber = currentTypeNumber;
			else if (type == SubtitleType.SubRip)
				subRipTypeNumber = currentTypeNumber;
			
			formatComboBox.AppendText(typeInfo.Name + " (" + typeInfo.ExtensionsAsText + ")");
			currentTypeNumber++;
		}

		if (subtitleType == SubtitleType.Unknown) { //Active type isn't known, selecting MicroDVD or SubRip depending on Timing Mode
			TimingMode timingMode = Global.Document.TimingMode;
			if ((timingMode == TimingMode.Frames) && (microDVDTypeNumber != -1))
				formatComboBox.Active = microDVDTypeNumber;
			else if ((timingMode == TimingMode.Times) && (subRipTypeNumber != -1))
				formatComboBox.Active = subRipTypeNumber;
			else
				formatComboBox.Active = 0;
		}
		else {
			formatComboBox.Active = activeType;
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

	/* Event members */

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok) {
			int formatIndex = formatComboBox.Active;
			chosenSubtitleType = subtitleTypes[formatIndex].Type;
			chosenFilename = AddExtensionIfNeeded(chosenSubtitleType);
			int encodingIndex = GetActiveEncodingComboBoxItem();
			chosenEncoding = encodings[encodingIndex];
			actionDone = true;
		}
		Close();
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
