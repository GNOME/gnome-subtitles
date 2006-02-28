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

namespace GnomeSubtitles {

public class SaveSubtitleDialog : GladeWidget {
	private FileChooserDialog dialog = null;

	public SaveSubtitleDialog(GUI gui) : base(gui, WidgetNames.SaveAsSubtitleDialog) {
		dialog = (FileChooserDialog)GetWidget(WidgetNames.SaveAsSubtitleDialog);
		dialog.TransientFor = gui.Window;
		
		if (gui.Core.Subtitles.Properties.IsFilePathRooted)
			dialog.SetFilename(gui.Core.Subtitles.Properties.FilePath);
		else {
			dialog.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			dialog.CurrentName = gui.Core.Subtitles.Properties.FileName;
		}
	}
	

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		Console.WriteLine(args.ResponseId);
		
		if (args.ResponseId == ResponseType.Ok) {
			//GUI.SaveAs(dialog.Filename, Subtitle);
			Console.WriteLine("Saving As " + dialog.Filename);
			CloseDialog(); 
		}
		else if (args.ResponseId == ResponseType.Cancel) {
			CloseDialog();
		}
	}
	
	private void CloseDialog() {
		dialog.Destroy();
	}
	

}

}