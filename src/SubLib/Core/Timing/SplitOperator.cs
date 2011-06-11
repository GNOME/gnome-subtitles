/*
 * This file is part of SubLib.
 * Copyright (C) 2011 Pedro Castro
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

namespace SubLib.Core.Timing {

/// <summary>Performs split operations.</summary>
public class SplitOperator {
	private Subtitles subtitles = null;
	private int timeBetweenSubtitles = -1;
	
	public SplitOperator (Subtitles subtitles, int timeBetweenSubtitles) {
		this.subtitles = subtitles;
		this.timeBetweenSubtitles = timeBetweenSubtitles;
	}

	/* Public members */
	
	public Subtitle Split (Subtitle subtitle) {
		if (!isOperationValid(subtitle))
			return null;
		
		Subtitle subtitle2 = Split(subtitle, this.subtitles.Properties, this.timeBetweenSubtitles);
		return subtitle2;
	}
	
	
	/* Private members */
	
	/// <summary>Splits a subtitle in two halves. The subtitle passed as parameter is turned into the first half and the second half is returned.</summary>
	/// <param name="subtitle">The subtitle to split</param>
	/// <param name="subtitleProperties">The subtitle properties</param>
	/// <param name="timeBetweenSubtitles">Time between the 2 subtitles, in milliseconds</param>
	private Subtitle Split (Subtitle subtitle, SubtitleProperties subtitleProperties, int timeBetweenSubtitles) {
		Subtitle subtitle2 = subtitle.Clone(subtitleProperties);
		
		/* Change timings */
		int originalStart = (int)subtitle.Times.Start.TotalMilliseconds;
		int originalEnd = (int)subtitle.Times.End.TotalMilliseconds;

		if ((originalEnd - originalStart) <= timeBetweenSubtitles) {
			/* Not possible to have the predefined time between subtitle, subtitle 2 will start at subtitle's end time */
			int originalMiddle = (originalStart + originalEnd) / 2;
			TimeSpan newSubtitleEnd = TimeSpan.FromMilliseconds(originalMiddle);
			subtitle.Times.End = newSubtitleEnd;
			subtitle2.Times.Start = newSubtitleEnd;
		}
		else {
			int newSubtitleEnd = (originalStart + originalEnd - timeBetweenSubtitles) / 2;
			int subtitle2Start = newSubtitleEnd + timeBetweenSubtitles;
			subtitle.Times.End = TimeSpan.FromMilliseconds(newSubtitleEnd);
			subtitle2.Times.Start = TimeSpan.FromMilliseconds(subtitle2Start);
		}
		
		/* Change text */
		//FIXME divide text in half
	
		return subtitle2;
	}
	
	private bool isOperationValid (Subtitle subtitle) {
		return subtitle.Times.End >= subtitle.Times.Start;
	}
}

}
