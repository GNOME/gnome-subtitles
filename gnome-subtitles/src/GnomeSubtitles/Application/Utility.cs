/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
 * Copyright (C) 2006 Pedro Castro
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

using Gtk;
using System;

namespace GnomeSubtitles {

public class Utility {
  
    static public int TextWidth(Widget widget, string text, int margins) {
    		Pango.Layout layout = widget.CreatePangoLayout(text);
    		int width, height;
    		layout.GetPixelSize(out width, out height);
    		return width + margins;
    }
    
    	static public string TimeSpanToText (TimeSpan time) {
		return time.Hours.ToString("D2") + ":" + time.Minutes.ToString("D2") +
				":" + time.Seconds.ToString("D2") + "." + time.Milliseconds.ToString("D3");
	}



}

}