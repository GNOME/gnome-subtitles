/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2018 Pedro Castro
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

namespace GnomeSubtitles.Core {

public class Clipboards {
	private Clipboard clipboard = null;
	private Clipboard primary = null;
	private bool watchPrimaryChanges = false;

	/// <summary>Initializes a new instance of the <see cref="Clipboards" /> class.</summary>
	/// <remarks><see cref="WatchPrimaryChanges"> is set to false at start, it needs to be enabled afterwards.</remarks>
	public Clipboards () {
		clipboard = Clipboard.Get(Gdk.Selection.Clipboard);
		primary = Clipboard.Get(Gdk.Selection.Primary);
	}

	public bool WatchPrimaryChanges {
		set {
			if (value && (!watchPrimaryChanges)) {
				primary.OwnerChange += OnOwnerChange;
				watchPrimaryChanges = true;
			}
			else if ((!value) && watchPrimaryChanges) {
				primary.OwnerChange -= OnOwnerChange;
				watchPrimaryChanges = false;
			}
		}
	}

    public void Copy () {
    	Widget widget = Base.Ui.Window.Focus;
    	if (widget is SpinButton)
    		(widget as SpinButton).CopyClipboard();
    	else if (widget is TextView)
    		(widget as TextView).Buffer.CopyClipboard(clipboard);
    }

    public void Cut () {
		Widget widget = Base.Ui.Window.Focus;
    	if (widget is SpinButton)
    		(widget as SpinButton).CutClipboard();
    	else if (widget is TextView)
    		(widget as TextView).Buffer.CutClipboard(clipboard, true);
    }

    public void Paste () {
		Widget widget = Base.Ui.Window.Focus;
    	if (widget is SpinButton)
    		(widget as SpinButton).PasteClipboard();
    	else if (widget is TextView)
    		(widget as TextView).Buffer.PasteClipboard(clipboard);
    }
    
    private bool ValidWidgetHasFocus () {
    	SpinButton start, end, duration;
    	TextView textEdit, translationEdit;
    	Base.Ui.Edit.GetEditableWidgets (out start, out end, out duration, out textEdit, out translationEdit);
    	return start.HasFocus || end.HasFocus || duration.HasFocus || textEdit.HasFocus || translationEdit.HasFocus;
    }
    
    private bool IsCutCopyAvailable (Clipboard clipboard) {
    	if (!Base.Ui.Window.IsActive) {
    		return false;
		}
		
		string text = clipboard.WaitForText();
		if (String.IsNullOrEmpty(text)) {
			return false;
		}
		
		return ValidWidgetHasFocus();
    }
    
    
    /* Event members */
    
    private void OnOwnerChange (object o, OwnerChangeArgs args) {
		Base.Ui.Menus.SetCutCopySensitivity(IsCutCopyAvailable(o as Clipboard));
    }

}

}
