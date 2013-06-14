/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2010 Pedro Castro
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

/// <summary>Performs adjustment operations.</summary>
public class AdjustOperator {
	private Subtitles subtitles = null;

	public AdjustOperator (Subtitles subtitles) {
		this.subtitles = subtitles;
	}

	/* Public members */

	/// <summary>Auto adjusts the subtitles given the correct times for the first and last subtitle.</summary>
	/// <remarks>The subtitles are first shifted to the first subtitle's correct time, and then proportionally
	/// adjusted using the last subtitle's correct time.</remarks>
	/// <param name="startTime">The correct start time for the first subtitle.</param>
	/// <param name="endTime">The correct start time for the last subtitle.</param>
	/// <returns>Whether the subtitles could be adjusted.</returns>
	public bool Adjust (TimeSpan startTime, TimeSpan endTime) {
		int startIndex = 0;
		int endIndex = subtitles.Collection.Count - 1;
		return Adjust(startIndex, startTime, endIndex, endTime);
	}

	/// <summary>Auto adjusts a range of subtitles given their first and last correct times.</summary>
	/// <remarks>The subtitles are first shifted to the first subtitle's correct time, and then proportionally
	/// adjusted using the last subtitle's correct time.</remarks>
	/// <param name="startIndex">The subtitle index to start the adjustment with.</param>
	/// <param name="startTime">The correct start time for the first subtitle.</param>
	/// <param name="endIndex">The subtitle index to end the adjustment with.</param>
	/// <param name="endTime">The correct start time for the last subtitle.</param>
	/// <returns>Whether the subtitles could be adjusted.</returns>
	public bool Adjust (int startIndex, TimeSpan startTime, int endIndex, TimeSpan endTime) {
		return SyncUtil.Sync(subtitles, startIndex, startTime, endIndex, endTime, true);
	}

	/// <summary>Auto adjusts the subtitles given the correct frames for the first and last subtitle.</summary>
	/// <remarks>The subtitles are first shifted to the first subtitle's correct frame, and then proportionally
	/// adjusted using the last subtitle's correct frame.</remarks>
	/// <param name="startFrame">The correct start frame for the first subtitle.</param>
	/// <param name="endFrame">The correct start frame for the last subtitle.</param>
	/// <returns>Whether the subtitles could be adjusted.</returns>
	public bool Adjust (int startFrame, int endFrame) {
		int startIndex = 0;
		int endIndex = subtitles.Collection.Count - 1;
		return Adjust(startIndex, startFrame, endIndex, endFrame);
	}

	/// <summary>Auto adjusts a range of subtitles given their first and last correct frames.</summary>
	/// <remarks>The subtitles are first shifted to the first subtitle's correct frame, and then proportionally
	/// adjusted using the last subtitle's correct frame.</remarks>
	/// <param name="startIndex">The subtitle index to start the adjustment with.</param>
	/// <param name="startFrame">The correct start frame for the first subtitle.</param>
	/// <param name="endIndex">The subtitle index to end the adjustment with.</param>
	/// <param name="endFrame">The correct start frame for the last subtitle.</param>
	/// <returns>Whether the subtitles could be adjusted.</returns>
	public bool Adjust (int startIndex, int startFrame, int endIndex, int endFrame) {
		return SyncUtil.Sync(subtitles, startIndex, startFrame, endIndex, endFrame, true);
	}

}

}
