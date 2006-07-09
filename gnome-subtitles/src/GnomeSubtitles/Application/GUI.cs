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
using System.Text;
using SubLib;

namespace GnomeSubtitles {

public class GUI : GladeWidget {
	private ApplicationCore core = null;
	private App window = null;
	private SubtitleView subtitleView = null;	
	private SubtitleEdit subtitleEdit = null;
	
	public GUI () {
		core = new ApplicationCore(this);
		Init(ExecutionInfo.GladeMasterFileName, WidgetNames.MainWindow, core.Handlers);
		core.Handlers.Init(this.Glade);
		
		window = (App)GetWidget(WidgetNames.MainWindow);
		subtitleView = new SubtitleView(this, this.Glade);
		subtitleEdit = new SubtitleEdit(this, this.Glade);
		
		if (ExecutionInfo.Args.Length > 0)
			Open(ExecutionInfo.Args[0]);
		else
			SetStartUpSensitivity();
			
		core.Program.Run();
    }
      
	public ApplicationCore Core {
		get { return core; }
	}
	
	public App Window {
		get { return window; }
	}
	
	public SubtitleView SubtitleView {
		get { return subtitleView; }
	}
	
	public SubtitleEdit SubtitleEdit {
		get { return subtitleEdit; }
	}
    
    
    
    public void Close() {
    		core.Program.Quit();
    }

    public void New () {
    		core.New();
    		NewDocument();
    }
    
    public void Open (string fileName) {
   	 	core.Open(fileName);
    		NewDocument();
    }
    
    public void Open (string fileName, Encoding encoding) {
		core.Open(fileName, encoding);
    		NewDocument();
    }
    
    public void Save () {
    		core.Subtitles.Save();
    		core.CommandManager.WasModified = false;
    		SetWindowTitle(false);
    }
    
    public void SaveAs (string filePath, SubtitleType subtitleType, Encoding encoding) {
		core.Subtitles.SaveAs(filePath, subtitleType, encoding);
		SetActiveTimingMode();
		SetWindowTitle(false);
    }
	
	public void SetTimingMode (TimingMode mode) {
		core.Subtitles.Properties.TimingMode = mode;
		subtitleView.UpdateTimingMode();
		subtitleEdit.UpdateTimingMode();
	}
	
	public void SetWindowTitle (bool modified) {
		string prefix = (modified ? "*" : String.Empty);
		window.Title = prefix + core.Subtitles.Properties.FileName +
			" - " + ExecutionInfo.ApplicationName;
	}
	
	public void OnSubtitleSelection (Subtitle subtitle) {
		SetActiveStyles(subtitle.Style.Bold, subtitle.Style.Italic, subtitle.Style.Underline);
		subtitleEdit.LoadSubtitle(subtitle);
	}
	
	public void OnSubtitleSelection (TreePath[] paths) {
		bool bold, italic, underline;
		GetGlobalStyles(paths, out bold, out italic, out underline);
		SetActiveStyles(bold, italic, underline);
		subtitleEdit.Sensitive = false;
	}
	
	public void RefreshAndReselect () {
		subtitleView.Refresh();
		subtitleView.Reselect();
	}
	
	static public float FrameRateFromMenuItem (string menuItem) {
		return (float)Convert.ToDouble(menuItem.Split(' ')[0]);
	}
    
    
    /* Private methods */
    
	private void NewDocument () {
		SetNewDocumentSensitivity();
		SetActiveTimingMode();
		SetFrameRateMenus();
		SetWindowTitle(false);

		subtitleView.SetUp();
		subtitleView.Load(core.Subtitles);
		subtitleEdit.SetUp();
		
		subtitleView.SelectFirst();
	}
	
	private void SetActiveTimingMode () {
		if (core.Subtitles.Properties.TimingMode == TimingMode.Frames)
	    		(GetWidget(WidgetNames.FramesMenuItem) as RadioMenuItem).Active = true;
	    	else
	    		(GetWidget(WidgetNames.TimesMenuItem) as RadioMenuItem).Active = true;
	}
	
	private void SetFrameRateMenus () {
		MenuItem inputFrameRateMenuItem = GetWidget(WidgetNames.InputFrameRateMenuItem) as MenuItem;
		MenuItem movieFrameRateMenuItem = GetWidget(WidgetNames.MovieFrameRateMenuItem) as MenuItem;
		
		if (core.Subtitles.Properties.TimingMode == TimingMode.Frames) {
			SetMenuItemsSensitivity(inputFrameRateMenuItem.Submenu as Menu, true);
			SetMenuItemsSensitivity(movieFrameRateMenuItem.Submenu as Menu, true);
		}
		else {
			SetMenuItemsSensitivity(inputFrameRateMenuItem.Submenu as Menu, false);
			SetMenuItemsSensitivity(movieFrameRateMenuItem.Submenu as Menu, true);
		}
		
		(GetWidget(WidgetNames.InputFrameRateMenuItem25) as RadioMenuItem).Active = true;
		(GetWidget(WidgetNames.MovieFrameRateMenuItem25) as RadioMenuItem).Active = true;
	}
	
	private void SetActiveStyles (bool bold, bool italic, bool underline) {
		SetActiveStyle(WidgetNames.BoldMenuItem, bold, core.Handlers.OnBold);
		SetActiveStyle(WidgetNames.ItalicMenuItem, italic, core.Handlers.OnItalic);
		SetActiveStyle(WidgetNames.UnderlineMenuItem, underline, core.Handlers.OnUnderline);
	}
	
	private void SetActiveStyle (string menuItemName, bool active, EventHandler handler) {
		CheckMenuItem menuItem = (GetWidget(menuItemName) as CheckMenuItem);
		menuItem.Toggled -= handler;
		menuItem.Active = active;
		menuItem.Toggled += handler;		
	}
	
	private void GetGlobalStyles (TreePath[] paths, out bool bold, out bool italic, out bool underline) {
		Subtitles subtitles = core.Subtitles;
		bold = true;
		italic = true;
		underline = true;
		foreach (TreePath path in paths) {
			Subtitle subtitle = subtitles.Get(path);
			if ((bold == true) && !subtitle.Style.Bold) //bold hasn't been unset
				bold = false;
			if ((italic == true) && !subtitle.Style.Italic)
				italic = false;
			if ((underline == true) && !subtitle.Style.Underline)
				underline = false;
		}		
	}
	
	/* Only necessary because it isn't working in .glade */
	private void SetStartUpSensitivity () {
		SetUndoRedoSensitivity(false);
		SetGlobalSensitivity(false);
	}
	
	private void SetNewDocumentSensitivity () {
		SetUndoRedoSensitivity(false);
		SetGlobalSensitivity(true);
	}
	
	private void SetUndoRedoSensitivity (bool sensitivity) {
		/* Edit Menu */
		GetWidget(WidgetNames.UndoMenuItem).Sensitive = sensitivity;
		GetWidget(WidgetNames.RedoMenuItem).Sensitive = sensitivity;
		
		/* Toolbar */
		GetWidget(WidgetNames.UndoButton).Sensitive = sensitivity;
		GetWidget(WidgetNames.RedoButton).Sensitive = sensitivity;
	}
	
	private void SetGlobalSensitivity (bool sensitivity) {
		/* File Menu */
		GetWidget(WidgetNames.SaveMenuItem).Sensitive = sensitivity;
		GetWidget(WidgetNames.SaveAsMenuItem).Sensitive = sensitivity;
		/* Edit Menu */
		GetWidget(WidgetNames.CutMenuItem).Sensitive = sensitivity;
		GetWidget(WidgetNames.CopyMenuItem).Sensitive = sensitivity;
		GetWidget(WidgetNames.PasteMenuItem).Sensitive = sensitivity;
		GetWidget(WidgetNames.ClearMenuItem).Sensitive = sensitivity;
		/* View Menu */
		GetWidget(WidgetNames.TimesMenuItem).Sensitive = sensitivity;
		GetWidget(WidgetNames.FramesMenuItem).Sensitive = sensitivity;
		/* Format Menu */
		GetWidget(WidgetNames.BoldMenuItem).Sensitive = sensitivity;
		GetWidget(WidgetNames.ItalicMenuItem).Sensitive = sensitivity;
		GetWidget(WidgetNames.UnderlineMenuItem).Sensitive = sensitivity;
		/* Toolbar */
		GetWidget(WidgetNames.SaveButton).Sensitive = sensitivity;
	}
	
	private void SetMenuItemsSensitivity (Menu menu, bool sensitivity) {
		foreach (Widget widget in menu)
			widget.Sensitive = sensitivity;	
	}
	



}

}
