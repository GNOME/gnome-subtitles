/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2008 Pedro Castro
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

//TODO: check when hours >= 10 (2+ digits)
internal class SubtitleFormatAdvancedSubStationAlpha : SubtitleFormatSubStationAlpha {
	
	protected override string FormatName {
		get { return "Advanced Sub Station Alpha"; }
	}
	
	protected override SubtitleType FormatType {
		get { return SubtitleType.AdvancedSubStationAlpha; }
	}
	
	protected override string[] FormatExtensions {
		get { return new string[] { "ass" }; }
	}
	
	protected override string FormatBodyBeginOut {
		get { return "[Events]\nFormat: Layer, Start, End, Style, Actor, MarginL, MarginR, MarginV, Effect, Text\n"; }
	}
	
	protected override string FormatSubtitleOut {
		get { return "Dialogue: 0,<<StartHours,1>>:<<StartMinutes>>:<<StartSeconds>>.<<StartCentiseconds>>,<<EndHours,1>>:<<EndMinutes>>:<<EndSeconds>>.<<EndCentiseconds>>,Default,,0000,0000,0000,,<<Style>><<Text>><<EndOfStyle>>"; }
	}
	
	protected override string ScriptType {
		get { return "v4.00+"; }
	}
	
	protected override string StyleTypeIn {
		get { return @"V4\+"; }
	}
	
	protected override string StyleSection {
		get {
			return "[V4+ Styles]\nFormat: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\n" +
				"Style: Default,Tahoma,24,&H00FFFFFF,&H00FFFFFF,&H00FFFFFF,&H00C0C0C0,-1,0,0,0,100,100,0,0.00,1,2,3,2,20,20,20,1\n\n";
		}	
	}

}

}
