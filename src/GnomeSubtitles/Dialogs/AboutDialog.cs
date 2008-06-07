/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2007 Pedro Castro
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

namespace GnomeSubtitles {

public class AboutDialog : GladeDialog {
	private new Gtk.AboutDialog dialog = null;

	/* Constant strings */
	private const string gladeFilename = "AboutDialog.glade";
	private const string logoFilename = "gnome-subtitles-logo.png";
	

	public AboutDialog () {
		SetHooks();
		Init(gladeFilename);
	
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
		dialog.Name = "Gnome Subtitles"; //TODO Deprecated property, need to change to ProgramName. Scheduled for substitution when gtk# 2.12 is available in all major distros. 
		dialog.Comments += "\n\nUsing SubLib " + Global.Execution.SubLibVersion;
		dialog.Version = Global.Execution.Version;
		dialog.Logo = new Gdk.Pixbuf(null, logoFilename);
	}

	/* Event members */

	#pragma warning disable 169             //Disables warning about handlers not being used

	private void OnResponse (object o, ResponseArgs args) {
		switch (args.ResponseId) {
			case ResponseType.Close:
				Close();
				break;
			case ResponseType.Cancel:
				Close();
				break;
		}
	}

}

}
