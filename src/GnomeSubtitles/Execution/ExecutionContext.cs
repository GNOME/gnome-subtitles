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

using System;
using System.Reflection;

namespace GnomeSubtitles.Execution {

public class ExecutionContext {
	private Gtk.Application app = null;
	
	//Hack because gtk# doesn't support this flag yet (as of 2019). Ref: glib/gio/gioenums.h (GApplicationFlags.G_APPLICATION_NON_UNIQUE)
	private const GLib.ApplicationFlags GLibApplicationFlagsNonUnique = (GLib.ApplicationFlags)(1 << 5);

	/* Constant strings */
	private const string applicationName = "Gnome Subtitles";
	private const string applicationID = "org.gnome.GnomeSubtitles";
	private const string iconName = "gnome-subtitles";
	private const string executableName = "gnome-subtitles";

	/* Dynamic variables */
	private string[] args = null;

	public ExecutionContext (string[] args) {
		this.args = args;
	}
	

	/* Public properties */

	public string ApplicationName {
		get { return applicationName; }
	}

	public string ApplicationID {
		get { return applicationID; }
	}
	
	public string IconName {
		get { return iconName; }
	}

	public string ExecutableName {
		get { return executableName; }
	}

	//Unix only
	public string LocaleDir {
		get { return AppDomain.CurrentDomain.BaseDirectory + "../../share/locale"; }
	}

	public string Version {
		get { return RemoveTrailingZeros(Assembly.GetExecutingAssembly().GetName().Version.ToString()); }
	}
	
	public string Copyright {
		get { return ((AssemblyCopyrightAttribute)Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyCopyrightAttribute))).Copyright; }
	}

	public string GtkSharpVersion {
		get { return RemoveTrailingZeros(Assembly.Load("gtk-sharp").GetName().Version.ToString()); }
	}

	public string[] Args {
		get { return args; }
		set { args = value; }
	}

	public string TranslationDomain {
		get { return applicationID; }
	}
	
	public Gtk.Application Application {
		get { return app; }
	}


	/* Public methods */

	public void Execute (Action methodToExecute) {
		GLib.Global.ApplicationName = applicationName;
		
		app = new Gtk.Application(applicationID, GLibApplicationFlagsNonUnique);
		app.Activated += (sender, e) => {
			methodToExecute();
		};
		
		app.Run(applicationID, new string[]{ });
	}
	

	/* Private methods */

	private string RemoveTrailingZeros (string version) {
		while (version.EndsWith(".0")) {
			version = version.Remove(version.Length - 2);
		}
		if (!version.Contains(".")) {
			version += ".0";
		}
		return version;
	}

}

}