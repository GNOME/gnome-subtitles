/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
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
using System.IO;
using SubLib;

namespace GnomeSubtitles {

public class ApplicationCore {
	private ExecutionInfo executionInfo = null;
	private Gnome.Program program = null;
	private Subtitles subtitles = null;
	private EventHandlers handlers = null;
	private CommandManager commandManager = null;
	
	public ApplicationCore (ExecutionInfo executionInfo, GUI gui) {
		this.executionInfo = executionInfo;
		program = new Gnome.Program(executionInfo.ApplicationID,
			executionInfo.Version, Gnome.Modules.UI, executionInfo.Args);
		handlers = new EventHandlers(gui);
		commandManager = new CommandManager(25, handlers.OnUndoToggled, handlers.OnRedoToggled,
			handlers.OnCommandActivated, handlers.OnModified);
	}

	public ExecutionInfo ExecutionInfo {
		get { return executionInfo; }
	}
	
	public Gnome.Program Program {
		get { return program; }
	}
	
	public Subtitles Subtitles {
		get { return subtitles; }
	}
	
	public EventHandlers Handlers {
		get { return handlers; }
		set { handlers = value; }
	}
	
	public CommandManager CommandManager {
		get { return commandManager; }
	}
	
	public bool IsLoaded {
		get { return subtitles != null; }
	}

	
	public void New () {
		SubtitleFactory factory = new SubtitleFactory();
		subtitles = new Subtitles(factory.NewWithName("Unsaved Subtitles"));
		CheckSubtitleCount();	
	}
	
	public void Open (string fileName) {
		SubtitleFactory factory = new SubtitleFactory();
		factory.BeVerbose = true;
		SubLib.Subtitles openedSubtitles;
		try {
			openedSubtitles = factory.Open(fileName);
		}
		catch (FileNotFoundException exception) {
			openedSubtitles = factory.New(fileName);		
		}
		subtitles = new Subtitles(openedSubtitles);
		CheckSubtitleCount();
	}
	
	
	private void CheckSubtitleCount () {
		if (subtitles.Collection.Count == 0){
			Subtitle subtitle = new Subtitle(subtitles.Properties, TimeSpan.Zero, TimeSpan.FromSeconds(3.5));
			subtitles.Add(subtitle);
		}
	}
	
}

public class WidgetNames {
	public const string MainWindow = "mainWindow";
	
	/* File Menu */
	public const string SaveMenuItem = "saveMenuItem";
	public const string SaveAsMenuItem = "saveAsMenuItem";
	
	/* Edit Menu */
	public const string UndoMenuItem = "undoMenuItem";
	public const string RedoMenuItem = "redoMenuItem";
	public const string CutMenuItem = "cutMenuItem";
	public const string CopyMenuItem = "copyMenuItem";
	public const string PasteMenuItem = "pasteMenuItem";
	public const string ClearMenuItem = "clearMenuItem";
	public const string PropertiesMenuItem = "propertiesMenuItem";
	
	public const string TimesMenuItem = "timesMenuItem";
	public const string FramesMenuItem = "framesMenuItem";
	
	/* Toolbar */	
	public const string SaveButton = "toolbuttonSave";
	public const string UndoButton = "toolbuttonUndo";
	public const string RedoButton = "toolbuttonRedo";
	
	/* Dialogs */
	public const string AboutDialog = "aboutDialog";
	public const string OpenSubtitleDialog = "openSubtitleDialog";
	public const string SaveAsSubtitleDialog = "saveAsSubtitleDialog";
	
	/* Subtitle View */
	public const string SubtitleView = "subtitleListView";
	
	/* Subtitle Edit */
	public const string SubtitleEditHBox = "editAreaHBox";
	public const string StartSpinButton = "startSpinButton";
	public const string EndSpinButton = "endSpinButton";
	public const string DurationSpinButton = "durationSpinButton";
	public const string SubtitleTextView = "subtitleTextView";
	
}

}