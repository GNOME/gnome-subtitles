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
    
	
	/*	File Menu	*/
	
	public void OnNew (object o, EventArgs args) {
		gui.New();
	}
	
	public void OnOpen (object o, EventArgs args) {
		new OpenDialog(gui);
	}
	
	public void OnSave (object o, EventArgs args) {
		if (gui.Core.Subtitles.CanSave)
			gui.Save();
		else	 //Unsaved Document
			SaveAs();
	}
	
	public void OnSaveAs (object o, EventArgs args) {
		SaveAs();
	}
	    
    public void OnQuit (object o, EventArgs args) {
		gui.Close();
	}
	
	
	/*	Edit Menu	*/
	
	public void OnUndo (object o, EventArgs args) {
		gui.Core.CommandManager.Undo();
	}
	
	public void OnRedo (object o, EventArgs args) {
		gui.Core.CommandManager.Redo();
	}
	
	
	/*	View Menu	*/
	
	public void OnTimes (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			gui.SetTimingMode(TimingMode.Times);
	}

	public void OnFrames (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			gui.SetTimingMode(TimingMode.Frames);
	}
	
	
	/*	Help Menu	*/
	
	public void OnAbout (object o, EventArgs args) {
		new AboutDialog(gui);
	}
	
	
	/*	Window	*/
	
	public void OnDelete (object o, DeleteEventArgs args) {
    		gui.Close();
    }
    
    
    /* CommandManager related */
    
    public void OnUndoToggled (object o, EventArgs args) {
    		Widget button = glade.GetWidget(WidgetNames.UndoButton);
    		button.Sensitive = !button.Sensitive;
    		
		MenuItem menuItem = (MenuItem)glade.GetWidget(WidgetNames.UndoMenuItem);
		menuItem.Sensitive = !menuItem.Sensitive;
		if (!menuItem.Sensitive)
			(menuItem.Child as Label).Text = "Undo";
    }
    
     public void OnRedoToggled (object o, EventArgs args) {
    		Widget button = glade.GetWidget(WidgetNames.RedoButton);
    		button.Sensitive = !button.Sensitive;
    		
		MenuItem menuItem = (MenuItem)glade.GetWidget(WidgetNames.RedoMenuItem);
    		menuItem.Sensitive = !menuItem.Sensitive;
    		if (!menuItem.Sensitive)
			(menuItem.Child as Label).Text = "Redo";
    }
    
    public void OnCommandActivated (object o, EventArgs args) {
    		CommandManager commandManager = gui.Core.CommandManager;
    		if (commandManager.CanUndo) {
    			string undoDescription = commandManager.UndoDescription;
    			ToolButton undoButton = (ToolButton)(glade.GetWidget(WidgetNames.UndoButton));
    			undoButton.SetTooltip(tooltips, undoDescription, null);
    			MenuItem undoMenuItem = (MenuItem)(glade.GetWidget(WidgetNames.UndoMenuItem));
    			(undoMenuItem.Child as Label).Text = undoDescription;
    		}
    		if (commandManager.CanRedo) {
	    		string redoDescription = commandManager.RedoDescription;
    			ToolButton redoButton = (ToolButton)(glade.GetWidget(WidgetNames.RedoButton));
    			redoButton.SetTooltip(tooltips, redoDescription, null);
    			MenuItem redoMenuItem = (MenuItem)(glade.GetWidget(WidgetNames.RedoMenuItem));
    			(redoMenuItem.Child as Label).Text = redoDescription;
    		}
    }
    
    public void OnModified (object o, EventArgs args) {
		gui.SetWindowTitle(true);
    }
    
    /*	Subtitle View	*/
    
    public void OnRowActivated (object o, RowActivatedArgs args) {
    		gui.SubtitleEdit.TextGrabFocus();
    }


	/***** Private methods *****/
	private void SaveAs () {
		new SaveAsDialog(gui);
	}

}

}