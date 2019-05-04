/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2019 Pedro Castro
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

using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace SubLib.Core.Domain {

//TODO this can be optimized
/// <summary>Represents the text of a subtitle.</summary>
public class SubtitleText : ICloneable {
	private ArrayList lines = new ArrayList();

	/// <summary>Initializes a new instance of the <see cref="SubtitleText" /> class
	/// with the specified text, line break and trimming option.</summary>
	/// <param name="text">The subtitle text.</param>
	/// <param name="lineBreak">The text substring that marks the end of lines.</param>
	/// <param name="toTrimLines">Whether to trim every text line.</param>
	public SubtitleText (string text, string lineBreak, bool toTrimLines) {
		Set(text, lineBreak, toTrimLines);
	}

	/// <summary>Initializes a new instance of the <see cref="SubtitleText" /> class
	/// with the specified text.</summary>
	/// <remarks>Newline (\n) is used as the line break. The text lines are not trimmed.</remarks>
	/// <param name="text">The subtitle text.</param>
	public SubtitleText (string text) : this(text, "\n", false) {
	}

	/// <summary>Initializes a new instance of the <see cref="SubtitleText" /> class, with empty text.</summary>
	public SubtitleText() {
	}


	/* Public properties */

	/// <summary>Whether there is no text.</summary>
	public bool IsEmpty {
		get { return ((lines.Count == 0) || ((lines.Count == 1) && ((lines[0] as string).Length == 0))); }
	}

	public IEnumerator GetEnumerator () {
		return lines.GetEnumerator();
	}


	/* Public methods */

	public void Clear () {
		lines.Clear();
	}

	/// <summary>Gets the specified text line.</summary>
	/// <param name="index">The zero-based line number index.</param>
	/// <returns>The specified text line.</returns>
	public string GetLine (int index) {
		if ((index >= 0) && (index < lines.Count))
			return lines[index] as string;
		else
			return String.Empty;
	}

	/// <summary>Gets the text lines merged with the specified line break.</summary>
	/// <param name="lineBreak">The line break used to merge the text.</param>
	/// <returns>The subtitle text.</returns>
	public string Get (string lineBreak) {
		string text = String.Empty;
		IEnumerator textLines = lines.GetEnumerator();
		if (textLines.MoveNext()){
			text = (textLines.Current as string);
			while (textLines.MoveNext())
				text += lineBreak + (textLines.Current as string);
		}
		return text;
	}

	/// <summary>Gets the subtitle text.</summary>
	/// <remarks>The text lines end with the new line (\n) char.</remarks>
	/// <returns>The subtitle text.</returns>
	public string Get () {
		return Get("\n");
	}

	/// <summary>Gets the text lines merged with the specified line break and replaces those that are empty.</summary>
	/// <param name="replacement">The text to replace empty lines with.</param>
	/// <param name="lineBreak">The line break used to merge the text.</param>
	/// <remarks>A subtitle line is considered empty if it has no characters.</remarks>
	/// <returns>The subtitle text, after replacement.</returns>
	public string GetReplaceEmptyLines (string replacement, string lineBreak) {
		if (this.IsEmpty)
			return replacement;

		string text = String.Empty;
		IEnumerator textLines = lines.GetEnumerator();
		if (textLines.MoveNext()){
			string line = (textLines.Current as string);
			text = ReplaceLineIfEmpty(line, replacement);
			while (textLines.MoveNext()) {
				line = (textLines.Current as string);
				text += lineBreak + ReplaceLineIfEmpty(line, replacement);
			}
		}
		return text;
	}

	/// <summary>Gets the text lines and replaces those that are empty.</summary>
	/// <param name="replacement">The text to replace empty lines with.</param>
	/// <remarks>The text lines are merged by the newline (\n) char. A subtitle line is considered empty
	/// if it has no characters.</remarks>
	/// <returns>The subtitle text, after replacement.</returns>
	public string GetReplaceEmptyLines (string replacement) {
		return GetReplaceEmptyLines(replacement, "\n");
	}

	/// <summary>Gets and trims the text lines merged with the specified line break.</summary>
	/// <param name="lineBreak">The line break used to merge the text.</param>
	/// <remarks>A subtitle line is considered blank if it has only white spaces.</remarks>
	/// <returns>The subtitle text, after trimming.</returns>
	public string GetTrimLines (string lineBreak) {
		string text = String.Empty;
		IEnumerator textLines = lines.GetEnumerator();
		if (textLines.MoveNext()){
			string line = (textLines.Current as string);
			line = line.Trim();
			if (line != String.Empty)
				text += line;

			while (textLines.MoveNext()) {
				line = (textLines.Current as string);
				line = line.Trim();
				if (line != String.Empty)
					text += lineBreak + line;
			}
		}
		return text;
	}

	/// <summary>Gets and trims the text lines.</summary>
	/// <remarks>The text lines are merged by the newline (\n) char. A subtitle line is
	/// considered blank ifit has only white spaces.</remarks>
	/// <returns>The subtitle text, after trimming.</returns>
	public string GetTrimLines () {
		return GetTrimLines("\n");
	}

	public string[] GetLines () {
		return (string[])lines.ToArray(typeof(String));
	}

	/// <summary>Sets the subtitle text using the specified line break and trimming option.</summary>
	/// <param name="text">The subtitle text.</param>
	/// <param name="lineBreak">The text substring used to split the text in lines.</param>
	/// <param name="toTrimLines">Whether to trim every text line.</param>
	public void Set (string text, string lineBreak, bool toTrimLines) {
		if (toTrimLines)
			text = text.Trim();

		string escapedLineBreak = Regex.Escape(lineBreak);
		string spaceDelimiter = (toTrimLines ? @"\s*" : String.Empty);
		string regexPattern = spaceDelimiter + @escapedLineBreak + spaceDelimiter;
		string[] textLines = Regex.Split(text, regexPattern);
		Clear();
		foreach (string textLine in textLines)
			lines.Add(textLine);
	}

	/// <summary>Sets the subtitle text.</summary>
	/// <remarks>Newline (\n) is used as the line break. The text lines are not trimmed.</remarks>
	/// <param name="text">The subtitle text.</param>
	public void Set (string text) {
		Set(text, "\n", false);
	}

	public void Set (string[] newLines) {
		Clear();
		Add(newLines);
	}

	public void Add (string[] newLines) {
		foreach (string newLine in newLines) {
			string trimmedLine = newLine.Trim();
			if (trimmedLine != String.Empty)
				lines.Add(trimmedLine);
		}
	}

	public override string ToString() {
		string result = String.Empty;
	  	int lineNumber = 1;
  		foreach (string line in lines){
  			result += "\t" + lineNumber + ". " + line + "\n";
  			lineNumber++;
  		}
  		return result;
	}

	public object Clone() {
		SubtitleText clone = new SubtitleText();
		foreach (string line in lines) {
			clone.lines.Add(line);
		}
		return clone;
	}


	/* Private Methods */

	private string ReplaceLineIfEmpty (string textLine, string replacement) {
		if (textLine == String.Empty)
			return replacement;
		else
			return textLine;
	}

}

}
