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
		formatComboBox = (ComboBox)GetWidget(WidgetNames.SaveAsDialogFormatComboBox);
		encodingComboBox = (ComboBox)GetWidget(WidgetNames.SaveAsDialogEncodingComboBox);
		
		FillFormatComboBox();
		FillEncodingComboBox(encodingComboBox);
		
		if (gui.Core.Subtitles.Properties.IsFilePathRooted)
			Dialog.SetFilename(gui.Core.Subtitles.Properties.FilePath);
		else {
			Dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			Dialog.CurrentName = gui.Core.Subtitles.Properties.FileName;
		}
	}
	
	private void FillFormatComboBox () {
		SubtitleTypeInfo[] types = GUI.Core.Subtitles.AvailableTypes;
		foreach (SubtitleTypeInfo type in types)
			formatComboBox.AppendText(type.Name + " (" + type.ExtensionsAsText + ")");

		formatComboBox.Active = 0;
		subtitleTypes = types;
	}


	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		SubtitleType type = subtitleTypes[formatComboBox.Active].Type;
		Encoding encoding = Encoding.GetEncoding(Encodings[encodingComboBox.Active].CodePage);
		
		if (args.ResponseId == ResponseType.Ok) {
			GUI.SaveAs(Dialog.Filename, type, encoding);
			CloseDialog(); 
		}
		else if (args.ResponseId == ResponseType.Cancel) {
			CloseDialog();
		}
	}

}

}