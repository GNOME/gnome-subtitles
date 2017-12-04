/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2017 Pedro Castro
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
using SubLib.IO.Input;
using SubLib.Util;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SubLib.Core {

/// <summary>Represents the main mechanism for creating new <see cref="Subtitles" />.</summary>
/// <remarks>A <see cref="SubtitleFactory" /> is no longer needed after the subtitles have been created.</remarks>
public class SubtitleFactory {
	private IncompleteSubtitleCollection incompleteSubtitles = null;
	private FileProperties fileProperties = null;

	private bool includeIncompleteSubtitles = false;

	private Encoding encoding = null; //The encoding to be used to open a file
	private Encoding fallbackEncoding = Encoding.GetEncoding(1252); //The encoding to fall back to when no encoding is detected
	private float inputFrameRate = 25; //The frame rate to be used to open a frame-based file
	private int maxFileSize = 1000000; //The size limit for a subtitle file, in bytes

	private SubtitleType subtitleType = SubtitleType.Unknown;


	/* Public properties */

	/// <summary>The incomplete subtitles that were found when opening a file.</summary>
	/// <remarks>This is only used when <see cref="IncludeIncompleteSubtitles" /> is set.</remarks>
	public IncompleteSubtitleCollection IncompleteSubtitles {
		 get { return incompleteSubtitles; }
	}

	/// <summary>The properties of an opened file, after opening.</summary>
	public FileProperties FileProperties {
		 get { return fileProperties; }
	}

	/// <summary>Whether to enable the library to print messages to the console.</summary>
	/// <remarks>Messages will be shown along with the main methods of <see cref="SubtitleFactory" />
	/// and <see cref="Subtitles" />. The default value is true.</remarks>
	public bool LoggingEnabled {
		get { return Logger.Enabled; }
		set { Logger.Enabled = value; }
	}

	/// <summary>Whether to detect and store incomplete subtitles found upon open.</summary>
	/// <remarks>The default value is false.</remarks>
	public bool IncludeIncompleteSubtitles {
		get { return includeIncompleteSubtitles; }
		set { includeIncompleteSubtitles = value; }
	}

	/// <summary>The encoding to be used upon open.</summary>
	/// <remarks>When set to null, encoding auto-detection is used. The default value is null (use auto-detection).</remarks>
	public Encoding Encoding {
		get { return encoding; }
		set { encoding = value; }
	}

	/// <summary>The encoding to fallback to when using encoding auto-detection.</summary>
	/// <remarks>When using encoding auto-detection, this encoding will be used if no encoding could be auto-detected.
	/// Defaults to Windows-1252.</remarks>
	public Encoding FallbackEncoding {
		get { return fallbackEncoding; }
		set { fallbackEncoding = value; }
	}

	/// <summary>The type of the subtitle being opened.</summary>
	/// <remarks>When set to <see cref="SubtitleType.Unknown" />, subtitle type auto-detection is used.
	/// The default value is <see cref="SubtitleType.Unknown" /> (auto-detection).</remarks>
	public SubtitleType SubtitleType {
		get { return subtitleType; }
		set { subtitleType = value; }
	}

	/// <summary>The frame rate of the subtitle being opened, for frame-based files.</summary>
	public float InputFrameRate {
		get { return inputFrameRate; }
		set { inputFrameRate = value; }
	}

	/// <summary>The file size limit for subtitle files, in bytes.</summary>
	/// <remarks>Defaults to 1,000,000 bytes (1MB). Setting the value to -1 disables validation.</remarks>
	public int MaxFileSize {
		get { return maxFileSize; }
		set { maxFileSize = value; }
	}


	/* Public methods */

	/// <summary>Creates new empty <see cref="Subtitles" />.</summary>
	/// <returns>The newly created subtitles.</returns>
	public Subtitles New () {
		SubtitleCollection collection = new SubtitleCollection();
		SubtitleProperties properties = new SubtitleProperties();
		return new Subtitles(collection, properties);
	}

	/// <summary>Creates <see cref="Subtitles" /> by opening the file at the specified path.</summary>
	/// <remarks>The properties of the opened file are accessible with <see cref="FileProperties" />, after opening.</remarks>
	/// <returns>The opened subtitles.</returns>
	/// <exception cref="EncodingNotSupportedException">Thrown if a detected encoding is not supported by the platform.</exception>
	/// <exception cref="UnknownSubtitleFormatException">Thrown if a subtitle format could not be detected.</exception>
	public Subtitles Open (string path){
		SubtitleFormat format = null;
		string text = String.Empty;
		Encoding fileEncoding = null;

		if (maxFileSize != -1) {
			FileInfo info = new FileInfo(path);
			if (info.Length > maxFileSize) {
				throw new FileTooLargeException(String.Format("The file size ({0:n} bytes) is larger than the maximum limit ({1:n} bytes).", info.Length, maxFileSize));
			}
		}

		SubtitleInput input = new SubtitleInput(fallbackEncoding, subtitleType);
		if (encoding == null) {
			text = input.Read(path, out fileEncoding, out format);
		}
		else {
			text = input.Read(path, encoding, out format);
			fileEncoding = encoding;
		}

		if (IsTextEmpty(text))
			return EmptySubtitles(path);
		else
			return ParsedSubtitles(path, fileEncoding, format, inputFrameRate, text);
	}

	/// <summary>Creates <see cref="Subtitles" /> by opening the plain text file at the specified path.</summary>
	/// <remarks>The properties of the opened file are accessible with <see cref="FileProperties" />, after opening.</remarks>
	/// <returns>The opened lines turned into subtitles.</returns>
	/// <exception cref="EncodingNotSupportedException">Thrown if a detected encoding is not supported by the platform.</exception>
	/// <exception cref="UnknownSubtitleFormatException">Thrown if a subtitle format could not be detected.</exception>
	public Subtitles OpenPlain (string path, bool withCharacterNames, TimingMode timingMode, string lineSeparator) {
		string text = String.Empty;
		Encoding fileEncoding = null;

		SubtitleInput input = new SubtitleInput(fallbackEncoding, subtitleType);
		if (encoding == null) {
			text = input.ReadPlain(path, out fileEncoding);
		}
		else {
			text = input.ReadPlain(path, encoding);
			fileEncoding = encoding;
		}
		if (IsTextEmpty(text))
			return EmptySubtitles(path);
		else
			return ParsedSubtitlesPlain(path, fileEncoding, text, withCharacterNames,
		                            timingMode, lineSeparator);
	}

	/* Private members */

	private Subtitles ParsedSubtitles (string path, Encoding fileEncoding, SubtitleFormat format, float inputFrameRate, string text) {
		SubtitleCollection collection = null;
		SubtitleParser subtitleParser = new SubtitleParser(includeIncompleteSubtitles);
		ParsingProperties parsingProperties = subtitleParser.Parse(text, format, inputFrameRate, out collection, out incompleteSubtitles);

		SubtitleProperties subtitleProperties = new SubtitleProperties(parsingProperties);
		collection.SetPropertiesForAll(subtitleProperties);

		Subtitles subtitles = new Subtitles(collection, subtitleProperties);
		CompleteTimingsAfterParsing(subtitles, parsingProperties);

		fileProperties = new FileProperties(path, fileEncoding, format.Type, parsingProperties.TimingMode);

		Logger.Info("[SubtitleFactory] Opened \"{0}\" with encoding \"{1}\", format \"{2}\", timing mode \"{3}\" and frame rate \"{4}\" (input frame rate was \"{5}\")",
			path, fileEncoding, format.Name, parsingProperties.TimingMode, subtitleProperties.CurrentFrameRate, inputFrameRate);

		return subtitles;
	}

	private Subtitles ParsedSubtitlesPlain (string path, Encoding fileEncoding, string text, bool withCharacterNames, TimingMode timingMode, string lineSeparator) {
		SubtitleCollection collection = null;
		PlainTextParser plainParser = new PlainTextParser(withCharacterNames, lineSeparator);
		ParsingProperties parsingProperties = plainParser.Parse(text, timingMode, fileEncoding, out collection);

		SubtitleProperties subtitleProperties = new SubtitleProperties(parsingProperties);
		collection.SetPropertiesForAll(subtitleProperties);

		Subtitles subtitles = new Subtitles(collection, subtitleProperties);
		CompleteTimingsAfterParsing(subtitles, parsingProperties);

		fileProperties = new FileProperties(path, fileEncoding, parsingProperties.TimingMode);

		Logger.Info("[SubtitleFactory] Opened {0} with encoding {1}", path, fileEncoding);
		return subtitles;
	}

	private Subtitles EmptySubtitles (string path) {
		Subtitles subtitles = New();
		fileProperties = new FileProperties(path, Encoding.UTF8, SubtitleType.Unknown, TimingMode.Times);
		return subtitles;
	}

	private bool IsTextEmpty (string text) {
		Regex regex = new Regex(@"\s*");
		Match match = regex.Match(text);
		return (match.Length == text.Length);
	}

	private void CompleteTimingsAfterParsing(Subtitles subtitles, ParsingProperties parsingProperties){
		float originalFrameRate = subtitles.Properties.OriginalFrameRate;
		subtitles.Properties.SetCurrentFrameRate(originalFrameRate);

		if (parsingProperties.TimingMode == TimingMode.Times)
			subtitles.UpdateFramesFromTimes(originalFrameRate);
		else
			subtitles.UpdateTimesFromFrames(originalFrameRate);
	}

}

}

