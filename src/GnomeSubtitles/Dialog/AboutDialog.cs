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

public class AboutDialog : GladeDialog {
	private Gtk.AboutDialog dialog = null;

	/* Constant strings */
	private const string gladeFilename = "AboutDialog.glade";
	private const string logoFilename = "gnome-subtitles-logo.png";


	public AboutDialog () {
		SetHooks();
		Init(gladeFilename, true);

		dialog = GetDialog() as Gtk.AboutDialog;
		SetInfo();
	}

	/* Private members */

	/// <summary>Sets the Url and Email hooks. These must be set before the dialog is realized.</summary>
	private void SetHooks () {
		Gtk.AboutDialog.SetUrlHook(AboutDialogOpenUrl);
		Gtk.AboutDialog.SetEmailHook(AboutDialogOpenEmail);
	}

	private void AboutDialogOpenUrl (Gtk.AboutDialog about, string url) {
		Core.Util.OpenUrl(url);
	}

	private void AboutDialogOpenEmail (Gtk.AboutDialog about, string email) {
		Core.Util.OpenSendEmail(email);
	}

	private void SetInfo () {
		dialog.Version = Base.ExecutionContext.Version;
		dialog.Logo = new Gdk.Pixbuf(null, logoFilename);
	}

}

}
