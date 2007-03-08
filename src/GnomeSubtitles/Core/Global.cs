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

using Gtk;
using SubLib;
using System;
using System.IO;
using System.Text;

namespace GnomeSubtitles {

public class Global {
	private static bool initialized = false;
	private static Gnome.Program program = null;
	private static Glade.XML glade = null;
	
	private static GnomeSubtitles.GUI gui = null;
	private static EventHandlers handlers = null;
	private static CommandManager commandManager = null;
	private static Clipboards clipboards = null;
	
	private static Subtitles subtitles = null;
	private static TimingMode timingMode = TimingMode.Times;

	
	/* Public properties */
	
	public static GnomeSubtitles.GUI GUI {
		get { return gui; }
	}
	
	public static EventHandlers Handlers {
		get { return handlers; }
	}
	
	public static CommandManager CommandManager {
		get { return commandManager; }
	}
	
	public static Clipboards Clipboards {
		get { return clipboards; }
	}
	
	public static Subtitles Subtitles {
		get { return subtitles; }
		set { subtitles = value; }
	}
	
	public static bool AreSubtitlesLoaded {
		get { return subtitles != null; }
	}

	//This should be the prefered way to set the timing mode, should propagate throughout the GUI
	public static TimingMode TimingMode {
		get { return timingMode; }
		set { timingMode = value; }
	}

	public static bool TimingModeIsFrames {
		get { return timingMode == TimingMode.Frames; }
	}
	
	public static bool TimingModeIsTimes {
		get { return timingMode == TimingMode.Times; }
	}

	/* Public methods */
	
	/// <summary>Runs the main GUI, after initialization.</summary>
	/// <returns>Whether 
	public static bool Run () {
		try {
			if (!Init())
				throw new Exception("The Global environment was already initialized.");
			
			gui.Start();
			program.Run();
			return true;
		}
		catch (Exception exception) {
			Kill();
			new BugReportWindow(exception);
			return false;
		}
	}
	
	/// <summary>Quits the program.</summary>
	public static void Quit () {
		program.Quit();
	}
	
	public static Widget GetWidget (string name) {
		return glade.GetWidget(name);
	}

	/* Private members */
	
	/// <summary>Initializes the global program structure.</summary>
	/// <remarks>Nothing is done if initialization has already occured. The core value is checked for this,
	/// if it's null then initialization hasn't occured yet.</remarks>
	/// <returns>Whether initialization succeeded.</returns>
	private static bool Init () {
		if (initialized)
			return false;

		program = new Gnome.Program(ExecutionInfo.ApplicationID, ExecutionInfo.Version, Gnome.Modules.UI, ExecutionInfo.Args);
		handlers = new EventHandlers();
		commandManager = new CommandManager(25, handlers.OnUndoToggled, handlers.OnRedoToggled, handlers.OnCommandActivated, handlers.OnModified); //TODO 25 should be set on gconf
		clipboards = new Clipboards();

		gui = new GUI(handlers, out glade);
		clipboards.WatchPrimaryChanges = true;
		return true;
	}
	
	/// <summary>Kills the window in the most quick and unfriendly way.</summary>
	private static void Kill () {
		try {
	   		clipboards.WatchPrimaryChanges = false;
    		program.Quit();
			gui.Kill();
		}
		catch (Exception) {
			; //Nothing to do if there were errors while killing the window 
		}
	}


}

}
