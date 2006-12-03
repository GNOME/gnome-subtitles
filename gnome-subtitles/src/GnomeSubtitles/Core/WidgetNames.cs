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

namespace GnomeSubtitles {

//TODO this shouldn't need to exist
public class WidgetNames {
	public const string MainWindow = "mainWindow";
	
	/* File Menu */
	public const string SaveMenuItem = "saveMenuItem";
	public const string SaveAsMenuItem = "saveAsMenuItem";
	public const string HeadersMenuItem = "headersMenuItem";
	
	/* Edit Menu */
	public const string UndoMenuItem = "undoMenuItem";
	public const string RedoMenuItem = "redoMenuItem";
	public const string CutMenuItem = "cutMenuItem";
	public const string CopyMenuItem = "copyMenuItem";
	public const string PasteMenuItem = "pasteMenuItem";
	public const string InsertSubtitleMenuItem = "insertSubtitleMenuItem";
	public const string DeleteSubtitlesMenuItem = "deleteSubtitlesMenuItem";
	
	/* View Menu */
	public const string TimesMenuItem = "timesMenuItem";
	public const string FramesMenuItem = "framesMenuItem";
	
	/* Search Menu */
	public const string FindMenuItem = "findMenuItem";
	public const string FindNextMenuItem = "findNextMenuItem";
	public const string FindPreviousMenuItem = "findPreviousMenuItem";
	public const string ReplaceMenuItem = "replaceMenuItem";
	
	/* Format Menu */
	public const string BoldMenuItem = "boldMenuItem";
	public const string ItalicMenuItem = "italicMenuItem";
	public const string UnderlineMenuItem = "underlineMenuItem";

	/* Timings Menu */
	public const string InputFrameRateMenuItem = "inputFrameRateMenuItem";
	public const string MovieFrameRateMenuItem = "movieFrameRateMenuItem";
	public const string AdjustTimingsMenuItem = "adjustTimingsMenuItem";
	public const string ShiftMenuItem = "shiftTimingsMenuItem";
	
	/* Toolbar */
	public const string Toolbar = "toolbar";
	public const string SaveButton = "toolbuttonSave";
	public const string UndoButton = "toolbuttonUndo";
	public const string RedoButton = "toolbuttonRedo";
	public const string CutButton = "toolbuttonCut";
	public const string CopyButton = "toolbuttonCopy";
	public const string PasteButton = "toolbuttonPaste";
	public const string BoldButton = "toggletoolbuttonBold";
	public const string ItalicButton = "toggletoolbuttonItalic";
	public const string UnderlineButton = "toggletoolbuttonUnderline";
	
	
	/* Subtitle View */
	
	public const string SubtitleView = "subtitleListView";
	
	/* Subtitle Edit */
	public const string SubtitleEditHBox = "editAreaHBox";
	public const string StartSpinButton = "startSpinButton";
	public const string EndSpinButton = "endSpinButton";
	public const string DurationSpinButton = "durationSpinButton";
	public const string SubtitleTextView = "subtitleTextView";

	
}

}