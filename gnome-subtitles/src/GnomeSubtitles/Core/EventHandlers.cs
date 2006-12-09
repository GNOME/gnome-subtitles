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

/* TODO: Think about splitting this and using different methods in glade */
public class EventHandlers {
	
	private Tooltips tooltips = new Tooltips();

	public EventHandlers () {
		tooltips.Enable();
    }

	
	/* File Menu */
	
	public void OnNew (object o, EventArgs args) {
		Global.GUI.New(String.Empty);
	}
	
	public void OnOpen (object o, EventArgs args) {
		Global.GUI.Open();
	}
	
	public void OnSave (object o, EventArgs args) {
		Global.GUI.Save();
	}
	
	public void OnSaveAs (object o, EventArgs args) {
		Global.GUI.SaveAs();
	}
	
	public void OnHeaders (object o, EventArgs args) {
		new HeadersDialog();
	}

    public void OnQuit (object o, EventArgs args) {
		Global.GUI.Quit();
	}
	
	
	/* Edit Menu */
	
	public void OnUndo (object o, EventArgs args) {
		Global.CommandManager.Undo();
	}
	
	public void OnRedo (object o, EventArgs args) {
		Global.CommandManager.Redo();
	}
	
	public void OnCopy (object o, EventArgs args) {
		Global.Clipboards.Copy();
	}
	
	public void OnCut (object o, EventArgs args) {
		Global.Clipboards.Cut();
	}
	
	public void OnPaste (object o, EventArgs args) {
		Global.Clipboards.Paste();
	}
	
	public void OnFind (object o, EventArgs args) {
		Global.GUI.View.Search.ShowFind();
	}
	
	public void OnFindNext (object o, EventArgs args) {
		Global.GUI.View.Search.FindNext();
	}
	
	public void OnFindPrevious (object o, EventArgs args) {
		Global.GUI.View.Search.FindPrevious();
	}
	
	public void OnReplace (object o, EventArgs args) {
		Global.GUI.View.Search.ShowReplace();
	}
	
	public void OnInsertSubtitleBeforeSelection (object o, EventArgs args) {
		Global.CommandManager.Execute(new InsertSubtitleBeforeCommand());
	}
	
	public void OnInsertSubtitleAfterSelection (object o, EventArgs args) {
		if (Global.Subtitles.Count == 0)
			Global.CommandManager.Execute(new InsertFirstSubtitleCommand());
		else
			Global.CommandManager.Execute(new InsertSubtitleAfterCommand());
	}
	
	public void OnDeleteSubtitles (object o, EventArgs args) {
		if (Global.GUI.View.Selection.Count > 0)
			Global.CommandManager.Execute(new DeleteSubtitlesCommand());
	}
	
	
	/* View Menu */
	
	public void OnTimes (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			Global.GUI.OnToggleTimingMode(TimingMode.Times);
	}

	public void OnFrames (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			Global.GUI.OnToggleTimingMode(TimingMode.Frames);
	}
	
	/* Format Menu */
	
	public void OnBold (object o, EventArgs args) {
		bool newBoldValue = ((o is CheckMenuItem) ? (o as CheckMenuItem).Active : (o as ToggleToolButton).Active);  	
		Global.CommandManager.Execute(new ChangeBoldStyleCommand(newBoldValue));
	}
	
	public void OnItalic (object o, EventArgs args) {
		bool newItalicValue = ((o is CheckMenuItem) ? (o as CheckMenuItem).Active : (o as ToggleToolButton).Active);
		Global.CommandManager.Execute(new ChangeItalicStyleCommand(newItalicValue));
	}
	
	public void OnUnderline (object o, EventArgs args) {
		bool newUnderlineValue = ((o is CheckMenuItem) ? (o as CheckMenuItem).Active : (o as ToggleToolButton).Active);
		Global.CommandManager.Execute(new ChangeUnderlineStyleCommand(newUnderlineValue));
	}
	
	/*	Timings Menu */
	
	public void OnInputFrameRate (object o, EventArgs args) {
		RadioMenuItem menuItem = o as RadioMenuItem;
		if (menuItem.Active) {
			float frameRate = Menus.FrameRateFromMenuItem((menuItem.Child as Label).Text);
			Global.CommandManager.Execute(new ChangeInputFrameRateCommand(frameRate));
		}
	}
	
	public void OnMovieFrameRate (object o, EventArgs args) {
		RadioMenuItem menuItem = o as RadioMenuItem;
		if (menuItem.Active) {
			float frameRate = Menus.FrameRateFromMenuItem((menuItem.Child as Label).Text);
			Global.CommandManager.Execute(new ChangeMovieFrameRateCommand(frameRate));
		}
	}
	
	public void OnAdjustTimings (object o, EventArgs args) {
		new AdjustTimingsDialog();	
	}
	
	public void OnShift (object o, EventArgs args) {
		new ShiftTimingsDialog();
	}
	
	
	/*	Help Menu */
	
	public void OnHelpContents (object o, EventArgs args) {
		Util.OpenUrl("http://gsubtitles.sourceforge.net/help");
	}

	
	public void OnReportBug (object o, EventArgs args) {
		Util.OpenBugReport();
	}
	
	public void OnRequestFeature (object o, EventArgs args) {
		Util.OpenBugReport();
	}
	
	public void OnAbout (object o, EventArgs args) {
		new AboutDialog();
	}
	
	
	/*	Window	*/
	
	public void OnDelete (object o, DeleteEventArgs args) {
    	Global.GUI.Quit();
    	args.RetVal = true;
    }

    
    /* CommandManager related */
    
    public void OnUndoToggled (object o, EventArgs args) {
    	Widget button = Global.GetWidget(WidgetNames.UndoButton);
    	button.Sensitive = !button.Sensitive;
    		
		MenuItem menuItem = (MenuItem)Global.GetWidget(WidgetNames.UndoMenuItem);
		menuItem.Sensitive = !menuItem.Sensitive;
		if (!menuItem.Sensitive)
			(menuItem.Child as Label).Text = "Undo";
    }
    
     public void OnRedoToggled (object o, EventArgs args) {
    		Widget button = Global.GetWidget(WidgetNames.RedoButton);
    		button.Sensitive = !button.Sensitive;
    		
		MenuItem menuItem = (MenuItem)Global.GetWidget(WidgetNames.RedoMenuItem);
    		menuItem.Sensitive = !menuItem.Sensitive;
    		if (!menuItem.Sensitive)
			(menuItem.Child as Label).Text = "Redo";
    }
    
    public void OnCommandActivated (object o, EventArgs args) {
    	CommandManager commandManager = Global.CommandManager;
    	if (commandManager.CanUndo) {
    		string undoDescription = commandManager.UndoDescription;
    		ToolButton undoButton = (ToolButton)(Global.GetWidget(WidgetNames.UndoButton));
    		undoButton.SetTooltip(tooltips, undoDescription, null);
    		MenuItem undoMenuItem = (MenuItem)(Global.GetWidget(WidgetNames.UndoMenuItem));
    		(undoMenuItem.Child as Label).Text = undoDescription;
    	}
    	if (commandManager.CanRedo) {
	    	string redoDescription = commandManager.RedoDescription;
    		ToolButton redoButton = (ToolButton)(Global.GetWidget(WidgetNames.RedoButton));
    		redoButton.SetTooltip(tooltips, redoDescription, null);
    		MenuItem redoMenuItem = (MenuItem)(Global.GetWidget(WidgetNames.RedoMenuItem));
    		(redoMenuItem.Child as Label).Text = redoDescription;
    	}
    }
    
    public void OnModified (object o, EventArgs args) {
		Global.GUI.UpdateWindowTitle(true);
    }
    
    /*	Subtitle View	*/
    
    public void OnRowActivated (object o, RowActivatedArgs args) {
    	Global.GUI.View.Selection.ActivatePath();
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
    	Global.GUI.Menus.SetPasteSensitivity(true);
    }
    
    public void OnFocusOutSubtitleEdit (object o, FocusOutEventArgs args) {
    	Global.GUI.Menus.SetPasteSensitivity(false);
    }
    
    [GLib.ConnectBefore]
    public void OnSubtitleEditKeyPressed (object o, KeyPressEventArgs args) {
    	Gdk.Key key = args.Event.Key;
    	Gdk.ModifierType modifier = args.Event.State;
    	Gdk.ModifierType controlModifier = Gdk.ModifierType.ControlMask;
    	
    	if ((modifier & controlModifier) == controlModifier) { //Control was pressed
    		switch (key) {
    			case Gdk.Key.Page_Up:
    				Global.GUI.View.Selection.SelectPrevious();
					Global.GUI.Edit.TextGrabFocus();
    				args.RetVal = true;
    				break;
    			case Gdk.Key.Page_Down:
					Global.GUI.View.Selection.SelectNext();
					Global.GUI.Edit.TextGrabFocus();
    				args.RetVal = true;
    				break;
    		}
    	}
    }

}

}