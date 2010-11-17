/*
 * This file is part of SubLib.
 * Copyright (C) 2010 Pedro Castro
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
using SubLib.IO.Input;
using System;
using System.Text.RegularExpressions;

namespace SubLib.IO.SubtitleFormats {

internal class SubtitleFormatFABSubtitler : SubtitleFormat {
	
	internal SubtitleFormatFABSubtitler () {
		name = "FAB Subtitler";
		type = SubtitleType.FABSubtitler;
		mode = SubtitleMode.Times;
		extensions = new string[] { "txt" };
		lineBreak = "\n";
		
		format = @"\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d";
		
		subtitleIn = @"(?<StartHours>\d+)\s*:\s*(?<StartMinutes>\d+)\s*:\s*(?<StartSeconds>\d+)\s*:\s*(?<StartMillisecondsAsFrames>\d+)\s+(?<EndHours>\d+)\s*:\s*(?<EndMinutes>\d+)\s*:\s*(?<EndSeconds>\d+)\s*:\s*(?<EndMillisecondsAsFrames>\d+).*(\n(?<Text>(.*(?!\n\d+(\s*:\s*\d+){2})\n?)*.))?";
		
		subtitleOut = null;
	}
	
	internal override void GlobalInputGetProperties (string text, ParsingProperties properties) {
		properties.OriginalFrameRate = 25; //Framerate has to be PAL or NTSC, defaulting to PAL
	}
	
	internal override string GetDynamicSubtitleOut (SubtitleProperties properties) {
		bool isFrameRatePAL = IsFrameRatePAL(properties.CurrentFrameRate);
		string suf = GetFrameRateSuffix(isFrameRatePAL);
		return "<<StartHours>>:<<StartMinutes>>:<<StartSeconds>>:<<StartMillisecondsAsFrames" + suf + ">>  <<EndHours>>:<<EndMinutes>>:<<EndSeconds>>:<<EndMillisecondsAsFrames" + suf + ">>\n<<Text>>\n";
	}
	
	/// <summary>Returns whether the frame rate is closer to PAL (25) or to NTSC (29.97).</summary>
	private bool IsFrameRatePAL (float frameRate) {
		return (Math.Abs(frameRate - 25) <= Math.Abs(frameRate - 29.97));
	}
	
	private string GetFrameRateSuffix (bool isFrameRatePAL) {
		return (isFrameRatePAL ? "PAL" : "NTSC");
	}

}

}
