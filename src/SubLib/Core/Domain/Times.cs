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

namespace SubLib.Core.Domain {

/// <summary>Represents the times of a subtitle.</summary>
/// <remarks><see cref="Times" /> and <see cref="Frames" /> always exist for any <see cref="Subtitle" />.
/// This class is automatically constructed when constructing a <see cref="Subtitle" />.</remarks>
public class Times {
	private TimeSpan start = TimeSpan.Zero;
	private TimeSpan end = TimeSpan.Zero;
	private Subtitle subtitle = null;


	/// <summary>The start time.</summary>
	/// <remarks>Upon setting the start time, the start frame (<see cref="Frames.Start">Frames.Start</see>) is also updated based on
	/// the <see cref="SubtitleProperties.CurrentFrameRate" />.</remarks>
	public TimeSpan Start {
		get { return PreciseStart; }
		set {
			PreciseStart = value;
			subtitle.UpdateStartFrameFromTimes();
		}
	}
	
	/// <summary>The end time.</summary>
	/// <remarks>Upon setting the end time, the end frame (<see cref="Frames.End">Frames.End</see>) is also updated based on
	/// the <see cref="SubtitleProperties.CurrentFrameRate" />.</remarks>
	public TimeSpan End {
		get { return PreciseEnd; }
		set {
			PreciseEnd = value;
			subtitle.UpdateEndFrameFromTimes();
		}
	}
	
	/// <summary>The time duration.</summary>
	/// <remarks>Setting the duration maintains the start time and changes the end time.
	/// Upon setting the duration, the end frame (<see cref="Frames.End">Frames.End</see>) is also updated based on
	/// the <see cref="SubtitleProperties.CurrentFrameRate" />.</remarks>
	public TimeSpan Duration {
		get { return End - Start; }
		set { End = Start + value; }
	}

	/// <summary>Shifts the subtitle with a specified time span.</summary>
	/// <param name="time">The time span to shift the subtitle with, which can be positive or negative.</param>
	public void Shift (TimeSpan time) {
		PreciseStart += time;
		PreciseEnd += time;
		subtitle.UpdateFramesFromTimes();
	}

	public Times Clone () {
		return MemberwiseClone() as Times;
	}

	
	public override string ToString() {
  		return Start + "->" + End;
	}

	
	/* Internal members */

	internal Times (Subtitle subtitle) {
		this.subtitle = subtitle;
	}
	
	internal Times (Subtitle subtitle, TimeSpan start, TimeSpan end) {
		this.start = start;
		this.end = end;
		this.subtitle = subtitle;	
	}

	/// <remarks>Doesn't update frames.</remarks>
	internal TimeSpan PreciseStart {
		get { return start; }
		set { start = value; }
	}
	
	/// <remarks>Doesn't update frames.</remarks>
	internal TimeSpan PreciseEnd {
		get { return end; }
		set { end = value; }
	}
	
	/// <remarks>Doesn't update frames.</remarks>
	internal TimeSpan PreciseDuration {
		get { return end - start; }
		set { end = start + value; }
	}
	
	internal void Scale (double factor, TimeSpan baseTime) {
		double baseMilliseconds = baseTime.TotalMilliseconds;
	
		double start = PreciseStart.TotalMilliseconds;
		double newStart = baseMilliseconds + ((start - baseMilliseconds) * factor);
		PreciseStart = TimeSpan.FromMilliseconds(newStart);
		
		double end = PreciseEnd.TotalMilliseconds;
		double newEnd = baseMilliseconds + ((end - baseMilliseconds) * factor);
		PreciseEnd = TimeSpan.FromMilliseconds(newEnd);

		subtitle.UpdateFramesFromTimes();	
	}

}

}
