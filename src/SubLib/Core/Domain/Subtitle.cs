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

using SubLib.Core.Timing;
using System;

namespace SubLib.Core.Domain {
	
/// <summary>Represents a subtitle, including its time settings, text and text style.</summary>
public class Subtitle {
	private SubtitleProperties properties = null;
	private Times times = null;
	private Frames frames = null;
	private SubtitleText text = null;
	private SubtitleText translation = null;
	private Style style = null;
	
	/// <summary>Initializes a new instance of the <see cref="Subtitle" /> class, given the
	/// global subtitles' properties and the subtitle's text and style.</summary>
	/// <param name="properties">The subtitles' properties.</param>
	/// <param name="text">The subtitle's text.</param>
	/// <param name="style">The subtitle's style.</param>
	public Subtitle (SubtitleProperties properties, SubtitleText text, Style style) {
		this.properties = properties;
		this.text = text;
		this.style = style;
		
		times = new Times(this);
		frames = new Frames(this);
	}
	
	/// <summary>Initializes a new instance of the <see cref="Subtitle" /> class, given the
	/// global subtitles' properties and its start and end times.</summary>
	/// <param name="properties">The subtitles' properties.</param>
	/// <param name="startTime">The subtitle's start time.</param>
	/// <param name="endTime">The subtitle's end time.</param>
	public Subtitle (SubtitleProperties properties, TimeSpan startTime, TimeSpan endTime)
			: this(properties, new SubtitleText(), new Style()) {

		times.Start = startTime;
		times.End = endTime;
	}
	
	/// <summary>Initializes a new instance of the <see cref="Subtitle" /> class, given the
	/// global subtitles' properties and its start and end frames.</summary>
	/// <param name="properties">The subtitles' properties.</param>
	/// <param name="startFrame">The subtitle's start frame.</param>
	/// <param name="endFrame">The subtitle's end frame.</param>
	public Subtitle (SubtitleProperties properties, int startFrame, int endFrame)
			: this(properties, new SubtitleText(), new Style()) {

		frames.Start = startFrame;
		frames.End = endFrame;
	}
	
	/// <summary>Initializes a new instance of the <see cref="Subtitle" /> class, given the
	/// global subtitles' properties.</summary>
	/// <param name="properties">The subtitles' properties.</param>
	public Subtitle (SubtitleProperties properties)
			: this(properties, new SubtitleText(), new Style()){
	}
	
	/// <summary>The subtitle's text.</summary>
	public SubtitleText Text {
		get { return text; }
		set { text = value; }	
	}
	
	/// <summary>The subtitle's translated text.</summary>
	public SubtitleText Translation {
		get {
			if (translation == null)
				translation = new SubtitleText();
			
			return translation;
		}
	}
	
	/// <summary>The subtitle's text style.</summary>
	public Style Style {
		get { return style; }
		set { style = value; }
	}
	
	/// <summary>The subtitle's times.</summary>
	public Times Times {
		get { return times; }
	}
	
	/// <summary>The subtitle's frames.</summary>
	public Frames Frames {
		get { return frames; }
	}
		
	public override string ToString () {
  		return "* " + Times + " (" + Frames + ") " + Style + "\n" + Text.ToString();
	}
	
	/* Internal properties */
	
	internal SubtitleProperties Properties {
		set { properties = value; }
	}
	
	/* Internal methods */

	internal void UpdateFramesFromTimes (float frameRate) {
		UpdateStartFrameFromTimes(frameRate);
		UpdateEndFrameFromTimes(frameRate);
	}
	
	internal void UpdateFramesFromTimes () {
		UpdateStartFrameFromTimes();
		UpdateEndFrameFromTimes();
	}
	
	internal void UpdateTimesFromFrames (float frameRate) {
		UpdateStartTimeFromFrames(frameRate);
		UpdateEndTimeFromFrames(frameRate);
	}
	
	internal void UpdateTimesFromFrames () {
		UpdateStartTimeFromFrames();
		UpdateEndTimeFromFrames();
	}
	
	internal void UpdateStartFrameFromTimes (float frameRate) {
		frames.PreciseStart = TimingUtil.TimeToFrames(times.PreciseStart, frameRate);
	}
	
	internal void UpdateStartFrameFromTimes () {
		UpdateStartFrameFromTimes(properties.CurrentFrameRate);
	}
	
	internal void UpdateEndFrameFromTimes (float frameRate) {
		frames.PreciseEnd = TimingUtil.TimeToFrames(times.PreciseEnd, frameRate);
	}
	
	internal void UpdateEndFrameFromTimes () {
		UpdateEndFrameFromTimes(properties.CurrentFrameRate);
	}
	
	internal void UpdateStartTimeFromFrames (float frameRate) {
		times.PreciseStart = TimingUtil.FramesToTime(frames.PreciseStart, frameRate);
	}
	
	internal void UpdateStartTimeFromFrames () {
		UpdateStartTimeFromFrames(properties.CurrentFrameRate);
	}
	
	internal void UpdateEndTimeFromFrames (float frameRate) {
		times.PreciseEnd = TimingUtil.FramesToTime(frames.PreciseEnd, frameRate);
	}
	
	internal void UpdateEndTimeFromFrames () {
		UpdateEndTimeFromFrames(properties.CurrentFrameRate);
	}
	
	internal void ClearTranslation () {
		translation = null;
	}

}

}
