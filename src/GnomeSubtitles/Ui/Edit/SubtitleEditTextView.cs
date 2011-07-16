/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2009,2011 Pedro Castro
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

using GnomeSubtitles.Core;
using Gtk;
using SubLib.Core.Domain;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace GnomeSubtitles.Ui.Edit {

//FIXME italics, bolds, underlines, change style insted of applying tags
public abstract class SubtitleEditTextView {
	private TextView textView = null;

	private bool isBufferChangeSilent = false; //used to indicate whether a buffer change should set the subtitle text in the subtitle list
	private bool isBufferInsertManual = false; //used to indicate whether there were manual (not by the user) inserts to the buffer
	private bool isBufferDeleteManual = false; //used to indicate whether there were manual (not by the user) inserts to the buffer
	private bool isToggleOverwriteSilent = false; //used to indicate whether an overwrite toggle was manual
	
	/* Text tags */
	private TextTag boldTag = new TextTag("bold");
	private TextTag italicTag = new TextTag("italic");
	private TextTag underlineTag = new TextTag("underline");
	private ArrayList subtitleTags = new ArrayList(4); //4 not to resize container with 3 items added
	
	/* Other */
	private Subtitle subtitle = null;
	private IntPtr spellTextView = IntPtr.Zero;

	public SubtitleEditTextView (TextView textView) {
		this.textView = textView;

		/* Init tags */
    	boldTag.Weight = Pango.Weight.Bold;
    	italicTag.Style = Pango.Style.Italic;
    	underlineTag.Underline = Pango.Underline.Single;

		/* Init text view */
    	this.textView.Buffer.TagTable.Add(boldTag);
    	this.textView.Buffer.TagTable.Add(italicTag);
    	this.textView.Buffer.TagTable.Add(underlineTag);
   		this.textView.ModifyFont(Pango.FontDescription.FromString("sans 14"));

		/* Init margin */
		new SubtitleEditTextViewMargin(this.textView);

		Base.InitFinished += OnBaseInitFinished;
	}
	
	/* Abstract members */
	
	protected abstract SubtitleTextType GetTextType ();
	protected abstract void ChangeSubtitleTextContent (Subtitle subtitle, string text);
	protected abstract string GetSubtitleTextContent (Subtitle subtitle);
	protected abstract void ExecuteInsertCommand (int index, string insertion);
	protected abstract void ExecuteDeleteCommand (int index, string deletion, int cursor);
	protected abstract SpellLanguage GetSpellActiveLanguage ();
	protected abstract void ConnectLanguageChangedSignal ();
	
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
    		if (textView.Buffer.GetSelectionBounds(out start, out end)) //has selection
    			return textView.Buffer.GetText(start, end, false);
    		else
    			return String.Empty;
    	}
    }
	
	/* Public methods */
	
	public void LoadSubtitle (Subtitle subtitle) {
		this.subtitle = subtitle;
		LoadTags(subtitle.Style);
		SetText(GetSubtitleTextContent(subtitle));
	}

	public void InsertText (int startIndex, string text) {
		TextBuffer buffer = textView.Buffer;
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
		TextBuffer buffer = textView.Buffer;
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
		TextIter start = textView.Buffer.GetIterAtOffset(startIndex);
		TextIter end = textView.Buffer.GetIterAtOffset(endIndex);
		textView.Buffer.SelectRange(start, end);		
    }
	
	public void ReplaceSelection (string replacement) {
    	TextBuffer buffer = textView.Buffer;
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
		if (textView.Buffer.GetSelectionBounds(out startIter, out endIter)) { //has selection
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

    /* GtkSpell */
	[DllImport ("libgtkspell")]
	static extern IntPtr gtkspell_new_attach (IntPtr textView, string locale, IntPtr error);

	[DllImport ("libgtkspell")]
	static extern void gtkspell_detach (IntPtr obj);

	[DllImport ("libgtkspell")]
	static extern bool gtkspell_set_language (IntPtr textView, string lang, IntPtr error);
	
	private void GtkSpellDetach () {
		if (IsGtkSpellAttached()) {
			gtkspell_detach(spellTextView);
			spellTextView = IntPtr.Zero;
		}
	}
	
	private void GtkSpellAttach () {
		if (!IsGtkSpellAttached()) {
			spellTextView = gtkspell_new_attach(textView.Handle, null, IntPtr.Zero);
		}
	}
	
	private bool IsGtkSpellAttached () {
		return (spellTextView != IntPtr.Zero);
	}
	
	private bool GtkSpellSetLanguage (SpellLanguage language) {
		if (language == null) {
			if (IsGtkSpellAttached()) {
				GtkSpellDetach();
			}
			return false;
		}
		else {
			if (!IsGtkSpellAttached()) {
				GtkSpellAttach();
			}
			bool result = gtkspell_set_language(spellTextView, language.ID, IntPtr.Zero);
			if (!result)
				GtkSpellDetach();
			
			return result;
		}
	}
	

	/* Private methods */

    private void SetText (string text) {
    	isBufferChangeSilent = true;
    	isBufferInsertManual = true;
    	isBufferDeleteManual = true;
    	
    	textView.Buffer.Text = text;
    	
    	isBufferChangeSilent = false;
    	isBufferInsertManual = false;
    	isBufferDeleteManual = false;
    }

    private void SetTag (TextTag tag, TextIter start, TextIter end, bool activate) {
		if (activate)
			textView.Buffer.ApplyTag(tag, start, end);
		else
			textView.Buffer.RemoveTag(tag, start, end);
    }
    
	private void LoadTags (SubLib.Core.Domain.Style style) {
    	subtitleTags.Clear();
    	/*if (style.Bold)
    		subtitleTags.Add(boldTag);
    	if (style.Italic)
    		subtitleTags.Add(italicTag);
    	if (style.Underline)
    		subtitleTags.Add(underlineTag);*/
    		
    	if (style.Bold)
    		subtitleTags.Add(boldTag);
    	if (style.Italic) {
    		Pango.FontDescription fd = textView.PangoContext.FontDescription.Copy();
    		fd.Style = Pango.Style.Italic;
    		textView.ModifyFont(fd);
    	}
    	if (style.Underline)
    		subtitleTags.Add(underlineTag);
    }
    
    private void ApplyLoadedTags () {
    	TextBuffer buffer = textView.Buffer;
    	TextIter start = buffer.StartIter;
    	TextIter end = buffer.EndIter;
    	foreach (TextTag tag in subtitleTags)
			SetTag(tag, start, end, true);
    }
    
    private TextIter GetIterAtInsertMark () {
    	return textView.Buffer.GetIterAtMark(textView.Buffer.InsertMark);
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
		Core.Base.Ui.Status.Overwrite = textView.Overwrite;
	}
	
	private void PlaceCursor (int index) {
		TextIter iter = textView.Buffer.GetIterAtOffset(index);
		textView.Buffer.PlaceCursor(iter);
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
		textView.GrabFocus();
	}
	
	private ScrolledWindow GetScrolledWindow () {
		return textView.Parent as ScrolledWindow;
	}
	
	
	/* Event members */
	
	/// <summary>Toggles the overwrite status without emitting its event.</summary>
    protected void ToggleOverwriteSilent () {
    	isToggleOverwriteSilent = true;
    	textView.Overwrite = !textView.Overwrite;
    	isToggleOverwriteSilent = false;
    }

	private void OnBufferChanged (object o, EventArgs args) {
		if (!isBufferChangeSilent)
			ChangeSubtitleTextContent(subtitle, textView.Buffer.Text);
		
		ApplyLoadedTags();
		UpdateLineColStatus();
	}
	
	private void OnBufferMarkSet (object o, MarkSetArgs args) {
		UpdateLineColStatus();
	}
	
	[GLib.ConnectBefore]
	private void OnBufferInsertText (object o, InsertTextArgs args) {
		if (!isBufferInsertManual) {
			int index = args.Pos.Offset;
			string insertion = args.Text;
			ExecuteInsertCommand(index, insertion);
		}
		
		ApplyLoadedTags();		
		UpdateLineColStatus();
	}
	
	[GLib.ConnectBefore]
	private void OnBufferDeleteRange (object o, DeleteRangeArgs args) {
		if (!isBufferDeleteManual) {
			int index = args.Start.Offset;
			int length = args.End.Offset - index;
			string deletion = textView.Buffer.Text.Substring(index, length);
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
	
	private void OnSpellToggleEnabled () {
		bool enabled = Base.SpellLanguages.Enabled;
		if (enabled) {
			GtkSpellAttach();
			SpellLanguage language = GetSpellActiveLanguage();
			GtkSpellSetLanguage(language);
		}
		else
			GtkSpellDetach();
	}
	
	private void OnDestroyed (object o, EventArgs args) {
		GtkSpellDetach();
	}
	
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
		textView.Buffer.Changed += OnBufferChanged;
		textView.Buffer.MarkSet += OnBufferMarkSet;
		textView.Buffer.InsertText += OnBufferInsertText;
		textView.Buffer.DeleteRange += OnBufferDeleteRange;
		
		/* TextView signals */
		textView.FocusInEvent += OnFocusIn;
		textView.FocusOutEvent += OnFocusOut;
		textView.KeyPressEvent += OnKeyPressed;
		textView.ToggleOverwrite += OnToggleOverwrite;
		textView.Destroyed += OnDestroyed;
		
		/* Spell signals */
		Base.SpellLanguages.ToggleEnabled += OnSpellToggleEnabled;
		ConnectLanguageChangedSignal();
		
		/* Selection signals */
		Base.Ui.View.Selection.Changed += OnSubtitleSelectionChanged;
    }
    
    private void OnSubtitleSelectionChanged (TreePath[] paths, Subtitle subtitle) {
    	if (subtitle != null)
    		LoadSubtitle(subtitle);
    	else
    		ClearFields();
    }
    
    private void EmitToggleOverwrite () {
    	if (this.ToggleOverwrite != null)
    		this.ToggleOverwrite(this, EventArgs.Empty);
    }
    
    /* Protected members */
    
    protected void OnSpellLanguageChanged () {
		if (Base.SpellLanguages.Enabled) {
			SpellLanguage language = GetSpellActiveLanguage();
			GtkSpellSetLanguage(language);
		}
	}
	
	protected void SetVisibility (bool visible) {
		GetScrolledWindow().Visible = visible;
	}
	
	protected void ClearFields () {
		SetText(String.Empty);	
	}

}

}
