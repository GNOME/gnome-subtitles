/*
 * This file is part of SubLib.
 * Copyright (C) 2007-2019 Pedro Castro
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

namespace SubLib.IO.SubtitleFormats {

//TODO: warn when saving with 2+ lines
internal class SubtitleFormatKaraokeLyricsVKT : SubtitleFormat {

	internal SubtitleFormatKaraokeLyricsVKT() {
		name = "Karaoke Lyrics VKT";
		type = SubtitleType.KaraokeLyricsVKT;
		mode = SubtitleMode.Frames;
		extensions = new string[] { "vkt" };

		lineBreak = "|"; // It does not manage line breaks, but still using this char as a separator

		format = @"\{\s*\d+\s*.+\s*\}\n+\{\s*\d+\s*\}";

		subtitleIn = @"\{\s*(?<StartFrame>\d+)\s*(?<Text>.+)\}\n+\{\s*(?<EndFrame>\d+)\s*\}";

		subtitleOut = "{<<StartFrame>> <<Text>>}\n{<<EndFrame>> }";

		bodyEndIn = @"#\s*[\n#]+\s+THE END.";
		bodyEndOut = "#\n# THE END.\n";

		headers = new string[] {
        	@"FRAME RATE=(?<FrameRate>.*)" ,
        	@"CREATOR=(?<Author>.*)" ,
        	@"VIDEO SOURCE=(?<Source>.*)" ,
        	@"DATE=(?<Date>.*)"
		};

	}

	internal override string HeadersToString(SubtitleProperties subtitleProperties, FileProperties fileProperties) {
		Headers headers = subtitleProperties.Headers;
		return "# <HEAD>\n" +
			"# FRAME RATE=" + headers.FrameRate + "\n" +
			"# CREATOR=" + headers.Author + "\n" +
			"# VIDEO SOURCE=" + headers.Source + "\n" +
			"# DATE=" + headers.Date + "\n" +
			"# </HEAD>\n#\n";
	}
}

}

