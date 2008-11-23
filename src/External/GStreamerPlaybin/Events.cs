/*

	Copyright (c)  Goran Sterjov, Pedro Castro

    This file is part of the GStreamer Playbin Wrapper.
    Derived from Fuse.

    GStreamer Playbin Wrapper is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    GStreamer Playbin Wrapper is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with GStreamer Playbin Wrapper; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/



using System;

namespace GStreamer
{
	
	/// <summary>
	/// Arguments for a raised error.
	/// </summary>
	public sealed class ErrorEventArgs
	{
		string error, debug;
		
		internal ErrorEventArgs (string error, string debug)
		{
			this.error = error;
			this.debug = debug;
		}
		
        public string Error { get{ return error; } }
		public string Debug { get{ return debug; } }
    }
	
	
	
	/// <summary>
	/// Arguments for a raised buffer event.
	/// </summary>
	public sealed class BufferEventArgs
	{
		int progress;
		
		internal BufferEventArgs (int progress)
		{ this.progress = progress; }
		
        public int Progress { get{ return progress; } }
    }
	
	
	
	/// <summary>
	/// Arguments for a raised video info event.
	/// </summary>
	public sealed class VideoInfoEventArgs
	{
		VideoInfo video_info;
		
		internal VideoInfoEventArgs (VideoInfo video_info)
		{ this.video_info = video_info; }
		
        public VideoInfo VideoInfo { get{ return video_info; } }
    }
	
	
	
	/// <summary>
	/// Arguments for a raised video info event.
	/// </summary>
	public sealed class TagEventArgs
	{
		Tag tag;
		
		internal TagEventArgs (Tag tag)
		{ this.tag = tag; }
		
        public Tag Tag { get{ return tag; } }
    }
	
    
	
	/// <summary>
	/// Arguments for a raised state.
	/// </summary>
    public sealed class StateEventArgs
    {
    	MediaStatus state;
		
    	internal StateEventArgs (MediaStatus state)
    	{ this.state = state; }
		
    	public MediaStatus State { get{ return state; } }
    }
	
}