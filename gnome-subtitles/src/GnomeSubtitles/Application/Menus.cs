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
		(GetWidget(WidgetNames.Toolbar) as Toolbar).UnsetStyle();
		SetToolbarHomogeneity();
	}
	
	public void BlankStartUp () {
		SetBlankSensitivity();
	}

	public void NewDocument (bool wasLoaded) {
		SetNewDocumentSensitivity(wasLoaded);
		SetSubtitleCountDependentSensitivity(core.Subtitles.Collection.Count);
		SetActiveTimingMode();
		SetFrameRateMenus();	
	}
	
	public void OnSubtitleSelection (Subtitle subtitle) { 
		SetStylesActivity(subtitle.Style.Bold, subtitle.Style.Italic, subtitle.Style.Underline);
		SetSelectionDependentSensitivity(true);
	}
	
	public void OnSubtitleSelection (TreePath[] paths) {
		if (paths.Length == 0)
			OnNoSubtitlesSelected();
		else
			OnSubtitlesSelected(paths);
	}
	
	public void OnSubtitleCountChanged (int count) {
		SetSubtitleCountDependentSensitivity(count);
	}
	
	public void SetActiveTimingMode () {
		if (core.TimingModeIsFrames)
	    	SetCheckMenuItemActivity(WidgetNames.FramesMenuItem, true);
	    else
	    	SetCheckMenuItemActivity(WidgetNames.TimesMenuItem, true);
	}
	
	public void SetCutCopySensitivity (bool sensitivity) {
		SetSensitivity(WidgetNames.CutMenuItem, sensitivity);
		SetSensitivity(WidgetNames.CopyMenuItem, sensitivity);
		SetSensitivity(WidgetNames.CutButton, sensitivity);
		SetSensitivity(WidgetNames.CopyButton, sensitivity);		
	}
	
	public void SetPasteSensitivity (bool sensitivity) {
		SetSensitivity(WidgetNames.PasteMenuItem, sensitivity);
		SetSensitivity(WidgetNames.PasteButton, sensitivity);
	}
	
	/* Static members */
	
	static public float FrameRateFromMenuItem (string menuItem) {
		return (float)Convert.ToDouble(menuItem.Split(' ')[0]);
	}

	/* Private members */
	
	private void OnNoSubtitlesSelected () {
		SetSelectionDependentSensitivity(false);
		SetStylesActivity(false, false, false);
	}
	
	private void OnSubtitlesSelected (TreePath[] paths) {
		SetSelectionDependentSensitivity(true);
		bool bold, italic, underline;
		GetGlobalStyles(paths, out bold, out italic, out underline);
		SetStylesActivity(bold, italic, underline);		
	}
	
	private void SetSelectionDependentSensitivity (bool sensitivity) {
		SetStylesSensitivity(sensitivity);
		SetSensitivity(WidgetNames.DeleteSubtitlesMenuItem, sensitivity);	
	}
	
	private void SetSubtitleCountDependentSensitivity (int count) {
		if (count == 0) {
			SetSensitivity(WidgetNames.AdjustTimingsMenuItem, false);
			SetSensitivity(WidgetNames.ShiftMenuItem, false);
		}
		else if (count == 1) {
			SetSensitivity(WidgetNames.AdjustTimingsMenuItem, false);
			SetSensitivity(WidgetNames.ShiftMenuItem, true);
		}
		else {
			SetSensitivity(WidgetNames.AdjustTimingsMenuItem, true);
			SetSensitivity(WidgetNames.ShiftMenuItem, true);		
		}	
	}
	
	private void SetBlankSensitivity () {
		/* File Menu */
		SetSensitivity(WidgetNames.SaveMenuItem, false);
		SetSensitivity(WidgetNames.SaveAsMenuItem, false);
		SetSensitivity(WidgetNames.HeadersMenuItem, false);
		/* Edit Menu */
		SetSensitivity(WidgetNames.UndoMenuItem, false);
		SetSensitivity(WidgetNames.RedoMenuItem, false);	
		SetSensitivity(WidgetNames.CutMenuItem, false);
		SetSensitivity(WidgetNames.CopyMenuItem, false);
		SetSensitivity(WidgetNames.PasteMenuItem, false);
		/* Toolbar */
		SetSensitivity(WidgetNames.SaveButton, false);
		SetSensitivity(WidgetNames.UndoButton, false);
		SetSensitivity(WidgetNames.RedoButton, false);
		SetSensitivity(WidgetNames.CutButton, false);
		SetSensitivity(WidgetNames.CopyButton, false);
		SetSensitivity(WidgetNames.PasteButton, false);
		SetSensitivity(WidgetNames.BoldButton, false);
		SetSensitivity(WidgetNames.ItalicButton, false);
		SetSensitivity(WidgetNames.UnderlineButton, false);
	}
	
	private void SetNewDocumentSensitivity (bool wasLoaded) {
		if (!wasLoaded) {	
			/* File Menu */
			SetSensitivity(WidgetNames.SaveMenuItem, true);
			SetSensitivity(WidgetNames.SaveAsMenuItem, true);
			SetSensitivity(WidgetNames.HeadersMenuItem, true);
			/* Edit Menu */
			SetMenuSensitivity(WidgetNames.InsertSubtitleMenuItem, true);
			SetSensitivity(WidgetNames.DeleteSubtitlesMenuItem, true);
			/* View Menu */
			SetSensitivity(WidgetNames.TimesMenuItem, true);
			SetSensitivity(WidgetNames.FramesMenuItem, true);
			/* Toolbar */
			SetSensitivity(WidgetNames.SaveButton, true);
			/* Common for Format Menu and Toolbar*/
			SetStylesSensitivity(true);
		}
		else {
			/* Edit Menu */
			SetSensitivity(WidgetNames.UndoMenuItem, false);
			SetSensitivity(WidgetNames.RedoMenuItem, false);
			SetSensitivity(WidgetNames.CutMenuItem, false);
			SetSensitivity(WidgetNames.CopyMenuItem, false);
			SetSensitivity(WidgetNames.PasteMenuItem, false);
			/* Toolbar */
			SetSensitivity(WidgetNames.UndoButton, false);
			SetSensitivity(WidgetNames.RedoButton, false);
			SetSensitivity(WidgetNames.CutButton, false);
			SetSensitivity(WidgetNames.CopyButton, false);
			SetSensitivity(WidgetNames.PasteButton, false);
		}	
	}
	
	private void SetStylesActivity (bool bold, bool italic, bool underline) {
		SetCheckMenuItemActivity(WidgetNames.BoldMenuItem, bold, core.Handlers.OnBold);
		SetCheckMenuItemActivity(WidgetNames.ItalicMenuItem, italic, core.Handlers.OnItalic);
		SetCheckMenuItemActivity(WidgetNames.UnderlineMenuItem, underline, core.Handlers.OnUnderline);
		SetToggleToolButtonActivity(WidgetNames.BoldButton, bold, core.Handlers.OnBold);
		SetToggleToolButtonActivity(WidgetNames.ItalicButton, italic, core.Handlers.OnItalic);
		SetToggleToolButtonActivity(WidgetNames.UnderlineButton, underline, core.Handlers.OnUnderline);
	}
	
	private void SetStylesSensitivity (bool sensitivity) {
		if (GetWidget(WidgetNames.BoldMenuItem).Sensitive != sensitivity) {
			SetSensitivity(WidgetNames.BoldMenuItem, sensitivity);
			SetSensitivity(WidgetNames.ItalicMenuItem, sensitivity);
			SetSensitivity(WidgetNames.UnderlineMenuItem, sensitivity);
			SetSensitivity(WidgetNames.BoldButton, sensitivity);
			SetSensitivity(WidgetNames.ItalicButton, sensitivity);
			SetSensitivity(WidgetNames.UnderlineButton, sensitivity);
		}	
	}
	
	private void SetFrameRateMenus () {
		if (core.TimingModeIsFrames) {
			SetMenuSensitivity(WidgetNames.InputFrameRateMenuItem, true);
			SetMenuSensitivity(WidgetNames.MovieFrameRateMenuItem, true);
		}
		else {
			SetMenuSensitivity(WidgetNames.InputFrameRateMenuItem, false);
			SetMenuSensitivity(WidgetNames.MovieFrameRateMenuItem, true);
		}
		 
		float inputFrameRate = core.Subtitles.Properties.OriginalFrameRate;
		float movieFrameRate = core.Subtitles.Properties.CurrentFrameRate;
		
		SetCheckMenuItemActivity(InputFrameRateMenuItem(inputFrameRate), true, core.Handlers.OnInputFrameRate);
		SetCheckMenuItemActivity(MovieFrameRateMenuItem(movieFrameRate), true, core.Handlers.OnMovieFrameRate);
	}
	
	private void SetCheckMenuItemActivity (string menuItemName, bool isActive) {
		(GetWidget(menuItemName) as CheckMenuItem).Active = isActive;
	}
	
	private void SetCheckMenuItemActivity (string menuItemName, bool isActive, EventHandler handler) {
		CheckMenuItem menuItem = GetWidget(menuItemName) as CheckMenuItem;
		menuItem.Toggled -= handler;
		menuItem.Active = isActive;
		menuItem.Toggled += handler;		
	}
	
	private void SetToggleToolButtonActivity (string toggleToolButtonName, bool isActive, EventHandler handler) {
		ToggleToolButton toggleToolButton = GetWidget(toggleToolButtonName) as ToggleToolButton;
		toggleToolButton.Toggled -= handler;
		toggleToolButton.Active = isActive;
		toggleToolButton.Toggled += handler;		
	}
	
	private void SetSensitivity (string widgetName, bool isSensitive) {
		Widget widget = GetWidget(widgetName);
		if (widget != null)
			widget.Sensitive = isSensitive;
	}
	
	private void SetMenuSensitivity (string menuItemName, bool sensitivity) {
		MenuItem menuItem = GetWidget(menuItemName) as MenuItem;
		Menu menu = menuItem.Submenu as Menu;
		foreach (Widget widget in menu)
			widget.Sensitive = sensitivity;	
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
	
	private string InputFrameRateMenuItem (float frameRate) {
		return FrameRateToMenuItem(frameRate, "input");
	}
	
	private string MovieFrameRateMenuItem (float frameRate) {
		return FrameRateToMenuItem(frameRate, "movie");
	}
	
	private string FrameRateToMenuItem (float frameRate, string prefix) {
		int rate = 0;
		if (frameRate >= 30)
			rate = 30;
		else if (frameRate >= 27)
			rate = 29;
		else if (frameRate >= 25)
			rate = 25;
		else if (frameRate >= 24)
			rate = 24;
		else
			rate = 23;
		
		return prefix + "FPS" + rate + "MenuItem";
	}
	
	private void SetToolbarHomogeneity () {
		//Set this toolbutton homogeneous to false because it's a lot larger than the rest
		(GetWidget(WidgetNames.UnderlineButton) as ToolButton).Homogeneous = false;
	}

}

}