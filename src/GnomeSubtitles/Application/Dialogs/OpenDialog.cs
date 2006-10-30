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
using System.Text;

namespace GnomeSubtitles {

public class OpenDialog : SubtitleFileChooserDialog {
	private ComboBox encodingComboBox = null;
	
	private string fileName = String.Empty;
	private Encoding encoding = null;


	public OpenDialog (GUI gui) : base(gui, WidgetNames.OpenDialog){
		encodingComboBox = (ComboBox)GetWidget(WidgetNames.OpenDialogEncodingComboBox);
		FillEncodingComboBox(encodingComboBox);
		encodingComboBox.PrependText("-");
		encodingComboBox.PrependText("Auto Detected");
		encodingComboBox.Active = 0;
	
		if (gui.Core.IsLoaded && gui.Core.Subtitles.Properties.IsFilePathRooted)
			dialog.SetCurrentFolder(gui.Core.Subtitles.Properties.FileDirectory);
		else
			dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));		
	}
	
	public string FileName {
		get { return fileName; }
	}
	
	public Encoding Encoding {
		get { return encoding; }
	}
	
	public bool HasEncoding {
		get { return encoding != null; }
	}

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok) {
			fileName = dialog.Filename;
			if (encodingComboBox.Active != 0) {
				int encodingIndex = encodingComboBox.Active - 2;
				encoding = Encoding.GetEncoding(encodings[encodingIndex].CodePage);
			}
			actionDone = true;
		}
		CloseDialog();
	}
	
}

}