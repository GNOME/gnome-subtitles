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

using System.IO;
using SubLib.Application;

namespace GnomeSubtitles {

public class ApplicationCore {
	private ExecutionInfo executionInfo = null;
	private Gnome.Program program = null;
	private Subtitles subtitles = null;
	private WidgetNames widgetNames = null;
	
	public ApplicationCore (ExecutionInfo executionInfo) {
		this.executionInfo = executionInfo;
		program = new Gnome.Program(executionInfo.ApplicationID,
			executionInfo.Version, Gnome.Modules.UI, executionInfo.Args);
		widgetNames = new WidgetNames();
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
	
	public WidgetNames WidgetNames {
		get { return widgetNames; }
	}
	
	
	public void New () {
		SubtitleFactory factory = new SubtitleFactory();
		subtitles = new Subtitles(factory.NewWithName("Unsaved Subtitles"));	
	}
	
	public void Open (string fileName) {
		SubtitleFactory factory = new SubtitleFactory();
		SubLib.Domain.Subtitles openedSubtitles;
		try {
			openedSubtitles = factory.Open(fileName);
		}
		catch (FileNotFoundException exception) {
			openedSubtitles = factory.New(fileName);		
		}
		subtitles = new Subtitles(openedSubtitles);
	}
	
	
}

public class WidgetNames {
	public const string MainWindow = "mainWindow";
	public const string SubtitleView = "subtitleListView";
	
	public const string TimesMenuItem = "timesMenuItem";
	public const string FramesMenuItem = "framesMenuItem";
	
	public const string AboutDialog = "aboutDialog";
	public const string OpenSubtitleDialog = "openSubtitleDialog";
	public const string SaveSubtitleDialog = "saveSubtitleDialog";
	

}

}