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

using GnomeSubtitles.Core;
using Gtk;
using System;

namespace GnomeSubtitles.Dialog {

public abstract class GladeDialog : BaseDialog {
	private Glade.XML glade = null;

	/// <summary>Creates a new instance of the <see cref="GladeDialog" /> class.</summary>
	/// <remarks>The dialog isn't initialized. A call to <see cref="Init" /> is required to initialize this class.
	/// This is useful if one needs to do some operations before creating the dialog.</remarks>
	protected GladeDialog () {
	}

	/// <summary>Creates a new instance of the <see cref="GladeDialog" /> class, given the filename of the dialog.</summary>
	/// <param name="filename">The filename of the dialog.</param>
	protected GladeDialog (string filename) : this(filename, true) {
	}

	protected GladeDialog (string filename, bool autoconnect) {
		Init(filename, autoconnect);
	}

	/* Protected members */

	/// <summary>Constructs the dialog with the specified filename, and possibly sets it as persistent.</param>
	/// <param name="filename">The filename of the dialog.</param>
	/// <param name="autoconnect">Whether to autoconnect the event handlers.</param>
	/// <remarks>Constructing creates the dialog from its filename, autoconnects the handlers,
	/// sets the icon and also sets the dialog as transient for the main window.</summary>
	protected void Init (string filename, bool autoconnect) {
		glade = new Glade.XML(null, filename, null, Base.ExecutionContext.TranslationDomain);

		if (autoconnect)
			Autoconnect();
		else
			glade.BindFields(this);

		base.Init(glade.GetWidget("dialog") as Gtk.Dialog);
	}

	protected void Autoconnect () {
		glade.Autoconnect(this);
	}

}

}
