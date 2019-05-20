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

internal class SubtitleFormatPowerDivX : SubtitleFormat {

	internal SubtitleFormatPowerDivX () {
		name = "Power DivX";
		type = SubtitleType.PowerDivX;
		mode = SubtitleMode.Times;
		extensions = new string[] { "psb" };
		lineBreak = "|";

		format = @"\{\s*\d+:\d+:\d+\s*\}\{\s*\d+:\d+:\d+\s*\}\s*.+";

		subtitleIn = @"\{\s*(?<StartHours>\d+)\s*:\s*(?<StartMinutes>\d+)\s*:\s*(?<StartSeconds>\d+)\s*\}\s*\{\s*(?<EndHours>\d+)\s*:\s*(?<EndMinutes>\d+)\s*:\s*(?<EndSeconds>\d+)\s*\}\s*(?<Text>.*)";

		subtitleOut = "{<<StartHours>>:<<StartMinutes>>:<<StartSeconds>>}" +
			"{<<EndHours>>:<<EndMinutes>>:<<EndSeconds>>}<<Text>>";
	}

}

}