/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2017 Pedro Castro
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

using GnomeSubtitles.Core;
using GnomeSubtitles.Core.Command;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;
using System.Globalization;

namespace GnomeSubtitles.Ui {

public class Menus {

	/* Constant strings */
	private string videoTagText = Catalog.GetString("Video");

	public Menus () {
		SetDocumentSensitivity(false);
		SetBlankActivity();

		Base.InitFinished += OnBaseInitFinished;
	}


	/* Public properties */

	public float TimingsInputFrameRateActive {
		get {
			Menu menu = Base.GetWidget(WidgetNames.TimingsInputFrameRateMenu) as Menu;
			foreach (RadioMenuItem menuItem in menu.Children) {
				if (menuItem.Active) {
					return FrameRateFromMenuItem((menuItem.Child as Label).Text);
				}
			}
			return -1;
		}
	}


	/* Public methods */

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

	public void UpdateActiveInputFrameRateMenuItem (bool toggleHandlers) {
		float inputFrameRate = Base.Document.Subtitles.Properties.OriginalFrameRate;
		if (toggleHandlers)
			SetCheckMenuItemActivity(InputFrameRateMenuItem(inputFrameRate), true, OnTimingsInputFrameRateToggled);
		else
			SetCheckMenuItemActivity(InputFrameRateMenuItem(inputFrameRate), true);
	}

	/* Note: this method makes the assumption that the OnTimingsVideoFrameRateToggled event handler is connected to the
	   menu item, as it is disconnected and then connected again after changing the active menu item. */
	public void UpdateActiveVideoFrameRateMenuItem () {
		float videoFrameRate = Base.Document.Subtitles.Properties.CurrentFrameRate;
		SetCheckMenuItemActivity(VideoFrameRateMenuItem(videoFrameRate), true, OnTimingsVideoFrameRateToggled);
	}

	public void EnableFindNextPrevious () {
		SetSensitivity(WidgetNames.SearchFindNext, true);
		SetSensitivity(WidgetNames.SearchFindPrevious, true);
	}

	public void AddFrameRateVideoTag (float frameRate) {
		if (frameRate <= 0)
			return;

		string menuItemName = FrameRateToMenuItem(frameRate, "Video");
		string menuItemText = GetMenuItemText(menuItemName);

		menuItemText += " (" + videoTagText + ")";
		SetMenuItemText(menuItemName, menuItemText);
	}

	public void RemoveFrameRateVideoTag () {
		Menu menu = Base.GetWidget(WidgetNames.TimingsVideoFrameRateMenu) as Menu;
		foreach (Widget child in menu.Children) {
			if (!(child is MenuItem))
				continue;

			MenuItem menuItem = child as MenuItem;
			string text = GetMenuItemText(menuItem);
			string videoTagSuffix = GetVideoTagSuffix();

			int tagIndex = text.LastIndexOf(videoTagSuffix);
			if (tagIndex > 0) {
				text = text.Substring(0, tagIndex);
				SetMenuItemText(menuItem, text);
			}
		}
	}

	public MenuItem GetMenuItem (string widgetName) {
		return Base.GetWidget(WidgetNames.ViewLineLengths) as MenuItem;
	}


	/* Private members */

	/// <summary>Sets the sensitivity depending on 1 or more selected subtitles.</summary>
	/// <param name="selectionCount">The number of selected subtitles.</param>
	/// <param name="sensitivity">Whether the items are set sensitive.</param>
	private void SetNonZeroSelectionDependentSensitivity (int selectionCount, bool sensitivity) {
		SetStylesSensitivity(sensitivity);
		SetSensitivity(WidgetNames.EditDeleteSubtitles, sensitivity);
		SetSensitivity(WidgetNames.EditSplit, sensitivity);
		SetSensitivity(WidgetNames.EditMerge, selectionCount >= 2);
		SetSensitivity(WidgetNames.DeleteSubtitlesButton, sensitivity);
	}

	/// <summary>Sets the sensitivity depending on exactly 1 selected subtitle.</summary>
	/// <param name="sensitivity">Whether the items are set sensitive.</param>
	private void SetOneSelectionDependentSensitivity (bool sensitivity) {
		SetVideoSelectionDependentSensitivity(sensitivity);
		SetSensitivity(WidgetNames.ToolsTranslateText, sensitivity && Base.Document.IsTranslationLoaded);
		SetSensitivity(WidgetNames.ToolsTranslateTranslation, sensitivity && Base.Document.IsTranslationLoaded);
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

	private void SetBlankActivity () {
		SetCheckMenuItemActivity(WidgetNames.ToolsAutocheckSpelling, Base.SpellLanguages.Enabled);
		SetCheckMenuItemActivity(WidgetNames.ViewLineLengths, Base.Config.ViewLineLengths);
	}

	private void SetViewVideoActivity (bool activity) {
		SetCheckMenuItemActivity(WidgetNames.ViewVideo, activity);
	}

	private void SetDocumentSensitivity (bool documentLoaded) {
		/* Set Sensitivity that corresponds to the document loaded status */

		/* File Menu */
		SetSensitivity(WidgetNames.FileSave, documentLoaded);
		SetSensitivity(WidgetNames.FileSaveAs, documentLoaded);
		SetSensitivity(WidgetNames.FileHeaders, documentLoaded);
		SetSensitivity(WidgetNames.FileProperties, documentLoaded);
		SetSensitivity(WidgetNames.FileTranslationNew, documentLoaded);
		SetSensitivity(WidgetNames.FileTranslationOpen, documentLoaded);
		SetSensitivity(WidgetNames.FileClose, documentLoaded);
		/* Edit Menu */
		SetMenuSensitivity(WidgetNames.EditInsertSubtitleMenu, documentLoaded);
		/* View Menu */
		SetSensitivity(WidgetNames.ViewTimes, documentLoaded); //TODO always visible
		SetSensitivity(WidgetNames.ViewFrames, documentLoaded); //TODO always visible
		SetViewVideoSubtitlesSensitivity();
		/* Search Menu */
		SetSensitivity(WidgetNames.SearchFind, documentLoaded);
		SetSensitivity(WidgetNames.SearchReplace, documentLoaded);
		/* Timings Menu */
		SetSensitivity(WidgetNames.TimingsSynchronize, documentLoaded);
		/* Video Menu */
		SetVideoDocumentLoadedSensitivity(documentLoaded);
		/* Tools Menu */
		SetToolsAutocheckSpellingSensitivity(documentLoaded);
		SetSensitivity(WidgetNames.ToolsSetLanguages, documentLoaded);
		/* Toolbar */
		SetSensitivity(WidgetNames.SaveButton, documentLoaded);
		SetSensitivity(WidgetNames.InsertSubtitleButton, documentLoaded);

		/* Set sensitivity that's only applicable when the document is not loaded */

		if (!documentLoaded) {
			/* Edit menu */
			SetSensitivity(WidgetNames.EditDeleteSubtitles, false);
			SetSensitivity(WidgetNames.EditUndo, false);
			SetSensitivity(WidgetNames.EditRedo, false);
			SetSensitivity(WidgetNames.EditCut, false);
			SetSensitivity(WidgetNames.EditCopy, false);
			SetSensitivity(WidgetNames.EditPaste, false);
			SetSensitivity(WidgetNames.EditSplit, false);
			SetSensitivity(WidgetNames.EditMerge, false);
			/* Search menu */
			SetSensitivity(WidgetNames.SearchFindNext, false);
			SetSensitivity(WidgetNames.SearchFindPrevious, false);
			/* Timings Menu */
			SetSensitivity(WidgetNames.TimingsAdjust, false);
			SetSensitivity(WidgetNames.TimingsShift, false);
			/* Toolbar */
			SetSensitivity(WidgetNames.DeleteSubtitlesButton, false);
			SetSensitivity(WidgetNames.UndoButton, false);
			SetSensitivity(WidgetNames.RedoButton, false);
			SetSensitivity(WidgetNames.CutButton, false);
			SetSensitivity(WidgetNames.CopyButton, false);
			SetSensitivity(WidgetNames.PasteButton, false);
			/* Common for Format Menu and Toolbar */
			SetStylesSensitivity(false);
		}
	}

	private void SetTranslationSensitivity (bool sensitivity) {
		SetSensitivity(WidgetNames.FileTranslationSave, sensitivity);
		SetSensitivity(WidgetNames.FileTranslationSaveAs, sensitivity);
		SetSensitivity(WidgetNames.FileTranslationClose, sensitivity);
		SetSensitivity(WidgetNames.FileTranslationReload, sensitivity);

		bool oneSubtitleSelected = (Core.Base.Ui.View.Selection.Count == 1);
		SetSensitivity(WidgetNames.ToolsTranslateText, sensitivity && oneSubtitleSelected);
		SetSensitivity(WidgetNames.ToolsTranslateTranslation, sensitivity && oneSubtitleSelected);
		SetViewVideoSubtitlesSensitivity();
	}

	private void SetToolsAutocheckSpellingSensitivity (bool sensitivity) {
		SetSensitivity(WidgetNames.ToolsAutocheckSpelling, sensitivity);
	}

	private void SetFrameRateMenus (bool documentLoaded) {
		if (documentLoaded) {
			if (Base.TimingMode == TimingMode.Frames) {
				UpdateActiveInputFrameRateMenuItem(false);
				SetMenuSensitivity(WidgetNames.TimingsInputFrameRateMenu, true);
				SetFrameRateMenuHandlers(Base.GetWidget(WidgetNames.TimingsInputFrameRateMenu) as Menu, true, OnTimingsInputFrameRateToggled);

				SetMenuSensitivity(WidgetNames.TimingsVideoFrameRateMenu, true);
				SetFrameRateMenuHandlers(Base.GetWidget(WidgetNames.TimingsVideoFrameRateMenu) as Menu, true, OnTimingsVideoFrameRateToggled);
				UpdateActiveVideoFrameRateMenuItem();
			}
			else {
				UpdateActiveInputFrameRateMenuItem(false);
				SetMenuSensitivity(WidgetNames.TimingsInputFrameRateMenu, false);

				SetMenuSensitivity(WidgetNames.TimingsVideoFrameRateMenu, true);
				SetFrameRateMenuHandlers(Base.GetWidget(WidgetNames.TimingsVideoFrameRateMenu) as Menu, true, OnTimingsVideoFrameRateToggled);
				UpdateActiveVideoFrameRateMenuItem();
			}
		}
		else {
			SetFrameRateMenuHandlers(Base.GetWidget(WidgetNames.TimingsInputFrameRateMenu) as Menu, false, OnTimingsInputFrameRateToggled);
			SetMenuSensitivity(WidgetNames.TimingsInputFrameRateMenu, true);
			SetFrameRateMenuHandlers(Base.GetWidget(WidgetNames.TimingsVideoFrameRateMenu) as Menu, false, OnTimingsVideoFrameRateToggled);
			SetMenuSensitivity(WidgetNames.TimingsVideoFrameRateMenu, false);
		}
	}

	private void SetStylesActivity (bool bold, bool italic, bool underline) {
		SetCheckMenuItemActivity(WidgetNames.EditFormatBold, bold, Base.Handlers.OnEditFormatBold);
		SetCheckMenuItemActivity(WidgetNames.EditFormatItalic, italic, Base.Handlers.OnEditFormatItalic);
		SetCheckMenuItemActivity(WidgetNames.EditFormatUnderline, underline, Base.Handlers.OnEditFormatUnderline);
		SetToggleToolButtonActivity(WidgetNames.BoldButton, bold, Base.Handlers.OnEditFormatBold);
		SetToggleToolButtonActivity(WidgetNames.ItalicButton, italic, Base.Handlers.OnEditFormatItalic);
		SetToggleToolButtonActivity(WidgetNames.UnderlineButton, underline, Base.Handlers.OnEditFormatUnderline);
	}

	private void SetActiveTimingMode (TimingMode mode) {
		if (mode == TimingMode.Times)
			SetCheckMenuItemActivity(WidgetNames.ViewTimes, true);
		else
			SetCheckMenuItemActivity(WidgetNames.ViewFrames, true);
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
		SetSensitivity(WidgetNames.VideoSeekTo, sensitivity);
		SetVideoDocumentLoadedSensitivity(Base.IsDocumentLoaded);

		/* Set video menu dependent sensitivity if there is 1 selected subtitle. */
		if ((Core.Base.Ui.View.Selection.Count == 1) && sensitivity)
			SetVideoSelectionDependentSensitivity(true);
		else
			SetVideoSelectionDependentSensitivity(false);
	}

	private void SetStylesSensitivity (bool sensitivity) {
		if (Base.GetWidget(WidgetNames.EditFormatBold).Sensitive != sensitivity) {
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
		if (Core.Base.Ui.Video.IsLoaded && sensitivity) {//TODO improve this
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
		bool isVideoLoaded = (Base.Ui != null) && Base.Ui.Video.IsLoaded;
		bool textSensitivity = isVideoLoaded && Base.IsDocumentLoaded;
		bool translationSensitivity = isVideoLoaded && textSensitivity && Base.Document.IsTranslationLoaded;
		SetViewVideoSubtitlesSensitivity(textSensitivity, translationSensitivity);
	}

	private void SetViewVideoSubtitlesSensitivity (bool textSensitivity, bool translationSensitivity) {
		SetSensitivity(WidgetNames.ViewVideoSubtitlesText, textSensitivity);
		SetSensitivity(WidgetNames.ViewVideoSubtitlesTranslation, translationSensitivity);
	}

	private void SetVideoDocumentLoadedSensitivity (bool isDocumentLoaded) {
		bool sensitivity = isDocumentLoaded && (Base.Ui != null) && Base.Ui.Video.IsLoaded;
		SetSensitivity(WidgetNames.VideoSelectNearestSubtitle, sensitivity);
		SetSensitivity(WidgetNames.VideoLoopSelectionPlayback, sensitivity);
		SetSensitivity(WidgetNames.VideoAutoSelectSubtitles, sensitivity);
		SetSensitivity(WidgetNames.EditInsertSubtitleAtVideoPosition, sensitivity);
	}

	private void SetCheckMenuItemActivity (string menuItemName, bool isActive) {
		(Base.GetWidget(menuItemName) as CheckMenuItem).Active = isActive;
	}

	private void SetCheckMenuItemActivity (string menuItemName, bool isActive, EventHandler handler) {
		CheckMenuItem menuItem = Base.GetWidget(menuItemName) as CheckMenuItem;
		menuItem.Toggled -= handler;
		menuItem.Active = isActive;
		menuItem.Toggled += handler;
	}

	private void SetToggleToolButtonActivity (string toggleToolButtonName, bool isActive, EventHandler handler) {
		ToggleToolButton toggleToolButton = Base.GetWidget(toggleToolButtonName) as ToggleToolButton;
		toggleToolButton.Toggled -= handler;
		toggleToolButton.Active = isActive;
		toggleToolButton.Toggled += handler;
	}

	private void SetSensitivity (string widgetName, bool isSensitive) {
		Widget widget = Base.GetWidget(widgetName);
		if (widget != null)
			widget.Sensitive = isSensitive;
	}

	private void SetMenuSensitivity (string menuName, bool sensitivity) {
		Menu menu = Base.GetWidget(menuName) as Menu;
		foreach (Widget widget in menu)
			widget.Sensitive = sensitivity;
	}

	private void GetGlobalStyles (TreePath[] paths, out bool bold, out bool italic, out bool underline) {
		Ui.View.Subtitles subtitles = Base.Document.Subtitles;
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
		MenuItem menuItem = Base.GetWidget(menuItemName) as MenuItem;
		return GetMenuItemText(menuItem);
	}

	private string GetMenuItemText (MenuItem menuItem) {
		Label label = menuItem.Child as Label;
		return label.Text;
	}

	private void SetMenuItemText (string menuItemName, string text) {
		MenuItem menuItem = Base.GetWidget(menuItemName) as MenuItem;
		SetMenuItemText(menuItem, text);
	}

	private void SetMenuItemText (MenuItem menuItem, string text) {
		Label label = menuItem.Child as Label;
		label.Text = text;
	}

	private string GetVideoTagSuffix () {
		return " (" + videoTagText + ")";
	}

	private void UpdateUndoRedoMessages () {
    	CommandManager commandManager = Base.CommandManager;

    	/* Update undo messages */
    	ToolButton undoButton = Base.GetWidget(WidgetNames.UndoButton) as ToolButton;
    	if (commandManager.CanUndo) {
    		string undoDescription = commandManager.UndoDescription;
			SetTooltip(undoButton, undoDescription);
    		MenuItem undoMenuItem = Base.GetWidget(WidgetNames.EditUndo) as MenuItem;
    		(undoMenuItem.Child as Label).Text = undoDescription;
    	}
		else
			ClearTooltip(undoButton);

		/* Update redo messages */
		ToolButton redoButton = Base.GetWidget(WidgetNames.RedoButton) as ToolButton;
    	if (commandManager.CanRedo) {
	    	string redoDescription = commandManager.RedoDescription;
    		SetTooltip(redoButton, redoDescription);
    		MenuItem redoMenuItem = Base.GetWidget(WidgetNames.EditRedo) as MenuItem;
    		(redoMenuItem.Child as Label).Text = redoDescription;
    	}
    	else
    		ClearTooltip(redoButton);
    }

    private void SetTooltip (Widget widget, string text) {
    	widget.TooltipText = text;
    }

    private void ClearTooltip (Widget widget) {
    	SetTooltip(widget, null);
    }

   	private float FrameRateFromMenuItem (string menuItem) {
		string frameRateText = menuItem.Split(' ')[0];
		NumberFormatInfo invariant = NumberFormatInfo.InvariantInfo;
		return (float)Convert.ToDouble(frameRateText, invariant);
	}


	/* Event members */

	private void OnBaseInitFinished () {
		Base.DocumentLoaded += OnBaseDocumentLoaded;
		Base.DocumentUnloaded += OnBaseDocumentUnloaded;
		Base.VideoLoaded += OnBaseVideoLoaded;
		Base.VideoUnloaded += OnBaseVideoUnloaded;
		Base.TranslationLoaded += OnBaseTranslationLoaded;
		Base.TranslationUnloaded += OnBaseTranslationUnloaded;
		Base.Ui.View.Selection.Changed += OnSubtitleViewSelectionChanged;
		Base.Ui.View.SubtitleCountChanged += OnSubtitleViewCountChanged;
		Base.SpellLanguages.TextLanguageChanged += OnSpellLanguagesLanguageChanged;
		Base.SpellLanguages.TranslationLanguageChanged += OnSpellLanguagesLanguageChanged;
		Base.CommandManager.UndoToggled += OnCommandManagerUndoToggled;
		Base.CommandManager.RedoToggled += OnCommandManagerRedoToggled;
		Base.CommandManager.CommandActivated += OnCommandManagerCommandActivated;
	}

	private void OnBaseDocumentLoaded (Document document) {
		SetDocumentSensitivity(true);
		SetFrameRateMenus(true);
		SetActiveTimingMode(Base.TimingMode);
		SetCheckMenuItemActivity(WidgetNames.ToolsAutocheckSpelling, Base.SpellLanguages.Enabled);
	}

	private void OnBaseDocumentUnloaded (Document document) {
		SetDocumentSensitivity(false);
		SetFrameRateMenus(false);
	}

	private void OnBaseVideoLoaded (Uri videoUri) {
		SetVideoSensitivity(true);
    	SetViewVideoSubtitlesSensitivity();
    	SetViewVideoActivity(true);

		if (Base.Ui.Video.HasVideo) {
			AddFrameRateVideoTag(Base.Ui.Video.FrameRate);
		}
	}

	private void OnBaseVideoUnloaded () {
		SetVideoSensitivity(false);
    	SetViewVideoSubtitlesSensitivity(false, false);
	}

	private void OnSubtitleViewCountChanged (int count) {
		SetSubtitleCountDependentSensitivity(count);
	}

	private void OnBaseTranslationLoaded () {
		SetTranslationSensitivity(true);
		UpdateUndoRedoMessages();
	}

	private void OnBaseTranslationUnloaded () {
		SetTranslationSensitivity(false);
    	SetViewVideoSubtitlesActivity(true);
    	UpdateUndoRedoMessages();
	}

	private void OnSubtitleViewSelectionChanged (TreePath[] paths, Subtitle subtitle) {
		if (subtitle != null) {
			/* One subtitle selected */
			SetStylesActivity(subtitle.Style.Bold, subtitle.Style.Italic, subtitle.Style.Underline);
			SetNonZeroSelectionDependentSensitivity(1, true);
			SetOneSelectionDependentSensitivity(true);
		}
		else {
			SetOneSelectionDependentSensitivity(false);
			int selectionCount = paths.Length;

			if (selectionCount == 0) {
				/* No selection */
				SetNonZeroSelectionDependentSensitivity(selectionCount, false);
				SetStylesActivity(false, false, false);
			}
			else {
				/* Multiple paths selected */
				SetNonZeroSelectionDependentSensitivity(selectionCount, true);
				bool bold, italic, underline;
				GetGlobalStyles(paths, out bold, out italic, out underline);
				SetStylesActivity(bold, italic, underline);
			}
		}
	}

	private void OnSpellLanguagesLanguageChanged () {
		SetToolsAutocheckSpellingSensitivity(true);
	}

	private void OnCommandManagerUndoToggled (object o, EventArgs args)  {
   		Widget button = Base.GetWidget(WidgetNames.UndoButton);
   		button.Sensitive = !button.Sensitive;

		MenuItem menuItem = Base.GetWidget(WidgetNames.EditUndo) as MenuItem;
		menuItem.Sensitive = !menuItem.Sensitive;
		if (!menuItem.Sensitive)
			(menuItem.Child as Label).Text = Catalog.GetString("Undo");
	}

	private void OnCommandManagerRedoToggled (object o, EventArgs args) {
    	Widget button = Base.GetWidget(WidgetNames.RedoButton);
    	button.Sensitive = !button.Sensitive;

		MenuItem menuItem = Base.GetWidget(WidgetNames.EditRedo) as MenuItem;
    	menuItem.Sensitive = !menuItem.Sensitive;
    	if (!menuItem.Sensitive)
			(menuItem.Child as Label).Text = Catalog.GetString("Redo");
    }

    private void OnCommandManagerCommandActivated (object o, CommandActivatedArgs args) {
    	UpdateUndoRedoMessages();
    }


	private void OnTimingsInputFrameRateToggled (object o, EventArgs args) {
		RadioMenuItem menuItem = o as RadioMenuItem;
		if (menuItem.Active) {
			float frameRate = FrameRateFromMenuItem((menuItem.Child as Label).Text);
			Base.CommandManager.Execute(new ChangeInputFrameRateCommand(frameRate));
		}
	}

	private void OnTimingsVideoFrameRateToggled (object o, EventArgs args) {
		RadioMenuItem menuItem = o as RadioMenuItem;
		if (menuItem.Active) {
			float frameRate = FrameRateFromMenuItem((menuItem.Child as Label).Text);
			Base.CommandManager.Execute(new ChangeVideoFrameRateCommand(frameRate));
		}
	}

	private void SetFrameRateMenuHandlers (Menu menu, bool enable, EventHandler handler) {
		foreach (RadioMenuItem menuItem in menu.Children) {
			if (enable)
				menuItem.Toggled += handler;
			else
				menuItem.Toggled -= handler;
		}
	}

}

}
