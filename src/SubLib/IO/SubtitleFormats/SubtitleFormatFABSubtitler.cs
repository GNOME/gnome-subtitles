/*
 * This file is part of SubLib.
 * Copyright (C) 2010-2019 Pedro Castro
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

/* Note: it's not clear whether FAB Subtitler supports framerates besides PAL and NTSC. Because of that, all framerates
   are supported here. The user should use the subtitle input and output framerate options accordingly. */
internal class SubtitleFormatFABSubtitler : SubtitleFormat {

	internal SubtitleFormatFABSubtitler () {
		name = "FAB Subtitler";
		type = SubtitleType.FABSubtitler;
		mode = SubtitleMode.Times;
		extensions = new string[] { "txt" };
		lineBreak = "\n";

		format = @"\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d";

		subtitleIn = @"(?<StartHours>\d+)\s*:\s*(?<StartMinutes>\d+)\s*:\s*(?<StartSeconds>\d+)\s*:\s*(?<StartMillisecondsAsFrames>\d+)\s+(?<EndHours>\d+)\s*:\s*(?<EndMinutes>\d+)\s*:\s*(?<EndSeconds>\d+)\s*:\s*(?<EndMillisecondsAsFrames>\d+).*(\n(?<Text>(.*(?!\n\d+(\s*:\s*\d+){2})\n?)*.))?";

		subtitleOut = "<<StartHours>>:<<StartMinutes>>:<<StartSeconds>>:<<StartMillisecondsAsFrames>>  <<EndHours>>:<<EndMinutes>>:<<EndSeconds>>:<<EndMillisecondsAsFrames>>\n<<Text>>\n";;
	}

}

}
