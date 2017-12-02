/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2017 Pedro Castro
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
using System;
using System.Text.RegularExpressions;

namespace SubLib.IO.SubtitleFormats {

internal class SubtitleFormatSubStationAlpha : SubtitleFormat {
	private static Regex styleExpression = new Regex(@"\{\\[ubi]\d\}", RegexOptions.IgnoreCase);

	internal SubtitleFormatSubStationAlpha () {
		name = FormatName;
    	type = FormatType;
    	extensions = FormatExtensions;

    	mode = SubtitleMode.Times;
    	lineBreak = @"\N";

    	format = @"\[\s*" + StyleTypeIn + @"\s*Styles\s*\][^\[]*\[\s*Events\s*\]\s*Format:\s*[^,\n]*(,[^,\n]*){9}";

    	subtitleIn = @"Dialogue:[^,]*,(?<StartHours>\d+):(?<StartMinutes>\d+):(?<StartSeconds>\d+)\.(?<StartCentiseconds>\d+),(?<EndHours>\d+):(?<EndMinutes>\d+):(?<EndSeconds>\d+)\.(?<EndCentiseconds>\d+)(,[^,]*){6},(?<Text>.*)";

    	subtitleOut = FormatSubtitleOut;
        bodyBeginOut = FormatBodyBeginOut;

		headers = new string[] {
        	@"Title:(?<Title>.*)" ,
        	@"Original\s*Script:(?<OriginalScript>.*)" ,
        	@"Original\s*Translation:(?<OriginalTranslation>.*)" ,
        	@"Original\s*Editing:(?<OriginalEditing>.*)" ,
        	@"Original\s*Timing:(?<OriginalTiming>.*)" ,
        	@"Original\s*Script\s*Checking:(?<OriginalScriptChecking>.*)" ,
        	@"Script\s*Updated\s*By:(?<ScriptUpdatedBy>.*)" ,
        	@"Collisions:(?<Collisions>.*)" ,
        	@"PlayResX:\s*(?<PlayResX>\d*)" ,
        	@"PlayResY:\s*(?<PlayResY>\d*)" ,
        	@"PlayDepth:\s*(?<PlayDepth>\d*)" ,
        	@"Timer:(?<Timer>.*)"
		};
	}

	internal override string StyleToString (Style style) {
		return StyleToString(style, "1");
	}

	internal override string EndOfStyleToString (Style style) {
		return StyleToString(style, "0");
	}
	
	internal override void SubtitleInputPostProcess (Subtitle subtitle) {
		string subtitleText = subtitle.Text.Get(lineBreak);
		string styleText = String.Empty;
		MatchCollection matches = styleExpression.Matches(subtitleText);
		foreach (Match match in matches) {
			styleText += match.Value;
		}
		Style style = StringToStyle(styleText);
		subtitle.Style = style;
		subtitleText = styleExpression.Replace(subtitleText, String.Empty);
		subtitle.Text.Set(subtitleText, lineBreak, true);
	}

	internal override string HeadersToString (SubtitleProperties subtitleProperties, FileProperties fileProperties) {
		Headers headers = subtitleProperties.Headers;
		return "[Script Info]\n" +
			Header("Title:", headers.Title, "<untitled>") +
			Header("Original Script:", headers.SubStationAlphaOriginalScript, "<unknown>") +
			"Script Type: " + ScriptType + "\n" +
			Header("Original Translation:", headers.SubStationAlphaOriginalTranslation) +
			Header("Original Editing:", headers.SubStationAlphaOriginalEditing) +
			Header("Original Timing:", headers.SubStationAlphaOriginalTiming) +
			Header("Original Script Checking:", headers.SubStationAlphaOriginalScriptChecking) +
			Header("Script Updated By:", headers.SubStationAlphaScriptUpdatedBy) +
			Header("Collisions:", headers.SubStationAlphaCollisions) +
			Header("PlayResX:", headers.SubStationAlphaPlayResX) +
			Header("PlayResY:", headers.SubStationAlphaPlayResY) +
			Header("PlayDepth:", headers.SubStationAlphaPlayDepth) +
			Header("Timer:", headers.SubStationAlphaTimer) + "\n" +
			StyleSection;
	}

	/* Protected members */

	protected virtual string FormatName {
		get { return "Sub Station Alpha"; }
	}

	protected virtual SubtitleType FormatType {
		get { return SubtitleType.SubStationAlpha; }
	}

	protected virtual string[] FormatExtensions {
		get { return new string[] { "ssa" }; }
	}

	protected virtual string FormatBodyBeginOut {
		get { return "[Events]\nFormat: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n"; }
	}

	protected virtual string FormatSubtitleOut {
		get { return "Dialogue: Marked=0,<<StartHours,1>>:<<StartMinutes>>:<<StartSeconds>>.<<StartCentiseconds>>,<<EndHours,1>>:<<EndMinutes>>:<<EndSeconds>>.<<EndCentiseconds>>,Default,NTP,0000,0000,0000,!Effect,<<Style>><<Text>><<EndOfStyle>>"; }
	}

	protected virtual string ScriptType {
		get { return "v4.00"; }
	}

	protected virtual string StyleTypeIn {
		get { return "V4"; }
	}

	protected virtual string StyleSection {
		get {
			return "[V4 Styles]\nFormat: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding\n" +
            	"Style: Default,Tahoma,24,16777215,16777215,16777215,0,-1,0,1,1,1,2,10,10,30,0,0\n\n";
		}
	}

	/* Private members */

	private string Header (string headerIntro, string headerValue, string defaultValue) {
		if (String.IsNullOrEmpty(headerValue))
			return headerIntro + " " + defaultValue + "\n";
		else
			return headerIntro + " " + headerValue + "\n";
	}

	private string Header (string headerIntro, string headerValue) {
		if (String.IsNullOrEmpty(headerValue))
			return String.Empty;
		else
			return headerIntro + " " + headerValue + "\n";
	}

	private string Header (string headerIntro, int headerValue) {
		return headerIntro + " " + headerValue + "\n";
	}

	protected string StyleToString (Style style, string suffix) {
		string styleText = String.Empty;
		if (style.Underline)
			styleText += @"{\u" + suffix + "}";
		if (style.Bold)
			styleText += @"{\b" + suffix + "}";
		if (style.Italic)
			styleText += @"{\i" + suffix + "}";
		return styleText;
	}

}

}
