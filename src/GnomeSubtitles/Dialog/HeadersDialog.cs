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

using System;
using Glade;
using Gtk;
using SubLib;

namespace GnomeSubtitles.Dialog {

public class HeadersDialog : GladeDialog {
	private Headers headers = null;

	/* Constant strings */
	private const string gladeFilename = "HeadersDialog.glade";
	private const string mpSubAudioTag = "AUDIO";
	private const string mpSubVideoTag = "VIDEO";

	/* Widgets */
	
	/* KaraokeLyricsLRC fields */
	[WidgetAttribute] private Entry entryKaraokeLRCTitle = null;
	[WidgetAttribute] private Entry entryKaraokeLRCAuthor = null;
	[WidgetAttribute] private Entry entryKaraokeLRCArtist = null;
	[WidgetAttribute] private Entry entryKaraokeLRCAlbum = null;
	[WidgetAttribute] private Entry entryKaraokeLRCMaker = null;
	[WidgetAttribute] private Entry entryKaraokeLRCVersion = null;
	[WidgetAttribute] private Entry entryKaraokeLRCProgram = null;

	/* KaraokeLyricsVKT fields */
	[WidgetAttribute] private Entry entryKaraokeVKTFrameRate = null;
	[WidgetAttribute] private Entry entryKaraokeVKTAuthor = null;
	[WidgetAttribute] private Entry entryKaraokeVKTSource = null;
	[WidgetAttribute] private Entry entryKaraokeVKTDate = null;

	/* MPSub fields */
	[WidgetAttribute] private Entry entryMPSubTitle = null;
	[WidgetAttribute] private Entry entryMPSubFile = null;
	[WidgetAttribute] private Entry entryMPSubAuthor = null;
	[WidgetAttribute] private Entry entryMPSubNote = null;
	[WidgetAttribute] private ComboBox comboBoxMPSubType = null;

	/* SubStationAlphaASS fields */
	[WidgetAttribute] private Entry entrySSAASSTitle = null;
	[WidgetAttribute] private Entry entrySSAASSOriginalScript = null;
	[WidgetAttribute] private Entry entrySSAASSOriginalTranslation = null;
	[WidgetAttribute] private Entry entrySSAASSOriginalEditing = null;
	[WidgetAttribute] private Entry entrySSAASSOriginalTiming = null;
	[WidgetAttribute] private Entry entrySSAASSOriginalScriptChecking = null;
	[WidgetAttribute] private Entry entrySSAASSScriptUpdatedBy = null;
	[WidgetAttribute] private Entry entrySSAASSCollisions = null;
	[WidgetAttribute] private Entry entrySSAASSTimer = null;
	[WidgetAttribute] private SpinButton spinButtonSSAASSPlayResX = null;
	[WidgetAttribute] private SpinButton spinButtonSSAASSPlayResY = null;
	[WidgetAttribute] private SpinButton spinButtonSSAASSPlayDepth = null;
	
	/* SubViewer1 fields */
	[WidgetAttribute] private Entry entrySubViewer1Title = null;
	[WidgetAttribute] private Entry entrySubViewer1Author = null;
	[WidgetAttribute] private Entry entrySubViewer1Source = null;
	[WidgetAttribute] private Entry entrySubViewer1Program = null;
	[WidgetAttribute] private Entry entrySubViewer1FilePath = null;
	[WidgetAttribute] private SpinButton spinButtonSubViewer1Delay = null;
	[WidgetAttribute] private SpinButton spinButtonSubViewer1CDTrack = null;
	
	/* SubViewer2 fields */
	[WidgetAttribute] private Entry entrySubViewer2Title = null;
	[WidgetAttribute] private Entry entrySubViewer2Author = null;
	[WidgetAttribute] private Entry entrySubViewer2Source = null;
	[WidgetAttribute] private Entry entrySubViewer2Program = null;
	[WidgetAttribute] private Entry entrySubViewer2FilePath = null;
	[WidgetAttribute] private Entry entrySubViewer2Comment = null;
	[WidgetAttribute] private Entry entrySubViewer2FontName = null;
	[WidgetAttribute] private Entry entrySubViewer2FontColor = null;
	[WidgetAttribute] private Entry entrySubViewer2FontStyle = null;
	[WidgetAttribute] private SpinButton spinButtonSubViewer2Delay = null;
	[WidgetAttribute] private SpinButton spinButtonSubViewer2CDTrack = null;
	[WidgetAttribute] private SpinButton spinButtonSubViewer2FontSize = null;
	
	
	public HeadersDialog () : base(gladeFilename) {
		headers = Global.Document.Subtitles.Properties.Headers;
		LoadHeaders();
	}
	
	
	/* Private members */
	
	private void LoadHeaders () {
		LoadKaraokeLRCHeaders();
		LoadKaraokeVKTHeaders();
		LoadMPSubHeaders();
		LoadSSAASSHeaders();
		LoadSubViewer1Headers();
		LoadSubViewer2Headers();
	}
	
	private void LoadKaraokeLRCHeaders() {
		entryKaraokeLRCTitle.Text = headers.Title;
		entryKaraokeLRCAuthor.Text = headers.MovieAuthor;
		entryKaraokeLRCArtist.Text = headers.Artist;
		entryKaraokeLRCAlbum.Text = headers.Album;
		entryKaraokeLRCMaker.Text = headers.Author;
		entryKaraokeLRCVersion.Text = headers.Version;
		entryKaraokeLRCProgram.Text = headers.Program;
	}

	private void LoadKaraokeVKTHeaders() {
		entryKaraokeVKTFrameRate.Text = headers.FrameRate;
		entryKaraokeVKTAuthor.Text = headers.Author;
		entryKaraokeVKTSource.Text = headers.VideoSource;
		entryKaraokeVKTDate.Text = headers.Date;
	}

	private void LoadMPSubHeaders () {
		entryMPSubTitle.Text = headers.Title;
		entryMPSubFile.Text = headers.FileProperties;
		entryMPSubAuthor.Text = headers.Author;
		entryMPSubNote.Text = headers.Comment;

		comboBoxMPSubType.Active = (headers.MediaType == mpSubAudioTag ? 0 : 1); 
	}
	
	private void LoadSSAASSHeaders () {
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
		entrySubViewer1Title.Text = headers.Title;	
	 	entrySubViewer1Author.Text = headers.Author;
	 	entrySubViewer1Source.Text = headers.VideoSource;
	 	entrySubViewer1Program.Text = headers.Program;
	 	entrySubViewer1FilePath.Text = headers.SubtitlesSource;
	 	
		spinButtonSubViewer1Delay.Value = headers.Delay;
		spinButtonSubViewer1CDTrack.Value = headers.CDTrack;
	}
	
	private void LoadSubViewer2Headers () {
		entrySubViewer2Title.Text = headers.Title;	
	 	entrySubViewer2Author.Text = headers.Author;
	 	entrySubViewer2Source.Text = headers.VideoSource;
	 	entrySubViewer2Program.Text = headers.Program;
	 	entrySubViewer2FilePath.Text = headers.SubtitlesSource;
	 	entrySubViewer2Comment.Text = headers.Comment;	
		entrySubViewer2FontName.Text = headers.FontName;	
		entrySubViewer2FontColor.Text = headers.FontColor;	
		entrySubViewer2FontStyle.Text = headers.FontStyle;
	 	
		spinButtonSubViewer2Delay.Value = headers.Delay;
		spinButtonSubViewer2CDTrack.Value = headers.CDTrack;
		spinButtonSubViewer2FontSize.Value = headers.FontSize;
	}
	
	private void StoreHeaders () {
		StoreKaraokeLRCHeaders();
		StoreKaraokeVKTHeaders();
		StoreMPSubHeaders();
		StoreSSAASSHeaders();
		StoreSubViewer1Headers();
		StoreSubViewer2Headers();
	}
	
	private void StoreKaraokeLRCHeaders() {
		headers.Title = entryKaraokeLRCTitle.Text;
		headers.MovieAuthor = entryKaraokeLRCAuthor.Text;
		headers.Artist = entryKaraokeLRCArtist.Text;
		headers.Album = entryKaraokeLRCAlbum.Text;
		headers.Author = entryKaraokeLRCMaker.Text;
		headers.Version = entryKaraokeLRCVersion.Text;
		headers.Program = entryKaraokeLRCProgram.Text;
	}

	private void StoreKaraokeVKTHeaders() {
		headers.FrameRate = entryKaraokeVKTFrameRate.Text;
		headers.Author = entryKaraokeVKTAuthor.Text;
		headers.VideoSource = entryKaraokeVKTSource.Text;
		headers.Date = entryKaraokeVKTDate.Text;
	}

	private void StoreMPSubHeaders () {
		headers.Title = entryMPSubTitle.Text;
		headers.FileProperties = entryMPSubFile.Text;
		headers.Author = entryMPSubAuthor.Text;
		headers.Comment = entryMPSubNote.Text;

		headers.MediaType = (comboBoxMPSubType.Active == 0 ? mpSubAudioTag : mpSubVideoTag);
	}
	
	private void StoreSSAASSHeaders () {
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
		headers.Title = entrySubViewer1Title.Text;	
	 	headers.Author = entrySubViewer1Author.Text;
	 	headers.VideoSource = entrySubViewer1Source.Text;
	 	headers.Program = entrySubViewer1Program.Text;
	 	headers.SubtitlesSource = entrySubViewer1FilePath.Text;
	 	
		headers.Delay = spinButtonSubViewer1Delay.ValueAsInt;
		headers.CDTrack = spinButtonSubViewer1CDTrack.ValueAsInt;
	}
	
	private void StoreSubViewer2Headers () {
		headers.Title = entrySubViewer2Title.Text;	
	 	headers.Author = entrySubViewer2Author.Text;
	 	headers.VideoSource = entrySubViewer2Source.Text;
	 	headers.Program = entrySubViewer2Program.Text;
	 	headers.SubtitlesSource = entrySubViewer2FilePath.Text;
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
		Close();
	}

}

}
