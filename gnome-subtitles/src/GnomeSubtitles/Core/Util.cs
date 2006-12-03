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
using SubLib;

namespace GnomeSubtitles {

public class Util {
  
    static public int TextWidth (Widget widget, string text, int margins) {
    	Pango.Layout layout = widget.CreatePangoLayout(text);
    	int width, height;
    	layout.GetPixelSize(out width, out height);
    	return width + margins;
    }
    
    /// <summary>Converts a timespan to a text representation.</summary>
    /// <remarks>The resulting string is in the format [-]hh:mm:ss.fff. This format is accepted by
    /// <see cref="TimeSpan.Parse" />.</remarks>
    /// <param name="time">The time to convert to text.</param>
    /// <returns>The text representation of the specified time.</returns>
    static public string TimeSpanToText (TimeSpan time) {
		return (time.TotalMilliseconds < 0 ? "-" : String.Empty) +
			time.Hours.ToString("00;00") + ":" + time.Minutes.ToString("00;00") +
			":" + time.Seconds.ToString("00;00") + "." + time.Milliseconds.ToString("000;000");
	}
	
	static public string MillisecondsToTimeText (int milliseconds) {
		return TimeSpanToText(TimeSpan.FromMilliseconds(milliseconds));
	}
	
	static public int TimeTextToMilliseconds (string text) {
		return (int)TimeSpan.Parse(text).TotalMilliseconds;	
	}
	
	static public int SpinButtonTimeWidth (SpinButton spinButton) {
		return TextWidth(spinButton, "00:00:00,000", 25);
	}
	
	public static void OnTimeInput (object o, InputArgs args) {
		SpinButton spinButton = o as SpinButton;
		try {
			args.NewValue = TimeTextToMilliseconds(spinButton.Text);
		}
		catch (Exception) {
			args.NewValue = spinButton.Value;
		}
		args.RetVal = 1;
	}
	
	public static void OnTimeOutput (object o, OutputArgs args) {
		SpinButton spinButton = o as SpinButton;
		spinButton.Numeric = false;
		spinButton.Text = MillisecondsToTimeText((int)spinButton.Value);
		spinButton.Numeric = true;
		args.RetVal = 1;
	}
	
	public static void SetSpinButtonTimingMode (SpinButton spinButton, TimingMode timingMode, bool canNegate) {
		if (timingMode == TimingMode.Frames) {
			spinButton.Adjustment.StepIncrement = 1;
    		spinButton.Adjustment.Upper = 3000000;
    		if (canNegate)
    			spinButton.Adjustment.Lower = -3000000;
		}
		else {
			spinButton.Input += OnTimeInput;
			spinButton.Output += OnTimeOutput;
			spinButton.Adjustment.StepIncrement = 100;
			spinButton.Adjustment.Upper = 86399999;
			if (canNegate)
				spinButton.Adjustment.Lower = -86399999;
		}
	}
	
	public static bool OpenUrl (string url) {
		if ((url == null) || (url == String.Empty))
			return false;

		try {
			Gnome.Url.Show(url);
		}
		catch (Exception) { //Exceptions for Url.Show are not documented in the gtk-sharp api
			return false;
		}
		
		return true;
	}
	
	public static bool OpenSendEmail (string email) {
		return OpenUrl("mailto:" + email);
	}

	public static bool OpenBugReport () {
		return OpenUrl("http://bugzilla.gnome.org/enter_bug.cgi?product=gnome-subtitles");
	}
	
	/// <summary>Returns the index of a <see cref="TreePath" />.</summary>
	public static int PathToInt (TreePath path) {
		return path.Indices[0];
	}
	
	/// <summary>Returns a <see cref="TreePath" /> corresponding to the specified index.</summary>
	public static TreePath IntToPath (int index) {
		return new TreePath(index.ToString());
	}
	
	/// <summary>Returns an array of <see cref="TreePath" /> from an array of ints.</summary>
	public static TreePath[] IntsToPaths (int[] indexes) {
		if (indexes == null)
			return null;
		
		int length = indexes.Length;
		TreePath[] paths = new TreePath[length];
		for (int position = 0 ; position < length ; position++) {
			int index = indexes[position];
			TreePath path = IntToPath(index);
			paths[position] = path;
		}
		return paths;
	}
	
	/// <summary>Returns the path that succeeds the specified path.</summary>
	public static TreePath PathNext (TreePath path) {
		int newIndex = PathToInt(path) + 1;
		return new TreePath(newIndex.ToString());
	}
	
	/// <summary>Returns the path that precedes the specified path.</summary>
	public static TreePath PathPrevious (TreePath path) {
		int newIndex = PathToInt(path) - 1;
		return new TreePath(newIndex.ToString());
	}

	/// <summary>Checks whether two path are equal. They are considered equal if they have the same indice.</summary>
	public static bool PathsAreEqual (TreePath path1, TreePath path2) {
		return (path1.Compare(path2) == 0);	
	}

}

}