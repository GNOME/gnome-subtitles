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

using SubLib.Core.Domain;
using System;
using System.Text.RegularExpressions;

namespace SubLib.IO.SubtitleFormats {

internal class SubtitleFormatSubRip : SubtitleFormat {
	private static Regex styleExpression = new Regex(@"</?\w*>", RegexOptions.IgnoreCase);

    internal SubtitleFormatSubRip () {
		name = "SubRip";
		type = SubtitleType.SubRip;
		mode = SubtitleMode.Times;
    	extensions = new string[] { "srt" };
		lineBreak = "\n";

		format = @"\d+[ \f\r\t\v]*\n\d+(\s*:\s*\d+){2}\s*[,.]\s*\d+\s*-->\s*\d+(\s*:\s*\d+){2}\s*[,.]\d+[ \f\r\t\v]*\n.+"; //Dot is here to support buggy SubRip files

		subtitleIn = @"(\d*[^\n]*\n)?((?<StartHours>\d+)[^\d\n]+)?(?<StartMinutes>\d+)[^\d\n]+(?<StartSeconds>\d+)([^\d\n](?<StartMilliseconds>\d+))?[^\d\n]+((?<EndHours>\d+)[^\d\n]+)?(?<EndMinutes>\d+)[^\d\n]+(?<EndSeconds>\d+)([^\d\n](?<EndMilliseconds>\d+))?[^\d\n]*(\n(?<Text>(.*(?!\n[ \f\r\t\v]*\d+[ \f\r\t\v]*\n\d+(\s*:\s*\d+){2})\n?)*.))?";

		subtitleOut = "<<SubtitleNumber>>\n" +
			"<<StartHours>>:<<StartMinutes>>:<<StartSeconds>>,<<StartMilliseconds>> --> <<EndHours>>:<<EndMinutes>>:<<EndSeconds>>,<<EndMilliseconds>>\n" +
			"<<Style>><<Text>><<EndOfStyle>>\n";
	}

	internal override string StyleToString (Style style) {
		string styleText = String.Empty;
		if (style.Underline)
			styleText += "<u>";
		if (style.Bold)
			styleText += "<b>";
		if (style.Italic)
			styleText += "<i>";
		return styleText;
	}

	internal override string EndOfStyleToString (Style style) {
		string styleText = String.Empty;
		if (style.Underline)
			styleText += "</u>";
		if (style.Bold)
			styleText += "</b>";
		if (style.Italic)
			styleText += "</i>";
		return styleText;
	}

	// can be optimized
	internal override void SubtitleInputPostProcess (Subtitle subtitle) {
		string subtitleText = subtitle.Text.Get(lineBreak);
		string styleText = String.Empty;
		MatchCollection matches = styleExpression.Matches(subtitleText);
		foreach (Match match in matches) {
			styleText +=  match.Value;
		}
		Style style = StringToStyle(styleText);
		subtitle.Style = style;
		subtitleText = styleExpression.Replace(subtitleText, String.Empty);
		subtitle.Text.Set(subtitleText, lineBreak, true);
	}

}

}
