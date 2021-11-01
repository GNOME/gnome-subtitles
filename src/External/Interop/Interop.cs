/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2020-2021 Pedro Castro
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

namespace External.Interop {

public class Interop {
	
	private const int RTLD_NOW = 2; // dlopen, see https://docs.oracle.com/cd/E86824_01/html/E54766/dlopen-3c.html#REFMAN3Adlopen-3c

	/* Function imports */
	
	[DllImport("libdl")]
	private static extern IntPtr dlopen (string filename, int flags);
	
	[DllImport("libdl")]
	private static extern IntPtr dlsym (IntPtr handle, string symbol);
	
	[DllImport("libdl")]
	private static extern int dlclose (IntPtr handl);
	
	
	public static IntPtr Open (string filename) {
		return dlopen(filename, RTLD_NOW);
	}
	
	public static IntPtr GetMethod (IntPtr lib, string methodName) {
		return dlsym(lib, methodName);
	}
	
	public static int Close (IntPtr lib) {
		return dlclose(lib);
	}

}

}