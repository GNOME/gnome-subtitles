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
using System.Text;
using SubLib;

namespace GnomeSubtitles {

public class ApplicationCore {
	private Gnome.Program program = null;
	private Subtitles subtitles = null;
	private EventHandlers handlers = null;
	private CommandManager commandManager = null;
	
	public ApplicationCore (GUI gui) {
		program = new Gnome.Program(ExecutionInfo.ApplicationID,
			ExecutionInfo.Version, Gnome.Modules.UI, ExecutionInfo.Args);
		handlers = new EventHandlers(gui);
		commandManager = new CommandManager(25, handlers.OnUndoToggled, handlers.OnRedoToggled,
			handlers.OnCommandActivated, handlers.OnModified);
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
		Open(fileName, null);
	}
	
	public void Open (string fileName, Encoding encoding) {
		SubtitleFactory factory = new SubtitleFactory();
		factory.Verbose = true;
		if (encoding != null)
			factory.Encoding = encoding;

		SubLib.Subtitles openedSubtitles = null;
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

}