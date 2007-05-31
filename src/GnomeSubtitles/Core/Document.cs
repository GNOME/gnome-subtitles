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
	private bool wasNormalModified = false;
	private bool wasTranslationModified = false;

	private FileProperties fileProperties = null;
	private bool canBeSaved = false; //Whether this document can be saved with existing fileProperties


	public Document () {
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
	
	public bool WasNormalModified {
		get { return wasNormalModified; }
	}
	
	public bool WasTranslationModified {
		get { return wasTranslationModified; }
	}
	
	/* Public methods */

	public void New (string path, bool wasLoaded) {
		SubtitleFactory factory = new SubtitleFactory();
		factory.Verbose = true;
		
		subtitles = new Subtitles(factory.New());
		fileProperties = new FileProperties(path);
		
		Global.GUI.UpdateFromNewDocument(wasLoaded);
	}
	
	public void Open (string path, Encoding encoding, bool wasLoaded) {
		SubtitleFactory factory = new SubtitleFactory();
		factory.Verbose = true;
		factory.Encoding = encoding;

		SubLib.Subtitles openedSubtitles = null;
		try {
			openedSubtitles = factory.Open(path);
		}
		catch (FileNotFoundException) {
			New(path, wasLoaded);
			return;
		}

		subtitles = new Subtitles(openedSubtitles);
		fileProperties = factory.FileProperties;
		
		if (fileProperties.SubtitleType != SubtitleType.Unknown)
			canBeSaved = true;
			
		Global.TimingMode = fileProperties.TimingMode;
		Global.GUI.UpdateFromNewDocument(wasLoaded);
	}

	public bool Save (FileProperties newFileProperties) {
		SubtitleSaver saver = new SubtitleSaver();
		saver.Save(subtitles, newFileProperties);
		
		fileProperties = saver.FileProperties;		
		canBeSaved = true;

		Global.GUI.Menus.SetActiveTimingMode(fileProperties.TimingMode);
		
		ClearNormalModified();
		return true;
	}
	
	public void UpdateFromCommandActivated (CommandTarget target) {
		if ((target == CommandTarget.Normal) && (!wasNormalModified)) {
			wasNormalModified = true;
			Global.GUI.UpdateFromDocumentModified(true);
		}
		else if ((target == CommandTarget.Translation) && (!wasTranslationModified)) {
			wasTranslationModified = true;
			Global.GUI.UpdateFromDocumentModified(true);
		}
	}


	/* Private methods */
	
	private void ClearNormalModified () {
		wasNormalModified = false;
		if (!wasTranslationModified) //Update the GUI if translation is also not in modified state
			Global.GUI.UpdateFromDocumentModified(false);
	}
	
	/*private void ClearTranslationModified () {
		wasTranslationModified = false;
		if (!wasNormalModified) //Update the GUI if normal is also not in modified state
			Global.GUI.UpdateFromDocumentModified(false);
	}*/

}

}