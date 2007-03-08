/*
 * This file is part of Gnome Subtitles.
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
using System.Reflection;

namespace GnomeSubtitles {

public class ExecutionInfo {
	private const string applicationName = "Gnome Subtitles";
	private const string applicationID = "GnomeSubtitles";
	private const string executableName = "gnome-subtitles";
	private const string subLibAssemblyName = "sublib";
	
	private static string[] args = null;
	
	
	public static string ApplicationName {
		get { return applicationName; }
	}
	
	public static string ApplicationID {
		get { return applicationID; }
	}
	
	public static string ExecutableName {
		get { return executableName; }
	}
	
	public static string Version {
		get { return RemoveTrailingZeros(Assembly.GetExecutingAssembly().GetName().Version.ToString()); }
	}

	public static string SubLibVersion {
		get { return RemoveTrailingZeros(Assembly.ReflectionOnlyLoad(subLibAssemblyName).GetName().Version.ToString()); }
	}
	
	public static string GnomeSharpVersion {
		get { return RemoveTrailingZeros(Assembly.Load("gnome-sharp").GetName().Version.ToString()); }
	}

	public static string GtkSharpVersion {
		get { return RemoveTrailingZeros(Assembly.Load("gtk-sharp").GetName().Version.ToString()); }
	}

	public static string GladeSharpVersion {
		get { return RemoveTrailingZeros(Assembly.Load("glade-sharp").GetName().Version.ToString()); }
	}

	public static string[] Args {
		get { return args; }
		set { args = value; }
	}
	
	public static string RemoveTrailingZeros (string version) {
		while (version.EndsWith(".0")) {
			version = version.Remove(version.Length - 2);
		}
		return version;
	}
	
}

}
