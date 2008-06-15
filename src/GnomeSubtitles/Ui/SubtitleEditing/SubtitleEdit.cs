/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2008 Pedro Castro
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
using System.Collections;
using SubLib;

namespace GnomeSubtitles.Ui.SubtitleEdit {

public class SubtitleEdit {

	/* Widgets */
	private	HBox hBox = null;
	private SubtitleEditSpinButtons spinButtons = null;
	private SubtitleEditText textEdit = null;
	private SubtitleEditTranslation translationEdit = null;

	/* Other */
	private Subtitle subtitle = null;
	

	public SubtitleEdit() {
		hBox = Global.GetWidget(WidgetNames.SubtitleEdit) as HBox;
		spinButtons = new SubtitleEditSpinButtons();
		textEdit = new SubtitleEditText(Global.GetWidget(WidgetNames.SubtitleEditText) as TextView);
		translationEdit = new SubtitleEditTranslation(Global.GetWidget(WidgetNames.SubtitleEditTranslation) as TextView);
		
		ConnectSignals();
    }


	/* Public properties */

    public bool Enabled {
    	get { return hBox.Sensitive; }
    	set {
			if (hBox.Sensitive == value)
				return;

			if (value == false)
				ClearFields();

			hBox.Sensitive = value;
    	}
    }
    
    public SubtitleEditText TextEdit {
    	get { return textEdit; }
    }
    
    public SubtitleEditTranslation TranslationEdit {
    	get { return translationEdit; }
    }
    
    /// <summary>The current text selection, either text or translation. An empty string if no text is selected.</summary>
    public string SelectedTextContent {
		get {
			string textSelection = textEdit.Selection;
			if (textSelection != String.Empty)
				return textSelection;
			else
				return translationEdit.Selection;
		}
	}
    
    public bool TextOrTranslationIsFocus {
    	get { return textEdit.IsFocus || translationEdit.IsFocus; }
    }
    
    /* Public methods */

    public void GetEditableWidgets (out SpinButton startSpinButton, out SpinButton endSpinButton,
    		out SpinButton durationSpinButton, out TextView textEdit, out TextView translationEdit) {
    	
    	spinButtons.GetWidgets(out startSpinButton, out endSpinButton, out durationSpinButton);
    	textEdit = this.textEdit.TextView;
    	translationEdit = this.translationEdit.TextView;
    }
	
	public void BlankStartUp () {
    	ClearFields();
    }
    
    public void UpdateFromNewDocument (bool wasLoaded) {
		spinButtons.UpdateFromTimingMode(Global.TimingMode, subtitle);
		translationEdit.Visible = false;
	}
	
	public void UpdateFromNewTranslationDocument () {
		if (Enabled)
			translationEdit.LoadSubtitle(subtitle);

    	translationEdit.Visible = true;
    }
    
    public void UpdateFromCloseTranslation () {
    	translationEdit.ClearFields();
    	translationEdit.Visible = false;
    }
	
	public void UpdateFromSelection (Subtitle subtitle) {
	   	this.Enabled = true;
    	this.subtitle = subtitle;
    	spinButtons.LoadTimings(subtitle);
    	textEdit.LoadSubtitle(subtitle);
    	translationEdit.LoadSubtitle(subtitle);
    }

	public void UpdateFromTimingMode (TimingMode mode) {
		spinButtons.UpdateFromTimingMode(mode, subtitle);
	}
	
	public bool GetTextSelectionBounds (out int start, out int end, out SubtitleTextType textType) {
    	if (textEdit.IsFocus) {
    		textType = SubtitleTextType.Text;
    		return textEdit.GetTextSelectionBounds(out start, out end);
    	}
    	else if (translationEdit.IsFocus) {
    		textType = SubtitleTextType.Translation;
    		return translationEdit.GetTextSelectionBounds(out start, out end);
    	}
    	else {
    		textType = SubtitleTextType.Text;
    		start = -1;
    		end = -1;
    		return false;    	
    	}
    }
    
    public void TextFocusOnSelection (int startIndex, int endIndex, SubtitleTextType textType) {
    	if (textType == SubtitleTextType.Text)
    		textEdit.FocusOnSelection(startIndex, endIndex);
    	else
    		translationEdit.FocusOnSelection(startIndex, endIndex);
    }
    
    public void ReplaceSelection (string replacement) {
    	if (textEdit.IsFocus)
    		textEdit.ReplaceSelection(replacement);
    	else if (translationEdit.IsFocus)
    		translationEdit.ReplaceSelection(replacement);
    }


    /* Private Methods */

    private void ClearFields () {
    	spinButtons.ClearFields();
    	textEdit.ClearFields();
    	translationEdit.ClearFields();
    }


    /* Event methods */
    
    private void OnTextViewToggleOverwrite (object o, EventArgs args) {
    	/* Reflect the update to the other respective TextView */
    	if (o == textEdit)
    		translationEdit.ToggleOverwriteSilent();
    	else
    		textEdit.ToggleOverwriteSilent();
    }

    private void ConnectSignals () {
    	textEdit.ToggleOverwrite += OnTextViewToggleOverwrite;
    	translationEdit.ToggleOverwrite += OnTextViewToggleOverwrite;
    }

}

}
