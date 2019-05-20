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
using System.Text;
using System.Text.RegularExpressions;

namespace SubLib.IO.Output {

internal class SubtitleOutput {
	private SubtitleFormat format = null;
	private SubtitleTextType textType = SubtitleTextType.Text;

	private SubtitleProperties subtitleProperties = null;
	private Subtitle subtitle = null;
	private Subtitle previousSubtitle = null;
	private int subtitleNumber = 1;

	internal SubtitleOutput (SubtitleFormat format, SubtitleTextType textType) {
		this.format = format;
		this.textType = textType;
	}

	internal string Build (SubtitleCollection collection, SubtitleProperties subtitleProperties, FileProperties fileProperties) {
		this.subtitleProperties = subtitleProperties;
		StringBuilder output = new StringBuilder();
		if (format.HasHeaders)
			output.Append(format.HeadersToString(subtitleProperties, fileProperties));

		if (format.HasBodyBegin)
			output.Append(format.BodyBeginOut);

		string subtitleExpression = GetSubtitleExpression(format, subtitleProperties, fileProperties);
		Regex fieldExpression = new Regex(@"<<(?<Field>\w+)(,(?<Width>\d+))?>>");
		MatchEvaluator matchEvaluator = new MatchEvaluator(this.FieldEvaluator);

		foreach (Subtitle currentSubtitle in collection) {
			subtitle = currentSubtitle;
			string outputSubtitle = fieldExpression.Replace(subtitleExpression, matchEvaluator);
			output.Append(outputSubtitle);
			output.Append("\n");
			subtitleNumber++;
			previousSubtitle = subtitle;
		}

		if (format.HasBodyEnd)
			output.Append(format.BodyEndOut);

		subtitle = null;
		previousSubtitle = null;
		subtitleNumber = 1;

		ConvertNewlines(output, fileProperties);
		return output.ToString();
	}


	/* Private members */

	private string FieldEvaluator (Match match) {
		Group fieldGroup = match.Groups["Field"];
		string field = fieldGroup.Value;

		switch (field) {
			case "StartFrame":
				int startFrame = subtitle.Frames.Start;
				return FormatedField(startFrame, match);
			case "StartElapsedFrames":
				int previousFrames = (previousSubtitle == null ? 0 : previousSubtitle.Frames.End);
				int startElapsedFrames = subtitle.Frames.Start - previousFrames;
				return FormatedField(startElapsedFrames, match);
			case "EndFrame":
				int endFrame = subtitle.Frames.End;
				return FormatedField(endFrame, match);
			case "EndElapsedFrames":
				int endElapsedFrames = subtitle.Frames.Duration;
				return FormatedField(endElapsedFrames, match);
			case "StartHours":
				int startHours = subtitle.Times.Start.Hours;
				return FormatedField(startHours, 2, match);
			case "StartMinutes":
				int startMinutes = subtitle.Times.Start.Minutes;
				return FormatedField(startMinutes, 2, match);
			case "StartSeconds":
				int startSeconds = subtitle.Times.Start.Seconds;
				return FormatedField(startSeconds, 2, match);
			case "StartDeciseconds":
				int startDeciseconds = DivideAndRound(subtitle.Times.Start.Milliseconds, 100);
				return FormatedField(startDeciseconds, 1, match);
			case "StartTotalDeciseconds":
				int startTotalDeciseconds = DivideAndRound((int)subtitle.Times.Start.TotalMilliseconds, 100);
				return startTotalDeciseconds.ToString();
			case "StartCentiseconds":
				int startCentiseconds = DivideAndRound(subtitle.Times.Start.Milliseconds, 10);
				return FormatedField(startCentiseconds, 2, match);
			case "StartMilliseconds":
				int startMilliseconds = subtitle.Times.Start.Milliseconds;
				return FormatedField(startMilliseconds, 3, match);
			case "StartMillisecondsAsFrames":
				int startMillisecondsAsFrames = (int)TimingUtil.TimeMillisecondsToFrames(subtitle.Times.Start.Milliseconds, this.subtitleProperties.CurrentFrameRate);
				return FormatedField(startMillisecondsAsFrames, 2, match);
			case "StartMillisecondsAsFramesPAL":
				int startMillisecondsAsFramesPAL = (int)TimingUtil.TimeMillisecondsToFrames(subtitle.Times.Start.Milliseconds, 25);
				return FormatedField(startMillisecondsAsFramesPAL, 2, match);
			case "StartMillisecondsAsFramesNTSC":
				int startMillisecondsAsFramesNTSC = (int)TimingUtil.TimeMillisecondsToFrames(subtitle.Times.Start.Milliseconds, 29.97F);
				return FormatedField(startMillisecondsAsFramesNTSC, 2, match);
			case "EndMillisecondsAsFrames":
				int endMillisecondsAsFrames = (int)TimingUtil.TimeMillisecondsToFrames(subtitle.Times.End.Milliseconds, this.subtitleProperties.CurrentFrameRate);
				return FormatedField(endMillisecondsAsFrames, 2, match);
			case "EndMillisecondsAsFramesPAL":
				int endMillisecondsAsFramesPAL = (int)TimingUtil.TimeMillisecondsToFrames(subtitle.Times.End.Milliseconds, 25);
				return FormatedField(endMillisecondsAsFramesPAL, 2, match);
			case "EndMillisecondsAsFramesNTSC":
				int endMillisecondsAsFramesNTSC = (int)TimingUtil.TimeMillisecondsToFrames(subtitle.Times.End.Milliseconds, 29.97F);
				return FormatedField(endMillisecondsAsFramesNTSC, 2, match);
			case "StartElapsedTime":
				TimeSpan previousTime = (previousSubtitle == null ? TimeSpan.Zero : previousSubtitle.Times.End);
				TimeSpan startElapsedTime = subtitle.Times.Start - previousTime;
				return FormatedField(startElapsedTime.TotalSeconds);
			case "EndHours":
				int endHours = subtitle.Times.End.Hours;
				return FormatedField(endHours, 2, match);
			case "EndMinutes":
				int endMinutes = subtitle.Times.End.Minutes;
				return FormatedField(endMinutes, 2, match);
			case "EndSeconds":
				int endSeconds = subtitle.Times.End.Seconds;
				return FormatedField(endSeconds, 2, match);
			case "EndDeciseconds":
				int endDeciseconds = DivideAndRound(subtitle.Times.End.Milliseconds, 100);
				return FormatedField(endDeciseconds, 1, match);
			case "EndTotalDeciseconds":
				int endTotalDeciseconds = DivideAndRound((int)subtitle.Times.End.TotalMilliseconds, 100);
				return endTotalDeciseconds.ToString();
			case "EndCentiseconds":
				int endCentiseconds = DivideAndRound(subtitle.Times.End.Milliseconds, 10);
				return FormatedField(endCentiseconds, 2, match);
			case "EndMilliseconds":
				int endMilliseconds = subtitle.Times.End.Milliseconds;
				return FormatedField(endMilliseconds, 3, match);
			case "EndElapsedTime":
				TimeSpan endElapsedTime = subtitle.Times.Duration;
				return FormatedField(endElapsedTime.TotalSeconds);
			case "Text":
				SubtitleText subtitleText = (textType == SubtitleTextType.Text ? subtitle.Text : subtitle.Translation);
				string text = subtitleText.GetTrimLines(format.LineBreak);
				return text.ToString();
			case "Style":
				string style = format.StyleToString(subtitle.Style);
				return style.ToString();
			case "EndOfStyle":
				string endOfStyle = format.EndOfStyleToString(subtitle.Style);
				return endOfStyle.ToString();
			case "SubtitleNumber":
				return FormatedField(subtitleNumber, match);
			default:
				return match.Value;
		}
	}

	private string FormatedField (int field, int defaultWidth, Match match) {
		Group group = match.Groups["Width"];
		int width = (group.Success ? Convert.ToInt32(group.Value) : defaultWidth);
		return DimensionField(field, width);
	}

	private string FormatedField (int field, Match match) {
		Group group = match.Groups["Width"];
		if (group.Success) {
			int width = Convert.ToInt32(group.Value);
			return DimensionField(field, width);
		}
		else
			return field.ToString();
	}

	private string FormatedField (double field) {
		return field.ToString("0.###", CultureInfo.InvariantCulture);
	}

	//TODO fix precision when saving files. If our number width is greater than 'width', some of the last digits will be removed
	private string DimensionField (int field, int width) {
		return field.ToString("D" + width).Substring(0, width);
	}

	private int DivideAndRound (int number, int denominator) {
		return (int)Math.Round((double)number / denominator);
	}

	private void ConvertNewlines (StringBuilder builder, FileProperties properties) {
		NewlineType type = properties.NewlineType;
		if ((type == NewlineType.Unknown) || (type == NewlineType.Unix))
			return;

		string newline = (type == NewlineType.Windows ? "\r\n" : "\r"); //Windows : Macintosh
		builder.Replace("\n", newline);
	}

	private string GetSubtitleExpression (SubtitleFormat format, SubtitleProperties subtitleProperties, FileProperties fileProperties) {
		if (format.Mode == SubtitleMode.Both) {
			if (fileProperties.TimingMode == TimingMode.Times)
				return format.SubtitleOutTimesMode;
			else
				return format.SubtitleOutFramesMode;
		}
		else {
			if (format.SubtitleOut != null)
				return format.SubtitleOut;
			else
				return format.GetDynamicSubtitleOut(subtitleProperties);
		}
	}

}

}
