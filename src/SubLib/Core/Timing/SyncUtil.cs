/*
 * This file is part of SubLib.
 * Copyright (C) 2009 Pedro Castro
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

namespace SubLib.Core.Timing {

public class SyncUtil {

	public static bool Sync (Subtitles subtitles, SyncPoint start, SyncPoint end) {
		return Sync(subtitles, start.SubtitleNumber, start.Correct.Time, end.SubtitleNumber, end.Correct.Time);
	}

	/// <summary>Auto syncs a range of subtitles given their first and last correct times.</summary>
	/// <remarks>The subtitles are first shifted to the first subtitle's correct time, and then proportionally 
	/// adjusted using the last subtitle's correct time.</remarks>
	/// <param name="subtitles">The subtitles to sync.</param>
	/// <param name="startIndex">The subtitle index to start the adjustment with.</param>
	/// <param name="startTime">The correct start time for the first subtitle.</param>
	/// <param name="endIndex">The subtitle index to end the adjustment with.</param>
	/// <param name="endTime">The correct start time for the last subtitle.</param>
	/// <returns>Whether the subtitles could be adjusted.</returns>
	public static bool Sync (Subtitles subtitles, int startIndex, TimeSpan startTime, int endIndex, TimeSpan endTime) {
		if (!AreSyncArgsValid(subtitles, startIndex, startTime, endIndex, endTime))
			return false;

		/* Shift subtitles to the start point */
		Subtitle startSubtitle = subtitles.Collection.Get(startIndex);
		TimeSpan shift = startTime - startSubtitle.Times.PreciseStart;
		if (shift != TimeSpan.Zero) {
			ShiftOperator shiftOp = new ShiftOperator(subtitles);
			shiftOp.Shift(shift, startIndex, endIndex);
		}
		
		/* Auto adjust timings with proportion */
		Subtitle endSubtitle = subtitles.Collection.Get(endIndex);
		double factor = (endTime - startTime).TotalMilliseconds / (endSubtitle.Times.PreciseStart - startTime).TotalMilliseconds;
		for (int index = startIndex ; index <= endIndex ; index++) {
			Subtitle subtitle = subtitles.Collection.Get(index);
			subtitle.Times.Scale(factor, startTime);
		}
		return true;
	}
	
	/// <summary>Auto syncs a range of subtitles given their first and last correct frames.</summary>
	/// <remarks>The subtitles are first shifted to the first subtitle's correct frame, and then proportionally 
	/// adjusted using the last subtitle's correct frame.</remarks>
	/// <param name="subtitles">The subtitles to sync.</param>
	/// <param name="startIndex">The subtitle index to start the adjustment with.</param>
	/// <param name="startFrame">The correct start frame for the first subtitle.</param>
	/// <param name="endIndex">The subtitle index to end the adjustment with.</param>
	/// <param name="endFrame">The correct start frame for the last subtitle.</param>
	/// <returns>Whether the subtitles could be adjusted.</returns>
	public static bool Sync (Subtitles subtitles, int startIndex, int startFrame, int endIndex, int endFrame) {
		if (!AreSyncArgsValid(subtitles, startIndex, startFrame, endIndex, endFrame))
			return false;

		/* Shift subtitles to the start point */
		Subtitle startSubtitle = subtitles.Collection.Get(startIndex);
		int shift = (int)(startFrame - startSubtitle.Frames.PreciseStart);
		if (shift != 0) {
			ShiftOperator shiftOp = new ShiftOperator(subtitles);
			shiftOp.Shift(shift, startIndex, endIndex);
		}
		
		/* Auto adjust timings with proportion */
		Subtitle endSubtitle = subtitles.Collection.Get(endIndex);
		double factor = (endFrame - startFrame) / (endSubtitle.Frames.PreciseStart - startFrame);
		for (int index = startIndex ; index <= endIndex ; index++) {
			Subtitle subtitle = subtitles.Collection.Get(index);
			subtitle.Frames.Scale(factor, startFrame);
		}
		return true;
	}
	
	/* Private members */
	
	public static bool AreSyncPointsValid (Subtitles subtitles, SyncPoint start, SyncPoint end) {
		return AreSyncArgsValid(subtitles, start.SubtitleNumber, start.Correct.Time, end.SubtitleNumber, end.Correct.Time)
			&& AreSyncArgsValid(subtitles, start.SubtitleNumber, start.Correct.Frame, end.SubtitleNumber, end.Correct.Frame);
	}
	
	private static bool AreSyncArgsValid (Subtitles subtitles, int startIndex, TimeSpan startTime, int endIndex, TimeSpan endTime) {
		if (!AreSyncIndicesValid(subtitles, startIndex, endIndex))
			return false;
		else if (!(startTime < endTime))
			return false;
		else
			return true;
	}
	
	private static bool AreSyncArgsValid (Subtitles subtitles, int startIndex, int startTime, int endIndex, int endTime) {
		if (!AreSyncIndicesValid(subtitles, startIndex, endIndex))
			return false;
		else if (!(startTime < endTime))
			return false;
		else
			return true;
	}
	
	private static bool AreSyncIndicesValid (Subtitles subtitles, int startIndex, int endIndex) {
		int subtitleCount = subtitles.Collection.Count;
		if (subtitleCount < 2)
			return false;
		else if (!(startIndex < endIndex))
			return false;
		else if ((startIndex < 0) || (startIndex >= (subtitleCount - 1)))
			return false;
		else if (endIndex >= subtitleCount)
			return false;
		else
			return true;
	}

}

}
