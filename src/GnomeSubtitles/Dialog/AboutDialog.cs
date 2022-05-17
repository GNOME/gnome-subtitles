/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2022 Pedro Castro
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

using Mono.Unix;
using GnomeSubtitles.Core;
using Gtk;

namespace GnomeSubtitles.Dialog {

public class AboutDialog : BaseDialog {

	private const string LogoFilename = "gnome-subtitles-logo.png";

	public AboutDialog () {
		Init(BuildDialog());
	}

	/* Private members */

	private Gtk.AboutDialog BuildDialog () {
		Gtk.AboutDialog dialog = new Gtk.AboutDialog();

		dialog.Modal = true;

		dialog.Title = Catalog.GetString("About Gnome Subtitles");
		dialog.ProgramName = Base.ExecutionContext.ApplicationName;
		dialog.Version = Base.ExecutionContext.Version;
		dialog.Logo = new Gdk.Pixbuf(null, LogoFilename);
		dialog.Website = "https://gnomesubtitles.org";
		dialog.WebsiteLabel = dialog.Website;
		dialog.Comments = Catalog.GetString("Video subtitling for the GNOME desktop");
		dialog.Copyright = Base.ExecutionContext.Copyright;
		dialog.LicenseType = License.Gpl20;

		dialog.Authors = new string[]{
			"Pedro Castro"
		};

		dialog.Artists = new string[]{
			"Stefan A. Keel (Sak)"
		};

		return dialog;
	}

}

}