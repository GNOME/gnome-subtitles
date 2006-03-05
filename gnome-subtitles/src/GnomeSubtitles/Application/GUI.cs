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
	
	public GUI(ExecutionInfo executionInfo) {
		core = new ApplicationCore(executionInfo, this);
		Init(executionInfo.GladeMasterFileName, WidgetNames.MainWindow, core.Handlers);
		core.Handlers.Init(this.Glade);
		
		window = (App)GetWidget(WidgetNames.MainWindow);
		subtitleView = new SubtitleView(this, this.Glade);
		subtitleEdit = new SubtitleEdit(this, this.Glade);
		
		if (executionInfo.Args.Length > 0)
			Open(executionInfo.Args[0]);
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
			" - " + core.ExecutionInfo.Name;
	}
    
    
    /* Private methods */
    
	private void NewDocument () {
		SetNewDocumentSensitivity();
		SetActiveTimingMode();
		SetWindowTitle(false);

		subtitleView.Show();
		subtitleEdit.Show();	
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
	
	private void SetActiveTimingMode () {
		if (core.Subtitles.Properties.TimingMode == TimingMode.Frames)
	    		(GetWidget(WidgetNames.FramesMenuItem) as RadioMenuItem).Active = true;
	    	else
	    		(GetWidget(WidgetNames.TimesMenuItem) as RadioMenuItem).Active = true;
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
		/* Toolbar */
		GetWidget(WidgetNames.SaveButton).Sensitive = sensitivity;
	}


}

}
