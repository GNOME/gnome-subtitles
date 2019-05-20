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

internal class SubtitleFormatSubViewer1 : SubtitleFormat {

	internal SubtitleFormatSubViewer1 () {
		name = "SubViewer 1.0";
		type = SubtitleType.SubViewer1;
		mode = SubtitleMode.Times;
		extensions = new string[] { "sub" };
		lineBreak = "|";

		format = @"\**\s*START\s*SCRIPT\s*\**[^\[]*\[\d+:\d+:\d+\]\s*.*\s*\[\d+:\d+:\d+\]";

		subtitleIn = @"\[\s*(?<StartHours>\d+)\s*:\s*(?<StartMinutes>\d+)\s*:\s*(?<StartSeconds>\d+)\s*\]\s*(?<Text>.*)\s*\[\s*(?<EndHours>\d+)\s*:\s*(?<EndMinutes>\d+)\s*:\s*(?<EndSeconds>\d+)\s*\]";

		subtitleOut = "[<<StartHours>>:<<StartMinutes>>:<<StartSeconds>>]\n" +
			"<<Text>>\n" +
			"[<<EndHours>>:<<EndMinutes>>:<<EndSeconds>>]\n";

		bodyEndIn = @"\[\s*end\s*\]\s*\**\s*END\s*SCRIPT\s*\**";

		bodyBeginOut = "[BEGIN]\n******** START SCRIPT ********\n";
		bodyEndOut = "[end]\n******** END SCRIPT ********\n";

		headers = new string[] {
        	@"\[\s*TITLE\s*\].*\n(?<Title>.*)" ,
        	@"\[\s*AUTHOR\s*\].*\n(?<Author>.*)" ,
        	@"\[\s*SOURCE\s*\].*\n(?<Source>.*)" ,
        	@"\[\s*PRG\s*\].*\n(?<Program>.*)" ,
        	@"\[\s*FILEPATH\s*\].*\n(?<FilePath>.*)" ,
        	@"\[\s*DELAY\s*\].*\n(?<Delay>.*)" ,
        	@"\[\s*CD\s*TRACK\s*\].*\n(?<CdTrack>.*)"
		};
	}

	internal override string HeadersToString (SubtitleProperties subtitleProperties, FileProperties fileProperties) {
		Headers headers = subtitleProperties.Headers;
		return "[TITLE]\n" + headers.Title + "\n" +
			"[AUTHOR]\n" + headers.Author + "\n" +
			"[SOURCE]\n" + headers.Source + "\n" +
			"[PRG]\n" + headers.Program + "\n" +
			"[FILEPATH]\n" + headers.FilePath + "\n" +
			"[DELAY]\n" + headers.Delay + "\n" +
			"[CD TRACK]\n" + headers.CDTrack + "\n";
	}

}

}
