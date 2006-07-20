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
using SubLib;

namespace GnomeSubtitles {

public class Menus : GladeWidget {
	ApplicationCore core = null;

	public Menus (GUI gui, Glade.XML glade) : base(gui, glade) {
		core = gui.Core;
	}

	public void NewDocument (bool wasLoaded) {
		SetNewDocumentSensitivity();
		SetActiveTimingMode();
		SetFrameRateMenus();	
	}
	
	public void SetActiveTimingMode () {
		if (core.Subtitles.Properties.TimingMode == TimingMode.Frames)
	    	SetActivity(WidgetNames.FramesMenuItem, true);
	    else
	    	SetActivity(WidgetNames.TimesMenuItem, true);
	}

	/* Only necessary because it isn't working in .glade */
	public void BlankStartUp () {
		SetUndoRedoSensitivity(false);
		SetGlobalSensitivity(false);
	}
	
	public void SetActiveStyles (bool bold, bool italic, bool underline) {
		SetActivity(WidgetNames.BoldMenuItem, bold, core.Handlers.OnBold);
		SetActivity(WidgetNames.ItalicMenuItem, italic, core.Handlers.OnItalic);
		SetActivity(WidgetNames.UnderlineMenuItem, underline, core.Handlers.OnUnderline);
	}
	
	/* Static members */
	
	static public float FrameRateFromMenuItem (string menuItem) {
		return (float)Convert.ToDouble(menuItem.Split(' ')[0]);
	}

	/* Private members */
	
	private void SetNewDocumentSensitivity () {
		SetUndoRedoSensitivity(false);
		SetGlobalSensitivity(true);
	}
	
	private void SetFrameRateMenus () {
		if (core.Subtitles.Properties.TimingMode == TimingMode.Frames) {
			SetMenuSensitivity(WidgetNames.InputFrameRateMenuItem, true);
			SetMenuSensitivity(WidgetNames.MovieFrameRateMenuItem, true);
		}
		else {
			SetMenuSensitivity(WidgetNames.InputFrameRateMenuItem, false);
			SetMenuSensitivity(WidgetNames.MovieFrameRateMenuItem, true);
		}
		
		SetActivity(WidgetNames.InputFrameRateMenuItem25, true, core.Handlers.OnInputFrameRate);
		SetActivity(WidgetNames.MovieFrameRateMenuItem25, true, core.Handlers.OnMovieFrameRate);
	}
	
	private void SetGlobalSensitivity (bool sensitivity) {
		/* File Menu */
		SetSensitivity(WidgetNames.SaveMenuItem, sensitivity);
		SetSensitivity(WidgetNames.SaveAsMenuItem, sensitivity);
		/* Edit Menu */
		SetSensitivity(WidgetNames.CutMenuItem, sensitivity);
		SetSensitivity(WidgetNames.CopyMenuItem, sensitivity);
		SetSensitivity(WidgetNames.PasteMenuItem, sensitivity);
		/* View Menu */
		SetSensitivity(WidgetNames.TimesMenuItem, sensitivity);
		SetSensitivity(WidgetNames.FramesMenuItem, sensitivity);
		/* Format Menu */
		SetSensitivity(WidgetNames.BoldMenuItem, sensitivity);
		SetSensitivity(WidgetNames.ItalicMenuItem, sensitivity);
		SetSensitivity(WidgetNames.UnderlineMenuItem, sensitivity);
		
		/* Toolbar */
		SetSensitivity(WidgetNames.SaveButton, sensitivity);
	}
	
	private void SetUndoRedoSensitivity (bool sensitivity) {
		/* Edit Menu */
		SetSensitivity(WidgetNames.UndoMenuItem, sensitivity);
		SetSensitivity(WidgetNames.RedoMenuItem, sensitivity);		
		/* Toolbar */
		SetSensitivity(WidgetNames.UndoButton, sensitivity);
		SetSensitivity(WidgetNames.RedoButton, sensitivity);
	}
	
	private void SetActivity (string menuItemName, bool isActive) {
		(GetWidget(menuItemName) as CheckMenuItem).Active = isActive;
	}
	
	private void SetActivity (string menuItemName, bool isActive, EventHandler handler) {
		CheckMenuItem menuItem = GetWidget(menuItemName) as CheckMenuItem;
		menuItem.Toggled -= handler;
		menuItem.Active = isActive;
		menuItem.Toggled += handler;		
	}
	
	private void SetSensitivity (string widgetName, bool isSensitive) {
		GetWidget(widgetName).Sensitive = isSensitive;
	}
	
	private void SetMenuSensitivity (string menuItemName, bool sensitivity) {
		MenuItem menuItem = GetWidget(menuItemName) as MenuItem;
		Menu menu = menuItem.Submenu as Menu;
		foreach (Widget widget in menu)
			widget.Sensitive = sensitivity;	
	}



}

}