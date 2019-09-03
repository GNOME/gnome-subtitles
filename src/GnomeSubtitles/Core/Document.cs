/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2019 Pedro Castro
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
using SubLib.Core;
using SubLib.Core.Domain;
using System;
using System.IO;
using System.Text;

namespace GnomeSubtitles.Core {

/* Delegates */
public delegate void DocumentModificationStatusChangedHandler (bool modified);

public class Document {
	private Ui.View.Subtitles subtitles = null;
	private bool isTranslationLoaded = false; //Whether a translation document is loaded, either using New or Open Translation
	private bool wasTextModified = false;
	private bool wasTranslationModified = false;

	private FileProperties textFile = null;
	private FileProperties translationFile = null;
	private bool canTextBeSaved = false; //Whether the text document can be saved with existing textFile properties
	private bool canTranslationBeSaved = false; //Whether the translation document can be saved with existing translationFile properties


	public Document () {
		New();
		ConnectInitSignals();
	}

	public Document (string path, Encoding encoding) {
		Open(path, encoding);
		ConnectInitSignals();
	}

	/* Events */

	public event DocumentModificationStatusChangedHandler ModificationStatusChanged;

	/* Public properties */

	public FileProperties TextFile {
		get {
			/*Note: this must be changed if the unsaved text filename is to be dynamically generated on Save time.
			  For now its creation is enforced as its name is shown in the title bar. */
			if (textFile == null) {
				textFile = CreateNewTextFileProperties();
			}
			return textFile;
		}
	}

	public FileProperties TranslationFile {
		get { return translationFile; }
	}

	public bool IsTranslationLoaded {
		get { return isTranslationLoaded; }
	}

	/* Whether the text file properties is set */
	public bool HasTextFileProperties {
		get { return textFile != null; }
	}

	/* Whether the translation file properties is set */
	public bool HasTranslationFileProperties {
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

	public string UnsavedTextFilename {
		get {
			//To translators: this is the filename for new files (before being saved for the first time)
			return Catalog.GetString("Unsaved Subtitles");
		}
	}

	public string UnsavedTranslationFilename {
		get {
			string filename = (((textFile == null) || (textFile.Filename == String.Empty)) ? this.UnsavedTextFilename : textFile.FilenameWithoutExtension);
			string language = (Base.SpellLanguages.HasActiveTranslationLanguage ? Base.SpellLanguages.ActiveTranslationLanguage.ID : String.Empty);
			//To translators: this defines the name of a translation file. {0}=filename, {1}=language. Example: MovieName (fr translation)
			string translatedString = (language != String.Empty ? Catalog.GetString("{0} ({1} translation)") : Catalog.GetString("{0} (translation)"));
			object[] args = {filename, language};
			return Core.Util.GetFormattedText(translatedString, args);
		}
	}


	/* Public methods */

	public bool Save (FileProperties newFileProperties) {
		SubtitleSaver saver = new SubtitleSaver();
		saver.Save(subtitles, newFileProperties, SubtitleTextType.Text);

		textFile = saver.FileProperties;
		canTextBeSaved = true;

		ClearTextModified();
		return true;
	}

	public void NewTranslation () {
		if (this.isTranslationLoaded) {
			CloseTranslation();
		}
		this.isTranslationLoaded = true;
	}

	public void OpenTranslation (string path, Encoding encoding) {
		if (this.IsTranslationLoaded)
			CloseTranslation();

		SubtitleFactory factory = new SubtitleFactory();
		factory.Encoding = encoding;
		factory.FallbackEncoding = GetFallbackEncoding();

		SubLib.Core.Domain.Subtitles openedTranslation = factory.Open(path);
		FileProperties newTranslationFile = factory.FileProperties;
		AddExtraSubtitles(openedTranslation);

		Translations translations = new Translations();
		translations.Import(subtitles, openedTranslation, Base.Config.TimingsTimeBetweenSubtitles);

		if (newTranslationFile.SubtitleType != SubtitleType.Unknown)
			canTranslationBeSaved = true;

		this.translationFile = newTranslationFile;
		this.isTranslationLoaded = true;
	}

	public void Close () {
		DisconnectInitSignals();
	}

	public void CloseTranslation () {
		RemoveTranslationFromSubtitles();
		ClearTranslationStatus();
	}

	public bool SaveTranslation (FileProperties newFileProperties) {
		SubtitleSaver saver = new SubtitleSaver();
		saver.Save(subtitles, newFileProperties, SubtitleTextType.Translation);

		translationFile = saver.FileProperties;
		canTranslationBeSaved = true;

		ClearTranslationModified();
		return true;
	}


	/* Private methods */

	/* Used in the object construction. Path is used when running from the command line specifying an inexistent file. */
	private void New (string path) {
		New();
		textFile = new FileProperties(path);
	}

	/* Used in the object construction */
	private void New () {
		SubtitleFactory factory = new SubtitleFactory();

		subtitles = new Ui.View.Subtitles(factory.New());
	}

	/* Used in the object construction */
	private void Open (string path, Encoding encoding) {
		SubtitleFactory factory = new SubtitleFactory();
		factory.Encoding = encoding;
		factory.FallbackEncoding = GetFallbackEncoding();
		factory.InputFrameRate = Base.Ui.Menus.TimingsInputFrameRateActive;

		SubLib.Core.Domain.Subtitles openedSubtitles = null;
		try {
			openedSubtitles = factory.Open(path);
		}
		catch (FileNotFoundException) {
			New(path);
			return;
		}

		subtitles = new Ui.View.Subtitles(openedSubtitles);
		textFile = factory.FileProperties;

		if (textFile.SubtitleType != SubtitleType.Unknown)
			canTextBeSaved = true;
	}

	private void ClearTextModified () {
		wasTextModified = false;
		if (!wasTranslationModified) //Emit the event if translation is also not in modified state
			EmitModificationStatusChangedEvent(false);
	}

	private void ClearTranslationModified () {
		wasTranslationModified = false;
		if (!wasTextModified) //Emit the event if text is also not in modified state
			EmitModificationStatusChangedEvent(false);
	}

	private void RemoveTranslationFromSubtitles () {
		Translations translations = new Translations();
		translations.Clear(subtitles);
	}

	private void ClearTranslationStatus () {
		this.isTranslationLoaded = false;
		this.wasTranslationModified = false;
		this.translationFile = null;
		this.canTranslationBeSaved = false;

		ClearTranslationModified();
	}

	private void AddExtraSubtitles (SubLib.Core.Domain.Subtitles translation) {
		int extraCount = translation.Collection.Count - subtitles.Collection.Count;
		if (extraCount > 0)
			subtitles.AddExtra(extraCount);
	}

	private Encoding GetFallbackEncoding () {
		ConfigFileOpenFallbackEncoding fallbackEncodingConfig = Base.Config.FileOpenFallbackEncoding;
		if (fallbackEncodingConfig == ConfigFileOpenFallbackEncoding.CurrentLocale) {
			return Encodings.GetEncoding(Encodings.SystemDefault.CodePage);
		}

		string encodingCode = Base.Config.FileOpenFallbackEncodingFixed;
		EncodingDescription encodingDescription = EncodingDescription.Empty;
		Encodings.Find(encodingCode, ref encodingDescription);
		return Encodings.GetEncoding(encodingDescription.CodePage);
	}

	private FileProperties CreateNewTextFileProperties () {
		return new FileProperties(this.UnsavedTextFilename);
	}



	/* Event members */

	private void ConnectInitSignals () {
		Base.CommandManager.CommandActivated += OnCommandManagerCommandActivated;
	}

	private void DisconnectInitSignals () {
		Base.CommandManager.CommandActivated -= OnCommandManagerCommandActivated;
	}

	private void OnCommandManagerCommandActivated (object o, CommandActivatedArgs args) {
    	if ((args.Target == CommandTarget.Normal) && (!wasTextModified)) {
			wasTextModified = true;
			EmitModificationStatusChangedEvent(true);
		}
		else if ((args.Target == CommandTarget.Translation) && (!wasTranslationModified)) {
			wasTranslationModified = true;
			EmitModificationStatusChangedEvent(true);
		}
		else if ((args.Target == CommandTarget.NormalAndTranslation) && ((!wasTextModified) || (!wasTranslationModified))) {
			wasTextModified = true;
			wasTranslationModified = true;
			EmitModificationStatusChangedEvent(true);
		}
    }

	private void EmitModificationStatusChangedEvent (bool modified) {
		if (ModificationStatusChanged != null)
			ModificationStatusChanged(modified);
	}

}

}