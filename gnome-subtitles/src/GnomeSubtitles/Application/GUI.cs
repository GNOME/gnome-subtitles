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
	private Menus menus = null;
	private SubtitleView subtitleView = null;	
	private SubtitleEdit subtitleEdit = null;
	
	public GUI () {
		core = new ApplicationCore(this);
		Init(ExecutionInfo.GladeMasterFileName, WidgetNames.MainWindow, core.Handlers);
		core.Handlers.Init(this.Glade);
		
		window = (App)GetWidget(WidgetNames.MainWindow);
		window.Icon = new Gdk.Pixbuf(null, "gnome-subtitles.svg");
		
		subtitleView = new SubtitleView(this, this.Glade);
		subtitleEdit = new SubtitleEdit(this, this.Glade);
		menus = new Menus(this, this.Glade);
		
		if (ExecutionInfo.Args.Length > 0)
			Open(ExecutionInfo.Args[0]);
		else
			BlankStartUp();
		
		try {
			core.Program.Run();
		}
		catch (Exception exception) {
			ForceTerminate();
			throw exception;
		}
    }
    
    private void ForceTerminate () {
    	core.Clipboards.WatchPrimaryChanges = false;
    	core.Program.Quit();
		window.Destroy();
    }
      
	public ApplicationCore Core {
		get { return core; }
	}
	
	public App Window {
		get { return window; }
	}

	public Menus Menus {
		get { return menus; }
	}
	
	public SubtitleView SubtitleView {
		get { return subtitleView; }
	}
	
	public SubtitleEdit SubtitleEdit {
		get { return subtitleEdit; }
	}
    
    public void Close() {
		if (ToCloseAfterWarning) {
			core.Program.Quit();
		}
    }

    public void New () {
    	if (ToNewAfterWarning) {
    		bool wasLoaded = core.IsLoaded;
    		core.New();
    		NewDocument(wasLoaded);
    	}
    }
    
    public void Open () {
    	OpenDialog dialog = new OpenDialog(this);
    	bool toOpen = dialog.WaitForResponse();
    	if (toOpen && ToOpenAfterWarning) {
    		string fileName = dialog.FileName;
    		if (dialog.HasEncoding) {
    			Encoding encoding = dialog.Encoding;
    			Open(fileName, encoding);    		
    		}
    		else
    			Open(fileName);
    	}
    }
    
    public void Open (string fileName) {
    	bool wasLoaded = core.IsLoaded;
   	 	core.Open(fileName);
    	NewDocument(wasLoaded);
    }
    
    public void Open (string fileName, Encoding encoding) {
    	bool wasLoaded = core.IsLoaded;
		core.Open(fileName, encoding);
    	NewDocument(wasLoaded);
    }
    
    public bool Save () {
    	if (core.Subtitles.CanSave) {
			core.Subtitles.Save();
	    	core.CommandManager.WasModified = false;
    		SetWindowTitle(false);
    		return true;
		}
		else	 //Unsaved Document
			return SaveAs();
    }
    
   	public bool SaveAs () {
		SaveAsDialog dialog = new SaveAsDialog(this);
		return dialog.WaitForResponse();
	}
    
    public bool SaveAs (string filePath, SubtitleType subtitleType, Encoding encoding) {
		core.Subtitles.SaveAs(filePath, subtitleType, encoding);
		menus.SetActiveTimingMode();
		SetWindowTitle(false);
		return true;
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
		menus.OnSubtitleSelection(subtitle);
		subtitleEdit.LoadSubtitle(subtitle);
	}
	
	public void OnSubtitleSelection (TreePath[] paths) {
		menus.OnSubtitleSelection(paths);
		subtitleEdit.Sensitive = false;
	}
	
	public void RefreshAndReselect () {
		subtitleView.Refresh();
		subtitleView.Reselect();
	}
    
    /* Private methods */
    
    private void BlankStartUp () {
    	menus.BlankStartUp();
    	subtitleView.BlankStartUp();
    	subtitleEdit.BlankStartUp();
    }
    
	private void NewDocument (bool wasLoaded) {
		SetWindowTitle(false);

		menus.NewDocument(wasLoaded);
		subtitleView.NewDocument(wasLoaded, core.Subtitles);
		subtitleEdit.NewDocument(wasLoaded);
		
		subtitleView.SelectFirst();
	}
	
	private bool ExistUnsavedChanges {
		get { return core.CommandManager.WasModified; }
	}

    private bool ToCloseAfterWarning {
    	get {
    		if (ExistUnsavedChanges) {
	    		ConfirmationDialog dialog = new CloseConfirmationDialog(this);
    			return dialog.WaitForResponse();
    		}
    		else
	    		return true; 
		}
	}
    
    private bool ToNewAfterWarning {
    	get {
    		if (ExistUnsavedChanges) {
	    		ConfirmationDialog dialog = new NewConfirmationDialog(this);
    			return dialog.WaitForResponse();
    		}
    		else
	    		return true; 
		}
	}
	
	private bool ToOpenAfterWarning {
    	get {
    		if (ExistUnsavedChanges) {
	    		ConfirmationDialog dialog = new OpenConfirmationDialog(this);
    			return dialog.WaitForResponse();
    		}
    		else
	    		return true; 
		}
	}

}

}
