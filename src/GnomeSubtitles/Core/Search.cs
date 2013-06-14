/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2009 Pedro Castro
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

using GnomeSubtitles.Core.Command;
using GnomeSubtitles.Dialog;
using Gtk;
using SubLib.Core.Domain;
using SubLib.Core.Search;
using System;
using System.Text.RegularExpressions;

namespace GnomeSubtitles.Core {

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
		Base.Ui.Menus.EnableFindNextPrevious();

		if (dialog.Backwards)
			return FindPrevious();
		else
			return FindNext();
	}

	/// <summary>Finds the previous match.</summary>
	/// <returns>Whether text was found.</returns>
	public bool FindPrevious () {
		return Find(true);
	}

	/// <summary>Finds the next match.</summary>
	/// <returns>Whether text was found.</returns>
	public bool FindNext () {
		return Find(false);
	}

	public bool Replace () {
		if (!SelectionMatchesSearch())
			return Find();

		string replacement = dialog.Replacement;
		Base.Ui.Edit.ReplaceSelection(replacement);

		return Find();
	}

	public void ReplaceAll () {
		Base.CommandManager.Execute(new ReplaceAllCommand(dialog.ForwardRegex, dialog.Replacement));
	}

	/// <summary>Does some cleanup to make sure the dialog is like a brand new one.</summary>
	/// <remarks>The old dialog is closed and nullified</remarks>
	public void Clear () {
		if (dialog != null) {
			dialog.Destroy();
			dialog = null;
		}
	}

	#region Private methods

	private void InitDialog (bool showReplace) {
		dialog = Base.Dialogs.Get(typeof(SearchDialog)) as SearchDialog;

		dialog.Show(showReplace);
	}

	/// <summary>Finds text in the subtitles using the specified direction and the options set in the Find dialog.</summary>
	/// <param name="backwards">Whether to search backwards.</param>
	/// <returns>Whether the text was found.</returns>
	private bool Find (bool backwards) {
		if (dialog == null)
			return false;

		/* Get selection range */
		SubtitleTextType textType;
		int selectionStart, selectionEnd;
		GetTextContentSelectionIndexes(out selectionStart, out selectionEnd, out textType);

		/* Get remaining properties */
		int subtitle = GetFocusedSubtitle();
		Regex regex = (backwards ? dialog.BackwardRegex : dialog.ForwardRegex);
		int index = (backwards ? selectionStart : selectionEnd);

		/* Search */
		SubtitleSearchOptions options = new SubtitleSearchOptions(regex, textType, subtitle, index, dialog.Wrap, backwards);
		SearchOperator searchOp = new SearchOperator(Base.Document.Subtitles);
		SubtitleSearchResults results = searchOp.Find(options);

		/* If no text was found, return */
		if (results == null)
			return false;

		/* Text was found, selecting it */
		int start, end;
		GetIndexesToSelect(results.Index, results.Index + results.Length, backwards, out start, out end);
		Base.Ui.View.Selection.Select(Util.IntToPath(results.Subtitle), true, true, start, end, results.TextType);
		return true;
	}

	/// <summary>Gets the indexes of the current text selection.</summary>
	/// <param name="start">The start of the selection.</param>
	/// <param name="end">The end of the selection.</param>
	/// <param name="textType">The type of text content selected.</param>
	/// <remarks>If no subtitle is being edited, both indexes are set to zero.
	/// If no text is selected, both indexes are set to the position of the cursor.</remarks>
	private void GetTextContentSelectionIndexes (out int start, out int end, out SubtitleTextType textType) {
		if (Base.Ui.Edit.Enabled && Base.Ui.Edit.TextOrTranslationIsFocus)
			Base.Ui.Edit.GetTextSelectionBounds(out start, out end, out textType);
		else {
			start = 0;
			end = 0;
			textType = SubtitleTextType.Text;
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

	/// <summary>The currently focused subtitle, or 0 if none is.</summary>
	private int GetFocusedSubtitle() {
			TreePath focus = Base.Ui.View.Selection.Focus;
			if (focus != null)
				return Util.PathToInt(focus);
			else
				return 0;
	}

	private bool SelectionMatchesSearch () {
			string selection = Base.Ui.Edit.SelectedTextContent;
			if (selection == String.Empty)
				return false;

			Match match = dialog.ForwardRegex.Match(selection); //Either forward and backward regexes work
			return (match.Success && (match.Length == selection.Length));
	}

	#endregion

}

}
