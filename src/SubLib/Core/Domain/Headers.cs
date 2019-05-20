/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2019 Pedro Castro
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
using SubLib.IO.SubtitleFormats;

namespace SubLib.Core.Domain {

public enum HeadersMediaType { Audio, Video }

/// <summary>Represents the headers of the supported subtitle formats.</summary>
public class Headers : ICloneable {

	/* Common properties */

	/// <summary>The movie's title.</summary>
	public string Title { get; set; }

	/// <summary>The subtitles' author.</summary>
	public string Author { get; set; }

	/// <summary>The subtitles' artist.</summary>
	public string Artist { get; set; }

	/// <summary>The subtitles' album.</summary>
	public string Album { get; set; }

	/// <summary>The subtitles' file creator.</summary>
	public string FileCreator { get; set; }

	/// <summary>The name of the subtitles' program.</summary>
	public string Program { get; set; }

	/// <summary>Th subtitles' version.</summary>
	public string Version { get; set; }

	/// <summary>The movie's frame rate.</summary>
	public string FrameRate { get; set; }

	/// <summary>The subtitles' date.</summary>
	public string Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");

	/// <summary>The video source.</summary>
	public string Source { get; set; }

	/// <summary>The subtitles' file path.</summary>
	public string FilePath { get; set; }

	/// <summary>A comment or note on the subtitles.</summary>
	public string Comment { get; set; }

	/// <summary>The delay of the subtitles.</summary>
	public int Delay { get; set; } = 0;

	/// <summary>The CD track of the subtitles.</summary>
	public int CDTrack { get; set; } = 1;

	/// <summary>The Media Type of the subtitles, which can be 'VIDEO' or 'AUDIO'.</summary>
	/// <remarks>This property is only set if the value is 'VIDEO' or 'AUDIO'. It's case insensitive.</remarks>
	public HeadersMediaType? MediaType { get; set; } = HeadersMediaType.Video;


	/* SubViewer 2 */

	/// <summary>The subtitles' font color.</summary>
	public string SubViewer2FontColor { get; set; } = "&HFFFFFF";

	/// <summary>The subtitles' font style.</summary>
	public string SubViewer2FontStyle { get; set; } = "bd";

	/// <summary>The subtitles' font name.</summary>
	public string SubViewer2FontName { get; set; } = "Tahoma";

	/// <summary>The subtitles' font size.</summary>
	public int SubViewer2FontSize { get; set; } = 24;


	/* MPSub */

	/// <summary>The File properties, in the format 'size,md5'.</summary>
	public string MPSubFileProperties { get; set; }

	public string MPSubMediaType {
		get {
			if (MediaType == null) {
				return null;
			} else {
				return MediaType.ToString().ToUpper();
			}
		}
		set {
			if (SubtitleFormatMPSub.HeaderMediaTypeVideo.Equals(value, StringComparison.InvariantCultureIgnoreCase)) {
				MediaType = HeadersMediaType.Video;
			} else if (SubtitleFormatMPSub.HeaderMediaTypeAudio.Equals(value, StringComparison.InvariantCultureIgnoreCase)) {
				MediaType = HeadersMediaType.Audio;
			} else {
				MediaType = null;
			}
		}
	}


	/* Sub Station Alpha */

	/// <summary>The Original Script of the subtitles.</summary>
	public string SubStationAlphaOriginalScript { get; set; }

	/// <summary>The Original Translation of the subtitles.</summary>
	public string SubStationAlphaOriginalTranslation { get; set; }

	/// <summary>The Original Editing of the subtitles.</summary>
	public string SubStationAlphaOriginalEditing { get; set; }

	/// <summary>The Original Timing of the subtitles.</summary>
	public string SubStationAlphaOriginalTiming { get; set; }

	/// <summary>The Original Script Checking of the subtitles.</summary>
	public string SubStationAlphaOriginalScriptChecking { get; set; }

	/// <summary>The Script Updated By of the subtitles.</summary>
	public string SubStationAlphaScriptUpdatedBy { get; set; }

	/// <summary>The Collisions of the subtitles.</summary>
	public string SubStationAlphaCollisions { get; set; }

	/// <summary>The Timer of the subtitles.</summary>
	public string SubStationAlphaTimer { get; set; }


	/// <summary>The PlayResX of the subtitles.</summary>
	public int SubStationAlphaPlayResX { get; set; } = 0;

	/// <summary>The PlayResY of the subtitles.</summary>
	public int SubStationAlphaPlayResY { get; set; } = 0;

	/// <summary>The PlayDepth of the subtitles.</summary>
	public int SubStationAlphaPlayDepth { get; set; } = 0;


	/* Public methods */

	public object Clone () {
		return this.MemberwiseClone();
	}

}

}