/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2019 Pedro Castro
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
using System.Text.RegularExpressions;

namespace SubLib.Core.Search {

/// <summary>Performs search operations.</summary>
public class SearchOperator {
	private Subtitles subtitles = null;

	public SearchOperator (Subtitles subtitles) {
		this.subtitles = subtitles;
	}

	/* Public members */


	/// <summary>Searches for text using the specified search options.</summary>
	/// <param name="options">The search options.</param>
	/// <returns>The search results, or null in case no results were found.</returns>
	public SubtitleSearchResults Find (SubtitleSearchOptions options) {
		if (options.Backwards)
			return FindBackward(options);
		else
			return FindForward(options);
	}

	/// <summary>Replaces all occurrences of some text with the specified replacement.</summary>
	/// <param name="regex">A regular expression used to find the text to be replaced.</param>
	/// <param name="replacement">The text that will be used as a replacement.</param>
	/// <param name="lineBreak">The line break to use between multiple lines of text in each subtitle.</param>
	/// <remarks>The newline (\n) char is used as the line break.</remarks>
	/// <returns>The previous text of the replaced subtitles.</returns>
	public ArrayList ReplaceAll (Regex regex, string replacement) {
		return ReplaceAll(regex, replacement, "\n");
	}

	/// <summary>Replaces all occurrences of some text with the specified replacement.</summary>
	/// <param name="regex">A regular expression used to find the text to be replaced.</param>
	/// <param name="replacement">The text that will be used as a replacement.</param>
	/// <param name="lineBreak">The line break to use between multiple lines of text in each subtitle.</param>
	/// <returns>The previous text of the replaced subtitles.</returns>
	public ArrayList ReplaceAll (Regex regex, string replacement, string lineBreak) {
		ArrayList replaced = new ArrayList();
		MatchEvaluationCounter counter = new MatchEvaluationCounter(replacement);

		for (int subtitleNumber = 0 ; subtitleNumber < subtitles.Collection.Count ; subtitleNumber++) {
			Subtitle subtitle = subtitles.Collection[subtitleNumber];

			/* Handle text */
			string oldText = subtitle.Text.Get(lineBreak);
			string newText = ReplaceText(oldText, regex, replacement, counter);
			string textToStore = null;
			if (newText != null) {
				subtitle.Text.Set(newText, lineBreak, false);
				textToStore = oldText;
			}

			/* Handle translation */
			string oldTranslation = subtitle.Translation.Get(lineBreak);
			string newTranslation = ReplaceText(oldTranslation, regex, replacement, counter);
			string translationToStore = null;
			if (newTranslation != null) {
				subtitle.Translation.Set(newTranslation, lineBreak, false);
				translationToStore = oldTranslation;
			}

			if ((textToStore != null) || (translationToStore != null)) {
				replaced.Add(new SubtitleReplaceResult(subtitleNumber, textToStore, translationToStore));
			}
		}

		return replaced;
	}

	/// <summary>Finds the subtitle containing the specified time.</summary>
	/// <param name="time">The time position.</param>
	/// <returns>The number of the subtitle that was found, or -1 if no subtitle was found.</returns>
	public int FindWithTime (TimeSpan time) {
		SubtitleCollection collection = subtitles.Collection;

		for (int subtitleNumber = 0 ; subtitleNumber < collection.Count ; subtitleNumber++) {
			Subtitle subtitle = collection[subtitleNumber];
			if ((time >= subtitle.Times.Start) && (time <= subtitle.Times.End)) {
				return subtitleNumber;
			}
		}

		return -1; // No subtitles were found
	}

	/// <summary>Finds the subtitle nearer the specified time.</summary>
	/// <param name="time">The time position.</param>
	/// <returns>The number of the subtitle that was found, or -1 if no subtitle was found.</returns>
	public int FindNearTime (TimeSpan time) {
		SubtitleCollection collection = subtitles.Collection;
		if (collection.Count == 0) {
			return -1;
		}

		/* Check if before the first subtitle */
		if (time < collection[0].Times.Start) {
			return 0;
		}

		/* Iterate subtitles two by two - the last subtitle is handled in pair and individually afterwards */
		for (int subtitleNumber = 0 ; subtitleNumber < collection.Count - 1 ; subtitleNumber++) {
			Subtitle subtitle = collection[subtitleNumber];

			/* Continue iterating if didn't reach subtitle start yet */
			TimeSpan start = subtitle.Times.Start;
			if (time < start) {
				continue;
			}

			/* Check if time is contained by the subtitle */
			TimeSpan end = subtitle.Times.End;
			if (time <= end) {
				return subtitleNumber;
			}

			/* Check if contained between this and the next subtitle, and which is nearest */
			int nextSubtitleIndex = subtitleNumber + 1;
			Subtitle nextSubtitle = collection[nextSubtitleIndex];
			TimeSpan nextSubtitleStart = nextSubtitle.Times.Start;
			if (time < nextSubtitleStart) {
				return ((time - end) < (nextSubtitleStart - time)) ? subtitleNumber : nextSubtitleIndex;
			}
		}

		/* If no rule matched before, time must be after last subtitle */
		return collection.Count - 1;
	}

	/* Private members */


	/// <summary>Searches forward for text using the specified search options.</summary>
	/// <param name="options">The search options.</param>
	/// <returns>The search results, or null in case no results were found.</returns>
	private SubtitleSearchResults FindForward (SubtitleSearchOptions options) {
		SubtitleCollection collection = subtitles.Collection;

		if (collection.Count == 0)
			return null;

		/* Search the startSubtitle subtitle starting at the startIndex */
		SubtitleSearchResults results = FindInSubtitleFromIndex(options.StartSubtitle, options.LineBreak, options.Regex, options.StartIndex, options.TextType, options.Backwards);
		if (results != null)
			return results;

		/* Iterate through the rest of the collection */
		for (int subtitleNumber = options.StartSubtitle + 1 ; subtitleNumber < collection.Count ; subtitleNumber++) {
			results = FindInSubtitle(subtitleNumber, options.LineBreak, options.Regex, options.Backwards);
			if (results != null)
				return results;
		}

		if (options.Wrap) {
			/* Iterate from the beginning back to the subtitle */
			for (int subtitleNumber = 0 ; subtitleNumber < options.StartSubtitle ; subtitleNumber++) {
				results = FindInSubtitle(subtitleNumber, options.LineBreak, options.Regex, options.Backwards);
				if (results != null)
					return results;
			}
			/* Search the startSubtitle ending at the startIndex */
			results = FindInSubtitleTillIndex(options.StartSubtitle, options.LineBreak, options.Regex, options.StartIndex, options.TextType, options.Backwards);
			if (results != null)
				return results;
		}

		/* Text not found */
		return null;
	}

	/// <summary>Searches backward for text using the specified search options.</summary>
	/// <param name="options">The search options.</param>
	/// <returns>The search results, or null in case no results were found.</returns>
	private SubtitleSearchResults FindBackward (SubtitleSearchOptions options) {
		SubtitleCollection collection = subtitles.Collection;

		if (collection.Count == 0)
			return null;

		/* Search the subtitle starting at the startIndex */
		SubtitleSearchResults results = FindInSubtitleFromIndex(options.StartSubtitle, options.LineBreak, options.Regex, options.StartIndex, options.TextType, options.Backwards);
		if (results != null)
			return results;

		/* Iterate through the start of the collection */
		for (int subtitleNumber = options.StartSubtitle - 1 ; subtitleNumber > 0 ; subtitleNumber--) {
			results = FindInSubtitle(subtitleNumber, options.LineBreak, options.Regex, options.Backwards);
			if (results != null)
				return results;
		}

		if (options.Wrap) {
			/* Iterate from the end back to the subtitle */
			for (int subtitleNumber = collection.Count - 1 ; subtitleNumber > options.StartSubtitle ; subtitleNumber--) {
				results = FindInSubtitle(subtitleNumber, options.LineBreak, options.Regex, options.Backwards);
				if (results != null)
					return results;
			}
			/* Search the subtitle ending at the startIndex */
			results = FindInSubtitleTillIndex(options.StartSubtitle, options.LineBreak, options.Regex, options.StartIndex, options.TextType, options.Backwards);
			if (results != null)
				return results;
		}

		/* Text not found */
		return null;
	}

	/// <returns>The <see cref="SubtitleSearchResults" />, or null if the text was not found.</returns>
	private SubtitleSearchResults FindInSubtitle (int subtitleNumber, string lineBreak, Regex regex, bool backwards) {
		if (backwards) {
			/* Find first in the translation */
			SubtitleSearchResults results = FindInTextContent(subtitleNumber, lineBreak, regex, SubtitleTextType.Translation);
			if (results != null)
				return results;

			/* Not found in the translation, finding in the text */
			return FindInTextContent(subtitleNumber, lineBreak, regex, SubtitleTextType.Text);
		}
		else {
			/* Find first in the text */
			SubtitleSearchResults results = FindInTextContent(subtitleNumber, lineBreak, regex, SubtitleTextType.Text);
			if (results != null)
				return results;

			/* Not found in the text, finding in the translation */
			return FindInTextContent(subtitleNumber, lineBreak, regex, SubtitleTextType.Translation);
		}
	}

	/// <returns>The <see cref="SubtitleSearchResults" />, or null if the text was not found.</returns>
	private SubtitleSearchResults FindInSubtitleFromIndex (int subtitleNumber, string lineBreak, Regex regex, int startIndex, SubtitleTextType textType, bool backwards) {
		if (backwards) {
			if (textType == SubtitleTextType.Text) {
				/* Find in the text starting at the specified index */
				return FindInTextContentFromIndex(subtitleNumber, lineBreak, regex, startIndex, SubtitleTextType.Text);
			}
			else {
				/* Find first in the translation starting at the specified index */
				SubtitleSearchResults results = FindInTextContentFromIndex(subtitleNumber, lineBreak, regex, startIndex, SubtitleTextType.Translation);
				if (results != null)
					return results;

				/* Not found in the translation, finding in the text */
				return FindInTextContent(subtitleNumber, lineBreak, regex, SubtitleTextType.Text);
			}
		}
		else {
			if (textType == SubtitleTextType.Text) {
				/* Find first in the text starting at the specified index */
				SubtitleSearchResults results = FindInTextContentFromIndex(subtitleNumber, lineBreak, regex, startIndex, SubtitleTextType.Text);
				if (results != null)
					return results;

				/* Not found in the text, finding in the translation */
				return FindInTextContent(subtitleNumber, lineBreak, regex, SubtitleTextType.Translation);
			}
			else {
				/* Find in the translation starting at the specified index */
				return FindInTextContentFromIndex(subtitleNumber, lineBreak, regex, startIndex, SubtitleTextType.Translation);
			}
		}
	}

	/// <returns>The <see cref="SubtitleSearchResults" />, or null if the text was not found.</returns>
	private SubtitleSearchResults FindInSubtitleTillIndex (int subtitleNumber, string lineBreak, Regex regex, int endIndex, SubtitleTextType textType, bool backwards) {
		if (backwards) {
			if (textType == SubtitleTextType.Text) {
				/* Find first in the translation */
				SubtitleSearchResults results = FindInTextContent(subtitleNumber, lineBreak, regex, SubtitleTextType.Translation);
				if (results != null)
					return results;

				/* Not found in the text, finding in the text till the specified index */
				return FindInTextContentTillIndex(subtitleNumber, lineBreak, regex, endIndex, SubtitleTextType.Text, backwards);
			}
			else {
				/* Find in the translation till specified index */
				return FindInTextContentTillIndex(subtitleNumber, lineBreak, regex, endIndex, SubtitleTextType.Translation, backwards);
			}
		}
		else {
			if (textType == SubtitleTextType.Text) {
				/* Find in the text ending at the specified index */
				return FindInTextContentTillIndex(subtitleNumber, lineBreak, regex, endIndex, SubtitleTextType.Text, backwards);
			}
			else {
				/* Find first in the text */
				SubtitleSearchResults results = FindInTextContent(subtitleNumber, lineBreak, regex, SubtitleTextType.Text);
				if (results != null)
					return results;

				/* Not found in the text, finding in the translation till the specified index */
				return FindInTextContentTillIndex(subtitleNumber, lineBreak, regex, endIndex, SubtitleTextType.Translation, backwards);
			}
		}
	}

	private SubtitleSearchResults FindInTextContent (int subtitleNumber, string lineBreak, Regex regex, SubtitleTextType textType) {
		SubtitleText text = subtitles.GetSubtitleText(subtitleNumber, textType);
		return MatchValues(regex.Match(text.Get(lineBreak)), subtitleNumber, textType, 0);
	}

	private SubtitleSearchResults FindInTextContentFromIndex (int subtitleNumber, string lineBreak, Regex regex, int startIndex, SubtitleTextType textType) {
		SubtitleText text = subtitles.GetSubtitleText(subtitleNumber, textType);
		return MatchValues(regex.Match(text.Get(lineBreak), startIndex), subtitleNumber, textType, 0);
	}

	private SubtitleSearchResults FindInTextContentTillIndex (int subtitleNumber, string lineBreak, Regex regex, int endIndex, SubtitleTextType textType, bool backwards) {
		SubtitleText text = subtitles.GetSubtitleText(subtitleNumber, textType);
		string subtitleText = text.Get(lineBreak);

		if (backwards) {
			string subtitleTextSubstring = subtitleText.Substring(endIndex);
			return MatchValues(regex.Match(subtitleTextSubstring), subtitleNumber, textType, endIndex);
		}
		else {
			string subtitleTextSubstring = subtitleText.Substring(0, endIndex);
			return MatchValues(regex.Match(subtitleTextSubstring), subtitleNumber, textType, 0);
		}
	}

	private SubtitleSearchResults MatchValues (Match match, int subtitleNumber, SubtitleTextType textType, int charsBeforeMatchInput) {
		if (match.Success) {
			return new SubtitleSearchResults(subtitleNumber, textType, match.Index + charsBeforeMatchInput, match.Length);
		}
		else
			return null;
	}

	private string ReplaceText (string text, Regex regex, string replacement, MatchEvaluationCounter counter) {
		counter.EvaluationOccurred = false;
		string newText = regex.Replace(text, counter.Evaluator);
		return (counter.EvaluationOccurred ? newText : null);
	}


}

}
