/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2019 Pedro Castro
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
using SubLib.Util;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GnomeSubtitles.Execution {

public class Executable {

	[DllImport("libc")]
	private static extern int prctl(int option, byte [] arg2, ulong arg3, ulong arg4, ulong arg5); //Used in Linux
	[DllImport("libc")]
	private static extern void setproctitle(byte [] fmt, byte [] str_arg); //Used in BSD's


	/* Public members */

	public static bool SetProcessNamePrctl (string name) {
		try {
			if (prctl(15, Encoding.ASCII.GetBytes(name + "\0"), 0, 0, 0) != 0) { // 15 = PR_SET_NAME
				Logger.Error("Error setting process name with prctl: {0}", Mono.Unix.Native.Stdlib.GetLastError());
			}
		}
		catch (Exception e) {
			Logger.Error(e, "Setting the process name using prctl has thrown an exception");
        	return false;
        }
        return true;
	}

	public static bool SetProcessNameSetproctitle (string name) {
		try {
			setproctitle(Encoding.ASCII.GetBytes("%s\0"), Encoding.ASCII.GetBytes(name + "\0"));
		}
		catch (Exception e) {
			Logger.Error(e, "Setting the process name using setproctitle has thrown an exception");
        	return false;
        }
        return true;
	}

    public static void SetProcessName(string name) {
    	if (!SetProcessNamePrctl(name))
    		SetProcessNameSetproctitle(name);
    }

	public static void Main (string[] args) {
		//ExceptionManager.UnhandledException += OnUnhandledException;

		ExecutionContext executionContext = new ExecutionContext(args);

		/* If on unix, set process name to gnome-subtitles instead of mono default */
		if (executionContext.PlatformIsUnix) {
			SetProcessName(executionContext.ExecutableName);
		}

		GLib.Log.SetDefaultHandler((string domain, GLib.LogLevelFlags level, string message) => {
			/* Ignore GLib Critical message: "Source ID <number> was not found when attempting to remove it". Context:
			 * 1) A change was introduced in GLib commit https://gitlab.gnome.org/GNOME/glib/commit/a919be3d39150328874ff647fb2c2be7af3df996
			 *    which made it output a critical error when removing an event which had already been removed. Only the
			 *    message is displayed, no exception is thrown.
			 * 2) Gtk-sharp wasn't checking if events were there before removing them, so applications based on it started
			 *    displaying lots of messages like this. This was fixed in https://github.com/mono/gtk-sharp/commit/7ea0c4afaf405df2dfc5a42e098e9023ecc1c51c
			 *    however it hasn't been released yet (the latest gtk-sharp version for gtk3 is 2.99.3 which was before this commit).
			 * 3) The gtk-sharp developers have been asked to release 2.99.4 on this thread but it hasn't happened yet:
			 *    https://gtk-sharp-list.ximian.narkive.com/oTBGM2Yx/please-release-gtk-sharp-2-99-4
			 * 4) So, as a temporary fix (until who knows when), we're just ignoring these messages here so they don't clutter up the logs.
			 */
			if ((level == GLib.LogLevelFlags.Critical)
					&& (domain == "GLib")
					&& message.StartsWith("Source ID")
					&& message.EndsWith("was not found when attempting to remove it")) {

				return;
			}
			
			Console.Error.WriteLine(domain + " | " + level + " | " + message);
		});
		
		Base.Run(executionContext);
	}

	/* Private members */

	///// <summary>Kills the window in the most quick and unfriendly way.</summary>
	//private static void Kill () {
	//	try {
	//   		Base.Kill();
	//	}
	//	catch (Exception) {
	//		; //Nothing to do if there were errors while killing the window
	//	}
	//}


	/* Event members */

	//Commenting because this isn't getting the Exception with its full stack trace
	//private static void OnUnhandledException (UnhandledExceptionArgs args) {
	//	if (args.ExceptionObject is Exception) {
	//		BugReporter.Report(args.ExceptionObject as Exception);
	//		throw new Exception("Unhandled Exception", args.ExceptionObject as Exception);
	//	}
		
	//	throw new Exception("Unhandled Exception with no inner exception");
	//	//Kill();
	//}

}

}