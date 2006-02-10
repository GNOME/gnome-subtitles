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

namespace GnomeSubtitles {

public class ExecutionInfo {
	private const string name = "Gnome Subtitles";
	private const string applicationID = "GnomeSubtitles";
	private const string executable = "gnome-subtitles";
	private const string version = "@GS-VERSION@";
	private const string gladeMasterFileName = "gnome-subtitles.glade";
	
	private string[] args = null;
	

	public ExecutionInfo(string[] programArgs) {
		args = programArgs;
	}
	
	
	public string Name {
		get { return name; }
	}
	
	public string ApplicationID {
		get { return applicationID; }
	}
	
	public string Executable {
		get { return executable; }
	}
	
	public string Version {
		get { return version; }
	}
	
	public string GladeMasterFileName {
		get { return gladeMasterFileName; }
	}

	public string[] Args {
		get { return args; }
	}
	



}

}