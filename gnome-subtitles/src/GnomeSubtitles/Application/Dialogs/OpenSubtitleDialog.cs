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

namespace GnomeSubtitles {

public class OpenSubtitleDialog : GladeWidget {
	private FileChooserDialog dialog = null;

	public OpenSubtitleDialog (GUI gui) : base(gui, WidgetNames.OpenSubtitleDialog){
		dialog = (FileChooserDialog)GetWidget(WidgetNames.OpenSubtitleDialog);
		dialog.TransientFor = gui.Window;
	}
	

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void CloseDialog() {
		dialog.Destroy();
	}
	
    private void OnDelete (object o, DeleteEventArgs args) {
    		CloseDialog();
    }
    
    private void OnCancel (object o, EventArgs args) {
    		CloseDialog();
    }
    
    private void OnOpen (object o, EventArgs args) {
     	string fileName = dialog.Filename;
		GUI.Open(fileName);
		CloseDialog();    		
    }

}

}