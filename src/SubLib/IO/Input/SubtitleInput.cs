/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2018 Pedro Castro
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
using SubLib.IO.SubtitleFormats;
using SubLib.Util;
using System;
using System.IO;
using System.Text;

namespace SubLib.IO.Input {

internal class SubtitleInput {
	private Encoding fallbackEncoding = null;
	private SubtitleType subtitleType = SubtitleType.Unknown;

	internal SubtitleInput (Encoding fallbackEncoding, SubtitleType subtitleType) {
		this.fallbackEncoding = fallbackEncoding;
		this.subtitleType = subtitleType;
	}

	/// <exception cref="EncodingNotSupportedException">Thrown if the encoding is not supported by the platform.</exception>
	/// <exception cref="UnknownSubtitleFormatException">Thrown if the subtitle format could not be detected.</exception>
	internal string Read (string path, out Encoding encoding, out SubtitleFormat format) {
		using (FileStream fileStream = FileInputOutput.OpenFileForReading(path)) {
			return ReadSubtitleText(true, fileStream, out encoding, out format);
		}
	}

	/// <exception cref="UnknownSubtitleFormatException">Thrown if the subtitle format could not be detected.</exception>
	internal string Read (string path, Encoding encoding, out SubtitleFormat format) {
		using (FileStream fileStream = FileInputOutput.OpenFileForReading(path)) {
			return TestEncoding(fileStream, encoding, out format);
		}
	}

	/// <exception cref="EncodingNotSupportedException">Thrown if the encoding is not supported by the platform.</exception>
	internal string ReadPlain (string path, out Encoding encoding) {
		using (FileStream fileStream = FileInputOutput.OpenFileForReading(path)) {
			SubtitleFormat format = null;
			return ReadSubtitleText(false, fileStream, out encoding, out format);
		}
	}

	/// <exception cref="EncodingNotSupportedException">Thrown if the encoding is not supported by the platform.</exception>
	internal string ReadPlain (string path, Encoding encoding) {
		using (FileStream fileStream = FileInputOutput.OpenFileForReading(path)) {
			return TestEncoding(fileStream, encoding);
		}
	}

	/* Private methods */

	/// <summary>Checks the encoding of a file.</summary>
	/// <param name="isSubtitleFile">If it is a subtitle file or a plain text one.</param>
	/// <param name="fileStream">The stream for reading the file.</param>
	/// <param name="usedEncoding">The encoding supposedly used.</param>
	/// <param name="usedFormat">The subtitle format used.</param>
	/// <exception cref="EncodingNotSupportedException">Thrown if the encoding is not supported by the platform.</exception>
	/// <exception cref="UnknownSubtitleFormatException">Thrown if the subtitle format could not be detected.</exception>
	private string ReadSubtitleText (bool isSubtitleFile, FileStream fileStream, out Encoding usedEncoding, out SubtitleFormat usedFormat) {
		/* Init the out arguments */
		usedEncoding = null;
		usedFormat = null;

		/* Detect code pages */
		int[] codePages = FileInputOutput.DetectCodePages(fileStream);

		/* Check if no codepage was detected */
		if (codePages.Length == 0) {
			Logger.Info("[SubtitleInput] No encoding was automatically detected. Using the fall-back encoding \"{0}\"", fallbackEncoding.WebName);
			string text;
			if (isSubtitleFile)
				text = TestEncoding(fileStream, fallbackEncoding, out usedFormat);
			else
				text = TestEncoding(fileStream, fallbackEncoding);
			usedEncoding = fallbackEncoding;
			return text;
		}

		/* The first code page represents the most probable encoding. If any problem occurs when trying to use
		 * that code page, this problem is registered. The remaining code pages are then tried, and if none works,
		 * the first occurring error is the one to be reported. */
		Exception firstEncodingException = null;
		Exception firstSubtitleFormatException = null;
		int firstCodePage = codePages[0];
		try {
			string text;
			if (isSubtitleFile)
				text = TestCodePage(fileStream, firstCodePage, out usedEncoding, out usedFormat);
			else
				text = TestCodePagePlain(fileStream, firstCodePage, out usedEncoding);
			return text;
		}
		catch (EncodingNotSupportedException e) {
			firstEncodingException = e;
		}
		catch (UnknownSubtitleFormatException e) {
			firstSubtitleFormatException = e;
		}

		/* Problems were found, going to try additional code pages */
		for (int count = 1 ; count < codePages.Length ; count++) {
			try {
				int codePage = codePages[count];
				string text;
				if (isSubtitleFile)
					text = TestCodePage(fileStream, codePage, out usedEncoding, out usedFormat);
				else
					text = TestCodePagePlain(fileStream, codePage, out usedEncoding);
				return text;
			}
			catch (Exception) {
				//Don't do anything, will try the next code page
			}
		}

		/* No code page worked, throwing the exceptions caught for the first (more probable) code page */
		if (firstEncodingException != null)
			throw firstEncodingException;
		else
			throw firstSubtitleFormatException;

	}

	/// <exception cref="EncodingNotSupportedException">Thrown if the encoding is not supported by the platform.</exception>
	/// <exception cref="UnknownSubtitleFormatException">Thrown if the subtitle format could not be detected.</exception>
	private string TestCodePage (FileStream fileStream, int codePage, out Encoding encoding, out SubtitleFormat format) {
		/* Check the encoding */
		TestCodePageCommon(codePage, out encoding);
		return TestEncoding(fileStream, encoding, out format);
	}

	/// <exception cref="EncodingNotSupportedException">Thrown if the encoding is not supported by the platform.</exception>
	private string TestCodePagePlain (FileStream fileStream, int codePage, out Encoding encoding) {
		/* Check the encoding */
		TestCodePageCommon(codePage, out encoding);
		return TestEncoding(fileStream, encoding);
	}

	private void TestCodePageCommon (int codePage, out Encoding encoding) {
		/* Check the encoding */
		try {
			encoding = Encoding.GetEncoding(codePage);
		}
		catch (Exception) {
			throw new EncodingNotSupportedException();
		}
	}

	/// <exception cref="UnknownSubtitleFormatException">Thrown if the subtitle format could not be detected.</exception>
	private string TestEncoding (FileStream fileStream, Encoding encoding, out SubtitleFormat format) {
		/* Get the text */
		string text = TestEncoding(fileStream, encoding);

		/* Check the subtitle format */
		format = GetSubtitleFormat(text);

		return text;
	}

	private string TestEncoding (FileStream fileStream, Encoding encoding) {
		Logger.Info("[SubtitleInput] Trying encoding \"{0}\"", encoding.WebName);
		/* Get the text */
		string text = FileInputOutput.ReadFile(fileStream, encoding, true);

		return text;
	}

	/// <exception cref="UnknownSubtitleFormatException">Thrown if the subtitle format could not be detected.</exception>
	private SubtitleFormat GetSubtitleFormat (string text) {
		if (subtitleType == SubtitleType.Unknown) {
			Logger.Info("[SubtitleInput] Trying to autodetect the subtitle format.");
		} else {
			Logger.Info("[SubtitleInput] Trying subtitle format \"{0}\"", subtitleType);
		}

		SubtitleFormat subtitleFormat = null;
		if (subtitleType == SubtitleType.Unknown)
			subtitleFormat = BuiltInSubtitleFormats.Detect(text);
		else
			subtitleFormat = BuiltInSubtitleFormats.GetFormat(subtitleType);

		return subtitleFormat;
	}

}

}