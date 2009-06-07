/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2009 Pedro Castro
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

	/// <summary>Replaces all occurences of some text with the specified replacement.</summary>
	/// <param name="regex">A regular expression used to find the text to be replaced.</param>
	/// <param name="replacement">The text that will be used as a replacement.</param>
	/// <param name="lineBreak">The line break to use between multiple lines of text in each subtitle.</param>
	/// <remarks>The newline (\n) char is used as the line break.</remarks>
	/// <returns>The previous text of the replaced subtitles.</returns>
	public ArrayList ReplaceAll (Regex regex, string replacement) {
		return ReplaceAll(regex, replacement, "\n");
	}

	/// <summary>Replaces all occurences of some text with the specified replacement.</summary>
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
	
	/// <summary>Finds the subtitle that contains the specified time position.</summary>
	/// <param name="time">The time position, in seconds.</param>
	/// <returns>The found subtitle number, or -1 if no subtitle was found.</returns>
	public int FindWithTime (float time) {
		SubtitleCollection collection = subtitles.Collection;
	
		if (collection.Count == 0)
			return -1;
		
		for (int subtitleNumber = 0 ; subtitleNumber < collection.Count ; subtitleNumber++) {
			Subtitle subtitle = collection[subtitleNumber];
			double start = subtitle.Times.Start.TotalSeconds;
			if (time < start)
				continue;
			
			double end = subtitle.Times.End.TotalSeconds;
			if (time <= end)
				return subtitleNumber;
		}
		return -1; // No subtitles were found 
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
		return MatchValues(regex.Match(text.Get(lineBreak)), subtitleNumber, textType);
	}
	
	private SubtitleSearchResults FindInTextContentFromIndex (int subtitleNumber, string lineBreak, Regex regex, int startIndex, SubtitleTextType textType) {
		SubtitleText text = subtitles.GetSubtitleText(subtitleNumber, textType);
		return MatchValues(regex.Match(text.Get(lineBreak), startIndex), subtitleNumber, textType);
	}
	
	private SubtitleSearchResults FindInTextContentTillIndex (int subtitleNumber, string lineBreak, Regex regex, int endIndex, SubtitleTextType textType, bool backwards) {
		SubtitleText text = subtitles.GetSubtitleText(subtitleNumber, textType);
		string matchText = text.Get(lineBreak);
		int startIndex = (backwards ? matchText.Length : 0);
		int length = (backwards ? matchText.Length - endIndex : endIndex);		
		return MatchValues(regex.Match(text.Get(lineBreak), startIndex, length), subtitleNumber, textType);
	}
	
	private SubtitleSearchResults MatchValues (Match match, int subtitleNumber, SubtitleTextType textType) {
		if (match.Success)
			return new SubtitleSearchResults(subtitleNumber, textType, match.Index, match.Length);
		else
			return null;
	}

	private string ReplaceText (string text, Regex regex, string replacement, MatchEvaluationCounter counter) {
		counter.EvaluationOccured = false;
		string newText = regex.Replace(text, counter.Evaluator);
		return (counter.EvaluationOccured ? newText : null);
	}
	
	
}

}
