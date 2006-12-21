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
using Pango; //TODO delete
using SubLib;
using System;

namespace GnomeSubtitles {

/* TODO: Think about splitting this and using different methods in glade */
public class EventHandlers {
	
	private Tooltips tooltips = new Tooltips();

	public EventHandlers () {
		tooltips.Enable();
    }

	
	/* File Menu */
	
	public void OnFileNew (object o, EventArgs args) {
		//Global.GUI.New(String.Empty); TODO uncomment
		/*Gnome.Font font = Gnome.Font.FindClosestFromFullName("arial");
		Console.WriteLine(font.FamilyName);
		Console.WriteLine(font.FontName);
		Console.WriteLine(font.FullName);
		Console.WriteLine(font.ItalicAngle);
		Console.WriteLine(font.Size);
		Pango.Font pango = font.GetClosestPangoFont();
		Console.WriteLine(pango.Describe().ToFilename());*/
		FontDescription desc = Pango.FontDescription.FromString("Arial");
		System.Console.WriteLine(desc);
		System.Console.WriteLine(desc.ToFilename());
		
	}
	
	public void OnFileOpen (object o, EventArgs args) {
		Global.GUI.Open();
	}
	
	public void OnFileSave (object o, EventArgs args) {
		Global.GUI.Save();
	}
	
	public void OnFileSaveAs (object o, EventArgs args) {
		Global.GUI.SaveAs();
	}
	
	public void OnFileHeaders (object o, EventArgs args) {
		new HeadersDialog();
	}

    public void OnFileQuit (object o, EventArgs args) {
		Global.GUI.Quit();
	}
	
	
	/* Edit Menu */
	
	public void OnEditUndo (object o, EventArgs args) {
		Global.CommandManager.Undo();
	}
	
	public void OnEditRedo (object o, EventArgs args) {
		Global.CommandManager.Redo();
	}
	
	public void OnEditCopy (object o, EventArgs args) {
		Global.Clipboards.Copy();
	}
	
	public void OnEditCut (object o, EventArgs args) {
		Global.Clipboards.Cut();
	}
	
	public void OnEditPaste (object o, EventArgs args) {
		Global.Clipboards.Paste();
	}
	
	public void OnEditFormatBold (object o, EventArgs args) {
		bool newBoldValue = ((o is CheckMenuItem) ? (o as CheckMenuItem).Active : (o as ToggleToolButton).Active);  	
		Global.CommandManager.Execute(new ChangeBoldStyleCommand(newBoldValue));
	}
	
	public void OnEditFormatItalic (object o, EventArgs args) {
		bool newItalicValue = ((o is CheckMenuItem) ? (o as CheckMenuItem).Active : (o as ToggleToolButton).Active);
		Global.CommandManager.Execute(new ChangeItalicStyleCommand(newItalicValue));
	}
	
	public void OnEditFormatUnderline (object o, EventArgs args) {
		bool newUnderlineValue = ((o is CheckMenuItem) ? (o as CheckMenuItem).Active : (o as ToggleToolButton).Active);
		Global.CommandManager.Execute(new ChangeUnderlineStyleCommand(newUnderlineValue));
	}
	
	public void OnEditInsertSubtitleBefore (object o, EventArgs args) {
		Global.CommandManager.Execute(new InsertSubtitleBeforeCommand());
	}
	
	public void OnEditInsertSubtitleAfter (object o, EventArgs args) {
		if (Global.Subtitles.Count == 0)
			Global.CommandManager.Execute(new InsertFirstSubtitleCommand());
		else
			Global.CommandManager.Execute(new InsertSubtitleAfterCommand());
	}
	
	public void OnEditDeleteSubtitles (object o, EventArgs args) {
		if (Global.GUI.View.Selection.Count > 0)
			Global.CommandManager.Execute(new DeleteSubtitlesCommand());
	}


	/* View Menu */
	
	public void OnViewTimes (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			Global.GUI.OnToggleTimingMode(TimingMode.Times);
	}

	public void OnViewFrames (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			Global.GUI.OnToggleTimingMode(TimingMode.Frames);
	}
	
	public void OnViewVideo (object o, EventArgs args) {
		if ((o as CheckMenuItem).Active)
			Global.GUI.Video.Show();
		else
			Global.GUI.Video.Hide();
	}
	
	/* Search Menu */
	
		
	public void OnSearchFind (object o, EventArgs args) {
		Global.GUI.View.Search.ShowFind();
	}
	
	public void OnSearchFindNext (object o, EventArgs args) {
		Global.GUI.View.Search.FindNext();
	}
	
	public void OnSearchFindPrevious (object o, EventArgs args) {
		Global.GUI.View.Search.FindPrevious();
	}
	
	public void OnSearchReplace (object o, EventArgs args) {
		Global.GUI.View.Search.ShowReplace();
	}
	
	/*	Timings Menu */
	
	public void OnTimingsInputFrameRate (object o, EventArgs args) {
		RadioMenuItem menuItem = o as RadioMenuItem;
		if (menuItem.Active) {
			float frameRate = Menus.FrameRateFromMenuItem((menuItem.Child as Label).Text);
			Global.CommandManager.Execute(new ChangeInputFrameRateCommand(frameRate));
		}
	}
	
	public void OnTimingsVideoFrameRate (object o, EventArgs args) {
		RadioMenuItem menuItem = o as RadioMenuItem;
		if (menuItem.Active) {
			float frameRate = Menus.FrameRateFromMenuItem((menuItem.Child as Label).Text);
			Global.CommandManager.Execute(new ChangeVideoFrameRateCommand(frameRate));
		}
	}
	
	public void OnTimingsAdjust (object o, EventArgs args) {
		new AdjustTimingsDialog();	
	}
	
	public void OnTimingsShift (object o, EventArgs args) {
		new ShiftTimingsDialog();
	}
	
	
	/* Video Menu */
	
	public void OnVideoOpen (object o, EventArgs args) {
		VideoFileChooserDialog dialog = new VideoFileChooserDialog();
		bool toOpen = dialog.WaitForResponse();
		if (toOpen) {
			System.Console.WriteLine("Setting view activity to true");
			Global.GUI.Menus.SetViewVideoActivity(true);
			Global.GUI.Menus.SetVideoSensitivity(true);
			Global.GUI.Video.Open(dialog.Filename);
		}
	}
	
	public void OnVideoClose (object o, EventArgs args) {
		Global.GUI.Video.Close();
		Global.GUI.Menus.SetVideoSensitivity(false);
	}

	public void OnVideoPlayPause (object o, EventArgs args) {
		Global.GUI.Video.TogglePlayPause();
	}
	
	public void OnVideoRewind (object o, EventArgs args) {
		Global.GUI.Video.Rewind();
	}
	
	public void OnVideoForward (object o, EventArgs args) {
		Global.GUI.Video.Forward();
	}
	
	
	/*	Help Menu */
	
	public void OnHelpContents (object o, EventArgs args) {
		Util.OpenUrl("http://gnome-subtitles.sourceforge.net/help");
	}

	public void OnHelpRequestFeature (object o, EventArgs args) {
		Util.OpenBugReport();
	}
	
	public void OnHelpReportBug (object o, EventArgs args) {
		Util.OpenBugReport();
	}
	
	public void OnHelpAbout (object o, EventArgs args) {
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
    		
		MenuItem menuItem = (MenuItem)Global.GetWidget(WidgetNames.EditUndo);
		menuItem.Sensitive = !menuItem.Sensitive;
		if (!menuItem.Sensitive)
			(menuItem.Child as Label).Text = "Undo";
    }
    
     public void OnRedoToggled (object o, EventArgs args) {
    		Widget button = Global.GetWidget(WidgetNames.RedoButton);
    		button.Sensitive = !button.Sensitive;
    		
		MenuItem menuItem = (MenuItem)Global.GetWidget(WidgetNames.EditRedo);
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
    		MenuItem undoMenuItem = (MenuItem)(Global.GetWidget(WidgetNames.EditUndo));
    		(undoMenuItem.Child as Label).Text = undoDescription;
    	}
    	if (commandManager.CanRedo) {
	    	string redoDescription = commandManager.RedoDescription;
    		ToolButton redoButton = (ToolButton)(Global.GetWidget(WidgetNames.RedoButton));
    		redoButton.SetTooltip(tooltips, redoDescription, null);
    		MenuItem redoMenuItem = (MenuItem)(Global.GetWidget(WidgetNames.EditRedo));
    		(redoMenuItem.Child as Label).Text = redoDescription;
    	}
    }
    
    public void OnModified (object o, EventArgs args) {
		Global.GUI.UpdateWindowTitle(true);
    }
    
    /* Video */

	public void OnPlayPauseToggled (object o, EventArgs args) {
    	if ((o as ToggleButton).Active)
			Global.GUI.Video.Play();
		else
			Global.GUI.Video.Pause();
	}

	public void OnVideoSliderValueChanged (object o, EventArgs args) {
		Global.GUI.Video.UpdateFromSliderValueChanged();
	}
    
    /*	Subtitle View	*/
    
    public void OnRowActivated (object o, RowActivatedArgs args) {
    	Global.GUI.View.Selection.ActivatePath();
    }
    
    public void OnSubtitleViewKeyPressed (object o, KeyPressEventArgs args) {
    	Gdk.Key key = args.Event.Key;
    	if (key == Gdk.Key.Delete)
    		OnEditDeleteSubtitles(o, EventArgs.Empty);
		else if (key == Gdk.Key.Insert)
			OnEditInsertSubtitleAfter(o, EventArgs.Empty);
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
