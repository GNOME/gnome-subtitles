/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2017 Pedro Castro
 *
 * SubLib is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * SubLib is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.IO;

namespace SubLib.Util {

public class Logger {
	private enum LogType { Info, Error };

	public static bool Enabled { get; set; } = true;

	public static void Info (string message, params object[] args) {
		if (Enabled) {
			WriteLine(LogType.Info, message, args);
		}
	}

	public static void Error (string message, params object[] args) {
		if (Enabled) {
			WriteLine(LogType.Error, message, args);
		}
	}

	/// <summary>
	/// Same as Error(message, args) but prints an exception after the error message.
	/// </summary>
	/// <param name="exception">Exception.</param>
	/// <param name="message">Error message.</param>
	/// <param name="args">Error message arguments.</param>
	public static void Error (Exception exception, string message, params object[] args) {
		Error(message + "\n" + exception.ToString(), args);
	}

	public static void Error (Exception exception) {
		Error(exception.ToString());
	}


	private static void WriteLine (LogType type, string message, params object[] args) {
		TextWriter writer = (type == LogType.Info ? Console.Out : Console.Error);
		writer.WriteLine(DateTime.Now.ToString("HH:mm:ss.f") + " "
			+ type.ToString().ToUpper() + " "
			+ message,
			args);
	}

}

}
