/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2008 Pedro Castro
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

using GnomeSubtitles.Core.Command;
using GnomeSubtitles.Dialog;
using GnomeSubtitles.Execution;
using GnomeSubtitles.Ui;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;
using System.IO;
using System.Text;

namespace GnomeSubtitles.Core {

public class Base {
	private static Glade.XML glade = null;
	
	private static MainUi ui = null;
	private static ExecutionContext executionContext = null;
	private static EventHandlers handlers = null;
	private static CommandManager commandManager = null;
	private static Clipboards clipboards = null;
	private static Config config = null;
	private static Dialogs dialogs = null;
	private static SpellLanguages spellLanguages = null;
	
	private static Document document = null;
	private static TimingMode timingMode = TimingMode.Times;

	
	/* Public properties */
	
	public static MainUi Ui {
		get { return ui; }
	}
	
	public static ExecutionContext ExecutionContext {
		get { return executionContext; }
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
	
	public static Config Config {
		get { return config; }
	}
	
	public static Dialogs Dialogs {
		get { return dialogs; }
	}
	
	public static SpellLanguages SpellLanguages {
		get { return spellLanguages; }
	}
	
	public static Document Document {
		get { return document; }
	}
	
	public static bool IsDocumentLoaded {
		get { return document != null; }
	}
	
	public static TimingMode TimingMode {
		get { return timingMode; }
		set {
			if (timingMode != value) {
				timingMode = value;
				ui.UpdateFromTimingMode(value);
			}		
		}
	}

	public static bool TimingModeIsFrames {
		get { return timingMode == TimingMode.Frames; }
	}

	public static bool TimingModeIsTimes {
		get { return timingMode == TimingMode.Times; }
	}


	/* Public methods */
	
	/// <summary>Runs the main GUI, after initialization.</summary> 
	public static void Run (ExecutionContext executionContext) {
		if (!Init(executionContext))
			throw new Exception("The Base environment was already initialized.");
			
		ui.Start();
		executionContext.RunApplication();
	}
	
	/// <summary>Quits the program.</summary>
	public static void Quit () {
		executionContext.QuitApplication();
	}
	
	public static void Kill () {
		clipboards.WatchPrimaryChanges = false;
		ui.Kill();
		executionContext.QuitApplication();
	}
	
	public static void CreateDocumentNew (string path) {
		bool wasLoaded = IsDocumentLoaded;
		document = new Document(path, wasLoaded);
		
		CommandManager.Clear();
		Ui.UpdateFromDocumentModified(false);
		Ui.UpdateFromNewDocument(wasLoaded);		
	}
	
	public static void CreateDocumentOpen (string path, Encoding encoding) {
		bool wasLoaded = IsDocumentLoaded;
		document = new Document(path, encoding, wasLoaded);

		CommandManager.Clear();
		TimingMode = document.TextFile.TimingMode;
		Ui.UpdateFromDocumentModified(false);
		Ui.UpdateFromNewDocument(wasLoaded);
	}
	
	public static Widget GetWidget (string name) {
		return glade.GetWidget(name);
	}

	/* Private members */
	
	/// <summary>Initializes the base program structure.</summary>
	/// <remarks>Nothing is done if initialization has already occured. The core value is checked for this,
	/// if it's null then initialization hasn't occured yet.</remarks>
	/// <returns>Whether initialization succeeded.</returns>
	private static bool Init (ExecutionContext newExecutionContext) {
		if ((executionContext != null) && (executionContext.Initialized))
			return false;

		executionContext = newExecutionContext;
		executionContext.InitApplication();
		
		/* Initialize Command manager */
		commandManager = new CommandManager();
		
		/* Initialize handlers */
		handlers = new EventHandlers();
		
		/* Initialize misc */
		clipboards = new Clipboards();
		config = new Config();
		dialogs = new Dialogs();
		spellLanguages = new SpellLanguages();

		/* Initialize the GUI */
		ui = new MainUi(handlers, out glade);
		clipboards.WatchPrimaryChanges = true;
		Catalog.Init(ExecutionContext.TranslationDomain, ConfigureDefines.LocaleDir);

		return true;
	}

}

}
