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

	public void ShowFind () {
		InitDialog(false);
	}
	
	public void ShowReplace () {
		InitDialog(true);
	}

	public bool Find () {
		if (dialog.Backwards)
			return FindPrevious();
		else
			return FindNext();
	}

	public bool FindPrevious () {
		return Find(true);
	}
	
	public bool FindNext () {
		return Find(false);
	}
	
	public bool Replace () {
		if (!SelectionMatchesSearch)
			return Find();
		
		string replacement = dialog.Replacement;
		Global.GUI.Edit.ReplaceSelection(replacement);
		
		return Find();
	}
	
	/* Private properties */
	
	/// <summary>The currently focused subtitle, or 0 if none is.</summary>
	private int FocusedSubtitle {
		get {
			TreePath focus = Global.GUI.View.Selection.Focus;
			if (focus != null)
				return Util.PathToInt(focus);
			else
				return 0;
		}
	}
	
	private bool SelectionMatchesSearch {
		get {
			string selection = Global.GUI.Edit.TextSelection;
			if (selection == String.Empty)
				return false;
			
			Match match = dialog.ForwardRegex.Match(selection); //Either forward and backward regexes work
			return (match.Success && (match.Length == selection.Length));
		}	
	}

	/* Private methods */
	
	private void InitDialog (bool showReplace) {
		if (dialog == null)
			dialog = new SearchDialog(); //SearchDialog starts invisible as default
	
		dialog.ShowReplace = showReplace;
		dialog.ShowDialog();
	}

	/// <summary>Finds text in the subtitles using the specified direction and the options set in the Find dialog.</summary>
	/// <param name="backwards">Whether to search backwards.</param>
	/// <returns>Whether the text was found.</returns>
	private bool Find (bool backwards) {
		if (dialog == null)
			return false;

		int selectionStart, selectionEnd;
		GetSelectionIndexes(out selectionStart, out selectionEnd);
		
		int foundIndex, foundLength, foundSubtitle;
		if (backwards)
			foundSubtitle = Global.Subtitles.FindBackwards(dialog.BackwardRegex, FocusedSubtitle, selectionStart, dialog.Wrap, out foundIndex, out foundLength);
		else
			foundSubtitle = Global.Subtitles.Find(dialog.ForwardRegex, FocusedSubtitle, selectionEnd, dialog.Wrap, out foundIndex, out foundLength);

		if (foundSubtitle == -1) //Text not found
			return false;
		else {
			int start, end;
			GetIndexesToSelect(foundIndex, foundIndex + foundLength, backwards, out start, out end);
			Global.GUI.View.Selection.Select(Util.IntToPath(foundSubtitle), true, true, start, end);
			return true;
		}
	}

	/// <summary>Gets the indexes of the current text selection.</summary>
	/// <param name="start">The start of the selection.</param>
	/// <param name="end">The end of the selection.</param>
	/// <remarks>If no subtitle is being edited, both indexes are set to zero.
	/// If no text is selected, both indexes are set to the position of the cursor.</remarks>
	private void GetSelectionIndexes (out int start, out int end) {
		if (Global.GUI.Edit.Enabled && Global.GUI.Edit.TextIsFocus)
			Global.GUI.Edit.GetTextSelectionBounds(out start, out end);
		else {
			start = 0;
			end = 0;
		}
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