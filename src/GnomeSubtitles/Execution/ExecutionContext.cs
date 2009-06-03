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

using Gtk;
using System;
using System.IO;
using System.Reflection;

namespace GnomeSubtitles.Execution {

public class ExecutionContext {
	private bool initialized = false;
	private bool running = false;

	/* Constant strings */
	private const string applicationName = "Gnome Subtitles";
	private const string applicationID = "gnome-subtitles";
	
	private string[] args = null;
	
	public ExecutionContext (string[] args) {
		this.args = args;
	}
	
	/* Public properties */
	
	public bool Initialized {
		get { return initialized; }
	}
	
	public string ApplicationName {
		get { return applicationName; }
	}
	
	public string ApplicationID {
		get { return applicationID; }
	}
	
	public string ExecutableName {
		get { return applicationID; }
	}
	
	public string Version {
		get { return RemoveTrailingZeros(Assembly.GetExecutingAssembly().GetName().Version.ToString()); }
	}

	public string GtkSharpVersion {
		get { return RemoveTrailingZeros(Assembly.Load("gtk-sharp").GetName().Version.ToString()); }
	}

	public string GladeSharpVersion {
		get { return RemoveTrailingZeros(Assembly.Load("glade-sharp").GetName().Version.ToString()); }
	}
	
	public string GConfSharpVersion {
		get { return RemoveTrailingZeros(Assembly.Load("gconf-sharp").GetName().Version.ToString()); }
	}

	public string[] Args {
		get { return args; }
		set { args = value; }
	}
	
	public string TranslationDomain {
		get { return applicationID; }
	}
	

	/* Public methods */
	
	public void InitApplication () {
		if (!initialized) {	
			initialized = true;
			Application.Init();
		}
	}
	
	public void RunApplication () {
		if (initialized && (!running)) {
			running = true;
			Application.Run();
		}
	}
	
	public void QuitApplication () {
		initialized = false;
		running = false;
		Application.Quit();
	}
	

	/* Private methods */
	
	private string RemoveTrailingZeros (string version) {
		while (version.EndsWith(".0")) {
			version = version.Remove(version.Length - 2);
		}
		return version;
	}

}

}
