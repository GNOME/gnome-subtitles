/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2019 Pedro Castro
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

using SubLib.Core.Domain;
using SubLib.IO.Input;
using System;

namespace SubLib.IO.SubtitleFormats {

internal abstract class SubtitleFormat {

	/* Required members */
	protected string name;			//the name of the format.
	protected SubtitleType type;	//the subtitle type
	protected SubtitleMode mode;	//the subtitle mode
	protected string[] extensions;	//the extensions used

	protected string lineBreak;		//used to split the subtitle's text lines
	protected string format;		//regex used to detect the file's format
	protected string subtitleIn;	//regex used to read a subtitle
	protected string subtitleOut = null;	//expression with tags used to write a subtitle; GetDynamicSubtitleOut can be used instead if the expression is dynamic

	/* Optional members */
	protected string[] headers = null;				//used to read the subtitles' headers
	protected string comments = String.Empty;		//used to discard comments
	protected string bodyBeginOut = String.Empty;	//used to write the beginning of the subtitles' body
	protected string bodyEndIn = String.Empty;		//used to detect the end of the subtitles' body
	protected string bodyEndOut = String.Empty;		//used to write the end of the subtitles' body

	/* Required members for subtitles that support both frames and times */
	//The following two are used instead of subtitleIn
	protected string subtitleInTimesMode = String.Empty;	//regex used to read a subtitle in times mode
	protected string subtitleInFramesMode = String.Empty;	//regex used to read a subtitle in frames mode
	//The following two are used instead of subtitleOut
	protected string subtitleOutTimesMode = String.Empty;   //regex used to output a subtitle in times mode
	protected string subtitleOutFramesMode = String.Empty;  //regex used to output a subtitle in times mode

	internal string Name {
		get { return name; }
	}

	internal SubtitleType Type {
		get { return type; }
	}

	internal SubtitleMode Mode {
		get { return mode; }
	}

	/// <remarks>Only use this when the Mode is not Both. This is provided as a convenience
	/// for when the Mode is either Frames or Times.</remarks>
	internal TimingMode ModeAsTimingMode {
		get {
			if (mode == SubtitleMode.Frames)
				return TimingMode.Frames;
			else
				return TimingMode.Times;
		}
	}

	internal string[] Extensions {
		get { return extensions; }
	}

	internal string LineBreak {
		get { return lineBreak; }
	}

	internal string Format {
		get { return format; }
	}

	internal string SubtitleIn {
		get { return subtitleIn; }
	}

	internal string SubtitleOut {
		get { return subtitleOut; }
	}

	internal bool HasHeaders {
		get { return headers != null; }
	}

	internal string[] Headers {
		get { return headers; }
	}

	internal bool HasComments {
		get { return comments != String.Empty; }
	}

	internal string Comments {
		get { return comments; }
	}

	internal bool HasBodyBegin {
		get { return bodyBeginOut != String.Empty; }
	}

	internal string BodyBeginOut {
		get { return bodyBeginOut; }
	}

	internal bool HasBodyEnd {
		get { return bodyEndOut != String.Empty; }
	}

	internal string BodyEndIn {
		get { return bodyEndIn; }
	}

	internal string BodyEndOut {
		get { return bodyEndOut; }
	}

	internal string SubtitleInTimesMode {
		get { return subtitleInTimesMode; }
	}

	internal string SubtitleInFramesMode {
		get { return subtitleInFramesMode; }
	}

	internal string SubtitleOutTimesMode {
		get { return subtitleOutTimesMode; }
	}

	internal string SubtitleOutFramesMode {
		get { return subtitleOutFramesMode; }
	}

	internal virtual Style StringToStyle (string styleText) {
		Style style = new Style();
		foreach (char character in styleText) {
			if ((character == 'u') || (character == 'U'))
				style.Underline = true;
			else if ((character == 'b') || (character == 'B'))
				style.Bold = true;
			else if ((character == 'i') || (character == 'I'))
				style.Italic = true;
		}
		return style;
	}

	internal virtual string StyleToString (Style style) {
		return String.Empty;
	}

	internal virtual string EndOfStyleToString (Style style) {
		return String.Empty;
	}

	internal virtual void SubtitleInputPostProcess (Subtitle subtitle) {
		return;
	}

	internal virtual void GlobalInputGetProperties (string text, ParsingProperties properties) {
		return;
	}

	internal virtual string GetDynamicSubtitleOut (SubtitleProperties properties) {
		return null;
	}

    internal virtual string HeadersToString (SubtitleProperties subtitleProperties, FileProperties fileProperties) {
        return String.Empty;
    }

	/* Public members */

	public override string ToString(){
		return Enum.GetName(typeof(SubtitleType), Type);
	}
}

}
