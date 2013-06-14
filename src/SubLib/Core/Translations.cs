/*
 * This file is part of SubLib.
 * Copyright (C) 2007-2008 Pedro Castro
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

namespace SubLib.Core {

/// <summary>Allows to import translation subtitles into existing subtitles.</summary>
public class Translations {

	/// <summary>Creates a new instance of the <see cref="Translations" /> class.</summary>
	public Translations () {
	}

	/* Public methods */

	//TODO have more elaborate heuristics, taking the times into account and reporting errors
	/// <summary>Imports translated subtitles into existing subtitles.</summary>
	/// <param name="subtitles">The subtitles to import the translation to.</param>
	/// <param name="translation">The translated subtitles.</param>
	public void Import (Subtitles subtitles, Subtitles translation, int timeBetweenSubtitles) {
		AddExtraSubtitles(subtitles, translation, timeBetweenSubtitles);
		CopyTranslation(subtitles, translation);
	}

	/// <summary>Removes the entire translation from existing subtitles.</summary>
	/// <param name="subtitles">The subtitles to remove the translation from.</param>
	public void Clear (Subtitles subtitles) {
		foreach (Subtitle subtitle in subtitles.Collection)
			subtitle.ClearTranslation();
	}

	/* Private methods */

	/// <summary>Adds the number of subtitles that the translation has more than the original subtitles.</summary>
	/// <remarks>A gap between subtitles of <see cref="SubtitleConstants.MinTimeBetweenSubtitles" /> will be used.</remarks>
	private void AddExtraSubtitles (Subtitles subtitles, Subtitles translation) {
		AddExtraSubtitles(subtitles, translation, (int)(SubtitleConstants.MinTimeBetweenSubtitles));
	}

	/// <summary>Adds the number of subtitles missing comparing to the translation.</summary>
	private void AddExtraSubtitles (Subtitles subtitles, Subtitles translation, int timeBetweenSubtitles) {
		int translationCount = translation.Collection.Count;
		int subtitlesCount = subtitles.Collection.Count;
		int extraCount = translationCount - subtitlesCount;

		if (extraCount <= 0)
			return;

		for (int position = subtitlesCount - 1 ; position < translationCount - 1 ; position++) {
			subtitles.Collection.AddNewAfter(position, subtitles.Properties, timeBetweenSubtitles);
		}
	}

	/// <summary>Copies the translation to the subtitles.</summary>
	private void CopyTranslation (Subtitles subtitles, Subtitles translation) {
		for (int subtitleNumber = 0 ; subtitleNumber < translation.Collection.Count ; subtitleNumber++) {
			Subtitle translated = translation.Collection[subtitleNumber];
			Subtitle original = subtitles.Collection[subtitleNumber];

			string translatedText = translated.Text.Get();
			original.Translation.Set(translatedText);
		}
	}

}

}
