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
	
	private Tooltips tooltips = new Tooltips();

	public EventHandlers (GUI gui){
		this.gui = gui;
		tooltips.Enable();
    }
    
    
    public void Init (Glade.XML glade) {
	    	this.glade = glade;
    }
    
	
	/* File Menu */
	
	public void OnNew (object o, EventArgs args) {
		gui.New();
	}
	
	public void OnOpen (object o, EventArgs args) {
		gui.Open();
	}
	
	public void OnSave (object o, EventArgs args) {
		gui.Save();
	}
	
	public void OnSaveAs (object o, EventArgs args) {
		gui.SaveAs();
	}
	
	public void OnHeaders (object o, EventArgs args) {
		new HeadersDialog(gui);
	}
	    
    public void OnQuit (object o, EventArgs args) {
		gui.Close();
	}
	
	
	/* Edit Menu */
	
	public void OnUndo (object o, EventArgs args) {
		gui.Core.CommandManager.Undo();
	}
	
	public void OnRedo (object o, EventArgs args) {
		gui.Core.CommandManager.Redo();
	}
	
	public void OnCopy (object o, EventArgs args) {
		gui.Core.Clipboards.Copy();
	}
	
	public void OnCut (object o, EventArgs args) {
		gui.Core.Clipboards.Cut();
	}
	
	public void OnPaste (object o, EventArgs args) {
		gui.Core.Clipboards.Paste();
	}
	
	public void OnInsertSubtitleBeforeSelection (object o, EventArgs args) {
		gui.Core.CommandManager.Execute(new InsertSubtitleCommand(gui, false));
	}
	
	public void OnInsertSubtitleAfterSelection (object o, EventArgs args) {
		gui.Core.CommandManager.Execute(new InsertSubtitleCommand(gui, true));
	}
	
	public void OnDeleteSubtitles (object o, EventArgs args) {
		if (gui.SubtitleView.SelectedPathCount > 0)
			gui.Core.CommandManager.Execute(new DeleteSubtitlesCommand(gui));
	}
	
	
	/* View Menu */
	
	public void OnTimes (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			gui.OnToggleTimingMode(TimingMode.Times);
	}

	public void OnFrames (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			gui.OnToggleTimingMode(TimingMode.Frames);
	}
	
	
	/* Format Menu */
	
	public void OnBold (object o, EventArgs args) {
		bool newBoldValue = ((o is CheckMenuItem) ? (o as CheckMenuItem).Active : (o as ToggleToolButton).Active);  	
		gui.Core.CommandManager.Execute(new ChangeBoldStyleCommand(gui, newBoldValue));
	}
	
	public void OnItalic (object o, EventArgs args) {
		bool newItalicValue = ((o is CheckMenuItem) ? (o as CheckMenuItem).Active : (o as ToggleToolButton).Active);
		gui.Core.CommandManager.Execute(new ChangeItalicStyleCommand(gui, newItalicValue));
	}
	
	public void OnUnderline (object o, EventArgs args) {
		bool newUnderlineValue = ((o is CheckMenuItem) ? (o as CheckMenuItem).Active : (o as ToggleToolButton).Active);
		gui.Core.CommandManager.Execute(new ChangeUnderlineStyleCommand(gui, newUnderlineValue));
	}
	
	/*	Timings Menu */
	
	public void OnInputFrameRate (object o, EventArgs args) {
		RadioMenuItem menuItem = o as RadioMenuItem;
		if (menuItem.Active) {
			float frameRate = Menus.FrameRateFromMenuItem((menuItem.Child as Label).Text);
			gui.Core.CommandManager.Execute(new ChangeInputFrameRateCommand(gui, frameRate));
		}
	}
	
	public void OnMovieFrameRate (object o, EventArgs args) {
		RadioMenuItem menuItem = o as RadioMenuItem;
		if (menuItem.Active) {
			float frameRate = Menus.FrameRateFromMenuItem((menuItem.Child as Label).Text);
			gui.Core.CommandManager.Execute(new ChangeMovieFrameRateCommand(gui, frameRate));
		}
	}
	
	public void OnAdjustTimings (object o, EventArgs args) {
		new AdjustTimingsDialog(gui);	
	}
	
	public void OnShift (object o, EventArgs args) {
		new ShiftTimingsDialog(gui);	
	}
	
	
	/*	Help Menu */
	
	public void OnReportBug (object o, EventArgs args) {
		Gnome.Url.Show("http://sourceforge.net/tracker/?func=add&group_id=129996&atid=716496");
	}
	
	public void OnRequestFeature (object o, EventArgs args) {
		Gnome.Url.Show("http://sourceforge.net/tracker/?func=add&group_id=129996&atid=716499");
	}
	
	public void OnAbout (object o, EventArgs args) {
		new AboutDialog(gui);
	}
	
	
	/*	Window	*/
	
	public void OnDelete (object o, DeleteEventArgs args) {
    	gui.Close();
    	args.RetVal = true;
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
    
    public void OnSubtitleViewKeyPressed (object o, KeyPressEventArgs args) {
    	Gdk.Key key = args.Event.Key;
    	if (key == Gdk.Key.Delete)
    		OnDeleteSubtitles(o, EventArgs.Empty);
		else if (key == Gdk.Key.Insert)
			OnInsertSubtitleAfterSelection(o, EventArgs.Empty);
    }
    
    /*	Subtitle Edit	*/
    
    public void OnFocusInSubtitleEdit (object o, FocusInEventArgs args) {
    	gui.Menus.SetPasteSensitivity(true);
    }
    
    public void OnFocusOutSubtitleEdit (object o, FocusOutEventArgs args) {
    	gui.Menus.SetPasteSensitivity(false);
    }
    
    [GLib.ConnectBefore]
    public void OnSubtitleEditKeyPressed (object o, KeyPressEventArgs args) {
    	Gdk.Key key = args.Event.Key;
    	Gdk.ModifierType modifier = args.Event.State;
    	Gdk.ModifierType controlModifier = Gdk.ModifierType.ControlMask;
    	
    	if ((modifier & controlModifier) == controlModifier) { //Control was pressed
    		switch (key) {
    			case Gdk.Key.Page_Up:
    				gui.SubtitleView.SelectPrevious();
					gui.SubtitleEdit.TextGrabFocus();
    				args.RetVal = true;
    				break;
    			case Gdk.Key.Page_Down:
					gui.SubtitleView.SelectNext();
					gui.SubtitleEdit.TextGrabFocus();
    				args.RetVal = true;
    				break;
    		}
    	}
    }

}

}