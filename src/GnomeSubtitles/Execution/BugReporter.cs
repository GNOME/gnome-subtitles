/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007 Pedro Castro
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace GnomeSubtitles {

public class BugReporter {

	public static void Report (Exception exception) {
		Console.Error.WriteLine(exception);
		string bugInfo = GetBugInfo(exception);

		try {
			RunBugBuddy(exception, bugInfo);
		}
		catch (Win32Exception) { //Could not run bug buddy, opening the custom error dialog
			new BugReportWindow(exception, bugInfo);
		}
	}
	
	/* Private members */
	
	private static void RunBugBuddy (Exception exception, string bugInfo) {
		string path = WriteBugInfo(exception, bugInfo);

		Process process = new Process();
		process.StartInfo.FileName = "bug-buddy";
		process.StartInfo.Arguments = "--appname=gnome-subtitles --include=" + path;
		process.StartInfo.UseShellExecute = false;

		process.Start();
	}
	
	private static string GetBugInfo (Exception exception) {
		return "Gnome Subtitles version: " + ExecutionInfo.Version + "\n"
			+ "SubLib version: " + ExecutionInfo.SubLibVersion + "\n"
			+ "GtkSharp version: " + ExecutionInfo.GtkSharpVersion + "\n"
			+ "GnomeSharp version: " + ExecutionInfo.GnomeSharpVersion + "\n"
			+ "GladeSharp version: " + ExecutionInfo.GladeSharpVersion + "\n"
			+ "GConfSharp version: " + ExecutionInfo.GConfSharpVersion + "\n\n"
			+ "Stack trace:" + "\n"
			+ exception.ToString();
	}
	
	private static string WriteBugInfo (Exception exception, string bugInfo) {
		string path = GetTempPath();
		File.WriteAllText(path, bugInfo);
		return path;
	}

	private static string GetTempPath () {
		try {
			string path = Path.GetTempFileName();
			if ((path != null) && (path != String.Empty))
				return path;
		}
		catch (IOException) {
			//Don't do anything, a random name will be chosen next
		}
		
		/* Could not get path in the previous method, trying alternative */
		Random random = new Random();
		int number = random.Next(10000);
		return Path.GetTempPath() + Path.DirectorySeparatorChar + number + ".tmp";
	}

}

}