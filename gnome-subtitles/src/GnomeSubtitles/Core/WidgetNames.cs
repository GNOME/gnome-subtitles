/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
 * Copyright (C) 2006-2007 Pedro Castro
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

public class WidgetNames {

	/* Window */

	public const string MainPaned = "mainPaned";

	/* Menu Bar */
	
	/* File Menu */
	public const string FileSave = "fileSave";
	public const string FileSaveAs = "fileSaveAs";
	public const string FileHeaders = "fileHeaders";
	
	/* Edit Menu */
	public const string EditUndo = "editUndo";
	public const string EditRedo = "editRedo";
	public const string EditCut = "editCut";
	public const string EditCopy = "editCopy";
	public const string EditPaste = "editPaste";
	public const string EditFormatBold = "editFormatBold";
	public const string EditFormatItalic = "editFormatItalic";
	public const string EditFormatUnderline = "editFormatUnderline";
	public const string EditInsertSubtitle = "editInsertSubtitle";
	public const string EditInsertSubtitleBefore = "editInsertSubtitleBefore";
	public const string EditDeleteSubtitles = "editDeleteSubtitles";
	
	/* View Menu */
	public const string ViewTimes = "viewTimes";
	public const string ViewFrames = "viewFrames";
	public const string ViewVideo = "viewVideo";
	
	/* Search Menu */
	public const string SearchFind = "searchFind";
	public const string SearchFindNext = "searchFindNext";
	public const string SearchFindPrevious = "searchFindPrevious";
	public const string SearchReplace = "searchReplace";

	/* Timings Menu */
	public const string TimingsInputFrameRate = "timingsInputFrameRate";
	public const string TimingsVideoFrameRate = "timingsVideoFrameRate";
	public const string TimingsAdjust = "timingsAdjust";
	public const string TimingsShift = "timingsShift";
	
	/* Video Menu */
	
	public const string VideoClose = "videoClose";
	public const string VideoPlayPause = "videoPlayPause";
	public const string VideoRewind = "videoRewind";
	public const string VideoForward = "videoForward";
	public const string VideoSeekToSelection = "videoSeekToSelection";
	public const string VideoSetSubtitleStart = "videoSetSubtitleStart";
	public const string VideoSetSubtitleEnd = "videoSetSubtitleEnd";


	/* Tool Bar */
	
	public const string Toolbar = "toolbar";
	public const string SaveButton = "saveToolButton";
	public const string UndoButton = "undoToolButton";
	public const string RedoButton = "redoToolButton";
	public const string CutButton = "cutToolButton";
	public const string CopyButton = "copyToolButton";
	public const string PasteButton = "pasteToolButton";
	public const string FindButton = "findToolButton";
	public const string ReplaceButton = "replaceToolButton";
	public const string BoldButton = "boldToolButton";
	public const string ItalicButton = "italicToolButton";
	public const string UnderlineButton = "underlineToolButton";
	
	/* Video */
	
	public const string VideoAreaHBox = "videoAreaHBox";
	public const string VideoTimingsVBox = "videoTimingsVBox";
	public const string VideoSubtitleLabel = "videoSubtitle";
	public const string VideoSubtitleLabelEventBox = "videoSubtitleEventBox";
	public const string VideoPositionLabel = "videoControlsPositionLabel";
	public const string VideoPositionValueLabel = "videoControlsPositionValueLabel";
	public const string VideoLengthValueLabel = "videoControlsLengthValueLabel";
	public const string VideoSetSubtitleStartButton = "videoSetSubtitleStartButton";
	public const string VideoSetSubtitleStartButtonImage = "videoSetSubtitleStartButtonImage";
	public const string VideoSetSubtitleEndButton = "videoSetSubtitleEndButton";
	public const string VideoSetSubtitleEndButtonImage = "videoSetSubtitleEndButtonImage";
	public const string VideoFrame = "videoFrame";
	public const string VideoPlaybackHBox = "videoPlaybackHBox";
	public const string VideoSlider = "videoSlider";
	public const string VideoPlayPauseButton = "videoPlayPauseButton";
	
	/* Subtitle View */
	
	public const string SubtitleView = "subtitleView";
	
	/* Subtitle Edit */
	
	public const string SubtitleEdit = "subtitleEdit";
	public const string StartSpinButton = "startSpinButton";
	public const string EndSpinButton = "endSpinButton";
	public const string DurationSpinButton = "durationSpinButton";
	public const string SubtitleTextView = "subtitleTextView";
	
}

}