/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2010 Pedro Castro
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

using SubLib.Core.Timing;
using System;

namespace SubLib.Core.Domain {

/// <summary>Represents the frames of a subtitle.</summary>
/// <remarks><see cref="Frames" /> and <see cref="Times" /> always exist for any <see cref="Subtitle" />.
/// This class is automatically constructed when constructing a <see cref="Subtitle" />.</remarks>
public class Frames {
	private double start = 0;
	private double end = 0;
	private Subtitle subtitle = null;


	/* Public properties */

	/// <summary>The start frame.</summary>
	/// <remarks>Upon setting the start frame, the start time (<see cref="Times.Start">Times.Start</see>) is also updated based on
	/// the <see cref="SubtitleProperties.CurrentFrameRate" />.</remarks>
	public int Start {
		get { return (int)Math.Round(PreciseStart); }
		set {
			PreciseStart = value;
			subtitle.UpdateStartTimeFromFrames();
		}
	}

	/// <summary>The end frame.</summary>
	/// <remarks>Upon setting the end frame, the end time (<see cref="Times.End">Times.End</see>) is also updated based on
	/// the <see cref="SubtitleProperties.CurrentFrameRate" />.</remarks>
	public int End {
		get { return (int)Math.Round(PreciseEnd); }
		set {
			PreciseEnd = value;
			subtitle.UpdateEndTimeFromFrames();
		}
	}

	/// <summary>The duration, in frames.</summary>
	/// <remarks>Setting the duration maintains the start frame and changes the end frame.
	/// Upon setting the duration, the end time (<see cref="Times.End">Times.End</see>) is also updated based on
	/// the <see cref="SubtitleProperties.CurrentFrameRate" />.</remarks>
	public int Duration {
		get { return End - Start; }
		set { End = Start + value; }
	}

	/* Public methods */

	/// <summary>Shifts the subtitle with a specified amount of frames.</summary>
	/// <param name="frames">The number of frames to shift the subtitle with, which can be positive or negative.</param>
	public void Shift (int frames) {
		PreciseStart += frames;
		PreciseEnd += frames;
		subtitle.UpdateTimesFromFrames();
	}

	public override string ToString() {
  		return Start + "->" + End;
	}

	public Frames Clone (Subtitle subtitleClone) {
		Frames clone = this.MemberwiseClone() as Frames;
		clone.SetFieldsForDeepClone(subtitleClone);
		return clone;
	}


		/* Internal members */

	internal Frames (Subtitle subtitle) {
		this.subtitle = subtitle;
	}

	internal Frames (Subtitle subtitle, int start, int end) {
		this.start = start;
		this.end = end;
		this.subtitle = subtitle;
	}

	/// <remarks>Doesn't update times.</remarks>
	internal double PreciseStart {
		get { return start; }
		set { start = value; }
	}

	/// <remarks>Doesn't update times.</remarks>
	internal double PreciseEnd {
		get { return end; }
		set { end = value; }
	}

	/// <remarks>Doesn't update times.</remarks>
	internal double PreciseDuration {
		get { return end - start; }
		set { end = start + value; }
	}

	internal void Scale (double factor, int baseFrame) {

		PreciseStart = SyncUtil.Scale(PreciseStart, baseFrame, factor);
		PreciseEnd = SyncUtil.Scale(PreciseEnd, baseFrame, factor);

		subtitle.UpdateTimesFromFrames();
	}

	/* Private members */

	private void SetFieldsForDeepClone (Subtitle subtitle) {
		this.subtitle = subtitle;
	}

}

}
