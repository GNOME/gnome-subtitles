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

using System;
using Glade;
using Gtk;
using SubLib;

namespace GnomeSubtitles {


public class HeadersDialog : GladeDialog {
	private SubtitleHeaders subtitleHeaders = null;

	/* Constant strings */
	private const string dialogName = "headersDialog";

	/* Widgets */
	
	/* MPSub fields */
	[WidgetAttribute]
	private Entry entryMPSubTitle;
	[WidgetAttribute]
	private Entry entryMPSubFile;
	[WidgetAttribute]
	private Entry entryMPSubAuthor;
	[WidgetAttribute]
	private Entry entryMPSubNote;
	[WidgetAttribute]
	private ComboBox comboBoxMPSubType;

	/* SubStationAlphaASS fields */
	[WidgetAttribute]
	private Entry entrySSAASSTitle;
	[WidgetAttribute]
	private Entry entrySSAASSOriginalScript;
	[WidgetAttribute]
	private Entry entrySSAASSOriginalTranslation;
	[WidgetAttribute]
	private Entry entrySSAASSOriginalEditing;
	[WidgetAttribute]
	private Entry entrySSAASSOriginalTiming;
	[WidgetAttribute]
	private Entry entrySSAASSOriginalScriptChecking;
	[WidgetAttribute]
	private Entry entrySSAASSScriptUpdatedBy;
	[WidgetAttribute]
	private Entry entrySSAASSCollisions;
	[WidgetAttribute]
	private Entry entrySSAASSTimer;
	[WidgetAttribute]
	private SpinButton spinButtonSSAASSPlayResX;
	[WidgetAttribute]
	private SpinButton spinButtonSSAASSPlayResY;
	[WidgetAttribute]
	private SpinButton spinButtonSSAASSPlayDepth;
	
	/* SubViewer1 fields */
	[WidgetAttribute]
	private Entry entrySubViewer1Title;
	[WidgetAttribute]
	private Entry entrySubViewer1Author;
	[WidgetAttribute]
	private Entry entrySubViewer1Source;
	[WidgetAttribute]
	private Entry entrySubViewer1Program;
	[WidgetAttribute]
	private Entry entrySubViewer1FilePath;
	[WidgetAttribute]
	private SpinButton spinButtonSubViewer1Delay;
	[WidgetAttribute]
	private SpinButton spinButtonSubViewer1CDTrack;
	
	/* SubViewer2 fields */
	[WidgetAttribute]
	private Entry entrySubViewer2Title;
	[WidgetAttribute]
	private Entry entrySubViewer2Author;
	[WidgetAttribute]
	private Entry entrySubViewer2Source;
	[WidgetAttribute]
	private Entry entrySubViewer2Program;
	[WidgetAttribute]
	private Entry entrySubViewer2FilePath;
	[WidgetAttribute]
	private Entry entrySubViewer2Comment;
	[WidgetAttribute]
	private Entry entrySubViewer2FontName;
	[WidgetAttribute]
	private Entry entrySubViewer2FontColor;
	[WidgetAttribute]
	private Entry entrySubViewer2FontStyle;
	[WidgetAttribute]
	private SpinButton spinButtonSubViewer2Delay;
	[WidgetAttribute]
	private SpinButton spinButtonSubViewer2CDTrack;
	[WidgetAttribute]
	private SpinButton spinButtonSubViewer2FontSize;
	
	
	public HeadersDialog () : base(dialogName) {
		subtitleHeaders = Global.Subtitles.Properties.Headers;
		LoadHeaders();
	}
	
	
	/* Private members */
	
	private void LoadHeaders () {
		LoadMPSubHeaders();
		LoadSSAASSHeaders();
		LoadSubViewer1Headers();
		LoadSubViewer2Headers();
	}
	
	private void LoadMPSubHeaders () {
		SubtitleHeadersMPSub headers = subtitleHeaders.MPSub;
	
		entryMPSubTitle.Text = headers.Title;
		entryMPSubFile.Text = headers.File;
		entryMPSubAuthor.Text = headers.Author;
		entryMPSubNote.Text = headers.Note;

		comboBoxMPSubType.Active = (headers.MediaType == "AUDIO" ? 0 : 1); 
	}
	
	private void LoadSSAASSHeaders () {
		SubtitleHeadersSubStationAlphaASS headers = subtitleHeaders.SubStationAlphaASS;
	
		entrySSAASSTitle.Text = headers.Title;	
		entrySSAASSOriginalScript.Text = headers.OriginalScript;	
		entrySSAASSOriginalTranslation.Text = headers.OriginalTranslation;
		entrySSAASSOriginalEditing.Text = headers.OriginalEditing;	
		entrySSAASSOriginalTiming.Text = headers.OriginalTiming;
		entrySSAASSOriginalScriptChecking.Text = headers.OriginalScriptChecking;	
		entrySSAASSScriptUpdatedBy.Text = headers.ScriptUpdatedBy;	
		entrySSAASSCollisions.Text = headers.Collisions;	
		entrySSAASSTimer.Text = headers.Timer;

		spinButtonSSAASSPlayResX.Value = headers.PlayResX;
		spinButtonSSAASSPlayResY.Value = headers.PlayResY;
		spinButtonSSAASSPlayDepth.Value = headers.PlayDepth;	
	}
	
	private void LoadSubViewer1Headers () {
		SubtitleHeadersSubViewer1 headers = subtitleHeaders.SubViewer1;
	
		entrySubViewer1Title.Text = headers.Title;	
	 	entrySubViewer1Author.Text = headers.Author;
	 	entrySubViewer1Source.Text = headers.Source;
	 	entrySubViewer1Program.Text = headers.Program;
	 	entrySubViewer1FilePath.Text = headers.FilePath;
	 	
		spinButtonSubViewer1Delay.Value = headers.Delay;
		spinButtonSubViewer1CDTrack.Value = headers.CDTrack;
	}
	
	private void LoadSubViewer2Headers () {
		SubtitleHeadersSubViewer2 headers = subtitleHeaders.SubViewer2;
	
		entrySubViewer2Title.Text = headers.Title;	
	 	entrySubViewer2Author.Text = headers.Author;
	 	entrySubViewer2Source.Text = headers.Source;
	 	entrySubViewer2Program.Text = headers.Program;
	 	entrySubViewer2FilePath.Text = headers.FilePath;
	 	entrySubViewer2Comment.Text = headers.Comment;	
		entrySubViewer2FontName.Text = headers.FontName;	
		entrySubViewer2FontColor.Text = headers.FontColor;	
		entrySubViewer2FontStyle.Text = headers.FontStyle;
	 	
		spinButtonSubViewer2Delay.Value = headers.Delay;
		spinButtonSubViewer2CDTrack.Value = headers.CDTrack;
		spinButtonSubViewer2FontSize.Value = headers.FontSize;
	}
	
	private void StoreHeaders () {
		StoreMPSubHeaders();
		StoreSSAASSHeaders();
		StoreSubViewer1Headers();
		StoreSubViewer2Headers();
	}
	
	private void StoreMPSubHeaders () {
		SubtitleHeadersMPSub headers = subtitleHeaders.MPSub;
	
		headers.Title = entryMPSubTitle.Text;
		headers.File = entryMPSubFile.Text;
		headers.Author = entryMPSubAuthor.Text;
		headers.Note = entryMPSubNote.Text;

		headers.MediaType = comboBoxMPSubType.ActiveText; 
	}
	
	private void StoreSSAASSHeaders () {
		SubtitleHeadersSubStationAlphaASS headers = subtitleHeaders.SubStationAlphaASS;
	
		headers.Title = entrySSAASSTitle.Text;	
		headers.OriginalScript = entrySSAASSOriginalScript.Text;	
		headers.OriginalTranslation = entrySSAASSOriginalTranslation.Text;
		headers.OriginalEditing = entrySSAASSOriginalEditing.Text;	
		headers.OriginalTiming = entrySSAASSOriginalTiming.Text;
		headers.OriginalScriptChecking = entrySSAASSOriginalScriptChecking.Text;	
		headers.ScriptUpdatedBy = entrySSAASSScriptUpdatedBy.Text;
		headers.Collisions = entrySSAASSCollisions.Text;
		headers.Timer = entrySSAASSTimer.Text;

		headers.PlayResX = spinButtonSSAASSPlayResX.ValueAsInt;
		headers.PlayResY = spinButtonSSAASSPlayResY.ValueAsInt;
		headers.PlayDepth = spinButtonSSAASSPlayDepth.ValueAsInt;
	}
	
	private void StoreSubViewer1Headers () {
		SubtitleHeadersSubViewer1 headers = subtitleHeaders.SubViewer1;
	
		headers.Title = entrySubViewer1Title.Text;	
	 	headers.Author = entrySubViewer1Author.Text;
	 	headers.Source = entrySubViewer1Source.Text;
	 	headers.Program = entrySubViewer1Program.Text;
	 	headers.FilePath = entrySubViewer1FilePath.Text;
	 	
		headers.Delay = spinButtonSubViewer1Delay.ValueAsInt;
		headers.CDTrack = spinButtonSubViewer1CDTrack.ValueAsInt;
	}
	
	private void StoreSubViewer2Headers () {
		SubtitleHeadersSubViewer2 headers = subtitleHeaders.SubViewer2;
	
		headers.Title = entrySubViewer2Title.Text;	
	 	headers.Author = entrySubViewer2Author.Text;
	 	headers.Source = entrySubViewer2Source.Text;
	 	headers.Program = entrySubViewer2Program.Text;
	 	headers.FilePath = entrySubViewer2FilePath.Text;
	 	headers.Comment = entrySubViewer2Comment.Text;
		headers.FontName = entrySubViewer2FontName.Text;	
		headers.FontColor = entrySubViewer2FontColor.Text;
		headers.FontStyle = entrySubViewer2FontStyle.Text;
	 	
		headers.Delay = spinButtonSubViewer2Delay.ValueAsInt;
		headers.CDTrack = spinButtonSubViewer2CDTrack.ValueAsInt;
		headers.FontSize = spinButtonSubViewer2FontSize.ValueAsInt;
	}
	
	/* Event handlers */

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok) {
			StoreHeaders();
		}
		CloseDialog();
	}

}

}