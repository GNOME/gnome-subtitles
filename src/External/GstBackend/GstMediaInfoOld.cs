/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2021 Pedro Castro
 *
 * Gnome Subtitles is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * Gnome Subtitles is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Runtime.InteropServices;

namespace External.GstBackendOld {

[StructLayout(LayoutKind.Sequential)]
public class GstMediaInfoOld {
	long duration;
	
	bool has_video;
	int width;
	int height;
	float aspect_ratio;
	float frame_rate;
	
	bool has_audio;

	public GstMediaInfoOld(IntPtr ptr) {
		if (ptr != IntPtr.Zero) {
			Marshal.PtrToStructure(ptr, this);
		}
	}
	
	
	/* Public properties */
	
	public long Duration {
		get { return duration; }
	}
	
	public bool HasVideo {
		get { return has_video; }
	}
	
	public int Width {
		get { return width; }
	}
	
	public int Height
		{ get { return height; }
	}
	
	public float AspectRatio {
		get { return aspect_ratio; }
	}
	
	public float FrameRate {
		get { return frame_rate; }
	}
	
	public bool HasAudio {
		get { return has_audio; }
	}

}

}
