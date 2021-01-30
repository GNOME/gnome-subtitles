/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2021 Pedro Castro
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

using External.GtkSpell;
using GnomeSubtitles.Core;
using Gtk;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Ui.Edit {

public abstract class SubtitleEditTextView {
	private TextView textView = null;
	private SpellChecker spellChecker = null;

	private bool isBufferChangeSilent = false; //used to indicate whether a buffer change should set the subtitle text in the subtitle list
	private bool isBufferInsertManual = false; //used to indicate whether there were manual (not by the user) inserts to the buffer
	private bool isBufferDeleteManual = false; //used to indicate whether there were manual (not by the user) inserts to the buffer
	private bool isToggleOverwriteSilent = false; //used to indicate whether an overwrite toggle was manual

	/* Constants */
	private String textFont = "sans";
	private int textFontSize = 14;

	/* Text tags */
	private TextTag underlineTag = new TextTag("underline");

	/* Other */
	private Subtitle subtitle = null;

	public SubtitleEditTextView (TextView textView) {
		this.textView = textView;

		/* Init tags */
    	underlineTag.Underline = Pango.Underline.Single;
    	this.textView.Buffer.TagTable.Add(underlineTag);

		/* Init margin */
		new SubtitleEditTextViewMargin(this.textView);
		
		this.spellChecker = new SpellChecker(textView);

		Base.InitFinished += OnBaseInitFinished;
	}

	/* Abstract members */

	protected abstract SubtitleTextType GetTextType ();
	protected abstract void ChangeSubtitleTextContent (Subtitle subtitle, string text);
	protected abstract string GetSubtitleTextContent (Subtitle subtitle);
	protected abstract void ExecuteInsertCommand (int index, string insertion);
	protected abstract void ExecuteDeleteCommand (int index, string deletion, int cursor);
	protected abstract SpellLanguage GetSpellActiveLanguage ();

	/* Events */
	public event EventHandler ToggleOverwrite = null;

	/* Public properties */

	public TextView TextView {
		get { return textView; }
	}

	public bool Enabled {
		get { return textView.Sensitive; }
	}

    public bool IsFocus {
    	get { return textView.IsFocus; }
    }

    public bool OverwriteStatus {
    	get { return textView.Overwrite; }
    }

	/// <summary>The text that is currently selected, or <see cref="Selection.Empty" /> if no text is selected.</summary>
    public string Selection {
    	get {
    		if (!this.Enabled)
    			return String.Empty;

    		TextIter start, end;
    		if (this.textView.Buffer.GetSelectionBounds(out start, out end)) //has selection
    			return this.textView.Buffer.GetText(start, end, false);
    		else
    			return String.Empty;
    	}
    }

	/* Public methods */

	public void LoadSubtitle (Subtitle subtitle) {
		this.subtitle = subtitle;

		SetFont(subtitle.Style);
		SetText(GetSubtitleTextContent(subtitle));
		ApplyTags();
	}

	public void InsertText (int startIndex, string text) {
		TextBuffer buffer = this.textView.Buffer;
		isBufferInsertManual = true;

		buffer.BeginUserAction();
		GrabFocus();
		PlaceCursor(startIndex);
		buffer.InsertAtCursor(text);
		FocusOnSelection(startIndex, startIndex + text.Length);
    	buffer.EndUserAction();

    	isBufferInsertManual = false;
	}

	public void DeleteText (int startIndex, int endIndex) {
		TextBuffer buffer = this.textView.Buffer;
		isBufferDeleteManual = true;

		buffer.BeginUserAction();
		GrabFocus();
		TextIter start = buffer.GetIterAtOffset(startIndex);
		TextIter end = buffer.GetIterAtOffset(endIndex);
		buffer.Delete(ref start, ref end);
		buffer.EndUserAction();

		isBufferDeleteManual = false;
	}

	public void FocusOnSelection (int startIndex, int endIndex) {
    	GrabFocus();
		TextIter start = this.textView.Buffer.GetIterAtOffset(startIndex);
		TextIter end = this.textView.Buffer.GetIterAtOffset(endIndex);
		this.textView.Buffer.SelectRange(start, end);
    }

	public void ReplaceSelection (string replacement) {
    	TextBuffer buffer = this.textView.Buffer;
    	buffer.BeginUserAction();
    	buffer.DeleteSelection(true, true);
    	buffer.InsertAtCursor(replacement);
    	buffer.EndUserAction();
    }

    /// <summary>Gets the bounds of the current selection, if text is selected.</summary>
	/// <param name="start">The start index of the selection.</param>
	/// <param name="end">The end index of the selection.</param>
	/// <remarks>If no text is selected, both start and end will contain the index of the cursor position.</remarks>
	/// <returns>Whether text was selected.</returns>
	public bool GetTextSelectionBounds (out int start, out int end) {
		TextIter startIter, endIter;
		if (this.textView.Buffer.GetSelectionBounds(out startIter, out endIter)) { //has selection
			start = startIter.Offset;
			end = endIter.Offset;
			return true;
		}
		else {
			int cursorIndex = GetCursorIndex();
			start = cursorIndex;
			end = cursorIndex;
			return false;
		}
	}

		
	/* Private methods */

    private void SetText (string text) {
    	isBufferChangeSilent = true;
    	isBufferInsertManual = true;
    	isBufferDeleteManual = true;

    	this.textView.Buffer.Text = text;

    	isBufferChangeSilent = false;
    	isBufferInsertManual = false;
    	isBufferDeleteManual = false;
    }

    /// <summary>Sets font with bold and italic if applicable.</summary>
    private void SetFont (SubLib.Core.Domain.Style style) {
    	Pango.FontDescription font = Pango.FontDescription.FromString(this.textFont + (style.Bold ? " bold" : String.Empty) + (style.Italic ? " italic" : String.Empty) + " " + this.textFontSize);
		this.textView.OverrideFont(font);
    }

    /* Currently applies underline tag */
    private void ApplyTags () {
    	if (this.subtitle.Style.Underline) {
    		ApplyTag(this.underlineTag, this.textView.Buffer.StartIter, this.textView.Buffer.EndIter, true);
    	}
    }

    private void ApplyTag (TextTag tag, TextIter start, TextIter end, bool activate) {
		if (activate)
			this.textView.Buffer.ApplyTag(tag, start, end);
		else
			this.textView.Buffer.RemoveTag(tag, start, end);
    }

    private TextIter GetIterAtInsertMark () {
    	return this.textView.Buffer.GetIterAtMark(this.textView.Buffer.InsertMark);
    }

    private void GetLineColumn (out int line, out int column) {
    	TextIter iter = GetIterAtInsertMark();
		line = iter.Line + 1;
		column = iter.LineOffset + 1;
    }

	private void UpdateLineColStatus () {
    	if ((!Enabled) || (!IsFocus))
    		return;

		/* Get the cursor position */
		int line, column;
		GetLineColumn(out line, out column);

		/* Update the status bar */
		Core.Base.Ui.Status.SetPosition(GetTextType(), line, column);
	}

	private void UpdateOverwriteStatus () {
		Core.Base.Ui.Status.Overwrite = this.textView.Overwrite;
	}

	private void PlaceCursor (int index) {
		TextIter iter = this.textView.Buffer.GetIterAtOffset(index);
		this.textView.Buffer.PlaceCursor(iter);
	}

	/// <summary>Returns the cursor index, or -1 if the text view is not enabled.</summary>
    private int GetCursorIndex () {
    	if (!this.Enabled)
    		return -1;
    	else {
    		TextIter iter = GetIterAtInsertMark();
    		return iter.Offset;
    	}
    }

	private void GrabFocus () {
		this.textView.GrabFocus();
	}

	private ScrolledWindow GetScrolledWindow () {
		return this.textView.Parent as ScrolledWindow;
	}


	/* Event members */

	/// <summary>Toggles the overwrite status without emitting its event.</summary>
    protected void ToggleOverwriteSilent () {
    	isToggleOverwriteSilent = true;
    	this.textView.Overwrite = !this.textView.Overwrite;
    	isToggleOverwriteSilent = false;
    }

	private void OnBufferChanged (object o, EventArgs args) {
		if (!isBufferChangeSilent)
			ChangeSubtitleTextContent(subtitle, this.textView.Buffer.Text);

		ApplyTags();
		UpdateLineColStatus();
	}

	private void OnBufferMarkSet (object o, MarkSetArgs args) {
		UpdateLineColStatus();
	}

	[GLib.ConnectBefore]
	private void OnBufferInsertText (object o, InsertTextArgs args) {
		if (!isBufferInsertManual) {
			int index = args.Pos.Offset;
			string insertion = args.NewText;
			ExecuteInsertCommand(index, insertion);
		}

		ApplyTags();
		UpdateLineColStatus();
	}

	[GLib.ConnectBefore]
	private void OnBufferDeleteRange (object o, DeleteRangeArgs args) {
		if (!isBufferDeleteManual) {
			int index = args.Start.Offset;
			int length = args.End.Offset - index;
			string deletion = this.textView.Buffer.Text.Substring(index, length);
			ExecuteDeleteCommand(index, deletion, GetCursorIndex());
		}
	}

    private void OnFocusIn (object o, FocusInEventArgs args) {
    	UpdateLineColStatus();
		UpdateOverwriteStatus();

		Core.Base.Ui.Menus.SetPasteSensitivity(true);
	}

	private void OnFocusOut (object o, FocusOutEventArgs args) {
		Core.Base.Ui.Menus.SetPasteSensitivity(false);
    	Core.Base.Ui.Status.ClearEditRelatedStatus();
	}

	private void OnToggleOverwrite (object o, EventArgs args) {
		/* Update the GUI overwrite status */
    	UpdateOverwriteStatus();

		/* Emit the toggle event */
		if (!isToggleOverwriteSilent)
			EmitToggleOverwrite();
	}

	//private void OnDestroy (object o, EventArgs args) {
	//	spellChecker.Disable();
	//}

	[GLib.ConnectBefore]
    private void OnKeyPressed (object o, KeyPressEventArgs args) {
    	Gdk.Key key = args.Event.Key;
    	Gdk.ModifierType modifier = args.Event.State;
    	Gdk.ModifierType controlModifier = Gdk.ModifierType.ControlMask;

    	if ((modifier & controlModifier) == controlModifier) { //Control was pressed
    		switch (key) {
    			case Gdk.Key.Page_Up:
    				Core.Base.Ui.View.Selection.SelectPrevious();
					GrabFocus();
    				args.RetVal = true;
    				break;
    			case Gdk.Key.Page_Down:
					Core.Base.Ui.View.Selection.SelectNext();
					GrabFocus();
    				args.RetVal = true;
    				break;
    		}
    	}
    }

    private void OnBaseInitFinished () {

		/* Buffer signals */
		this.textView.Buffer.Changed += OnBufferChanged;
		this.textView.Buffer.MarkSet += OnBufferMarkSet;
		this.textView.Buffer.InsertText += OnBufferInsertText;
		this.textView.Buffer.DeleteRange += OnBufferDeleteRange;

		/* TextView signals */
		this.textView.FocusInEvent += OnFocusIn;
		this.textView.FocusOutEvent += OnFocusOut;
		this.textView.KeyPressEvent += OnKeyPressed;
		this.textView.ToggleOverwrite += OnToggleOverwrite;
		//this.textView.Destroyed += OnDestroy;
    }

    private void EmitToggleOverwrite () {
    	if (this.ToggleOverwrite != null)
    		this.ToggleOverwrite(this, EventArgs.Empty);
    }

    /* Protected members */
    
    protected void SetSpellLanguage () {
		if (Base.SpellLanguages.Enabled) {
			SpellLanguage language = GetSpellActiveLanguage();
			if (language != null) {
				spellChecker.Enable(language.ID);
				return;
			}
		}
		
		spellChecker.Disable();
	}

    protected void OnSpellLanguageChanged () {
    	SetSpellLanguage();
	}

	protected void OnSpellEnabledToggled () {
		SetSpellLanguage();
	}

	protected void OnSubtitleSelectionChanged (TreePath[] paths, Subtitle subtitle) {
		if (subtitle != null)
    		LoadSubtitle(subtitle);
    	else
    		ClearFields();
    }

	protected void SetVisibility (bool visible) {
		GetScrolledWindow().Visible = visible;
		if (!visible) {
			ClearFields();
		}
	}

	protected void ClearFields () {
		SetText(String.Empty);
	}

}

}