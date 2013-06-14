/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2008 Pedro Castro
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

/// <summary>Performs shift operations.</summary>
public class ShiftOperator {
	private Subtitles subtitles = null;

	public ShiftOperator (Subtitles subtitles) {
		this.subtitles = subtitles;
	}

	/* Public members */

	/// <summary>Shifts the subtitles a specified amount of time.</summary>
	/// <param name="time">The time to use, which can be positive or negative.</param>
	public void Shift (TimeSpan time) {
		foreach (Subtitle subtitle in subtitles.Collection)
			subtitle.Times.Shift(time);
	}

	/// <summary>Shifts a range of subtitles a specified amount of time.</summary>
	/// <param name="time">The time to use, which can be positive or negative.</param>
	/// <param name="startIndex">The subtitle index the range begins with.</param>
	/// <param name="endIndex">The subtitle index the range ends with.</param>
	public void Shift (TimeSpan time, int startIndex, int endIndex) {
		if (!AreShiftArgsValid(startIndex, endIndex))
			return;

		for (int index = startIndex ; index <= endIndex ; index++) {
			Subtitle subtitle = subtitles.Collection.Get(index);
			subtitle.Times.Shift(time);
		}
	}

	/// <summary>Shifts the subtitles a specified amount of frames.</summary>
	/// <param name="frames">The frames to use, which can be positive or negative.</param>
	public void Shift (int frames) {
		foreach (Subtitle subtitle in subtitles.Collection)
			subtitle.Frames.Shift(frames);
	}

	/// <summary>Shifts a range of subtitles a specified amount of frames.</summary>
	/// <param name="frames">The frames to use, which can be positive or negative.</param>
	/// <param name="startIndex">The subtitle index the range begins with.</param>
	/// <param name="endIndex">The subtitle index the range ends with.</param>
	public void Shift (int frames, int startIndex, int endIndex) {
		if (!AreShiftArgsValid(startIndex, endIndex))
			return;

		for (int index = startIndex ; index <= endIndex ; index++) {
			Subtitle subtitle = subtitles.Collection.Get(index);
			subtitle.Frames.Shift(frames);
		}
	}


	/* Private members */

	private bool AreShiftArgsValid (int startIndex, int endIndex) {
		int subtitleCount = subtitles.Collection.Count;
		if (subtitleCount == 0)
			return false;
		else if (!(startIndex <= endIndex))
			return false;
		else if ((startIndex < 0) || (startIndex >= subtitleCount))
			return false;
		else if (endIndex >= subtitleCount)
			return false;
		else
			return true;
	}

}

}
