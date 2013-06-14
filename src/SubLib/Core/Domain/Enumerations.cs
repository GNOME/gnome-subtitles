/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2011 Pedro Castro
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

namespace SubLib.Core.Domain {

/// <summary>
/// Represents a timing mode, which can be frame or time based.
/// </summary>
public enum TimingMode {
	/// <summary>Time based timing mode.</summary>
	Times,

	/// <summary>Frame based timing mode.</summary>
	Frames
};

/// <summary>
/// Represents a supported subtitle file type.
/// </summary>
public enum SubtitleType {
	/// <summary>Unknown subtitle type.</summary>
	Unknown ,

	/// <summary>Micro DVD subtitle type.</summary>
	MicroDVD,

	/// <summary>SubRip subtitle type.</summary>
	SubRip,

	/// <summary>Sub Station Alpha subtitle type.</summary>
    SubStationAlpha,

	/// <summary>Advanced Sub Station Alpha subtitle type.</summary>
	AdvancedSubStationAlpha,

	/// <summary>MPlayer subtitle type.</summary>
	MPlayer,

	/// <summary>MPlayer2 subtitle type.</summary>
	MPlayer2,

	/// <summary>MPSub subtitle type.</summary>
	MPSub,

	/// <summary>SubViewer 1.0 subtitle type.</summary>
    SubViewer1,

    /// <summary>SubViewer 2.0 subtitle type.</summary>
    SubViewer2,

	/// <summary>AQ Title subtitle type.</summary>
	AQTitle,

	/// <summary>MacSUB subtitle type.</summary>
	MacSUB,

	/// <summary>Phoenix Japanimation Society subtitle type.</summary>
	PhoenixJapanimationSociety,

	/// <summary>Panimator subtitle type.</summary>
	Panimator,

	/// <summary>Sofni subtitle type.</summary>
	Sofni,

	/// <summary>SubCreator 1.x subtitle type.</summary>
	SubCreator1x,

	/// <summary>ViPlay Subtitle File subtitle type.</summary>
	ViPlaySubtitleFile,

	/// <summary>DKS Subtitle Format subtitle type.</summary>
	DKSSubtitleFormat,

	/// <summary>Power DivX subtitle type.</summary>
	PowerDivX,

    /// <summary>Karaoke Lyrics LRC subtitle type.</summary>
    KaraokeLyricsVKT,

    /// <summary>Karaoke Lyrics LRC subtitle type.</summary>
    KaraokeLyricsLRC,

    /// <summary>Adobe Encore DVD subtitle type.</summary>
	AdobeEncoreDVD,

	/// <summary>FAB Subtitler subtitle type.</summary>
	FABSubtitler
};

/// <summary>
/// Represents a type of newline.
/// </summary>
public enum NewlineType {
	/// <summary>Unknown newline type.</summary>
	Unknown ,

	/// <summary>Macintosh newline type.</summary>
    Macintosh,

	/// <summary>Unix newline type.</summary>
	Unix,

	/// <summary>Windows newline type.</summary>
	Windows

};

/// <summary>
/// Represents the type of text content.
/// </summary>
public enum SubtitleTextType {
	/// <summary>The text.</summary>
	Text,

	/// <summary>The translation.</summary>
	Translation
};


/// <summary>
/// Represents the timing mode used by a subtitle format.
/// </summary>
public enum SubtitleMode {
	/// <summary>Time based timing mode.</summary>
	Times,

	/// <summary>Frame based timing mode.</summary>
	Frames,

	/// <summary>Time and Frame based timing mode.</summary>
	Both
};

}
