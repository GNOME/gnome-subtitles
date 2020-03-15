/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2019 Pedro Castro
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

namespace SubLib.Core.Timing {

/// <summary>Performs frame rate operations.</summary>
public class FrameRateOperator {
	private Subtitles subtitles = null;

	public FrameRateOperator (Subtitles subtitles) {
		this.subtitles = subtitles;
	}

	/* Public members */


	/// <summary>Changes the current frame rate of the subtitles.</summary>
	/// <param name="frameRate">The new frame rate to be used.</param>
	/// <remarks>This changes the frame rate currently being used with the subtitles, which is sometimes
	/// referred to as the output frame rate.</remarks>
	public void ChangeCurrent (float frameRate) {
		float currentFrameRate = subtitles.Properties.CurrentFrameRate;
		if (currentFrameRate != frameRate) {
			subtitles.Properties.SetCurrentFrameRate(frameRate);
			subtitles.UpdateFramesFromTimes(frameRate);
		}
	}

	/// <summary>Updates the subtitles' frames using the specified frame rate as the one they had when they were opened.</summary>
	/// <param name="frameRate">The original subtitles' frame rate.</param>
	/// <remarks>This results on having the subtitles with the frames they would have if they had been opened with this frame rate.
	/// The original frame rate is sometimes referred to as the input frame rate.</remarks>
	public void ChangeOriginal (float frameRate) {
		SubtitleProperties properties = subtitles.Properties;
		float originalFrameRate = properties.OriginalFrameRate;
		float currentFrameRate = properties.CurrentFrameRate;

		if (originalFrameRate != frameRate) {
			float conversionFrameRate = currentFrameRate * originalFrameRate / frameRate;
			subtitles.UpdateFramesFromTimes(conversionFrameRate);
			subtitles.UpdateTimesFromFrames(currentFrameRate);
			properties.SetOriginalFrameRate(frameRate);
		}
	}

}

}
