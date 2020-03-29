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

using SubLib.Core.Domain;
using SubLib.Core.Timing;
using SubLib.IO.SubtitleFormats;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SubLib.IO.Input {

internal class SubtitleParser {
	private bool includeIncompleteSubtitles = false;

	/* Delegate to use when parsing headers */
	private delegate bool ParseHeaderDelegate (Match match, ParsingProperties properties);

	internal SubtitleParser(bool includeIncompleteSubtitles) {
		this.includeIncompleteSubtitles = includeIncompleteSubtitles;
	}

	private string ClearComments (string text, SubtitleFormat format) {
		if (format.HasComments) {
			Regex regex = new Regex(format.Comments);
			string clearText = regex.Replace(text, String.Empty);
			return clearText;
		}
		else
			return text;
	}

	/// <summary>Parses the specified text, using the specified format.</summary>
	/// <remarks>The created <see cref="SubtitleCollection" /> will have its <see cref="SubtitleProperties" /> property set to null.
	/// It is mandatory to use <see cref="SubtitleCollection.SetPropertiesForAll" /> after.</remarks>
	internal ParsingProperties Parse (string text, SubtitleFormat format, float inputFrameRate,
			out SubtitleCollection collection, out IncompleteSubtitleCollection incompleteSubtitles){

		collection = new SubtitleCollection();
		incompleteSubtitles = new IncompleteSubtitleCollection();
		ParsingProperties properties = new ParsingProperties();
		properties.InputFrameRate = inputFrameRate;

		Regex subtitleRegex = null;
		int bodyIndex = 0;

		text = ClearComments(text, format);

		/* Read the headers if available */
		if (format.Mode == SubtitleMode.Both) {
			//Read headers to know if format is using Times or Frames
			bodyIndex = text.Length;
			int lastIndex = ReadHeaders(text, bodyIndex, format, properties);
			subtitleRegex = CreateSubtitleRegex(format, properties.TimingMode);

			/* Detect body index from matching the first subtitle or the end of headers */
			bodyIndex = FindBodyIndex(text, format, subtitleRegex);
			if (lastIndex > bodyIndex)
				bodyIndex = lastIndex;
		}
		else {
			//End of headers is detected by start of subtitles' body
			properties.TimingMode = format.ModeAsTimingMode;
			subtitleRegex = CreateSubtitleRegex(format);
			bodyIndex = FindBodyIndex(text, format, subtitleRegex);
			ReadHeaders(text, bodyIndex, format, properties);
		}

		/* Get properties from the whole input, if available */
		format.GlobalInputGetProperties(text, properties);

		int textLength = text.Length;

		/* Read the subtitles */
		bodyIndex = ReadSubtitles(text, bodyIndex, textLength, subtitleRegex, format,
			properties, collection, incompleteSubtitles);

    	/* Read the end text of the subtitles */
    	bodyIndex = ReadBodyEnd(text, bodyIndex, format, collection, incompleteSubtitles);

		/* Check if there's still text remaining */
    	if ((bodyIndex < textLength) && includeIncompleteSubtitles)
    		AddIncompleteSubtitle(incompleteSubtitles, text.Substring(bodyIndex), collection.Count);

    	return properties;
	}


	/* Private members */

	private ParseHeaderDelegate GetHeaderParser (SubtitleType subtitleType) {
		switch (subtitleType) {
			case SubtitleType.SubViewer1:
				return new ParseHeaderDelegate(ParseHeaderSubViewer1);
			case SubtitleType.SubViewer2:
				return new ParseHeaderDelegate(ParseHeaderSubViewer2);
			case SubtitleType.KaraokeLyricsLRC:
				return new ParseHeaderDelegate(ParseHeaderKaraokeLyricsLRC);
			case SubtitleType.KaraokeLyricsVKT:
				return new ParseHeaderDelegate(ParseHeaderKaraokeLyricsVKT);
			case SubtitleType.MPSub:
				return new ParseHeaderDelegate(ParseHeaderMPSub);
			case SubtitleType.SubStationAlpha:
				return new ParseHeaderDelegate(ParseHeaderSubStationAlphaAAS);
			case SubtitleType.AdvancedSubStationAlpha:
				return new ParseHeaderDelegate(ParseHeaderSubStationAlphaAAS);
			default:
				return null;
		}
	}

	/// <returns>The index where subtitles start.</returns>
	private int ReadHeaders (string text, int bodyIndex, SubtitleFormat format, ParsingProperties properties) {
		if (!(format.HasHeaders && (bodyIndex > 0)))
			return 0;

		ParseHeaderDelegate headerParser = GetHeaderParser(format.Type);

		int lastIndex = 0; //the last index with header text
		string headerText = text.Substring(0, bodyIndex);
		foreach (string headerExpression in format.Headers) {
			Regex expression = new Regex(headerExpression, RegexOptions.IgnoreCase);
			Match match = expression.Match(headerText, 0, bodyIndex);
			if (match.Success) {
				/* Update the last index based on the header match */
				int matchLastIndex = match.Index + match.Length;
				if (matchLastIndex > lastIndex)
					lastIndex = matchLastIndex;

				headerParser(match, properties);
			}
		}
		return lastIndex;
	}

	private bool ParseHeaderSubViewer1 (Match match, ParsingProperties properties) {
		return ParseHeaderSubViewer1(match, properties, properties.Headers);
	}

	private bool ParseHeaderSubViewer1 (Match match, ParsingProperties properties, Headers headers) {
		string result = String.Empty;
		int intResult = 0;

		if (ParseGroup(match, "Title", ref result))
			headers.Title = result;
		else if (ParseGroup(match, "Author", ref result))
			headers.Author = result;
		else if (ParseGroup(match, "Source", ref result))
			headers.Source = result;
		else if (ParseGroup(match, "Program", ref result))
			headers.Program = result;
		else if (ParseGroup(match, "FilePath", ref result))
			headers.FilePath = result;
		else if (ParseGroup(match, "Delay", ref intResult))
			headers.Delay = intResult;
		else if (ParseGroup(match, "CdTrack", ref intResult))
			headers.CDTrack = intResult;
		else {
			return false;
		}
		return true;
	}

	private bool ParseHeaderSubViewer2 (Match match, ParsingProperties properties) {
		Headers headers = properties.Headers;
		string result = String.Empty;
		int intResult = 0;

		if (!ParseHeaderSubViewer1(match, properties, headers)) {
			if (ParseGroup(match, "Comment", ref result))
				headers.Comment = result;
			else if (ParseGroup(match, "FontName", ref result))
				headers.SubViewer2FontName = result;
			else if (ParseGroup(match, "FontColor", ref result))
				headers.SubViewer2FontColor = result;
			else if (ParseGroup(match, "FontStyle", ref result))
				headers.SubViewer2FontStyle = result;
			else if (ParseGroup(match, "FontSize", ref intResult))
				headers.SubViewer2FontSize = intResult;
			else
				return false;
		}
		return true;
	}

	private bool ParseHeaderKaraokeLyricsVKT (Match match, ParsingProperties properties) {
		Headers headers = properties.Headers;
		string result = String.Empty;

		if (ParseGroup(match, "FrameRate", ref result))
			headers.FrameRate = result;
		else if (ParseGroup(match, "Author", ref result))
			headers.Author = result;
		else if (ParseGroup(match, "Source", ref result))
			headers.Source = result;
		else if (ParseGroup(match, "Date", ref result))
			headers.Date = result;
		else {
			return false;
		}
		return true;
	}

	private bool ParseHeaderKaraokeLyricsLRC (Match match, ParsingProperties properties) {
		Headers headers = properties.Headers;
		string result = String.Empty;

		if (ParseGroup(match, "Title", ref result))
			headers.Title = result;
		else if (ParseGroup(match, "Author", ref result))
			headers.Author = result;
		else if (ParseGroup(match, "Artist", ref result))
			headers.Artist = result;
		else if (ParseGroup(match, "Album", ref result))
			headers.Album = result;
		else if (ParseGroup(match, "Maker", ref result))
			headers.FileCreator = result;
		else if (ParseGroup(match, "Version", ref result))
			headers.Version = result;
		else if (ParseGroup(match, "Program", ref result))
			headers.Program = result;
		else {
			return false;
		}

		return true;
	}

	private bool ParseHeaderMPSub (Match match, ParsingProperties properties) {
		Headers headers = properties.Headers;
		string result = String.Empty;
		float floatResult = 0;

		if (ParseGroup(match, "Title", ref result))
			headers.Title = result;
		else if (ParseGroup(match, "File", ref result))
			headers.MPSubFileProperties = result;
		else if (ParseGroup(match, "Author", ref result))
			headers.Author = result;
		else if (ParseGroup(match, "MediaType", ref result))
			headers.MPSubMediaType = result;
		else if (ParseGroup(match, "Note", ref result))
			headers.Comment = result;
		//Used to detect if a subtitles' timing mode is Times in the case of a format that supports both
		else if (ParseGroup(match, "TimingModeTimes", ref result))
			properties.TimingMode = TimingMode.Times;
		//Used to detect if a subtitles' timing mode is Frames in the case of a format that supports both
		else if (ParseGroup(match, "TimingModeFrames", ref floatResult)) {
			properties.TimingMode = TimingMode.Frames;
			properties.InputFrameRate = floatResult;
		}
		else {
			return false;
		}
		return true;
	}

	private bool ParseHeaderSubStationAlphaAAS (Match match, ParsingProperties properties) {
		Headers headers = properties.Headers;
		string result = String.Empty;
		int intResult = 0;

		if (ParseGroup(match, "Title", ref result))
			headers.Title = result;
		else if (ParseGroup(match, "OriginalScript", ref result))
			headers.SubStationAlphaOriginalScript = result;
		else if (ParseGroup(match, "OriginalTranslation", ref result))
			headers.SubStationAlphaOriginalTranslation = result;
		else if (ParseGroup(match, "OriginalEditing", ref result))
			headers.SubStationAlphaOriginalEditing = result;
		else if (ParseGroup(match, "OriginalTiming", ref result))
			headers.SubStationAlphaOriginalTiming = result;
		else if (ParseGroup(match, "OriginalScriptChecking", ref result))
			headers.SubStationAlphaOriginalScriptChecking = result;
		else if (ParseGroup(match, "ScriptUpdatedBy", ref result))
			headers.SubStationAlphaScriptUpdatedBy = result;
		else if (ParseGroup(match, "Collisions", ref result))
			headers.SubStationAlphaCollisions = result;
		else if (ParseGroup(match, "PlayResX", ref intResult))
			headers.SubStationAlphaPlayResX = intResult;
		else if (ParseGroup(match, "PlayResY", ref intResult))
			headers.SubStationAlphaPlayResY = intResult;
		else if (ParseGroup(match, "PlayDepth", ref intResult))
			headers.SubStationAlphaPlayDepth = intResult;
		else if (ParseGroup(match, "Timer", ref result))
			headers.SubStationAlphaTimer = result;
		else {
			return false;
		}
		return true;
	}

	private int ReadSubtitles (string text, int bodyIndex, int textLength, Regex subtitleRegex, SubtitleFormat format,
		ParsingProperties properties, SubtitleCollection collection, IncompleteSubtitleCollection incompleteSubtitles) {

		Subtitle previousSubtitle = null;

		/* Read the subtitles. BodyIndex points to the start of the subtitles, skipping its possible beginning text*/
		while (bodyIndex < textLength) {
			Match match = subtitleRegex.Match(text, bodyIndex);
			if (match.Success) {
    			Subtitle subtitle = ParseSubtitle(match, format, properties, previousSubtitle);
    			collection.Add(subtitle);
				AddIncompleteSubtitleIfExists(text, match, bodyIndex, collection.Count, incompleteSubtitles);
	    		bodyIndex = match.Index + match.Length;
				previousSubtitle = subtitle;
   			}
   			else
    			break;
   		}
   		return bodyIndex;
   	}

	private Subtitle ParseSubtitle (Match match, SubtitleFormat format, ParsingProperties properties, Subtitle previousSubtitle){
		SubtitleText text = ParseSubtitleText(match, format);
		Style style = ParseStyle(match, format);

		Subtitle subtitle = new Subtitle(null, text, style);

		if (properties.TimingMode == TimingMode.Frames) {
			Frames previousFrames = (previousSubtitle == null ? null : previousSubtitle.Frames);
			ParseFrames(match, subtitle.Frames, previousFrames);
		}
		else {
			Times previousTimes = (previousSubtitle == null ? null : previousSubtitle.Times);
			ParseTimes(match, subtitle.Times, previousTimes, properties);
		}

		format.SubtitleInputPostProcess(subtitle);
		return subtitle;
	}

	private void ParseTimes (Match match, Times times, Times previousTimes, ParsingProperties properties) {
		ParseStartTime(match, times, previousTimes, properties);
		ParseEndTime(match, times, previousTimes, properties);
	}

	private void ParseStartTime (Match match, Times times, Times previousTimes, ParsingProperties properties) {
		bool isTimeDefined = false;
		TimeSpan startTime = new TimeSpan(0);

		int result = 0;
		float floatResult = 0;
		if (ParseGroup(match, "StartHours", ref result)) {
			startTime += TimeSpan.FromHours(result);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "StartMinutes", ref result)) {
			startTime += TimeSpan.FromMinutes(result);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "StartSeconds", ref result)) {
			startTime += TimeSpan.FromSeconds(result);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "StartDeciseconds", ref result)) {
			startTime += TimeSpan.FromMilliseconds(result * 100);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "StartCentiseconds", ref result)) {
			startTime += TimeSpan.FromMilliseconds(result * 10);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "StartMilliseconds", ref result)) {
			startTime += TimeSpan.FromMilliseconds(result);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "StartMillisecondsAsFrames", ref result)) {
			startTime += TimingUtil.FramesToTime(result, properties.InputFrameRate);
			isTimeDefined = true;
		}

		if (ParseGroup(match, "StartElapsedTime", ref floatResult)) {
			if (previousTimes != null)
				startTime += previousTimes.PreciseEnd;

			startTime += TimeSpan.FromSeconds(floatResult);
			isTimeDefined = true;
		}
		if (isTimeDefined)
			times.PreciseStart = startTime;
	}

	private void ParseEndTime (Match match, Times times, Times previousTimes, ParsingProperties properties) {
		bool isTimeDefined = false;
		TimeSpan endTime = new TimeSpan(0);

		int result = 0;
		float floatResult = 0;
		if (ParseGroup(match, "EndHours", ref result)) {
			endTime += TimeSpan.FromHours(result);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "EndMinutes", ref result)) {
			endTime += TimeSpan.FromMinutes(result);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "EndSeconds", ref result)) {
			endTime += TimeSpan.FromSeconds(result);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "EndDeciseconds", ref result)) {
			endTime += TimeSpan.FromMilliseconds(result * 100);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "EndCentiseconds", ref result)) {
			endTime += TimeSpan.FromMilliseconds(result * 10);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "EndMilliseconds", ref result)) {
			endTime += TimeSpan.FromMilliseconds(result);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "EndMillisecondsAsFrames", ref result)) {
			endTime += TimingUtil.FramesToTime(result, properties.InputFrameRate);
			isTimeDefined = true;
		}
		if (ParseGroup(match, "EndElapsedTime", ref floatResult)) {
			endTime += times.PreciseStart + TimeSpan.FromSeconds(floatResult);
			isTimeDefined = true;
		}
		if (isTimeDefined)
			times.PreciseEnd = endTime;
	}

	private void ParseFrames (Match match, Frames frames, Frames previousFrames) {
		int result = 0;
		if (ParseGroup(match, "StartFrame", ref result))
			frames.PreciseStart = result;
		else if (ParseGroup(match, "StartElapsedFrames", ref result)) {
			double lastFrames = (previousFrames == null ? 0 : previousFrames.PreciseEnd);
			frames.PreciseStart = lastFrames + result;
		}

		if (ParseGroup(match, "EndFrame", ref result))
			frames.PreciseEnd = result;
		else if (ParseGroup(match, "EndElapsedFrames", ref result)) {
			frames.PreciseDuration = result;
		}
	}

	private SubtitleText ParseSubtitleText (Match match, SubtitleFormat subtitleFormat) {
		string text = String.Empty;
		if (ParseGroup(match, "Text", ref text))
			return new SubtitleText(text, subtitleFormat.LineBreak, true);
		else
			return new SubtitleText();
	}

	private Style ParseStyle (Match match, SubtitleFormat subtitleFormat) {
		string styleText = String.Empty;
		if (ParseGroup(match, "Style", ref styleText))
			return subtitleFormat.StringToStyle(styleText);
		else
			return new Style();
	}

	private bool ParseGroup (Match match, string groupName, ref string result) {
		Group group = match.Groups[groupName];
		if (group.Success) {
			result = group.Value.Trim();
			return true;
		}
		else
			return false;
	}

	private bool ParseGroup (Match match, string groupName, ref int result) {
		string textResult = String.Empty;
		bool returnValue = ParseGroup(match, groupName, ref textResult);
		if (returnValue) {
			try {
				result = Convert.ToInt32(textResult);
			} catch (Exception) {
				return false;
			}
		}
		return returnValue;
	}

	private bool ParseGroup (Match match, string groupName, ref float result) {
		string textResult = String.Empty;
		bool returnValue = ParseGroup(match, groupName, ref textResult);
		if (returnValue) {
			textResult = textResult.Replace(',', '.');

			try {
				result = (float)Convert.ToDouble(textResult, CultureInfo.InvariantCulture);
			} catch (Exception) {
				return false;
			}
		}
		return returnValue;
	}

    private int ReadBodyEnd (string text, int bodyIndex, SubtitleFormat format,
    		SubtitleCollection collection, IncompleteSubtitleCollection incompleteSubtitles) {

    	Regex bodyEnd = new Regex(format.BodyEndIn + @"\s*", RegexOptions.IgnoreCase);
    	Match bodyEndMatch = bodyEnd.Match(text, bodyIndex);
    	if (bodyEndMatch.Success) {
	   		AddIncompleteSubtitleIfExists(text, bodyEndMatch, bodyIndex, collection.Count, incompleteSubtitles);
    		bodyIndex = bodyEndMatch.Index + bodyEndMatch.Length;
    	}
    	return bodyIndex;
	}

	private bool IsThereIncompleteText (Match match, int bodyIndex) {
		return (match.Index > bodyIndex);
	}

	private void AddIncompleteSubtitle (IncompleteSubtitleCollection incompleteSubtitles, string incompleteText,
			int previousSubtitle) {

		if (!HasOnlyWhiteSpaces(incompleteText)) {
			IncompleteSubtitle incompleteSubtitle = new IncompleteSubtitle(previousSubtitle, incompleteText);
			incompleteSubtitles.Add(incompleteSubtitle);
		}
	}

	private bool HasOnlyWhiteSpaces (string text) {
		Regex emptyStringExpression = new Regex(@"\s*");
		Match emptyStringMatch = emptyStringExpression.Match(text);
		return (emptyStringMatch.Length == text.Length);
	}

	private int FindBodyIndex (string text, SubtitleFormat format, Regex subtitleRegex) {
		if (format.HasHeaders || format.HasBodyBegin) {
			Match subtitleMatch = subtitleRegex.Match(text);
			if (subtitleMatch.Success) {
				return subtitleMatch.Index;
			}
		}
		return 0;
	}

	private Regex CreateSubtitleRegex(SubtitleFormat format) {
		string subtitleInExpression = format.SubtitleIn + @"\s*"; //Ignore spaces between subtitles
		return new Regex(subtitleInExpression, RegexOptions.IgnoreCase);
	}

	// Used when a subtitle format supports both times and frames
	private Regex CreateSubtitleRegex(SubtitleFormat format, TimingMode timingMode) {
		string subtitleInExpression;
		if (timingMode == TimingMode.Times)
			subtitleInExpression = format.SubtitleInTimesMode + @"\s*";   //Ignore spaces between subtitles
		else
			subtitleInExpression = format.SubtitleInFramesMode + @"\s*";  //Ignore spaces between subtitles

		return new Regex(subtitleInExpression, RegexOptions.IgnoreCase);
	}

	private void AddIncompleteSubtitleIfExists (string text, Match match, int bodyIndex,
    		int subtitleCount, IncompleteSubtitleCollection incompleteSubtitles) {

    	if (includeIncompleteSubtitles && IsThereIncompleteText(match, bodyIndex)) {
    		int length = match.Index - bodyIndex;
    		string incompleteText = text.Substring(bodyIndex, length);
	    	AddIncompleteSubtitle(incompleteSubtitles, incompleteText, subtitleCount);
		}
    }

}

}
