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

using Gnome;
using Gtk;
using System;
using SubLib.Domain;

namespace GnomeSubtitles {

public class MainWindow : GladeWidget {
	private const string widgetName = "mainWindow";
	private App widget = null;
	
	private SubtitleView subtitleView = null;	
	//private MainStatusBar statusBar = null;
	
	public MainWindow(GUI gui) : base(gui, widgetName){
		widget = (App)GetWidget(widgetName);
		subtitleView = new SubtitleView(this.GUI, this.Glade);
		//statusBar = new MainStatusBar((AppBar)widget.Statusbar);
    }
    
	public void NewDocument () {
		RadioMenuItem timesMenuItem = (RadioMenuItem)GetWidget("timesMenuItem");
		RadioMenuItem framesMenuItem = (RadioMenuItem)GetWidget("framesMenuItem");
		timesMenuItem.Sensitive = true;
		framesMenuItem.Sensitive = true;
		if (GUI.Core.Subtitles.Properties.OriginalTimingMode == TimingMode.Frames)
	    		framesMenuItem.Active = true;
	    	else
	    		timesMenuItem.Active = true;
	    		
		subtitleView.NewDocument();
		widget.Title = GUI.Core.Subtitles.Properties.FileName + " - " + GUI.Core.ExecutionInfo.Name;
		widget.DefaultWidth = subtitleView.Width;
		//Console.WriteLine("Width: " + subtitleView.Width); NOT WORKING
	}
	
	private void SetTimingMode (TimingMode mode) {
		GUI.Core.Subtitles.Properties.TimingMode = mode;
		subtitleView.UpdateTimingMode();
	}

	#pragma warning disable 169		//Disables warning about handlers not being used
	
    private void OnDelete (object o, DeleteEventArgs args) {
    		GUI.Close();
    }
    
    private void OnQuit (object o, EventArgs args) {
		GUI.Close();
	}
	
	private void OnAbout (object o, EventArgs args) {
		new AboutDialog(GUI, widget);
	}
	
	private void OnNew (object o, EventArgs args) {
		GUI.New();
	}
	
	private void OnOpen (object o, EventArgs args) {
		new OpenSubtitleDialog(GUI, widget);
	}
	
	private void OnSaveAs (object o, EventArgs args) {
		new SaveSubtitleDialog(GUI, widget);
	}
	
	private void OnTimes (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			SetTimingMode(TimingMode.Times);
	}

	private void OnFrames (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			SetTimingMode(TimingMode.Frames);
	}
	

}

}