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
using SubLib;
using System;
using System.Text.RegularExpressions;

namespace GnomeSubtitles {

/// <summary>Represents a search environment.</summary>
/// <remarks>The search dialog is kept as a reference, and created on its first use.</remarks>
public class Search {
	private SearchDialog dialog = null;

	public Search () {
		
	}
	
	/* Public members */

	public void Find () {
		InitDialog(false);
	}

	public void FindPrevious () {
		Find(true);
	}
	
	public void FindNext () {
		Find(false);
	}
	
	public void Replace () {
		InitDialog(true);
	}

	/* Private members */
	
	private void InitDialog (bool showReplace) {
		if (dialog == null)
			dialog = new SearchDialog(); //SearchDialog starts invisible as default
	
		dialog.ShowReplace = showReplace;
		dialog.ShowDialog();
	}

	/// <summary></summary>
	/// <param name=""></param>
	/// <remarks></remarks>
	/// <returns>Whether the text was found.</returns>
	private bool Find (bool backwards) {
		if (dialog == null)
			return false;
	
		int startSubtitle, startIndex;
		GetCursorPosition(out startSubtitle, out startIndex);
		
		Regex regex = dialog.Regex;
		int foundIndex, foundLength, foundSubtitle;
		if (backwards)
			foundSubtitle = Global.Subtitles.FindBackwards(regex, startSubtitle, startIndex, dialog.Wrap, out foundIndex, out foundLength);
		else
			foundSubtitle = Global.Subtitles.Find(regex, startSubtitle, startIndex, dialog.Wrap, out foundIndex, out foundLength);

		System.Console.WriteLine("Find results: subtitle " + foundSubtitle + " at index " + foundIndex);
		if (foundSubtitle == -1) //Text not found
			return false;
		else {
			int start, end;
			GetIndexesToSelect(foundIndex, foundIndex + foundLength, backwards, out start, out end);
			Global.GUI.View.Selection.Select(Util.IntToPath(foundSubtitle), true, true, start, end);
			return true;
		}
	}	
	
	/// <summary>Gets the current position of the cursor.</summary>
	/// <param name="subtitle">The subtitle with focus. If the focused subtitle isn't selected, the first
	/// selected subtitle is used. If no subtitle is focused nor selected, the first subtitle of all is used.</param>
	/// <param name="index">The index of the cursor within the focused subtitle, if there is only one subtitle selected.</param>
	private void GetCursorPosition (out int subtitle, out int index) {
		TreePath focus = Global.GUI.View.Selection.Focus;
		subtitle = (focus != null ? Util.PathToInt(focus) :0);
		index = ((Global.GUI.Edit.Enabled && Global.GUI.Edit.TextIsFocus) ? Global.GUI.Edit.TextCursorIndex : 0);
	}

	private void GetIndexesToSelect (int start, int end, bool backwards, out int newStart, out int newEnd) {
		if (!backwards) {
			newStart = end;
			newEnd = start;
		}
		else {
			newStart = start;
			newEnd = end;		
		}
	}


}

}