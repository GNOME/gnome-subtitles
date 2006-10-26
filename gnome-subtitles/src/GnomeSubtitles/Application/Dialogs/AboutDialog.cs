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

	public AboutDialog (GUI gui) : base(gui, WidgetNames.AboutDialog, PreWidgetCreation) {
		dialog = base.dialog as Gtk.AboutDialog;
		
		SetInformation();
	}
	
	/* Private members */
	
		
	private void SetInformation () {
		dialog.Name = "Gnome Subtitles";
		dialog.Comments += "\n\nUsing SubLib " + ExecutionInfo.SubLibVersion;
		dialog.Version = ExecutionInfo.Version;
	}
	
	private static void PreWidgetCreation () {
		Gtk.AboutDialog.SetUrlHook(AboutDialogOpenUrl);
		Gtk.AboutDialog.SetEmailHook(AboutDialogOpenEmail);
	}
	
	private static void AboutDialogOpenUrl (Gtk.AboutDialog about, string url) {
		Utility.OpenUrl(url);
	}
	
	private static void AboutDialogOpenEmail (Gtk.AboutDialog about, string email) {
		Utility.OpenSendEmail(email);
	}

}

}