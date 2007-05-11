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

using Gnome;
using System;
using System.IO;
using System.Reflection;

namespace GnomeSubtitles {

public class Execution {
	private Program program = null;
	private bool initialized = false;

	/* Constant strings */
	private const string applicationName = "Gnome Subtitles";
	private const string applicationID = "gnome-subtitles";
	private const string subLibAssemblyName = "sublib";
	
	private string[] args = null;
	
	public Execution (string[] args) {
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

	public string SubLibVersion {
		get { return RemoveTrailingZeros(Assembly.ReflectionOnlyLoad(subLibAssemblyName).GetName().Version.ToString()); }
	}
	
	public string GtkSharpVersion {
		get { return RemoveTrailingZeros(Assembly.Load("gtk-sharp").GetName().Version.ToString()); }
	}
	
	public string GnomeSharpVersion {
		get { return RemoveTrailingZeros(Assembly.Load("gnome-sharp").GetName().Version.ToString()); }
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
	
	public string SystemShareDir {
		get { return program.GnomeDatadir; }
	}
	
	public string SystemShareLocaleDir {
		get { return Path.Combine(SystemShareDir, "locale"); }
	}
	
	public string SystemHelpDir {
		get {
			return Path.Combine(SystemShareDir,
				Path.Combine("gnome",
				Path.Combine("help", applicationID)));
		}
	}
	
	public string TranslationDomain {
		get { return applicationID; }
	}
	
	/* Public methods */
	
	public void Init () {
		program = new Program(applicationID, Version, Gnome.Modules.UI, args);
		initialized = true;
	}
	
	public void RunProgram () {
		program.Run();
	}
	
	public void QuitProgram () {
		program.Quit();
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
