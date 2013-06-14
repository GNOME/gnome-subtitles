/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2009 Pedro Castro
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

namespace SubLib.Core.Timing {

/// <summary>Contains utilitary methods for synchronization operations.</summary>
public class TimingUtil {

	/// <summary>Converts the specified frames to time, given a frame rate.</summary>
	/// <param name="frames">The frames to convert to time.</param>
	/// <param name="frameRate">The frame rate to be used in the conversion.</param>
	/// <returns>The time corresponding to the specified frames at the specified frame rate.</returns>
	public static TimeSpan FramesToTime (double frames, float frameRate) {
		double seconds = frames / frameRate;
		TimeSpan time = TimeSpan.FromSeconds(seconds);
		return time;
	}

	/// <summary>Converts the specified time to frames, given a frame rate.</summary>
	/// <param name="time">The time to convert to frames.</param>
	/// <param name="frameRate">The frame rate to be used in the conversion.</param>
	/// <returns>The frames corresponding to the specified time at the specified frame rate.</returns>
	public static double TimeToFrames (TimeSpan time, float frameRate) {
		double seconds = time.TotalSeconds;
		double frames = seconds * frameRate;
		return frames;
	}

	/// <summary>Converts the specified time to frames, given a frame rate.</summary>
	/// <param name="time">The time, in seconds, to convert to frames.</param>
	/// <param name="frameRate">The frame rate to be used in the conversion.</param>
	/// <returns>The frames corresponding to the specified time at the specified frame rate.</returns>
	public static double TimeToFrames (float time, float frameRate) {
		double frames = time * frameRate;
		return frames;
	}

	/// <summary>Converts the specified time to frames, given a frame rate.</summary>
	/// <param name="time">The time, in milliseconds, to convert to frames.</param>
	/// <param name="frameRate">The frame rate to be used in the conversion.</param>
	/// <returns>The frames corresponding to the specified time at the specified frame rate.</returns>
	public static double TimeMillisecondsToFrames (float time, float frameRate) {
		double frames = (time / 1000) * frameRate;
		return frames;
	}

}

}
