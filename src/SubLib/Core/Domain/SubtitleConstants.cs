/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2011 Pedro Castro
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

namespace SubLib.Core.Domain {

/// <summary>Contains constants for a set of subtitling parameters.</summary>
public class SubtitleConstants {

	/// <summary>The distance of the subtitles to the sides of the screen, in percent of screen dimensions.</summary>
	/// <remarks>The value of this property is 1/12.</remarks>
	public const float DistanceFromBottomFactor = 1/12;

	/// <summary>The maximum number of lines that should be allowed.</summary>
	/// <remarks>The value of this property is 2.</remarks>
	public const int MaxLineCount = 2;

	/// <summary>The maximum number of characters per line.</summary>
	/// <remarks>The value of this property is 35.</remarks>
	public const int MaxCharactersPerLine = 35;

	/// <summary>The maximum number of words per line.</summary>
	/// <remarks>The value of this property is 7.</remarks>
	public const int MaxWordsPerLine = 7;

	/// <summary>The lower bound on the average reading speed, in words per minute.</summary>
	/// <remarks>The value of this property is 150.</remarks>
	public const int MinAverageReadingWordsPerMinute = 150;

	/// <summary>The upper bound on the average reading speed, in words per minute.</summary>
	/// <remarks>The value of this property is 180.</remarks>
	public const int MaxAverageReadingWordsPerMinute = 180;

	/// <summary>The average reading speed, in words per minute.</summary>
	/// <remarks>The value of this property is 150.</remarks>
	public const int AverageReadingWordsPerMinute = 165;

	/// <summary>The lower bound on the average reading speed, in words per second.</summary>
/// <remarks>The value of this property is 2.5.</remarks>
	public const float MinAverageReadingWordsPerSecond = 2.5f;

	/// <summary>The upper bound on the average reading speed, in words per second.</summary>
/// <remarks>The value of this property is 3.</remarks>
	public const float MaxAverageReadingWordsPerSecond = 3;

	/// <summary>The average reading speed, in words per second.</summary>
	/// <remarks>The value of this property is 2.75.</remarks>
	public const float AverageReadingWordsPerSecond = 2.75f;

	/// <summary>The maximum duration of a full single-line subtitle, in seconds.</summary>
	/// <remarks>The value of this property is 3.5.</remarks>
	public const float MaxSingleLineSubtitleDuration = 3.5f;

	/// <summary>The average duration of a subtitle, in seconds.</summary>
	/// <remarks>The value of this property is 1.5.</remarks>
	public const float AverageSubtitleDuration = 1.5f;

	/// <summary>The maximum duration of a full two-line subtitle, in seconds.</summary>
	/// <remarks>The value of this property is 6.</remarks>
	public const float MaxTwoLineSubtitleDuration = 6;

	/// <summary>The duration of a single-word subtitle, in seconds.</summary>
	/// <remarks>The value of this property is 1.5.</remarks>
	public const float SingleWordSubtitleDuration = 1.5f;

	/// <summary>The amount of time the subtitle should appear after the initiation of the utterance, in seconds.</summary>
	/// <remarks>The value of this property is 0.25.</remarks>
	public const float LeadingInTime = 0.25f;

	/// <summary>The maximum amount of time the subtitle should be left on the screen after the end of the utterance, in seconds.</summary>
	/// <remarks>The value of this property is 2.</remarks>
	public const float LaggingOutTime = 2;

	/// <summary>The minimum amount of time between two consecutive subtitles, in seconds.</summary>
	/// <remarks>The value of this property is 0.25.</remarks>
	public const float MinTimeBetweenSubtitles = 0.25f;

	/// <summary>The maximum number of sentences that should be allowed per subtitle.</summary>
	/// <remarks>The value of this property is 2.</remarks>
	public const int MaxSentencesPerSubtitle = 2;

	public const float DefaultFrameRate = 25;
}

}