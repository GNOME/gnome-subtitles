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

using Gtk;

namespace GnomeSubtitles {

public class GUI {
	private Gnome.Program gnomeApplication = null;
	private MainWindow mainWindow = null;
	private ApplicationCore core = null;

	public GUI(ExecutionInfo executionInfo) {
		core = new ApplicationCore(executionInfo);
		gnomeApplication = new Gnome.Program(executionInfo.ApplicationID,
			executionInfo.Version, Gnome.Modules.UI, executionInfo.Args);
		mainWindow = new MainWindow(this);
		if (executionInfo.Args.Length > 0)
			Open(executionInfo.Args[0]);
		gnomeApplication.Run();
    }
    
	public ApplicationCore Core {
		get { return core; }
	}

    
    public void Close() {
    		gnomeApplication.Quit();
    }

    public void New () {
    		core.New();
    		mainWindow.NewDocument();
    }
    
    public void Open (string fileName) {
		core.Open(fileName);
    		mainWindow.NewDocument();
    }

}

}