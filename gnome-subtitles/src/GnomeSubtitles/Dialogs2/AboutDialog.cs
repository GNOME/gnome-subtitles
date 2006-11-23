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

namespace GnomeSubtitles {

public class AboutDialog : GladeDialog {
	private new Gtk.AboutDialog dialog = null;

	/* Constant strings */
	private const string dialogName = "aboutDialog";
	

	public AboutDialog () {
		SetHooks();
		Init(dialogName);
	
		dialog = base.dialog as Gtk.AboutDialog;
		SetInfo();
	}
	
	/* Private members */
	
	/// <summary>Sets the Url and Email hooks. These must be set before the dialog is realized.</summary>
	private void SetHooks () {
		Gtk.AboutDialog.SetUrlHook(AboutDialogOpenUrl);
		Gtk.AboutDialog.SetEmailHook(AboutDialogOpenEmail);
	}
	
	private void AboutDialogOpenUrl (Gtk.AboutDialog about, string url) {
		Util.OpenUrl(url);
	}
	
	private void AboutDialogOpenEmail (Gtk.AboutDialog about, string email) {
		Util.OpenSendEmail(email);
	}
		
	private void SetInfo () {
		dialog.Name = "Gnome Subtitles";
		dialog.Comments += "\n\nUsing SubLib " + ExecutionInfo.SubLibVersion;
		dialog.Version = ExecutionInfo.Version;
		dialog.Logo = Global.GUI.Window.Icon;
	}

}

}