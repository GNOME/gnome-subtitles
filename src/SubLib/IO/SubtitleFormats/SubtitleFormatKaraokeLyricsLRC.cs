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

//TODO: warn when saving subtitles with times > 99 minutes, warn when saving with 2+ lines
internal class SubtitleFormatKaraokeLyricsLRC : SubtitleFormat {

	internal SubtitleFormatKaraokeLyricsLRC() {
		name = "Karaoke Lyrics LRC";
		type = SubtitleType.KaraokeLyricsLRC;
		mode = SubtitleMode.Times;
		extensions = new string[] { "lrc" };

		lineBreak = "|"; // It does not manage line breaks, but still using this char as a separator

		format = @"\[\s*\d+:\d+[.,]\d+\s*\].+\n+\[\s*\d+:\d+[.,]\d+\s*\]";

		subtitleIn = @"\[\s*(?<StartMinutes>\d+)\s*:\s*(?<StartSeconds>\d+)\s*[.,]\s*(?<StartCentiseconds>\d+)\s*\]\s*(?<Text>.*)\n+\[\s*(?<EndMinutes>\d+)\s*:\s*(?<EndSeconds>\d+)\s*[.,]\s*(?<EndCentiseconds>\d+)\s*\]";

		subtitleOut = "[<<StartMinutes>>:<<StartSeconds>>.<<StartCentiseconds>>]<<Text>>\n" +
			"[<<EndMinutes>>:<<EndSeconds>>.<<EndCentiseconds>>]";

		headers = new string[] {
        	@"\[\s*ti:(?<Title>.*)\s*]" ,
        	@"\[\s*au:(?<Author>.*)\s*]" ,
        	@"\[\s*ar:(?<Artist>.*)\s*]" ,
        	@"\[\s*al:(?<Album>.*)\s*]" ,
        	@"\[\s*by:(?<Maker>.*)\s*]" ,
        	@"\[\s*ve:(?<Version>.*)\s*]" ,
        	@"\[\s*re:(?<Program>.*)\s*]"
		};

	}

	internal override string HeadersToString (SubtitleProperties subtitleProperties, FileProperties fileProperties) {
		Headers headers = subtitleProperties.Headers;
		return "[ti: " + headers.Title + "]\n" +
			"[au:" + headers.Author + "]\n" +
			"[ar:" + headers.Artist + "]\n" +
			"[al:" + headers.Album + "]\n" +
			"[by:" + headers.FileCreator + "]\n" +
			"[ve:" + headers.Version + "]\n" +
			"[re:" + headers.Program + "]\n";
	}
}

}

