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
using System;
using SubLib;

namespace GnomeSubtitles {

public class Clipboards {
	private GUI gui = null;
	
	private Clipboard clipboard = null;
	private Clipboard primary = null;
	
	public Clipboards (GUI gui) {
		this.gui = gui;
		
		clipboard = Gtk.Clipboard.Get(Gdk.Selection.Clipboard);
		primary = Gtk.Clipboard.Get(Gdk.Selection.Primary);
		primary.OwnerChange += OnOwnerChange;
	}

	public void OnOwnerChange (object o, OwnerChangeArgs args) {
    	Window window = gui.Window;
    	if ((!window.IsActive) || (args.Event.Owner == 0) || (!ValidWidgetHasFocus()))
    		gui.Menus.SetCopyCutSensitivity(false);
    	else {
    		gui.Menus.SetCopyCutSensitivity(true);    	
    	}
    }
    
    public bool ValidWidgetHasFocus () {
    	SpinButton start, end, duration;
    	TextView textView;
    	gui.SubtitleEdit.GetEditableWidgets (out start, out end, out duration, out textView);
    	return start.HasFocus || end.HasFocus || duration.HasFocus || textView.HasFocus;
    }
    
    public void Copy () {
    	Widget widget = gui.Window.Focus;
    	if (widget is SpinButton)
    		(widget as SpinButton).CopyClipboard();
    	else if (widget is TextView)
    		(widget as TextView).Buffer.CopyClipboard(clipboard);
    }
    
    public void Cut () {
		Widget widget = gui.Window.Focus;
    	if (widget is SpinButton)
    		(widget as SpinButton).CutClipboard();
    	else if (widget is TextView)
    		(widget as TextView).Buffer.CutClipboard(clipboard, true);
    }
    
    public void Paste () {
		Widget widget = gui.Window.Focus;
    	if (widget is SpinButton)
    		(widget as SpinButton).PasteClipboard();
    	else if (widget is TextView)
    		(widget as TextView).Buffer.PasteClipboard(clipboard);
    }

}

}