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
using System;
using System.Collections;

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

	public Subtitle[] Split (Subtitle subtitle) {
		if (!IsOperationValid(subtitle))
			return null;

		return Split(subtitle, this.subtitles.Properties, this.timeBetweenSubtitles);
	}


	/* Private members */

	/// <summary>Splits a single subtitle into multiple subtitles - one for each subtitle line.</summary>
	/// <param name="subtitle">The subtitle to split</param>
	/// <param name="subtitleProperties">The subtitle properties</param>
	/// <param name="timeBetweenSubtitles">Time between the 2 subtitles, in milliseconds</param>
	private Subtitle[] Split (Subtitle subtitle, SubtitleProperties subtitleProperties, int timeBetweenSubtitles) {

		//Get text lines
		ArrayList textLines = GetTextLines(subtitle);
		if (textLines == null) {
			return null;
		}

		//Get translation lines
		ArrayList translationLines = GetAlignedTranslationLines(subtitle, textLines);
		
		//Prepare times. There must be at least
		int originalStart = (int)subtitle.Times.Start.TotalMilliseconds;
		int originalEnd = (int)subtitle.Times.End.TotalMilliseconds;
		int originalDuration = originalEnd - originalStart;
		
		int newDuration = CalculateSubtitleDuration(originalDuration, timeBetweenSubtitles, textLines.Count);
		if (newDuration < timeBetweenSubtitles) {
			timeBetweenSubtitles = 0; //If each subtitle would have a duration shorter than timeBetweenSubtitles itself, we don't use it
			newDuration = CalculateSubtitleDuration(originalDuration, timeBetweenSubtitles, textLines.Count);
		}
		
		/* Create a new Subtitle for each line by cloning the original one and setting appropriate text and times.
		 * Note: textLines can have a single line (which means the other original lines were empty). In this case,
		 * the subtitle will still be changed as a form of "cleanup" where the empty lines are removed.
		 */
		ArrayList result = new ArrayList();
		for (int i = 0; i < textLines.Count; i++) {
			Subtitle newSubtitle = subtitle.Clone(subtitleProperties);
			
			//Set text
			newSubtitle.Text.Set(textLines[i] as String);
			
			//Set translation
			if (subtitle.HasTranslation) {
				if (i < translationLines.Count) { 
					newSubtitle.Translation.Set(translationLines[i] as String);
				} else {
					newSubtitle.Translation.Clear();
				}
			}
			
			int start = originalStart + i * (newDuration + timeBetweenSubtitles);
			newSubtitle.Times.Start = TimeSpan.FromMilliseconds(start);
			newSubtitle.Times.End = TimeSpan.FromMilliseconds(start + newDuration);

			result.Add(newSubtitle);
		}
		
		return (Subtitle[])result.ToArray(typeof(Subtitle));
	}
	
	private ArrayList GetTextLines (Subtitle subtitle) {
		//Check if we have multiple text lines in this subtitle. If we don't, just return.
		string[] originalLines = subtitle.Text.GetLines();
		if ((originalLines == null) || (originalLines.Length <= 1)) {
			return null;
		}

		//Remove empty or whitespace text lines.
		ArrayList textLines = GetNonEmptyLines(originalLines);
		if (textLines.Count == 0) {
			return null;
		}
		
		return textLines;
	}

	private ArrayList GetAlignedTranslationLines (Subtitle subtitle, ArrayList textLines) {
		if (!subtitle.HasTranslation) {
			return null;
		}
		
		string[] originalLines = subtitle.Translation.GetLines();
		if (originalLines == null) {
			return null;
		}

		//Remove empty or whitespace text lines.
		ArrayList translationLines = GetNonEmptyLines(originalLines);
		if (translationLines.Count > textLines.Count) {
			int lastTextIndex = textLines.Count - 1;
			string lastLine = String.Join("\n", (string[])translationLines.ToArray(typeof(string)), lastTextIndex, translationLines.Count - textLines.Count + 1);
			translationLines[lastTextIndex] = lastLine;
			translationLines.RemoveRange(lastTextIndex + 1, translationLines.Count - textLines.Count);
		}
		
		return translationLines;
	}
	
	private ArrayList GetNonEmptyLines (string[] lines) {
		ArrayList result = new ArrayList();
		
		foreach (string line in lines) {
			if (!String.IsNullOrWhiteSpace(line)) {
				result.Add(line);
			}
		}
		
		return result;
	}
	
	private int CalculateSubtitleDuration (int originalDuration, int timeBetweenSubtitles, int lineCount) {
		return (originalDuration - (timeBetweenSubtitles * (lineCount - 1))) / lineCount;
	}

	private bool IsOperationValid (Subtitle subtitle) {
		return subtitle.Times.End >= subtitle.Times.Start;
	}

}

}