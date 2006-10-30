/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
 * Copyright (C) 2006 Pedro Castro
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
using System;
using System.IO;
using System.Text;
using SubLib;

namespace GnomeSubtitles {

public class SaveAsDialog : SubtitleFileChooserDialog {
	private ComboBox formatComboBox = null;
	private ComboBox encodingComboBox = null;
	private SubtitleTypeInfo[] subtitleTypes = null;

	public SaveAsDialog (GUI gui) : base(gui, WidgetNames.SaveAsDialog) {
		dialog.DoOverwriteConfirmation = true;
		
		if (gui.Core.Subtitles.Properties.IsFilePathRooted)
			dialog.SetCurrentFolder(gui.Core.Subtitles.Properties.FileDirectory);
		else
			dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
		
		dialog.CurrentName = gui.Core.Subtitles.Properties.FileName;
		formatComboBox = (ComboBox)GetWidget(WidgetNames.SaveAsDialogFormatComboBox);
		encodingComboBox = (ComboBox)GetWidget(WidgetNames.SaveAsDialogEncodingComboBox);
		FillFormatComboBox();
		FillEncodingComboBox(encodingComboBox);
	}

	private void FillFormatComboBox () {
		subtitleTypes = Subtitles.AvailableTypesSorted;
		SubtitleType subtitleType = GUI.Core.Subtitles.Properties.SubtitleType; //The type of the subtitle file
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
			TimingMode timingMode = GUI.Core.TimingMode;
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
	
	private string UpdateFileNameExtension (string fileName, SubtitleType type) {
		SubtitleTypeInfo typeInfo = Subtitles.GetAvailableType(type);
		string newExtensionDotted = "." + typeInfo.PreferredExtension;

		int index = -1;
		string extensionDotted = "." + GetFileNameExtension(fileName, out index);
		
		if (extensionDotted == newExtensionDotted) //filename already has the correct extension
			return fileName;
		else if (index == -1) //filename doesn't have an extension, appending
			return fileName + newExtensionDotted;
		else if (IsSubtitleExtension(extensionDotted))  { //filename's extension is a subtitle extension
			int dotIndex = index - 1;
			return fileName.Substring(0, dotIndex) + newExtensionDotted;
		}
		else //filename's extension is not a subtitle extension
			return fileName + newExtensionDotted;
	}

	private string AddExtensionIfNeeded (SubtitleType type) {
		string fileName = dialog.Filename;
		int index = 0;
		string extension = GetFileNameExtension(fileName, out index);

		SubtitleTypeInfo typeInfo = Subtitles.GetAvailableType(type);
		if (typeInfo.HasExtension(extension))
			return fileName;
		else
			return fileName + "." + typeInfo.PreferredExtension;
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
	private string GetFileNameExtension (string fileName, out int index) {
		int dotIndex = fileName.LastIndexOf('.');
		if ((dotIndex != -1) && (dotIndex != (fileName.Length - 1))) {
			index = dotIndex + 1;
			return fileName.Substring(index);
		}
		else {
			index = -1;
			return String.Empty;
		}
	}


	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok) {
			SubtitleType type = subtitleTypes[formatComboBox.Active].Type;
			Encoding encoding = Encoding.GetEncoding(encodings[encodingComboBox.Active].CodePage);
			string fileName = AddExtensionIfNeeded(type);
			
			GUI.SaveAs(fileName, type, encoding);
			actionDone = true;
		}
		
		CloseDialog();
	}
	
	private void OnFormatChanged (object o, EventArgs args) {
		SubtitleType type = subtitleTypes[formatComboBox.Active].Type;
		string fileName = dialog.Filename;
		string folder = dialog.CurrentFolder;
		if (folder.Length != 0) {
			fileName = fileName.Substring(folder.Length + 1);
		}
		fileName = UpdateFileNameExtension(fileName, type);
		dialog.CurrentName = fileName;
	}

}

}