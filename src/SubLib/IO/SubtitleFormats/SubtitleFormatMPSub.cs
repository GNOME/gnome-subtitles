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

//TODO: warn when opening files with comments, warn when saving lines with '#' as they will be ignored/treated as comments
internal class SubtitleFormatMPSub : SubtitleFormat {

	public const string HeaderMediaTypeAudio = "AUDIO";
	public const string HeaderMediaTypeVideo = "VIDEO";

	internal SubtitleFormatMPSub () {
		name = "MPSub";
		type = SubtitleType.MPSub;
		mode = SubtitleMode.Both;
		extensions = new string[] { "sub" };
		lineBreak = "\n";

		format = @"TITLE\s*=.*\n(.*\n)*FORMAT\s*=.*\n(.*\n)*[ \f\r\t\v]*\n\d+([.,]\d+)? \d+([.,]\d+)?\s+.+";

		subtitleInTimesMode = @"(?<StartElapsedTime>\d+([.,]\d*)?)[^\d\n](?<EndElapsedTime>\d+([.,]\d*)?).*(?<Text>(\n?.*(?!\n[ \f\r\t\v]*\n))*.)";
		subtitleInFramesMode = @"(?<StartElapsedFrames>\d+)[^\d\n](?<EndElapsedFrames>\d+).*(?<Text>(\n?.*(?!\n[ \f\r\t\v]*\n))*.)";

		subtitleOutTimesMode = "<<StartElapsedTime>> <<EndElapsedTime>>\n<<Text>>\n";
		subtitleOutFramesMode = "<<StartElapsedFrames>> <<EndElapsedFrames>>\n<<Text>>\n";

		comments = "#.*";

		headers = new string[] {
        	@"TITLE\s*=(?<Title>.*)" ,
        	@"FILE\s*=(?<File>.*)" ,
        	@"AUTHOR\s*=(?<Author>.*)" ,
        	@"TYPE\s*=(?<MediaType>.*)" ,
        	@"FORMAT\s*=(?<TimingModeTimes>TIME)" ,
        	@"FORMAT\s*=(?<TimingModeFrames>\d+(.\d+)?)" ,
        	@"NOTE\s*=(?<Note>.*)"
		};
	}

	internal override string HeadersToString (SubtitleProperties subtitleProperties, FileProperties fileProperties) {
		Headers headers = subtitleProperties.Headers;
		string format = (fileProperties.TimingMode == TimingMode.Times ? "TIME" : subtitleProperties.CurrentFrameRate.ToString());
		return "TITLE=" + headers.Title + "\n" +
			"FILE=" + headers.MPSubFileProperties + "\n" +
			"AUTHOR=" + headers.Author + "\n" +
			"TYPE=" + headers.MPSubMediaType + "\n" +
			"FORMAT=" + format + "\n" +
			"NOTE=" + headers.Comment + "\n\n";
	}

}

}
