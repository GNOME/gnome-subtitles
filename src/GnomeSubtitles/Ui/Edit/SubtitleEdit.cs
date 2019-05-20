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

using GnomeSubtitles.Core;
using Gtk;
using System;
using SubLib.Core.Domain;

namespace GnomeSubtitles.Ui.Edit {

public class SubtitleEdit {

	/* Widgets */
	private	Box hBox = null;
	private SubtitleEditSpinButtons spinButtons = null;
	private SubtitleEditText textEdit = null;
	private SubtitleEditTranslation translationEdit = null;


	public SubtitleEdit() {
		hBox = Base.GetWidget(WidgetNames.SubtitleEdit) as Box;
		spinButtons = new SubtitleEditSpinButtons();
		textEdit = new SubtitleEditText(Base.GetWidget(WidgetNames.SubtitleEditText) as TextView);
		translationEdit = new SubtitleEditTranslation(Base.GetWidget(WidgetNames.SubtitleEditTranslation) as TextView);

		Base.InitFinished += OnBaseInitFinished;
    }


	/* Public properties */

    public bool Enabled {
    	get { return hBox.Sensitive; }
    	set {
			if (hBox.Sensitive != value)
				hBox.Sensitive = value;
    	}
    }

    public SubtitleEditText TextEdit {
    	get { return textEdit; }
    }

    public SubtitleEditTranslation TranslationEdit {
    	get { return translationEdit; }
    }

    public SubtitleEditSpinButtons SpinButtons {
    	get { return spinButtons; }
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


    /* Event members */

    private void OnBaseInitFinished () {
    	Base.Ui.View.Selection.Changed += OnSubtitleSelectionChanged;
    }

    private void OnSubtitleSelectionChanged (TreePath[] paths, Subtitle subtitle) {
		this.Enabled = (subtitle != null);
	}

}

}
