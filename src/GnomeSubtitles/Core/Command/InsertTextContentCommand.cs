/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2019 Pedro Castro
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

using GnomeSubtitles.Ui.Edit;
using System;

namespace GnomeSubtitles.Core.Command {

public abstract class InsertTextContentCommand : FixedSingleSelectionCommand {
	private int index; //The offset where the text was inserted
	private string text; //The inserted text

	public InsertTextContentCommand (string description, int index, string text) : base(description, true, false) {
		this.index = index;
		this.text = text;

		/* If text isn't a single char, it should have been pasted, so stop grouping */
		if (text.Length != 1)
			SetStopsGrouping(true);
	}

	/* Abstract members */
	protected abstract SubtitleEditTextView GetTextView ();

	/* Public members */

	public override bool CanGroupWith (Command command) {
		InsertTextContentCommand last = command as InsertTextContentCommand;

		return (this.Path.Compare(last.Path) == 0) //Paths are equal
			&& (this.index == (last.GetIndex() + last.GetText().Length)) //This index is the position after the last text
			&& (this.text.Length == 1) //A single char was inserted
			&& (!(Char.IsWhiteSpace(last.GetLastChar()) && (!Char.IsWhiteSpace(GetLastChar())))); //A space->non-space sequence is not the case
	}

	public override Command MergeWith (Command command) {
		InsertTextContentCommand last = command as InsertTextContentCommand;
		this.index = last.GetIndex();
		this.text = last.GetText() + this.text;
		return this;
	}

	public override void Undo () {
		Base.Ui.View.Selection.Select(Path, true, false);
		SubtitleEditTextView textView = GetTextView();
		textView.DeleteText(index, index + text.Length);
		PostProcess();
	}

	public override void Redo () {
		Base.Ui.View.Selection.Select(Path, true, false);
		SubtitleEditTextView textView = GetTextView();
		textView.InsertText(index, text);
		PostProcess();
	}

	/* Protected members */

	protected override void PostProcess () {
		Base.Ui.View.RedrawPath(Path);
	}

	protected int GetIndex () {
		return index;
	}

	protected string GetText () {
		return text;
	}

	protected char GetLastChar () {
		return text[text.Length - 1];
	}

}

}
