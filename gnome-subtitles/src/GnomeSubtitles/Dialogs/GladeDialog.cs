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

using System;
using Gtk;

namespace GnomeSubtitles {

public class GladeDialog {
	private Glade.XML glade = null;
	
	/* protected variables */
	protected Dialog dialog = null;
	protected bool actionDone = false;
	

	/// <summary>Initializes a new instance of the <see cref="GladeDialog" /> class.</summary>
	/// <remarks>The dialog isn't initialized. A call to <see cref="Init" /> is required to initialize this class.
	/// This is useful if one needs to do some operations before creating the dialog.</remarks>
	protected GladeDialog () {
	}	

	/// <summary>Initializes a new instance of the <see cref="GladeDialog" /> class, given the name of the dialog.</summary>
	/// <param name="dialogName">The name of the dialog.</param>
	protected GladeDialog (string dialogName) {
		Init(dialogName);
	}
	
	public bool WaitForResponse () {
		dialog.Run();
		return actionDone;
	}
	
	/* Protected members */
	
	protected void CloseDialog() {
		dialog.Destroy();
	}
	
	/// <summary>Constructs the dialog from the glade master file, autoconnects the handlers,
	/// sets the icon and sets the dialog as transient for the main window.</summary>
	/// <param name="dialogName">The name of the dialog.</param>
	protected void Init (string dialogName) {
		glade = new Glade.XML(ExecutionInfo.GladeMasterFileName, dialogName);
		glade.Autoconnect(this);
		dialog = glade.GetWidget(dialogName) as Dialog;
		
		Window window = Global.GUI.Window;
		dialog.TransientFor = window;
		dialog.Icon = window.Icon;
	}

}

}