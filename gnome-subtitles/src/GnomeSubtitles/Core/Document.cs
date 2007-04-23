/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2007 Pedro Castro
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

using SubLib;
using System.IO;
using System.Text;

namespace GnomeSubtitles {

public class Document {
	private Subtitles subtitles = null;

	private FileProperties fileProperties = null;
	private bool canBeSaved = false; //Whether this document can be saved with existing fileProperties


	public Document (string path) {
		New(path);
	}
	
	public Document (string path, Encoding encoding) {
		Open(path, encoding);	
	}
	
	/* Public properties */

	public FileProperties FileProperties {
		get { return fileProperties; }
	}

	public Subtitles Subtitles {
		get { return subtitles; }
	}
	
	public bool CanBeSaved {
		get { return canBeSaved; }
	}
	
	/* Public methods */
	
	public bool Save () {
		if (!canBeSaved)
			return false;

		fileProperties.TimingMode = Global.TimingMode;
		SubtitleSaver saver = new SubtitleSaver();
		saver.Save(subtitles, fileProperties);
		fileProperties = saver.FileProperties;
		return true;
	}

	public bool Save (string path, Encoding encoding, SubtitleType type) {
		fileProperties = new FileProperties(path, encoding, type, Global.TimingMode);
		
		canBeSaved = true;
		Save();

		Global.GUI.Menus.SetActiveTimingMode(fileProperties.TimingMode);
		return true;
	}

	public void UpdateTimingModeFromFileProperties () {
		Global.TimingMode = fileProperties.TimingMode;
	}
	
	/* Private methods */
	
	private void New (string path) {
		SubtitleFactory factory = new SubtitleFactory();
		factory.Verbose = true;
		
		subtitles = new Subtitles(factory.New());
		fileProperties = new FileProperties(path);
	}
	
	private void Open (string path, Encoding encoding) {
		SubtitleFactory factory = new SubtitleFactory();
		factory.Verbose = true;
		factory.Encoding = encoding;

		SubLib.Subtitles openedSubtitles = null;
		try {
			openedSubtitles = factory.Open(path);
		}
		catch (FileNotFoundException) {
			New(path);
			return;
		}

		subtitles = new Subtitles(openedSubtitles);
		fileProperties = factory.FileProperties;
		
		if (fileProperties.SubtitleType != SubtitleType.Unknown)
			canBeSaved = true;
			
		Global.TimingMode = fileProperties.TimingMode;
	}

}

}