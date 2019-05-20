/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2018 Pedro Castro
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
//using GnomeSubtitles.Dialog.Message;
//using Mono.Unix;
using SubLib.Util;
using System;
//using System.Diagnostics;
using System.IO;


namespace GnomeSubtitles.Execution {

public class BugReporter {
	//private static string noBugBuddyPrimaryMessage = Catalog.GetString("Could not open Bug Buddy, the bug reporting tool.");
	//private static string noBugBuddySecondaryMessage = Catalog.GetString("Bug information has been printed to the console.");


	//We don't run BugBuddy anymore. Only output some context information to the console.
	//The exception will be thrown afterwards and printed to the console anyway.
	public static void Report (Exception exception) {
		string bugInfo = GetBugInfo(null);
		Logger.Error(bugInfo);

		//try {
		//	RunBugBuddy(bugInfo);
		//}
		//catch (Exception) {
		//	BasicErrorDialog errorDialog = new BasicErrorDialog(noBugBuddyPrimaryMessage, noBugBuddySecondaryMessage);
		//	errorDialog.Show();
		//}
	}

	//private static void RunBugBuddy (string bugInfo) {
	//	string path = WriteBugInfo(bugInfo);

	//	Process process = new Process();
	//	process.StartInfo.FileName = "bug-buddy";
	//	process.StartInfo.Arguments = "--appname=gnome-subtitles --include=" + path;
	//	process.StartInfo.UseShellExecute = false;

	//	process.Start();
	//}

	private static string GetBugInfo (string message) {
		return "gnome-subtitles version: " + Base.ExecutionContext.Version + "\n"
			+ "gtk-sharp version: " + Base.ExecutionContext.GtkSharpVersion + "\n"
			+ (String.IsNullOrEmpty(message) ? "" : "Stack trace:\n" + message);
	}

	private static string WriteBugInfo (string bugInfo) {
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