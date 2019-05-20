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

using SubLib.Core.Domain;

namespace SubLib.IO.SubtitleFormats {

internal class SubtitleFormatSubViewer2 : SubtitleFormat {

	internal SubtitleFormatSubViewer2 () {
		name = "SubViewer 2.0";
		type = SubtitleType.SubViewer2;
		mode = SubtitleMode.Times;
		extensions = new string[] { "sub" };
		lineBreak = "[br]";

		format = @"\d\d:\d\d:\d\d.\d\d,\d\d:\d\d:\d\d.\d\d";

		subtitleIn = @"(?<StartHours>\d+)\s*:\s*(?<StartMinutes>\d+)\s*:\s*(?<StartSeconds>\d+)\s*\.\s*(?<StartCentiseconds>\d+)\s*,\s*(?<EndHours>\d+)\s*:\s*(?<EndMinutes>\d+)\s*:\s*(?<EndSeconds>\d+)\s*\.\s*(?<EndCentiseconds>\d+).*\n(?<Text>.*)";

		subtitleOut = "<<StartHours>>:<<StartMinutes>>:<<StartSeconds>>.<<StartCentiseconds>>,<<EndHours>>:<<EndMinutes>>:<<EndSeconds>>.<<EndCentiseconds>>\n<<Text>>\n";

		headers = new string[] {
        	@"\[\s*TITLE\s*\](?<Title>.*)" ,
        	@"\[\s*AUTHOR\s*\](?<Author>.*)" ,
        	@"\[\s*SOURCE\s*\](?<Source>.*)" ,
        	@"\[\s*PRG\s*\](?<Program>.*)" ,
        	@"\[\s*FILEPATH\s*\](?<FilePath>.*)" ,
        	@"\[\s*DELAY\s*\](?<Delay>.*)" ,
        	@"\[\s*CD\s*TRACK\s*\](?<CdTrack>.*)" ,
        	@"\[\s*COMMENT\s*\](?<Comment>.*)" ,
        	@"\[\s*COLF\s*\](?<FontColor>[^,\[\n]*)" ,
        	@"\[\s*STYLE\s*\](?<FontStyle>[^,\[\n]*)" ,
        	@"\[\s*SIZE\s*\](?<FontSize>[^,\[\n]*)" ,
        	@"\[\s*FONT\s*\](?<FontName>[^,\[\n]*)"
		};
	}

	internal override string HeadersToString (SubtitleProperties subtitleProperties, FileProperties fileProperties) {
		Headers headers = subtitleProperties.Headers;
		return "[INFORMATION]\n" +
			"[TITLE]" + headers.Title + "\n" +
			"[AUTHOR]" + headers.Author + "\n" +
			"[SOURCE]" + headers.Source + "\n" +
			"[PRG]" + headers.Program + "\n" +
			"[FILEPATH]" + headers.FilePath + "\n" +
			"[DELAY]" + headers.Delay + "\n" +
			"[CD TRACK]" + headers.CDTrack + "\n" +
			"[COMMENT]" + headers.Comment + "\n" +
			"[END INFORMATION]\n" +
			"[SUBTITLE]\n" +
			"[COLF]" + headers.SubViewer2FontColor + ",[STYLE]" + headers.SubViewer2FontStyle +
			",[SIZE]" + headers.SubViewer2FontSize + ",[FONT]" + headers.SubViewer2FontName + "\n";
	}

}

}
