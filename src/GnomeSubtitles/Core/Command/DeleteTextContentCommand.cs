/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2019 Pedro Castro
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

public abstract class DeleteTextContentCommand : FixedSingleSelectionCommand {
	private int index; //The start offset of the deleted text
	private string text; //The deleted text
	private bool toRight; //Whether the direction of the deletion was from the left to the right (or the contrary)

	public DeleteTextContentCommand (string description, int index, string text, int cursor) : base(description, true, false) {
		this.index = index;
		this.text = text;
		toRight = (cursor == index);

		/* If text isn't a single char, do not group */
		if (text.Length != 1)
			SetStopsGrouping(true);
	}

	/* Abstract members */
	protected abstract SubtitleEditTextView GetTextView ();

	/* Public members */

	public override bool CanGroupWith (Command command) {
		DeleteTextContentCommand last = command as DeleteTextContentCommand;
		return (this.Path.Compare(last.Path) == 0) //Paths are equal
			&& (this.text.Length == 1) //A single char was deleted
			&& (this.toRight == last.GetToRight()) //Directions are equal
			&& ((this.toRight && (this.index == last.GetIndex())) //Deletion is a continuation of the last deletion
				|| ((!this.toRight) && (this.index == last.GetIndex() - 1)))
			&& ((this.toRight && (!(Char.IsWhiteSpace(last.GetLastChar()) && (!Char.IsWhiteSpace(GetLastChar()))))) //A space->non-space sequence is not the case
				|| ((!this.toRight) && (!(Char.IsWhiteSpace(last.GetFirstChar()) && (!Char.IsWhiteSpace(GetLastChar()))))));
	}

	public override Command MergeWith (Command command) {
		DeleteTextContentCommand last = command as DeleteTextContentCommand;
		if (this.toRight)
			this.text = last.GetText() + this.text;
		else
			this.text = this.text + last.GetText();

		return this;
	}

	public override void Undo () {
		Base.Ui.View.Selection.Select(Path, true, false);
		SubtitleEditTextView textView = GetTextView();
		textView.InsertText(index, text);
		PostProcess();
	}

	public override void Redo () {
		Base.Ui.View.Selection.Select(Path, true, false);
		SubtitleEditTextView textView = GetTextView();
		textView.DeleteText(index, index + text.Length);
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

	protected bool GetToRight () {
		return toRight;
	}

	protected char GetFirstChar () {
		return text[0];
	}

	protected char GetLastChar () {
		return text[text.Length - 1];
	}

}

}
