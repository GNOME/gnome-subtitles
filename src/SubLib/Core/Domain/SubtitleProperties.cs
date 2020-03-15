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

using SubLib.IO.Input;
using System;

namespace SubLib.Core.Domain {

/// <summary>Represents the properties of subtitles.</summary>
/// <remarks>This class acts as a container which allows you to get and set a
/// variety of properties. Some of these properties are used in synchronization
/// and timing calculations.</remarks>
public class SubtitleProperties : ICloneable {
	private Headers headers = new Headers();

	private float originalFrameRate = 25;
	private float currentFrameRate = 25;


	public SubtitleProperties () {
	}

	public SubtitleProperties (Headers headers, float originalFrameRate, float currentFrameRate) {
		this.headers = headers;
		this.originalFrameRate = originalFrameRate;
		this.currentFrameRate = currentFrameRate;
	}

	/// <summary>Initializes a new instance of the <see cref="SubtitleProperties" />
	/// class, with defaults for all properties.</summary>
	internal SubtitleProperties (ParsingProperties properties) {
		headers = properties.Headers;
		originalFrameRate = properties.InputFrameRate;
	}


	/* Public properties */

	/// <summary>The headers used in some subtitle formats.</summary>
	public Headers Headers {
		get { return headers; }
	}

	/// <summary>The frame rate originally applied to the subtitles.</summary>
	/// <remarks>When converting between frame rates, this is the frame rate of the subtitles
	/// when they are opened. This is sometimes referred to as the input frame rate.</remarks>
	public float OriginalFrameRate {
		get { return originalFrameRate; }
	}

	/// <summary>The frame rate currently being used in the subtitles.</summary>
	/// <remarks>When converting between frame rates, this is the target frame rate of the
	/// subtitles. This is sometimes referred to as the output frame rate.</remarks>
	public float CurrentFrameRate {
		get { return currentFrameRate; }
	}


	/* Public methods */

	public override string ToString () {
		return "Original FPS = " + originalFrameRate + ", Current FPS = " + currentFrameRate + "\n" + headers.ToString();
	}

	public object Clone() {
		return new SubtitleProperties(this.Headers.Clone() as Headers, this.OriginalFrameRate, this.CurrentFrameRate);
	}


	/* Internal members */

	internal void SetCurrentFrameRate (float frameRate) {
		currentFrameRate = frameRate;
	}

	internal void SetOriginalFrameRate (float frameRate) {
		originalFrameRate = frameRate;
	}

}

}
