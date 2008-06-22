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
using Mono.Unix;
using SubLib;
using System.IO;
using System.Text;

namespace GnomeSubtitles.Core {

public class Document {
	private Ui.View.Subtitles subtitles = null;
	private bool wasTextModified = false;
	private bool wasTranslationModified = false;

	private FileProperties textFile = null;
	private FileProperties translationFile = null;
	private bool canTextBeSaved = false; //Whether the text document can be saved with existing textFile properties
	private bool canTranslationBeSaved = false; //Whether the translation document can be saved with existing translationFile properties


	public Document (string path, bool wasLoaded) {
		New(path, wasLoaded);
	}
	
	public Document (string path, Encoding encoding, bool wasLoaded) {
		Open(path, encoding, wasLoaded);
	}
	
	/* Public properties */

	public FileProperties TextFile {
		get { return textFile; }
	}
	
	public FileProperties TranslationFile {
		get { return translationFile; }
	}
	
	public bool IsTranslationLoaded {
		get { return translationFile != null; }
	}

	public Ui.View.Subtitles Subtitles {
		get { return subtitles; }
	}
	
	public bool CanTextBeSaved {
		get { return canTextBeSaved; }
	}
	
	public bool CanTranslationBeSaved {
		get { return canTranslationBeSaved; }
	}
	
	public bool WasTextModified {
		get { return wasTextModified; }
	}
	
	public bool WasTranslationModified {
		get { return wasTranslationModified; }
	}
	
	/* Public methods */

	public bool Save (FileProperties newFileProperties) {
		SubtitleSaver saver = new SubtitleSaver();
		saver.Save(subtitles, newFileProperties, SubtitleTextType.Text);
		
		textFile = saver.FileProperties;		
		canTextBeSaved = true;

		Base.Ui.Menus.SetActiveTimingMode(textFile.TimingMode);
		
		ClearTextModified();
		return true;
	}

	public void NewTranslation () {
		RemoveTranslationFromSubtitles();
		ClearTranslationStatus();
		CreateNewTranslationFileProperties();

		Base.Ui.UpdateFromNewTranslationDocument();
	}
	
	public void CloseTranslation () {
		RemoveTranslationFromSubtitles();
		ClearTranslationStatus();
		Base.Ui.UpdateFromCloseTranslation();
	}

	public void OpenTranslation (string path, Encoding encoding) {
		SubtitleFactory factory = new SubtitleFactory();
		factory.Verbose = true;
		factory.Encoding = encoding;

		SubLib.Subtitles openedTranslation = factory.Open(path);
		FileProperties newTranslationFile = factory.FileProperties;
		AddExtraSubtitles(openedTranslation);

		Translations translations = new Translations();
		translations.Import(subtitles, openedTranslation);

		ClearTranslationStatus();
		if (newTranslationFile.SubtitleType != SubtitleType.Unknown)
			canTranslationBeSaved = true;
	
		translationFile = newTranslationFile;
		Base.Ui.UpdateFromNewTranslationDocument();
	}
	
	public bool SaveTranslation (FileProperties newFileProperties) {
		SubtitleSaver saver = new SubtitleSaver();
		saver.Save(subtitles, newFileProperties, SubtitleTextType.Translation);
		
		translationFile = saver.FileProperties;		
		canTranslationBeSaved = true;
		
		ClearTranslationModified();
		return true;
	}
	
	public void UpdateFromCommandActivated (CommandTarget target) {
		if ((target == CommandTarget.Normal) && (!wasTextModified)) {
			wasTextModified = true;
			Base.Ui.UpdateFromDocumentModified(true);
		}
		else if ((target == CommandTarget.Translation) && (!wasTranslationModified)) {
			wasTranslationModified = true;
			Base.Ui.UpdateFromDocumentModified(true);
		}
	}


	/* Private methods */
	
	/* Used in the object construction */
	private void New (string path, bool wasLoaded) {
		SubtitleFactory factory = new SubtitleFactory();
		factory.Verbose = true;
		
		subtitles = new Ui.View.Subtitles(factory.New());
		textFile = new FileProperties(path);
	}
	
	/* Used in the object construction */
	private void Open (string path, Encoding encoding, bool wasLoaded) {
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

		subtitles = new Ui.View.Subtitles(openedSubtitles);
		textFile = factory.FileProperties;
		
		if (textFile.SubtitleType != SubtitleType.Unknown)
			canTextBeSaved = true;
	}
	
	private void ClearTextModified () {
		wasTextModified = false;
		if (!wasTranslationModified) //Update the GUI if translation is also not in modified state
			Base.Ui.UpdateFromDocumentModified(false);
	}
	
	private void ClearTranslationModified () {
		wasTranslationModified = false;
		if (!wasTextModified) //Update the GUI if text is also not in modified state
			Base.Ui.UpdateFromDocumentModified(false);
	}
	
	private void CreateNewTranslationFileProperties () {
		string filename = Catalog.GetString("Unsaved Translation");
		string path = (textFile.IsPathRooted ? Path.Combine(textFile.Directory, filename) : filename);
		translationFile = new FileProperties(path, textFile.Encoding, textFile.SubtitleType, textFile.TimingMode, textFile.NewlineType);
	}
	
	private void RemoveTranslationFromSubtitles () {
		Translations translations = new Translations();
		translations.Clear(subtitles);
	}
	
	private void ClearTranslationStatus () {
		wasTranslationModified = false;
		translationFile = null;
		canTranslationBeSaved = false;
		
		Base.CommandManager.ClearTarget(CommandTarget.Translation);
		ClearTranslationModified();
	}
	
	private void AddExtraSubtitles (SubLib.Subtitles translation) {
		int extraCount = translation.Collection.Count - subtitles.Collection.Count;
		if (extraCount > 0)
			subtitles.AddExtra(extraCount);	
	}

}

}