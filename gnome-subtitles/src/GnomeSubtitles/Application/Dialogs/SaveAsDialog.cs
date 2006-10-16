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

	public SaveAsDialog(GUI gui) : base(gui, WidgetNames.SaveAsDialog) {
		dialog.DoOverwriteConfirmation = true;
	
		formatComboBox = (ComboBox)GetWidget(WidgetNames.SaveAsDialogFormatComboBox);
		encodingComboBox = (ComboBox)GetWidget(WidgetNames.SaveAsDialogEncodingComboBox);
		
		FillFormatComboBox();
		FillEncodingComboBox(encodingComboBox);
		
		if (gui.Core.Subtitles.Properties.IsFilePathRooted) {
			System.Console.WriteLine("File path is: ");
			System.Console.WriteLine(gui.Core.Subtitles.Properties.FilePath);
			dialog.SetFilename(gui.Core.Subtitles.Properties.FilePath);
		}
		else {
			dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			dialog.CurrentName = gui.Core.Subtitles.Properties.FileName;
		}
	}
	
	private void FillFormatComboBox () {
		SubtitleTypeInfo[] types = Subtitles.AvailableTypesSorted;
		SubtitleType currentType = GUI.Core.Subtitles.Properties.SubtitleType;
		int activeFormat = 0, typeNumber = 0;
		
		foreach (SubtitleTypeInfo typeInfo in types) {
			if (typeInfo.Type == currentType)
				activeFormat = typeNumber;
				
			formatComboBox.AppendText(typeInfo.Name + " (" + typeInfo.ExtensionsAsText + ")");
			typeNumber++;
		}
		
		formatComboBox.Active = activeFormat;
		subtitleTypes = types;
	}
	
	private string HandleFileNameWithExtension (SubtitleType type) {
		SubtitleTypeInfo typeInfo = Subtitles.GetAvailableType(type);
		string extension = typeInfo.PreferredExtension;
		string fileName = dialog.Filename;
		
		if (fileName.EndsWith(extension))
			return fileName;
		else
			return fileName + "." + extension;
	}


	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok) {
			SubtitleType type = subtitleTypes[formatComboBox.Active].Type;
			Encoding encoding = Encoding.GetEncoding(encodings[encodingComboBox.Active].CodePage);
			string fileName = HandleFileNameWithExtension(type);
			
			GUI.SaveAs(fileName, type, encoding);
			actionDone = true;
		}
		
		CloseDialog();
	}

}

}