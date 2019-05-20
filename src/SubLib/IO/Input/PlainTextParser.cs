/*
 * This file is part of SubLib.
 * Copyright (C) 2007-2019 Pedro Castro
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
using System.Text;

namespace SubLib.IO.Input {

internal class PlainTextParser {

	private bool withCharacterNames = false;
	private string lineSeparator = String.Empty;
	private string text = String.Empty;

	internal PlainTextParser(bool withCharacterNames, string lineSeparator) {
		this.withCharacterNames = withCharacterNames;
		this.lineSeparator = lineSeparator;
	}

	internal PlainTextParser(bool withCharacterNames) : this(withCharacterNames, @"\n") {
	}
	/// <summary>Parses the specified text.</summary>
	/// <remarks>The created <see cref="SubtitleCollection" /> will have its <see cref="SubtitleProperties" /> property set to null.
	/// It is mandatory to use <see cref="SubtitleCollection.SetPropertiesForAll" /> after.</remarks>
	internal ParsingProperties Parse (string text, TimingMode timingMode, Encoding encoding, out SubtitleCollection collection) {

		collection = new SubtitleCollection();
		ParsingProperties properties = new ParsingProperties();
		this.text = text;
		properties.TimingMode = timingMode;

		/* Read the subtitles */
		ReadSubtitles(encoding, properties, collection);

		return properties;
	}

	private void ReadSubtitles (Encoding encoding, ParsingProperties properties, SubtitleCollection collection) {

		string[] lines = text.Split(new char[] {'\n'});
		for (int i = 0; i < lines.Length; i++) {
			SubtitleText stext = ParseSubtitleText(lines[i]);
			Style style = new Style();
			if(!stext.IsEmpty) {
				Subtitle subtitle = new Subtitle(null, stext, style);
				collection.Add(subtitle);
			}
		}

	}

	private SubtitleText ParseSubtitleText (string line) {
		string text = String.Empty;
		if (withCharacterNames) {
			string[] pieces = line.Split(new char[] { ':' });
			if (pieces.Length > 1)
				text = pieces[1];
			else
				text = pieces[0];
		}
		else
			text = line;

		if (text.Length > 0)
			return new SubtitleText(text, lineSeparator, true);
		else return new SubtitleText();

	}

}

}
