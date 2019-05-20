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

internal class SubtitleFormatPanimator : SubtitleFormat {

	internal SubtitleFormatPanimator() {
		name = "Panimator";
		type = SubtitleType.Panimator;
		mode = SubtitleMode.Times;
		extensions = new string[] { "pan" };

		lineBreak = "\n";

		format = @"/d\s+\d+\s+\d+\s+(.+\n)*\s*/d\s+\d+\s+\d+\s+/c";

		subtitleIn = @"/d\s+(?<StartSeconds>\d+)\s+(?<StartCentiseconds>\d+)\s+(?<Text>(.*(?!\n[ \f\r\t\v]*/d[ \f\r\t\v]+\d+[ \f\r\t\v]+\d+[ \f\r\t\v]*\n[ \f\r\t\v]*/c)\n)*.*\n)\s*/d\s+(?<EndSeconds>\d+)\s+(?<EndCentiseconds>\d+)\s+/c";

		subtitleOut = "/d <<StartSeconds>> <<StartCentiseconds>>\n<<Text>>\n"
				+ "/d <<EndSeconds>> <<EndCentiseconds>>\n/c";

	}

}

}