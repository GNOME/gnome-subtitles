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
using System.Runtime.InteropServices;
using System.Text;

namespace GnomeSubtitles {

public class Executable {

	[DllImport("libc")]
	private static extern int prctl(int option, byte [] arg2, ulong arg3, ulong arg4, ulong arg5);
    		
    public static void SetProcessName(string name) {
    	try {
   			if(prctl(15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes(name + "\0"), 0, 0, 0) != 0) {
	   			System.Console.WriteLine("Error setting process name: " + Mono.Unix.Native.Stdlib.GetLastError());
        	}
        }
        catch (Exception e) {
        	System.Console.WriteLine("Could not set the process name.");
        	System.Console.WriteLine(e);
        }
    }

	public static void Main (string[] args) {
		ExecutionInfo.Args = args;
		SetProcessName(ExecutionInfo.ExecutableName);
		Global.Run();
	}

}

}
