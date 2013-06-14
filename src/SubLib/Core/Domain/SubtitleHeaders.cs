/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2008 Pedro Castro
 *
 * SubLib is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * SubLib is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.IO;
using System.Text;

namespace SubLib.Core.Domain {

/// <summary>Represents the headers of the supported subtitle formats.</summary>
public class SubtitleHeaders {

	private SubtitleHeadersSubViewer1 subViewer1 = null;
	private SubtitleHeadersSubViewer2 subViewer2 = null;
	private SubtitleHeadersMPSub mPSub = null;
	private SubtitleHeadersSubStationAlphaASS subStationAlphaASS = null;
	private SubtitleHeadersKaraokeLyricsLRC karaokeLyricsLRC = null;
	private SubtitleHeadersKaraokeLyricsVKT karaokeLyricsVKT = null;

	/* Headers are initialized only if and when they're accessed */

	/// <summary>The headers for the SubViewer 1.0 subtitle type.</summary>
	public SubtitleHeadersSubViewer1 SubViewer1 {
		get {
			if (subViewer1 == null)
				subViewer1 = new SubtitleHeadersSubViewer1();

			return subViewer1;
		}
	}

	/// <summary>The headers for the SubViewer 2.0 subtitle type.</summary>
	public SubtitleHeadersSubViewer2 SubViewer2 {
		get {
			if (subViewer2 == null)
				subViewer2 = new SubtitleHeadersSubViewer2();

			return subViewer2;
		}
	}

	/// <summary>The headers for the MPSub subtitle type.</summary>
	public SubtitleHeadersMPSub MPSub {
		get {
			if (mPSub == null)
				mPSub = new SubtitleHeadersMPSub();

			return mPSub;
		}
	}

	/// <summary>The headers for the Sub Station Alpha and Advanced Sub Station Alpha subtitle types.</summary>
	public SubtitleHeadersSubStationAlphaASS SubStationAlphaASS {
		get {
			if (subStationAlphaASS == null)
				subStationAlphaASS = new SubtitleHeadersSubStationAlphaASS();

			return subStationAlphaASS;
		}
	}

	/// <summary>The headers for the Karaoke Lyrics LRC subtitle type.</summary>
	public SubtitleHeadersKaraokeLyricsLRC KaraokeLyricsLRC {
		get {
			if (karaokeLyricsLRC == null)
				karaokeLyricsLRC = new SubtitleHeadersKaraokeLyricsLRC();

			return karaokeLyricsLRC;
		}
	}

	/// <summary>The headers for the Karaoke Lyrics VKT subtitle type.</summary>
	public SubtitleHeadersKaraokeLyricsVKT KaraokeLyricsVKT {
		get {
			if (karaokeLyricsVKT == null)
				karaokeLyricsVKT = new SubtitleHeadersKaraokeLyricsVKT();

			return karaokeLyricsVKT;
		}
	}

	public override string ToString() {
		return "\t**** SUBTITLE HEADERS ****\t\n" +
			(subViewer1 != null ? subViewer1.ToString() : "! SubViewer 1.0 not used") + "\n" +
			(subViewer2 != null ? subViewer2.ToString() : "! SubViewer 2.0 not used") + "\n" +
			(mPSub != null ? mPSub.ToString() : "! MPSub not used") + "\n" +
			(subStationAlphaASS != null ? subStationAlphaASS.ToString() : "! SubStationAlpha and ASS not used") + "\n" +
			(karaokeLyricsLRC != null ? karaokeLyricsLRC.ToString() : "! Karaoke Lyrics LRC not used") + "\n" +
			(karaokeLyricsVKT != null ? karaokeLyricsVKT.ToString() : "! Karaoke Lyrics VKT not used");
	}

}

/// <summary>Represents the headers of the SubViewer 1.0 subtitle format.</summary>
public class SubtitleHeadersSubViewer1 {
	private string title = String.Empty;
	private string author = String.Empty;
	private string source = String.Empty;
	private string program = String.Empty;
	private string filePath = String.Empty;
	private int delay = 0;
	private int cdTrack = 0;

	/// <summary>The movie's title.</summary>
	public string Title {
		get { return title; }
		set { title = value; }
	}

	/// <summary>The subtitles' author.</summary>
	public string Author {
		get { return author; }
		set { author = value; }
	}

	/// <summary>The subtitles' source.</summary>
	public string Source {
		get { return source; }
		set { source = value; }
	}

	/// <summary>The name of the subtitles' program.</summary>
	public string Program {
		get { return program; }
		set { program = value; }
	}

	/// <summary>The subtitles' file path.</summary>
	public string FilePath {
		get { return filePath; }
		set { filePath = value; }
	}

	/// <summary>The delay of the subtitles.</summary>
	public int Delay {
		get { return delay; }
		set { delay = value; }
	}

	/// <summary>The delay of the subtitles as text.</summary>
	public string DelayAsText {
		get { return delay.ToString(); }
		set {
			try {
				delay = Convert.ToInt32(value);
			}
			catch (Exception) {
			}
		 }
	}

	/// <summary>The CD track of the subtitles.</summary>
	public int CDTrack {
		get { return cdTrack; }
		set { cdTrack = value; }
	}

	/// <summary>The CD track of the subtitles as text.</summary>
	public string CDTrackAsText {
		get { return cdTrack.ToString(); }
		set {
			try {
				cdTrack = Convert.ToInt32(value);
			}
			catch (Exception) {
			}
		 }
	}

	public override string ToString() {
		return "\t** SubViewer 1.0 Headers **\n" +
			"Title: " + title + ", Author: " + author + ", Source: " + source + ", Program: " + program +
			", FilePath: " + filePath + ", Delay: " + delay + ", CD Track: " + cdTrack;
	}

}

/// <summary>Represents the headers of the SubViewer 2.0 subtitle format.</summary>
public class SubtitleHeadersSubViewer2 : SubtitleHeadersSubViewer1 {
	private string comment = String.Empty;
	private string fontColor = "&HFFFFFF";
	private string fontStyle = "bd";
	private int fontSize = 24;
	private string fontName = "Tahoma";


	/// <summary>A comment on the subtitles.</summary>
	public string Comment {
		get { return comment; }
		set { comment = value; }
	}

	/// <summary>The subtitles' font color.</summary>
	public string FontColor {
		get { return fontColor; }
		set { fontColor = value; }
	}

	/// <summary>The subtitles' font style.</summary>
	public string FontStyle {
		get { return fontStyle; }
		set { fontStyle = value; }
	}

	/// <summary>The subtitles' font size.</summary>
	public int FontSize {
		get { return fontSize; }
		set { fontSize = value; }
	}

	/// <summary>The subtitles' font size as text.</summary>
	public string FontSizeAsText {
		get { return fontSize.ToString(); }
		set {
			try {
				fontSize = Convert.ToInt32(value);
			}
			catch (Exception) {
			}
		 }
	}

	/// <summary>The subtitles' font name.</summary>
	public string FontName {
		get { return fontName; }
		set { fontName = value; }
	}

	public override string ToString() {
		return "\t** SubViewer 2.0 Headers **\n" +
			"Base " + base.ToString() + "\n" +
			"FontColor: " + fontColor + ", FontStyle: " + fontStyle + ", FontSize: " + fontSize + ", FontName: " + fontName;
	}
}

/// <summary>Represents the headers of the MPSub subtitle format.</summary>
public class SubtitleHeadersMPSub {
	private string title = String.Empty;
	private string file = String.Empty;
	private string author = String.Empty;
	private string mediaType = "VIDEO";
	private string note = String.Empty;


	/// <summary>The movie's title.</summary>
	public string Title {
		get { return title; }
		set { title = value; }
	}

	/// <summary>The File properties, in the format 'size,md5'.</summary>
	public string File {
		get { return file; }
		set { file = value; }
	}

	/// <summary>The subtitles' author.</summary>
	public string Author {
		get { return author; }
		set { author = value; }
	}

	/// <summary>The Media Type of the subtitles, which can be 'VIDEO' or 'AUDIO'.</summary>
	/// <remarks>This property is only set if the value is 'VIDEO' or 'AUDIO'. It's case insensitive.</remarks>
	public string MediaType {
		get { return mediaType; }
		set {
			string type = value.ToUpper();
			if (type.Equals("VIDEO") || type.Equals("AUDIO"))
				mediaType = type;
		}
	}

	/// <summary>A note on the subtitles.</summary>
	public string Note {
		get { return note; }
		set { note = value; }
	}

	public override string ToString() {
		return "\t** MPSub Headers **\n" +
			"Title: " + title + ", File: " + file + ", Author: " + author + ", MediaType: " + mediaType + ", Note: " + note;
	}

}

/// <summary>Represents the headers of the Sub Station Alpha and Advanced Sub Station Alpha subtitle formats.</summary>
public class SubtitleHeadersSubStationAlphaASS {
	private string title = String.Empty;
	private string originalScript = "<unknown>";
	private string originalTranslation = String.Empty;
	private string originalEditing = String.Empty;
	private string originalTiming = String.Empty;
	private string originalScriptChecking = String.Empty;
	private string scriptUpdatedBy = String.Empty;
	private string collisions = String.Empty;
	private int playResX = 0;
	private int playResY = 0;
	private int playDepth = 0;
	private string timer = String.Empty;

	/// <summary>The movie's title.</summary>
	public string Title {
		get { return title; }
		set { title = value; }
	}

	/// <summary>The Original Script of the subtitles.</summary>
	public string OriginalScript {
		get { return originalScript; }
		set { originalScript = value; }
	}

	/// <summary>The Original Translation of the subtitles.</summary>
	public string OriginalTranslation {
		get { return originalTranslation; }
		set { originalTranslation = value; }
	}

	/// <summary>The Original Editing of the subtitles.</summary>
	public string OriginalEditing {
		get { return originalEditing; }
		set { originalEditing = value; }
	}

	/// <summary>The Original Timing of the subtitles.</summary>
	public string OriginalTiming {
		get { return originalTiming; }
		set { originalTiming = value; }
	}

	/// <summary>The Original Script Checking of the subtitles.</summary>
	public string OriginalScriptChecking {
		get { return originalScriptChecking; }
		set { originalScriptChecking = value; }
	}

	/// <summary>The Script Updated By of the subtitles.</summary>
	public string ScriptUpdatedBy {
		get { return scriptUpdatedBy; }
		set { scriptUpdatedBy = value; }
	}

	/// <summary>The Collisions of the subtitles.</summary>
	public string Collisions {
		get { return collisions; }
		set { collisions = value; }
	}

	/// <summary>The PlayResX of the subtitles.</summary>
	public int PlayResX {
		get { return playResX; }
		set { playResX = value; }
	}

	/// <summary>The PlayResX of the subtitles as text.</summary>
	public string PlayResXAsText {
		get { return playResX.ToString(); }
		set {
			try {
				playResX = Convert.ToInt32(value);
			}
			catch (Exception) {
			}
		 }
	}

	/// <summary>The PlayResY of the subtitles.</summary>
	public int PlayResY {
		get { return playResY; }
		set { playResY = value; }
	}

	/// <summary>The PlayResY of the subtitles as text.</summary>
	public string PlayResYAsText {
		get { return playResY.ToString(); }
		set {
			try {
				playResY = Convert.ToInt32(value);
			}
			catch (Exception) {
			}
		 }
	}

	/// <summary>The PlayDepth of the subtitles.</summary>
	public int PlayDepth {
		get { return playDepth; }
		set { playDepth = value; }
	}

	/// <summary>The PlayResY of the subtitles as text.</summary>
	public string PlayDepthAsText {
		get { return playDepth.ToString(); }
		set {
			try {
				playDepth = Convert.ToInt32(value);
			}
			catch (Exception) {
			}
		 }
	}

	/// <summary>The Timer of the subtitles.</summary>
	public string Timer {
		get { return timer; }
		set { timer = value; }
	}

	public override string ToString() {
		return "\t** SubStationAlpha and Advanced SubStationAlpha Headers **\n" +
			"Title: " + title + ", Original Script: " + originalScript + ", Original Translation: " + originalTranslation +
			", Original Editing: " + originalEditing + ", Original Timing: " + originalTiming +
			", Original Script Checking: " + originalScriptChecking + ", Script Updated By: " + scriptUpdatedBy +
			", Collisions: " + collisions + ", PlayResX: " + playResX + ", PlayResY: " + playResY +
			", PlayDepth: " + playDepth + ", Timer: " + timer;
	}

}

/// <summary>Represents the headers of the Karaoke Lyrics LRC subtitle format.</summary>
public class SubtitleHeadersKaraokeLyricsLRC {
	private string title = String.Empty;
	private string author = String.Empty;
	private string artist = String.Empty;
	private string album = String.Empty;
	private string maker = String.Empty;
	private string version = String.Empty;
	private string program = String.Empty;

	/// <summary>The movie's title.</summary>
	public string Title {
		get { return title; }
		set { title = value; }
	}

	/// <summary>The subtitles' author.</summary>
	public string Author {
		get { return author; }
		set { author = value; }
	}

	/// <summary>The subtitles' artist.</summary>
	public string Artist {
		get { return artist; }
		set { artist = value; }
	}

	/// <summary>The subtitles' album.</summary>
	public string Album {
		get { return album; }
		set { album = value; }
	}

	/// <summary>The subtitles' file maker.</summary>
	public string Maker {
		get { return maker; }
		set { maker = value; }
	}

	/// <summary>The version of the subtitles.</summary>
	public string Version {
		get { return version; }
		set { version = value; }
	}

	/// <summary>The name of the subtitles' program.</summary>
	public string Program {
		get { return program; }
		set { program = value; }
	}

	public override string ToString() {
		return "\t** Karaoke Lyrics LRC Headers **\n" +
			"Title: " + title + ", Author: " + author + ", Artist: " + artist +
			", Maker: " + maker + ", Version: " + version + ", Program: " + program;
	}

}

/// <summary>Represents the headers of the Karaoke Lyrics VKT subtitle format.</summary>
public class SubtitleHeadersKaraokeLyricsVKT {
	private string author = String.Empty;
	private string frameRate = String.Empty;
	private string source = String.Empty;
	private string date = DateTime.Today.ToString("yyyy-MM-dd");

	/// <summary>The subtitles' author.</summary>
	public string Author {
		get { return author; }
		set { author = value; }
	}

	/// <summary>The movie's frame rate.</summary>
	public string FrameRate {
		get { return frameRate; }
		set { frameRate = value; }
	}

	/// <summary>The video file path.</summary>
	public string Source {
		get { return source; }
		set { source = value; }
	}

	/// <summary>The subtitles' date.</summary>
	public string Date {
		get { return date; }
		set { date = value; }
	}

	public override string ToString() {
		return "\t** Karaoke Lyrics VKT Headers **\n" +
			"Author: " + author + ", FrameRate: " + frameRate +
			", Source: " + source + ", Date: " + date;
	}

}

}
