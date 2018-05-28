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

using SubLib.Util;
using System;
using System.Collections;
using System.IO;
using System.Text;
using org.mozilla.intl.chardet;

namespace SubLib.IO {

internal class FileInputOutput {

	/// <summary>Opens a file for reading.</summary>
	internal static FileStream OpenFileForReading (string fileName) {
		return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
	}

	/// <summary>Detects the possible code pages by reading a specified stream.</summary>
	/// <remarks>The stream reader position is reset after reading.</remarks>
	/// <returns>The detected code pages, which will have zero length if none was detected.</returns>
	internal static int[] DetectCodePages (Stream stream){
		const int BUFFERSIZE = 1024;
		nsDetector detector = new nsDetector(nsPSMDetector.ALL);
		detector.Init(null);

  		byte[] buffer = new byte[BUFFERSIZE] ;
		int readLength = 0;
		bool finished = false;

		while (!finished) {
			readLength = stream.Read(buffer, 0, buffer.Length);
			if (readLength == 0)
				break;

			finished = detector.DoIt(buffer, readLength, false);
		}
		detector.Done();
		stream.Seek(0, SeekOrigin.Begin);

		string[] detectedEncodings = detector.getProbableCharsets();
		Logger.Info("[FileInputOutput] Detected encodings: {0}", String.Join(", ", detectedEncodings));

		/* Check if no encoding was detected */
		if (detectedEncodings[0] == "nomatch")
			return new int[0];

		return GetCodePages(detectedEncodings);
	}

	/// <summary>Reads a file, given its <see cref="FileStream" /> and <see cref="Encoding" />.</summary>
	/// <param name="file">The <see cref="FileStream" />.</param>
	/// <param name="encoding">The <see cref="Encoding" />.</param>
	/// <param name="reposition">Whether to reposition the stream position to the beginning after reading.</param>
	/// <remarks>The newlines are converted to unix type.</remarks>
	/// <returns> The read text.</returns>
	internal static string ReadFile (FileStream file, Encoding encoding, bool reposition) {
		StreamReader reader = new StreamReader(file, encoding, false);
		string text = reader.ReadToEnd();
		if (reposition)
			file.Seek(0, SeekOrigin.Begin);

		return ConvertNewLinesToUnix(text);
	}

	internal static void WriteFile (string fileName, string text){
		using (StreamWriter writer = OpenFileForWriting(fileName)) {
			writer.Write(text);
		}
	}

	internal static void WriteFile (string fileName, string text, Encoding encoding) {
		using (StreamWriter writer = OpenFileForWriting(fileName, encoding)) {
			writer.Write(text);
		}
	}


	/* Private members */

	private static StreamWriter OpenFileForWriting (string fileName) {
		FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
		StreamWriter fileWriter = new StreamWriter(file);
		return fileWriter;
	}

	private static StreamWriter OpenFileForWriting (string fileName, Encoding encoding) {
		FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
		StreamWriter fileWriter = new StreamWriter(file, encoding);
		return fileWriter;
	}

	private static int[] GetCodePages (string[] encodings) {
		ArrayList codePages = new ArrayList();
		foreach (string encoding in encodings) {
			int codePage = GetCodePage(encoding);
			if (codePage != -1)
				codePages.Add(codePage);
		}
		return (int[])codePages.ToArray(typeof(int));
	}

	// Note: ISO-2022-CN, HZ-GB-2312 and x-euc-tw are not defined as their code pages were not found
	private static int GetCodePage (string encoding) {
		switch (encoding) {
			case "UTF-8": return 65001;
			case "windows-1252": return 1252;
			case "UTF-16LE": return 1200;
			case "UTF-16BE": return 1201;
			case "Shift_JIS": return 932;
			case "Big5": return 950;
			case "ISO-2022-KR": return 50225;
			case "ISO-2022-JP": return 50221;
			case "GB2312": return 936;
			case "GB18030": return 54936;
			case "EUC-KR": return 51949;
			case "EUC-JP": return 51932;
			default: return -1;
		}
	}

	/// <summary>Replaces the occurrences of Windows and Mac newline chars with unix newline.</summary>
	private static string ConvertNewLinesToUnix (string text) {
		text = text.Replace("\r\n", "\n"); //Replace Windows newline
		text = text.Replace("\r", "\n"); //Replace Mac newline
		return text;
	}

}

}
