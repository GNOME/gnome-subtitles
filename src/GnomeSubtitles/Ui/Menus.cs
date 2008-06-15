/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2008 Pedro Castro
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
using Mono.Unix;
using SubLib;
using System;
using System.Globalization;

namespace GnomeSubtitles.Ui {

public class Menus {
	#pragma warning disable 612 //TODO Tooltips has been deprecated
	private Tooltips tooltips = new Tooltips(); //TODO Tooltips has been deprecated, scheduled for substitution when gtk# 2.12 is available in all major distros (use SVN revision 968)

	/* Constant strings */
	private string videoTagText = Catalog.GetString("Video");

	/* Public methods */

	public Menus () {
		SetToolbarHomogeneity(); //TODO needed until homogeneity definition in glade starts working
		tooltips.Enable();
	}
	
	public void BlankStartUp () {
		SetBlankSensitivity();
		SetBlankActivity();
	}

	public void UpdateFromNewDocument (bool wasLoaded) {
		SetNewDocumentSensitivity(wasLoaded);
		SetSubtitleCountDependentSensitivity(Global.Document.Subtitles.Collection.Count);
		SetFrameRateMenus();
		SetActiveTimingMode(Global.TimingMode);
		
		SetToolsAutocheckSpellingSensitivity(true);
		SetCheckMenuItemActivity(WidgetNames.ToolsAutocheckSpelling, Global.SpellLanguages.Enabled);
	}
	
	public void UpdateFromNewTranslationDocument () {
		SetTranslationSensitivity(true);
		UpdateUndoRedoMessages();
	}
	
	public void UpdateFromCloseTranslation () {
    	SetTranslationSensitivity(false);
    	SetViewVideoSubtitlesActivity(true);
    	UpdateUndoRedoMessages();
    }
	
	public void UpdateFromSelection (Subtitle subtitle) { 
		SetStylesActivity(subtitle.Style.Bold, subtitle.Style.Italic, subtitle.Style.Underline);
		SetNonZeroSelectionDependentSensitivity(true);
		SetOneSelectionDependentSensitivity(true);
	}
	
	public void UpdateFromSelection (TreePath[] paths) {
		SetOneSelectionDependentSensitivity(false);
	
		if (paths.Length == 0)
			UpdateFromNoSelection();
		else
			UpdateFromSelectedPaths(paths);
	}
	
	public void UpdateFromSubtitleCount (int count) {
		SetSubtitleCountDependentSensitivity(count);
	}
	
	public void UpdateFromUndoToggled()  {
   		Widget button = Global.GetWidget(WidgetNames.UndoButton);
   		button.Sensitive = !button.Sensitive;
    		
		MenuItem menuItem = Global.GetWidget(WidgetNames.EditUndo) as MenuItem;
		menuItem.Sensitive = !menuItem.Sensitive;
		if (!menuItem.Sensitive)
			(menuItem.Child as Label).Text = Catalog.GetString("Undo");
	}
    
     public void UpdateFromRedoToggled () {
    	Widget button = Global.GetWidget(WidgetNames.RedoButton);
    	button.Sensitive = !button.Sensitive;
    		
		MenuItem menuItem = Global.GetWidget(WidgetNames.EditRedo) as MenuItem;
    	menuItem.Sensitive = !menuItem.Sensitive;
    	if (!menuItem.Sensitive)
			(menuItem.Child as Label).Text = Catalog.GetString("Redo");
    }
    
    public void UpdateFromCommandActivated () {
    	UpdateUndoRedoMessages();
    }
    
    public void UpdateFromOpenVideo () {
    	SetVideoSensitivity(true);
    	SetViewVideoSubtitlesSensitivity();
    }
    
    public void UpdateFromCloseVideo () {
    	SetVideoSensitivity(false);
    	SetViewVideoSubtitlesSensitivity(false, false);
    }

	public void SetActiveTimingMode (TimingMode mode) {
		if (mode == TimingMode.Times)
			SetCheckMenuItemActivity(WidgetNames.ViewTimes, true);
		else
			SetCheckMenuItemActivity(WidgetNames.ViewFrames, true);
	}
	
	public void SetCutCopySensitivity (bool sensitivity) {
		SetSensitivity(WidgetNames.EditCut, sensitivity);
		SetSensitivity(WidgetNames.EditCopy, sensitivity);
		SetSensitivity(WidgetNames.CutButton, sensitivity);
		SetSensitivity(WidgetNames.CopyButton, sensitivity);		
	}
	
	public void SetPasteSensitivity (bool sensitivity) {
		SetSensitivity(WidgetNames.EditPaste, sensitivity);
		SetSensitivity(WidgetNames.PasteButton, sensitivity);
	}
	
	public void UpdateActiveInputFrameRateMenuItem () {
		float inputFrameRate = Global.Document.Subtitles.Properties.OriginalFrameRate;
		SetCheckMenuItemActivity(InputFrameRateMenuItem(inputFrameRate), true, Global.Handlers.OnTimingsInputFrameRate);
	}
	
	public void UpdateActiveVideoFrameRateMenuItem () {
		float videoFrameRate = Global.Document.Subtitles.Properties.CurrentFrameRate;
		SetCheckMenuItemActivity(VideoFrameRateMenuItem(videoFrameRate), true, Global.Handlers.OnTimingsVideoFrameRate);
	}
	
	public void EnableFindNextPrevious () {
		SetSensitivity(WidgetNames.SearchFindNext, true);
		SetSensitivity(WidgetNames.SearchFindPrevious, true);
	}
	
	public void SetToolsAutocheckSpellingSensitivity (bool sensitivity) {
		SetSensitivity(WidgetNames.ToolsAutocheckSpelling, sensitivity);
	}
	
	public void SetViewVideoActivity (bool activity) {
		SetCheckMenuItemActivity(WidgetNames.ViewVideo, activity);
	}
	
	public void AddFrameRateVideoTag (float frameRate) {
		if (frameRate <= 0)
			return;
	
		string menuItemName = FrameRateToMenuItem(frameRate, "Video");
		string menuItemText = GetMenuItemText(menuItemName);
		
		menuItemText += " (" + videoTagText + ")";
		SetMenuItemText(menuItemName, menuItemText);	
	}
	
	public void RemoveFrameRateVideoTag (float frameRate) {
		if (frameRate <= 0)
			return;

		string menuItemName = FrameRateToMenuItem(frameRate, "Video");
		string menuItemText = GetMenuItemText(menuItemName);
		
		int lastSpaceIndex = menuItemText.LastIndexOf(' ');
		if (lastSpaceIndex < 0)
			return;
		
		string frameRateText = menuItemText.Substring(0, lastSpaceIndex);
		SetMenuItemText(menuItemName, frameRateText);
	}
	
	/* Static members */
	
	static public float FrameRateFromMenuItem (string menuItem) {
		string frameRateText = menuItem.Split(' ')[0];
		NumberFormatInfo invariant = NumberFormatInfo.InvariantInfo;
		return (float)Convert.ToDouble(frameRateText, invariant);
	}

	/* Private members */
	
	private void UpdateFromNoSelection () {
		SetNonZeroSelectionDependentSensitivity(false);
		SetStylesActivity(false, false, false);
	}
	
	private void UpdateFromSelectedPaths (TreePath[] paths) {
		SetNonZeroSelectionDependentSensitivity(true);
		bool bold, italic, underline;
		GetGlobalStyles(paths, out bold, out italic, out underline);
		SetStylesActivity(bold, italic, underline);		
	}
	
	/// <summary>Sets the sensitivity depending on 1 or more selected subtitles.</summary>
	/// <param name="sensitivity">Whether the items are set sensitive.</param>
	private void SetNonZeroSelectionDependentSensitivity (bool sensitivity) {
		SetStylesSensitivity(sensitivity);
		SetSensitivity(WidgetNames.EditDeleteSubtitles, sensitivity);
		SetSensitivity(WidgetNames.EditInsertSubtitleBefore, sensitivity);
	}
	
	/// <summary>Sets the sensitivity depending on exactly 1 selected subtitle.</summary>
	/// <param name="sensitivity">Whether the items are set sensitive.</param>
	private void SetOneSelectionDependentSensitivity (bool sensitivity) {
		SetVideoSelectionDependentSensitivity(sensitivity);
	}
	
	private void SetSubtitleCountDependentSensitivity (int count) {
		if (count == 0) {
			SetSensitivity(WidgetNames.TimingsAdjust, false);
			SetSensitivity(WidgetNames.TimingsShift, false);
		}
		else if (count == 1) {
			SetSensitivity(WidgetNames.TimingsAdjust, false);
			SetSensitivity(WidgetNames.TimingsShift, true);
		}
		else {
			SetSensitivity(WidgetNames.TimingsAdjust, true);
			SetSensitivity(WidgetNames.TimingsShift, true);
		}	
	}
	
	private void SetBlankSensitivity () {
		/* File Menu */
		SetSensitivity(WidgetNames.FileSave, false);
		SetSensitivity(WidgetNames.FileSaveAs, false);
		SetSensitivity(WidgetNames.FileHeaders, false);
		/* Edit Menu */
		SetSensitivity(WidgetNames.EditUndo, false);
		SetSensitivity(WidgetNames.EditRedo, false);	
		SetSensitivity(WidgetNames.EditCut, false);
		SetSensitivity(WidgetNames.EditCopy, false);
		SetSensitivity(WidgetNames.EditPaste, false);
		/* Search Menu */
		SetSensitivity(WidgetNames.SearchFind, false);
		SetSensitivity(WidgetNames.SearchFindNext, false);
		SetSensitivity(WidgetNames.SearchFindPrevious, false);
		SetSensitivity(WidgetNames.SearchReplace, false);
		/* Tools Menu */
		SetToolsAutocheckSpellingSensitivity(false);
		/* Toolbar */
		SetSensitivity(WidgetNames.SaveButton, false);
		SetSensitivity(WidgetNames.UndoButton, false);
		SetSensitivity(WidgetNames.RedoButton, false);
		SetSensitivity(WidgetNames.CutButton, false);
		SetSensitivity(WidgetNames.CopyButton, false);
		SetSensitivity(WidgetNames.PasteButton, false);
		SetSensitivity(WidgetNames.FindButton, false);
		SetSensitivity(WidgetNames.ReplaceButton, false);
		SetSensitivity(WidgetNames.BoldButton, false);
		SetSensitivity(WidgetNames.ItalicButton, false);
		SetSensitivity(WidgetNames.UnderlineButton, false);
	}
	
	private void SetBlankActivity () {
		SetCheckMenuItemActivity(WidgetNames.ToolsAutocheckSpelling, Global.SpellLanguages.Enabled);
	}
	
	private void SetNewDocumentSensitivity (bool wasLoaded) {
		if (!wasLoaded) {	
			/* File Menu */
			SetSensitivity(WidgetNames.FileSave, true);
			SetSensitivity(WidgetNames.FileSaveAs, true);
			SetSensitivity(WidgetNames.FileHeaders, true);
			SetSensitivity(WidgetNames.FileProperties, true);
			SetSensitivity(WidgetNames.FileTranslationNew, true);
			SetSensitivity(WidgetNames.FileTranslationOpen, true);
			/* Edit Menu */
			SetMenuSensitivity(WidgetNames.EditInsertSubtitle, true);
			SetSensitivity(WidgetNames.EditDeleteSubtitles, true);
			/* View Menu */
			SetSensitivity(WidgetNames.ViewTimes, true);
			SetSensitivity(WidgetNames.ViewFrames, true);
			SetViewVideoSubtitlesSensitivity();
			/* Search Menu */
			SetSensitivity(WidgetNames.SearchFind, true);
			SetSensitivity(WidgetNames.SearchReplace, true);
			/* Tools Menu */
			SetSensitivity(WidgetNames.ToolsSetTextLanguage, true);
			SetSensitivity(WidgetNames.ToolsSetTranslationLanguage, false);
			
			/* Toolbar */
			SetSensitivity(WidgetNames.SaveButton, true);
			SetSensitivity(WidgetNames.FindButton, true);
			SetSensitivity(WidgetNames.ReplaceButton, true);
			/* Common for Format Menu and Toolbar*/
			SetStylesSensitivity(true);
		}
		else {
			/* File Menu */
			SetSensitivity(WidgetNames.FileTranslationSave, false);
			SetSensitivity(WidgetNames.FileTranslationSaveAs, false);
			SetSensitivity(WidgetNames.FileTranslationClose, false);
			/* Edit Menu */
			SetSensitivity(WidgetNames.EditUndo, false);
			SetSensitivity(WidgetNames.EditRedo, false);
			SetSensitivity(WidgetNames.EditCut, false);
			SetSensitivity(WidgetNames.EditCopy, false);
			SetSensitivity(WidgetNames.EditPaste, false);
			/* View Menu */
			SetViewVideoSubtitlesSensitivity();
			/* Search Menu */
			SetSensitivity(WidgetNames.SearchFindNext, false);
			SetSensitivity(WidgetNames.SearchFindPrevious, false);
			/* Tools Menu */
			SetSensitivity(WidgetNames.ToolsSetTranslationLanguage, false);
			
			/* Toolbar */
			SetSensitivity(WidgetNames.UndoButton, false);
			SetSensitivity(WidgetNames.RedoButton, false);
			SetSensitivity(WidgetNames.CutButton, false);
			SetSensitivity(WidgetNames.CopyButton, false);
			SetSensitivity(WidgetNames.PasteButton, false);
		}	
	}
	
	private void SetTranslationSensitivity (bool sensitivity) {
		SetSensitivity(WidgetNames.FileTranslationSave, sensitivity);
		SetSensitivity(WidgetNames.FileTranslationSaveAs, sensitivity);
		SetSensitivity(WidgetNames.FileTranslationClose, sensitivity);
		SetSensitivity(WidgetNames.ToolsSetTranslationLanguage, sensitivity);
		SetViewVideoSubtitlesSensitivity();
	}
	
	private void SetFrameRateMenus () {
		if (Global.TimingMode == TimingMode.Frames) {
			SetMenuSensitivity(WidgetNames.TimingsInputFrameRate, true);
			SetMenuSensitivity(WidgetNames.TimingsVideoFrameRate, true);
		}
		else {
			SetMenuSensitivity(WidgetNames.TimingsInputFrameRate, false);
			SetMenuSensitivity(WidgetNames.TimingsVideoFrameRate, true);
		}
		
		UpdateActiveInputFrameRateMenuItem();
		UpdateActiveVideoFrameRateMenuItem();
	}
	
	private void SetStylesActivity (bool bold, bool italic, bool underline) {
		SetCheckMenuItemActivity(WidgetNames.EditFormatBold, bold, Global.Handlers.OnEditFormatBold);
		SetCheckMenuItemActivity(WidgetNames.EditFormatItalic, italic, Global.Handlers.OnEditFormatItalic);
		SetCheckMenuItemActivity(WidgetNames.EditFormatUnderline, underline, Global.Handlers.OnEditFormatUnderline);
		SetToggleToolButtonActivity(WidgetNames.BoldButton, bold, Global.Handlers.OnEditFormatBold);
		SetToggleToolButtonActivity(WidgetNames.ItalicButton, italic, Global.Handlers.OnEditFormatItalic);
		SetToggleToolButtonActivity(WidgetNames.UnderlineButton, underline, Global.Handlers.OnEditFormatUnderline);
	}
	
		
	private void SetViewVideoSubtitlesActivity (bool isTextActive) {
		if (isTextActive)
			SetCheckMenuItemActivity(WidgetNames.ViewVideoSubtitlesText, true);
		else
			SetCheckMenuItemActivity(WidgetNames.ViewVideoSubtitlesTranslation, true);
	}
	
	private void SetVideoSensitivity (bool sensitivity) {
		SetSensitivity(WidgetNames.VideoClose, sensitivity);
		SetSensitivity(WidgetNames.VideoPlayPause, sensitivity);
		SetSensitivity(WidgetNames.VideoRewind, sensitivity);
		SetSensitivity(WidgetNames.VideoForward, sensitivity);
		
		/* Set video menu dependent sensitivity if there is 1 selected subtitle. */
		if ((Global.GUI.View.Selection.Count == 1) && sensitivity)
			SetVideoSelectionDependentSensitivity(true);
		else
			SetVideoSelectionDependentSensitivity(false);
	}
	
	private void SetStylesSensitivity (bool sensitivity) {
		if (Global.GetWidget(WidgetNames.EditFormatBold).Sensitive != sensitivity) {
			SetSensitivity(WidgetNames.EditFormatBold, sensitivity);
			SetSensitivity(WidgetNames.EditFormatItalic, sensitivity);
			SetSensitivity(WidgetNames.EditFormatUnderline, sensitivity);
			SetSensitivity(WidgetNames.BoldButton, sensitivity);
			SetSensitivity(WidgetNames.ItalicButton, sensitivity);
			SetSensitivity(WidgetNames.UnderlineButton, sensitivity);
		}	
	}
	
	/// <summary>Set the video selection dependent menu items.</summary>
	/// <param name="sensitivity">Whether to set the menu items sensitive.</param>
	/// <remarks>The menu items are only set sensitive if the video is loaded.</remarks>
	private void SetVideoSelectionDependentSensitivity (bool sensitivity) {
		if (Global.GUI.Video.IsLoaded && sensitivity) {
			SetSensitivity(WidgetNames.VideoSeekToSelection, true);
			SetSensitivity(WidgetNames.VideoSetSubtitleStart, true);
			SetSensitivity(WidgetNames.VideoSetSubtitleEnd, true);
		}
		else {
			SetSensitivity(WidgetNames.VideoSeekToSelection, false);
			SetSensitivity(WidgetNames.VideoSetSubtitleStart, false);
			SetSensitivity(WidgetNames.VideoSetSubtitleEnd, false);
		}
	}
	
	private void SetViewVideoSubtitlesSensitivity () {
		bool isVideoLoaded = Global.GUI.Video.IsLoaded;
		bool textSensitivity = isVideoLoaded && Global.IsDocumentLoaded;
		bool translationSensitivity = isVideoLoaded && textSensitivity && Global.Document.IsTranslationLoaded;
		SetViewVideoSubtitlesSensitivity(textSensitivity, translationSensitivity);	
	}
	
	private void SetViewVideoSubtitlesSensitivity (bool textSensitivity, bool translationSensitivity) {
		SetSensitivity(WidgetNames.ViewVideoSubtitlesText, textSensitivity);
		SetSensitivity(WidgetNames.ViewVideoSubtitlesTranslation, translationSensitivity);
	}
	
	private void SetCheckMenuItemActivity (string menuItemName, bool isActive) {
		(Global.GetWidget(menuItemName) as CheckMenuItem).Active = isActive;
	}
	
	private void SetCheckMenuItemActivity (string menuItemName, bool isActive, EventHandler handler) {
		CheckMenuItem menuItem = Global.GetWidget(menuItemName) as CheckMenuItem;
		menuItem.Toggled -= handler;
		menuItem.Active = isActive;
		menuItem.Toggled += handler;		
	}
	
	private void SetToggleToolButtonActivity (string toggleToolButtonName, bool isActive, EventHandler handler) {
		ToggleToolButton toggleToolButton = Global.GetWidget(toggleToolButtonName) as ToggleToolButton;
		toggleToolButton.Toggled -= handler;
		toggleToolButton.Active = isActive;
		toggleToolButton.Toggled += handler;		
	}
	
	private void SetSensitivity (string widgetName, bool isSensitive) {
		Widget widget = Global.GetWidget(widgetName);
		if (widget != null)
			widget.Sensitive = isSensitive;
	}
	
	private void SetMenuSensitivity (string menuItemName, bool sensitivity) {
		MenuItem menuItem = Global.GetWidget(menuItemName) as MenuItem;
		Menu menu = menuItem.Submenu as Menu;
		foreach (Widget widget in menu)
			widget.Sensitive = sensitivity;	
	}
		
	private void GetGlobalStyles (TreePath[] paths, out bool bold, out bool italic, out bool underline) {
		Subtitles subtitles = Global.Document.Subtitles;
		bold = true;
		italic = true;
		underline = true;
		foreach (TreePath path in paths) {
			Subtitle subtitle = subtitles[path];
			if ((bold == true) && !subtitle.Style.Bold) //bold hasn't been unset
				bold = false;
			if ((italic == true) && !subtitle.Style.Italic)
				italic = false;
			if ((underline == true) && !subtitle.Style.Underline)
				underline = false;
		}		
	}
	
	private string InputFrameRateMenuItem (float frameRate) {
		return FrameRateToMenuItem(frameRate, "Input");
	}
	
	private string VideoFrameRateMenuItem (float frameRate) {
		return FrameRateToMenuItem(frameRate, "Video");
	}
	
	private string FrameRateToMenuItem (float frameRate, string type) {
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
		
		return "timings" + type + "FrameRate" + rate;
	}
	
	private string GetMenuItemText (string menuItemName) {
		MenuItem menuItem = Global.GetWidget(menuItemName) as MenuItem;
		Label label = menuItem.Child as Label;
		return label.Text;
	}
	
	private void SetMenuItemText (string menuItemName, string text) {
		MenuItem menuItem = Global.GetWidget(menuItemName) as MenuItem;
		Label label = menuItem.Child as Label;
		label.Text = text;	
	}
	
	private void SetToolbarHomogeneity () {
		Toolbar toolbar = Global.GetWidget(WidgetNames.Toolbar) as Toolbar;
		Widget[] toolItems = toolbar.Children;
		foreach (Widget item in toolItems)
			(item as ToolItem).Homogeneous = false;		
	}
	
	private void UpdateUndoRedoMessages () {
    	CommandManager commandManager = Global.CommandManager;
    	
    	/* Update undo messages */
    	ToolButton undoButton = Global.GetWidget(WidgetNames.UndoButton) as ToolButton;
    	if (commandManager.CanUndo) {
    		string undoDescription = commandManager.UndoDescription;
			SetTooltip(undoButton, undoDescription);
    		MenuItem undoMenuItem = Global.GetWidget(WidgetNames.EditUndo) as MenuItem;
    		(undoMenuItem.Child as Label).Text = undoDescription;
    	}
		else
			ClearTooltip(undoButton);
		
		/* Update redo messages */
		ToolButton redoButton = Global.GetWidget(WidgetNames.RedoButton) as ToolButton;
    	if (commandManager.CanRedo) {
	    	string redoDescription = commandManager.RedoDescription;
    		SetTooltip(redoButton, redoDescription);
    		MenuItem redoMenuItem = Global.GetWidget(WidgetNames.EditRedo) as MenuItem;
    		(redoMenuItem.Child as Label).Text = redoDescription;
    	}
    	else
    		ClearTooltip(redoButton);
    }
    
    private void SetTooltip (ToolButton toolButton, string text) {
    	toolButton.SetTooltip(tooltips, text, null);
    }
    
    private void ClearTooltip (ToolButton toolButton) {
    	SetTooltip(toolButton, null);
    }

}

}