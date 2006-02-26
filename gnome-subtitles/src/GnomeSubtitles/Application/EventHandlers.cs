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
using SubLib;

namespace GnomeSubtitles {

public class EventHandlers {
	private GUI gui = null;
	private Glade.XML glade = null;
	
	Tooltips tooltips = new Tooltips();

	public EventHandlers(GUI gui){
		this.gui = gui;
		tooltips.Enable();
    }
    
    public void Init (Glade.XML glade) {
	    	this.glade = glade;
    }
    
    
    public void OnUndoToggled (object o, EventArgs args) {
    		ToolButton button = (ToolButton)(glade.GetWidget(WidgetNames.UndoButton));
		button.Sensitive = !button.Sensitive;
    }
    
    public void OnRedoToggled (object o, EventArgs args) {
    		ToolButton button = (ToolButton)(glade.GetWidget(WidgetNames.RedoButton));
		button.Sensitive = !button.Sensitive;
    }
    
    public void OnCommandActivated (object o, EventArgs args) {
    		CommandManager commandManager = gui.Core.CommandManager;
    		if (commandManager.CanUndo) {
    			ToolButton undoButton = (ToolButton)(glade.GetWidget(WidgetNames.UndoButton));
    			undoButton.SetTooltip(tooltips, commandManager.UndoDescription, null);
    		}
    		if (commandManager.CanRedo) {
    			ToolButton redoButton = (ToolButton)(glade.GetWidget(WidgetNames.RedoButton));
    			redoButton.SetTooltip(tooltips, commandManager.RedoDescription, null);
    		}
    }
    
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	/*	File Menu	*/
	
	private void OnNew (object o, EventArgs args) {
		gui.New();
	}
	
	private void OnOpen (object o, EventArgs args) {
		new OpenSubtitleDialog(gui);
	}
	
	private void OnSaveAs (object o, EventArgs args) {
		new SaveSubtitleDialog(gui);
	}
	    
    private void OnQuit (object o, EventArgs args) {
		gui.Close();
	}
	
	
	/*	Edit Menu	*/
	
	private void OnUndo (object o, EventArgs args) {
		gui.Core.CommandManager.Undo();
	}
	
	private void OnRedo (object o, EventArgs args) {
		gui.Core.CommandManager.Redo();
	}
	
	
	/*	View Menu	*/
	
	private void OnTimes (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			gui.SetTimingMode(TimingMode.Times);
	}

	private void OnFrames (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			gui.SetTimingMode(TimingMode.Frames);
	}
	
	
	/*	Help Menu	*/
	
	private void OnAbout (object o, EventArgs args) {
		new AboutDialog(gui);
	}
	
	
	/*	Window	*/
	
	private void OnDelete (object o, DeleteEventArgs args) {
    		gui.Close();
    }


}

}