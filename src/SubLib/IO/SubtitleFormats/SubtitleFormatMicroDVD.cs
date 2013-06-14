/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2008 Pedro Castro
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

internal class SubtitleFormatMicroDVD : SubtitleFormat {

	internal SubtitleFormatMicroDVD () {
		name = "Micro DVD";
		type = SubtitleType.MicroDVD;
		mode = SubtitleMode.Frames;
		extensions = new string[] { "sub" };
		lineBreak = "|";

		format = @"\{\s*\d+\s*\}\s*\{\s*\d+\s*\}\s*.+";

		subtitleIn = @"\{[^\d\n]*(?<StartFrame>\d+)[^\}\n]*\}([^\{\n]*\{[^\d\n]*(?<EndFrame>\d+)[^\}\n]*\})?(?<Text>.*)";

		subtitleOut = "{<<StartFrame>>}{<<EndFrame>>}<<Style>><<Text>>";
	}

	private static Regex styleExpression =  new Regex(@"\{[^Y]*Y[^:]*:\w*\}", RegexOptions.IgnoreCase);

	internal override string StyleToString(Style style) {
		string styleText = String.Empty;
		if (style.Enabled) {
			styleText += "{Y:";
			if (style.Underline)
				styleText += 'u';
			if (style.Bold)
				styleText += 'b';
			if (style.Italic)
				styleText += 'i';
			styleText += '}';
		}
		return styleText;
	}

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
