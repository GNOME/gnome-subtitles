/*
 * This file is part of SubLib.
 * Copyright (C) 2007-2008 Pedro Castro
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

internal class SubtitleFormatSubCreator1x : SubtitleFormat {
	
    internal SubtitleFormatSubCreator1x () {
		name = "SubCreator 1.x";
		type = SubtitleType.SubCreator1x;
		mode = SubtitleMode.Times;
    	extensions = new string[] { "txt" };
		lineBreak = "|";
			
		format = @"\d+:\d+:\d+.\d:\s*.+\s+\d+:\d+:\d+.\d:";
		
		subtitleIn = @"(?<StartHours>\d+):(?<StartMinutes>\d+):(?<StartSeconds>\d+).(?<StartDeciseconds>\d+):\s*(?<Text>.+)\s+(?<EndHours>\d+):(?<EndMinutes>\d+):(?<EndSeconds>\d+).(?<EndDeciseconds>\d+)";
		
		subtitleOut = "<<StartHours>>:<<StartMinutes>>:<<StartSeconds>>.<<StartDeciseconds>>:" +
			"<<Text>>\n<<EndHours>>:<<EndMinutes>>:<<EndSeconds>>.<<EndDeciseconds>>:\n";
	}
	
}

}
