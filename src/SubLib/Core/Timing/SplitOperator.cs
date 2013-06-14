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

		/* Change subtitle text */
		string[] textLines = subtitle.Text.GetLines();
		if (textLines.Length == 1)
			subtitle2.Text.Clear();
		else if (textLines.Length > 1) {
			string[] textLinesHalf1 = null;
			string[] textLinesHalf2 = null;
			SplitArray(textLines, ref textLinesHalf1, ref textLinesHalf2);
			subtitle.Text.Set(textLinesHalf1);
			subtitle2.Text.Set(textLinesHalf2);
		}

		/* Change translation text */
		if (subtitle.HasTranslation) {
			string[] translationLines = subtitle.Translation.GetLines();
			if (translationLines.Length == 1)
				subtitle2.Translation.Clear();
			else if (translationLines.Length > 1) {
				string[] translationLinesHalf1 = null;
				string[] translationLinesHalf2 = null;
				SplitArray(translationLines, ref translationLinesHalf1, ref translationLinesHalf2);
				subtitle.Translation.Set(translationLinesHalf1);
				subtitle2.Translation.Set(translationLinesHalf2);
			}
		}

		return subtitle2;
	}

	private bool isOperationValid (Subtitle subtitle) {
		return subtitle.Times.End >= subtitle.Times.Start;
	}

	private void SplitArray<T> (T[] array, ref T[] half1, ref T[] half2) {
		if (array == null) {
			half1 = null;
			half2 = null;
			return;
		}

		int arrayLength = array.Length;
		if (arrayLength == 0) {
			half1 = new T[0];
			half2 = new T[0];
		}
		else if (arrayLength == 1) {
			half1 = new T[1];
			half1[0] = array[0];
			half2 = new T[0];
		}
		else {
			int half1Length = (int)Math.Round(arrayLength / 2d);
			int half2Length = arrayLength - half1Length;
			half1 = new T[half1Length];
			half2 = new T[half2Length];
			Array.Copy(array, 0, half1, 0, half1Length);
			Array.Copy(array, half1Length, half2, 0, half2Length);
		}
	}
}

}
