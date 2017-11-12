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
using System;
//using Glade;
using Gtk;
using SubLib.Core.Domain;

namespace GnomeSubtitles.Dialog {

public class HeadersDialog : BuilderDialog {
	private Headers headers = null;

	/* Constant strings */
	private const string gladeFilename = "HeadersDialog.glade";
	private const string mpSubAudioTag = "AUDIO";
	private const string mpSubVideoTag = "VIDEO";

	/* Widgets */

	/* KaraokeLyricsLRC fields */
	[Builder.Object] private Entry entryKaraokeLRCTitle = null;
	[Builder.Object] private Entry entryKaraokeLRCAuthor = null;
	[Builder.Object] private Entry entryKaraokeLRCArtist = null;
	[Builder.Object] private Entry entryKaraokeLRCAlbum = null;
	[Builder.Object] private Entry entryKaraokeLRCMaker = null;
	[Builder.Object] private Entry entryKaraokeLRCVersion = null;
	[Builder.Object] private Entry entryKaraokeLRCProgram = null;

	/* KaraokeLyricsVKT fields */
	[Builder.Object] private Entry entryKaraokeVKTFrameRate = null;
	[Builder.Object] private Entry entryKaraokeVKTAuthor = null;
	[Builder.Object] private Entry entryKaraokeVKTSource = null;
	[Builder.Object] private Entry entryKaraokeVKTDate = null;

	/* MPSub fields */
	[Builder.Object] private Entry entryMPSubTitle = null;
	[Builder.Object] private Entry entryMPSubFile = null;
	[Builder.Object] private Entry entryMPSubAuthor = null;
	[Builder.Object] private Entry entryMPSubNote = null;
	[Builder.Object] private ComboBox comboBoxMPSubType = null;

	/* SubStationAlphaASS fields */
	[Builder.Object] private Entry entrySSAASSTitle = null;
	[Builder.Object] private Entry entrySSAASSOriginalScript = null;
	[Builder.Object] private Entry entrySSAASSOriginalTranslation = null;
	[Builder.Object] private Entry entrySSAASSOriginalEditing = null;
	[Builder.Object] private Entry entrySSAASSOriginalTiming = null;
	[Builder.Object] private Entry entrySSAASSOriginalScriptChecking = null;
	[Builder.Object] private Entry entrySSAASSScriptUpdatedBy = null;
	[Builder.Object] private Entry entrySSAASSCollisions = null;
	[Builder.Object] private Entry entrySSAASSTimer = null;
	[Builder.Object] private SpinButton spinButtonSSAASSPlayResX = null;
	[Builder.Object] private SpinButton spinButtonSSAASSPlayResY = null;
	[Builder.Object] private SpinButton spinButtonSSAASSPlayDepth = null;

	/* SubViewer1 fields */
	[Builder.Object] private Entry entrySubViewer1Title = null;
	[Builder.Object] private Entry entrySubViewer1Author = null;
	[Builder.Object] private Entry entrySubViewer1Source = null;
	[Builder.Object] private Entry entrySubViewer1Program = null;
	[Builder.Object] private Entry entrySubViewer1FilePath = null;
	[Builder.Object] private SpinButton spinButtonSubViewer1Delay = null;
	[Builder.Object] private SpinButton spinButtonSubViewer1CDTrack = null;

	/* SubViewer2 fields */
	[Builder.Object] private Entry entrySubViewer2Title = null;
	[Builder.Object] private Entry entrySubViewer2Author = null;
	[Builder.Object] private Entry entrySubViewer2Source = null;
	[Builder.Object] private Entry entrySubViewer2Program = null;
	[Builder.Object] private Entry entrySubViewer2FilePath = null;
	[Builder.Object] private Entry entrySubViewer2Comment = null;
	[Builder.Object] private Entry entrySubViewer2FontName = null;
	[Builder.Object] private Entry entrySubViewer2FontColor = null;
	[Builder.Object] private Entry entrySubViewer2FontStyle = null;
	[Builder.Object] private SpinButton spinButtonSubViewer2Delay = null;
	[Builder.Object] private SpinButton spinButtonSubViewer2CDTrack = null;
	[Builder.Object] private SpinButton spinButtonSubViewer2FontSize = null;


	public HeadersDialog () : base(gladeFilename) {
		headers = Base.Document.Subtitles.Properties.Headers;
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

	/* Event members */

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			StoreHeaders();
		}
		return false;
	}

}

}
