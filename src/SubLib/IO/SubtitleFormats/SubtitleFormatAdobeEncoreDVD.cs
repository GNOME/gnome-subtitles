/*
 * This file is part of SubLib.
 * Copyright (C) 2007-2010 Pedro Castro
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

//TODO: warn when saving files with a framerate different than 25 or 29,97.
internal class SubtitleFormatAdobeEncoreDVD : SubtitleFormat {
	private Regex inputRegexPAL = new Regex(@"\d+(\s*:\s*\d+){3}\s+\d+(\s*:\s*\d+){3}\s*.+");

	internal SubtitleFormatAdobeEncoreDVD () {
		name = "Adobe Encore DVD";
		type = SubtitleType.AdobeEncoreDVD;
		mode = SubtitleMode.Times;
		extensions = new string[] { "txt" };
		lineBreak = "\n";

		format = @"\d+([:;]\d+){3} +\d+([:;]\d+){3} +.";

		subtitleIn = @"(\d+\s+)?(?<StartHours>\d+)\s*[:;]\s*(?<StartMinutes>\d+)\s*[:;]\s*(?<StartSeconds>\d+)\s*[:;]\s*(?<StartMillisecondsAsFrames>\d+)\s+(?<EndHours>\d+)\s*[:;]\s*(?<EndMinutes>\d+)\s*[:;]\s*(?<EndSeconds>\d+)\s*[:;]\s*(?<EndMillisecondsAsFrames>\d+)\s+(?<Text>((.(?!(\d+\s+)?\d+(\s*[:;]\s*\d+){3}))*\n)+)";

		subtitleOut = null;
	}

	internal override string GetDynamicSubtitleOut (SubtitleProperties properties) {
		bool isFrameRatePAL = IsFrameRatePAL(properties.CurrentFrameRate);
		char sep = GetTimingSeparator(isFrameRatePAL);
		string suf = GetFrameRateSuffix(isFrameRatePAL);
		return "<<SubtitleNumber>> <<StartHours>>" + sep + "<<StartMinutes>>" + sep + "<<StartSeconds>>" + sep + "<<StartMillisecondsAsFrames" + suf + ">> <<EndHours>>" + sep + "<<EndMinutes>>" + sep + "<<EndSeconds>>" + sep + "<<EndMillisecondsAsFrames" + suf + ">> <<Text>>";
	}

	internal override void GlobalInputGetProperties (string text, ParsingProperties properties) {
		bool isFrameRatePAL = inputRegexPAL.Match(text).Success;
		float frameRate = (isFrameRatePAL ? 25 : 29.97F);
		properties.InputFrameRate = frameRate;
	}

	/* Private members */
	/// <summary>Returns the PAL (25) or NTSC (29.97) timing separator char, according to whether the frame rate is PAL or NTSC.</summary>
	private char GetTimingSeparator (bool isFrameRatePAL) {
		return (isFrameRatePAL ? ':' : ';');
	}

	private string GetFrameRateSuffix (bool isFrameRatePAL) {
		return (isFrameRatePAL ? "PAL" : "NTSC");
	}

	/// <summary>Returns whether the frame rate is closer to PAL (25) or to NTSC (29.97).</summary>
	private bool IsFrameRatePAL (float frameRate) {
		return (Math.Abs(frameRate - 25) <= Math.Abs(frameRate - 29.97));
	}

}

}