/*
 * This file is part of SubLib.
 * Copyright (C) 2011-2019 Pedro Castro
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

namespace SubLib.Core.Timing {

/// <summary>Performs merge operations.</summary>
public class MergeOperator {
	private Subtitles subtitles = null;

	public MergeOperator (Subtitles subtitles) {
		this.subtitles = subtitles;
	}

	/* Public members */

	public bool Merge (int firstSubtitleNumber, int lastSubtitleNumber) {
		if (!isOperationValid(firstSubtitleNumber, lastSubtitleNumber))
			return false;
		else {
			Merge(this.subtitles, firstSubtitleNumber, lastSubtitleNumber);
			return true;
		}
	}


	/* Private members */

	/// <summary>Splits a subtitle in two halves. The subtitle passed as parameter is turned into the first half and the second half is returned.</summary>
	/// <param name="subtitles">The subtitle to split</param>
	/// <param name="subtitleProperties">The subtitle properties</param>
	/// <param name="timeBetweenSubtitles">Time between the 2 subtitles, in milliseconds</param>
	private void Merge (Subtitles subtitles, int firstSubtitleNumber, int lastSubtitleNumber) {
		Subtitle firstSubtitle = subtitles.Collection[firstSubtitleNumber];

		/* Add lines */
		for (int currentSubtitleNumber = firstSubtitleNumber + 1 ; currentSubtitleNumber <= lastSubtitleNumber ; currentSubtitleNumber++) {
			Subtitle currentSubtitle = subtitles.Collection[currentSubtitleNumber];
			firstSubtitle.Text.Add(currentSubtitle.Text.GetLines());
			if (currentSubtitle.HasTranslation) {
				firstSubtitle.Translation.Add(currentSubtitle.Translation.GetLines());
			}
		}

		/* Set times */
		Subtitle lastSubtitle = subtitles.Collection[lastSubtitleNumber];
		firstSubtitle.Times.End = lastSubtitle.Times.End;
	}

	bool isOperationValid (int firstSubtitleNumber, int lastSubtitleNumber) {
		if (firstSubtitleNumber > lastSubtitleNumber)
			return false;

		Subtitle firstSubtitle = subtitles.Collection[firstSubtitleNumber];
		Subtitle lastSubtitle = subtitles.Collection[lastSubtitleNumber];
		return (firstSubtitle != null) && (lastSubtitle != null) && (firstSubtitle.Times.Start <= lastSubtitle.Times.End);
	}
}

}
