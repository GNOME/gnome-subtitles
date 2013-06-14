/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2010 Pedro Castro
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
using SubLib.Exceptions;
using System;
using System.Text.RegularExpressions;

namespace SubLib.IO.SubtitleFormats {

internal static class BuiltInSubtitleFormats {

	/* Must be ordered with more precise/complex subtitle formats first, so simple/general/catch-all formats don't match other formats
	 * Must conform to the order in SubtitleType Enum */
	private static SubtitleFormat[] subtitleFormats = {
		new SubtitleFormatMicroDVD(),
		new SubtitleFormatSubRip(),
		new SubtitleFormatSubStationAlpha(),
		new SubtitleFormatAdvancedSubStationAlpha(),
		new SubtitleFormatMPlayer(),
		new SubtitleFormatMPlayer2(),
		new SubtitleFormatMPSub(),
		new SubtitleFormatSubViewer1(),
        new SubtitleFormatSubViewer2(),
		new SubtitleFormatAQTitle(),
		new SubtitleFormatMacSUB(),
		new SubtitleFormatPhoenixJapanimationSociety(),
		new SubtitleFormatPanimator(),
		new SubtitleFormatSofni(),
		new SubtitleFormatSubCreator1x(),
		new SubtitleFormatViPlaySubtitleFile(),
		new SubtitleFormatDKSSubtitleFormat(),
        new SubtitleFormatPowerDivX(),
        new SubtitleFormatKaraokeLyricsVKT(),
        new SubtitleFormatKaraokeLyricsLRC(),
        new SubtitleFormatAdobeEncoreDVD(),
        new SubtitleFormatFABSubtitler()
	};

	internal static SubtitleFormat[] SubtitleFormats {
		get { return subtitleFormats; }
	}

	internal static SubtitleFormat GetFormat (SubtitleType subtitleType) {
		if (subtitleType == SubtitleType.Unknown)
			return null;
		else
			return subtitleFormats[(int)subtitleType - 1];
	}

    internal static SubtitleFormat Detect (string subtitleText) {
    	int lengthToTest = Math.Min(subtitleText.Length, 2000); //only use the first 2000 chars
		foreach (SubtitleFormat format in BuiltInSubtitleFormats.SubtitleFormats) {
			bool matchSuccess = TrySubtitleFormat(format, subtitleText, lengthToTest);
			if (matchSuccess)
				return format;
		}
		throw new UnknownSubtitleFormatException();
	}

    private static bool TrySubtitleFormat (SubtitleFormat format, string subtitleText, int length) {
    		Regex expression = new Regex(format.Format, RegexOptions.IgnoreCase);
    		Match match = expression.Match(subtitleText, 0, length);
    		return match.Success;
    }

}

}
