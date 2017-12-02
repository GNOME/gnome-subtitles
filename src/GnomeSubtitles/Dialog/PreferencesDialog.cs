/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2017 Pedro Castro
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

//using Glade;
using GnomeSubtitles.Core;
using GnomeSubtitles.Ui.Component;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Dialog {

public class PreferencesDialog : BuilderDialog {

	/* Constant strings */
	private const string gladeFilename = "PreferencesDialog.glade";

	/* Components */
	private EncodingComboBox fileOpenEncoding = null;
	private EncodingComboBox fileOpenFallbackEncoding = null;
	private EncodingComboBox fileSaveEncoding = null;
	private SubtitleFormatComboBox fileSaveFormat = null;
	private NewlineTypeComboBox fileSaveNewline = null;

	/* Widgets */
	[Builder.Object] private CheckButton translationSaveAllCheckButton = null;
	[Builder.Object] private CheckButton videoAutoChooseFileCheckButton = null;
	[Builder.Object] private CheckButton autoBackupCheckButton = null;
	[Builder.Object] private CheckButton reactionDelayCheckButton = null;
	[Builder.Object] private CheckButton videoSeekCheckButton = null;
	[Builder.Object] private ComboBoxText fileOpenEncodingComboBox = null;
	[Builder.Object] private ComboBoxText fileOpenFallbackEncodingComboBox = null;
	[Builder.Object] private ComboBoxText fileSaveEncodingComboBox = null;
	[Builder.Object] private ComboBoxText fileSaveFormatComboBox = null;
	[Builder.Object] private ComboBoxText fileSaveNewlineComboBox = null;
	[Builder.Object] private SpinButton autoBackupTimeSpinButton = null;
	[Builder.Object] private SpinButton reactionDelaySpinButton = null;
	[Builder.Object] private SpinButton videoSeekRewindSpinButton = null;
	[Builder.Object] private SpinButton subtitleSplitSpinButton = null;

	public PreferencesDialog () : base(gladeFilename, false) {
		LoadValues();
		Autoconnect();
	}

	/* Private members */

	private void LoadValues () {
		LoadValuesFilesTab();
		LoadValuesEditingTab();
	}

	private void LoadValuesFilesTab () {
		/* Translation Save All */
		translationSaveAllCheckButton.Active = Base.Config.FileTranslationSaveAll;

		/* Defaults */
		SetDefaultsFileOpenEncoding();
		SetDefaultsFileOpenFallbackEncoding();
		SetDefaultsFileSaveEncoding();
		SetDefaultsFileSaveFormat();
		SetDefaultsFileSaveNewline();

		/* Video Auto choose file */
		videoAutoChooseFileCheckButton.Active = Base.Config.VideoAutoChooseFile;

		/* Auto Backup */
		SetAutoBackup();
	}

	private void LoadValuesEditingTab () {
		/* Video Seeking */
		videoSeekCheckButton.Active = Base.Config.VideoSeekOnChange;
		videoSeekRewindSpinButton.Value = Base.Config.VideoSeekOnChangeRewind;
		videoSeekRewindSpinButton.Sensitive = videoSeekCheckButton.Active;

		/* Subtitle Splitting */
		subtitleSplitSpinButton.Value = Base.Config.TimingsTimeBetweenSubtitles;

		/* Reaction Delay */
		SetReactionDelay();
	}

	private void SetDefaultsFileOpenEncoding () {
		string[] additionalActions = { Catalog.GetString("Remember the last used encoding") };
		int fixedEncoding = -1;
		ConfigFileOpenEncodingOption fileOpenEncodingOption = Base.Config.FileOpenEncodingOption;
		if (fileOpenEncodingOption == ConfigFileOpenEncodingOption.Specific) {
			string encodingName = Base.Config.FileOpenEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingName, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		fileOpenEncoding = new EncodingComboBox(true, additionalActions, fixedEncoding);
		fileOpenEncodingComboBox = fileOpenEncoding.Widget; //FIXME REMOVE
		if (fileOpenEncodingOption != ConfigFileOpenEncodingOption.Specific) {
			fileOpenEncoding.ActiveSelection = (int)fileOpenEncodingOption;
		}
		fileOpenEncoding.SelectionChanged += OnDefaultsFileOpenEncodingChanged;
	}

	private void SetDefaultsFileOpenFallbackEncoding () {
		int fixedEncoding = -1;
		ConfigFileOpenFallbackEncoding fileOpenFallbackEncodingConfig = Base.Config.FileOpenFallbackEncoding;
		if (fileOpenFallbackEncodingConfig == ConfigFileOpenFallbackEncoding.Fixed) {
			string encodingName = Base.Config.FileOpenFallbackEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingName, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		fileOpenFallbackEncoding = new EncodingComboBox(false, null, fixedEncoding);
		fileOpenFallbackEncodingComboBox = fileOpenFallbackEncoding.Widget; //FIXME REMOVE
		fileOpenFallbackEncoding.SelectionChanged += OnDefaultsFileOpenFallbackEncodingChanged;
	}

	private void SetDefaultsFileSaveEncoding () {
		string[] additionalActions = { Catalog.GetString("Keep the encoding used on file open"), Catalog.GetString("Remember the last used encoding") };
		int fixedEncoding = -1;
		ConfigFileSaveEncodingOption fileSaveEncodingOption = Base.Config.FileSaveEncodingOption;
		if (fileSaveEncodingOption == ConfigFileSaveEncodingOption.Specific) {
			string encodingName = Base.Config.FileSaveEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingName, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		fileSaveEncoding = new EncodingComboBox(false, additionalActions, fixedEncoding);
		fileSaveEncodingComboBox = fileSaveEncoding.Widget; //FIXME REMOVE
		if (fileSaveEncodingOption != ConfigFileSaveEncodingOption.Specific) {
			fileSaveEncoding.ActiveSelection = (int)fileSaveEncodingOption;
		}
		fileSaveEncoding.SelectionChanged += OnDefaultsFileSaveEncodingChanged;
	}

	private void SetDefaultsFileSaveFormat () {
		string[] additionalActions = { Catalog.GetString("Keep the format used on file open"), Catalog.GetString("Remember the last used format") };
		SubtitleType fixedFormat = SubtitleType.Unknown;
		ConfigFileSaveFormatOption fileSaveFormatOption = Base.Config.FileSaveFormatOption;
		if (fileSaveFormatOption == ConfigFileSaveFormatOption.Specific) {
			fixedFormat = Base.Config.FileSaveFormatFixed;
		}

		fileSaveFormat = new SubtitleFormatComboBox(fixedFormat, additionalActions);
		fileSaveFormatComboBox = fileSaveFormat.Widget; //FIXME REMOVE
		if (fileSaveFormatOption != ConfigFileSaveFormatOption.Specific) {
			fileSaveFormat.ActiveSelection = (int)fileSaveFormatOption;
		}
		fileSaveFormat.SelectionChanged += OnDefaultsFileSaveFormatChanged;
	}

	private void SetDefaultsFileSaveNewline () {
		string[] additionalActions = { Catalog.GetString("Remember the last used type") };
		NewlineType newlineTypeToSelect = NewlineType.Unknown;
		ConfigFileSaveNewlineOption fileSaveNewlineOption = Base.Config.FileSaveNewlineOption;
		if (fileSaveNewlineOption == ConfigFileSaveNewlineOption.Specific) {
			newlineTypeToSelect = Base.Config.FileSaveNewline;
		}

		fileSaveNewline = new NewlineTypeComboBox(newlineTypeToSelect, additionalActions);
		fileSaveNewlineComboBox = fileSaveNewline.Widget; //FIXME REMOVE
		if (fileSaveNewlineOption != ConfigFileSaveNewlineOption.Specific) {
			fileSaveNewline.ActiveSelection = (int)fileSaveNewlineOption;
		}
		fileSaveNewline.SelectionChanged += OnDefaultsFileSaveNewlineChanged;
	}

	private void SetAutoBackup () {
		bool autoBackupEnabled = Base.Config.BackupAuto;
		autoBackupCheckButton.Active = autoBackupEnabled;

		autoBackupTimeSpinButton.Sensitive = autoBackupEnabled;
		autoBackupTimeSpinButton.Value = Base.Config.BackupTime / 60; //Minutes
	}

	private void SetReactionDelay () {
		bool reactionDelayEnabled = Base.Config.VideoApplyReactionDelay;
		reactionDelayCheckButton.Active = reactionDelayEnabled;

		reactionDelaySpinButton.Sensitive = reactionDelayEnabled;
		reactionDelaySpinButton.Value = Base.Config.VideoReactionDelay;
	}

	private void ResetDialogToDefaults () {
		translationSaveAllCheckButton.Active = true;

		fileOpenEncoding.ActiveSelection = 0; //Auto detect
		fileOpenFallbackEncoding.ActiveSelection = 0; //Current Locale
		videoAutoChooseFileCheckButton.Active = true;

		fileSaveEncoding.ActiveSelection = 0; //Keep Existing
		fileSaveFormat.ActiveSelection = 0; //Keep Existing
		fileSaveNewline.ChosenNewlineType = NewlineType.Windows;

		autoBackupCheckButton.Active = true;
		autoBackupTimeSpinButton.Value = 2;

		reactionDelayCheckButton.Active = true;
		reactionDelaySpinButton.Value = 200;

		videoSeekCheckButton.Active = true;
		videoSeekRewindSpinButton.Value = 200;

		subtitleSplitSpinButton.Value = 100;
	}


	/* Event members */

	#pragma warning disable 169		//Disables warning about handlers not being used

	private void OnDefaultsFileOpenEncodingChanged (object o, EventArgs args) {
		int active = fileOpenEncoding.ActiveSelection;
		ConfigFileOpenEncodingOption activeOption = (ConfigFileOpenEncodingOption)Enum.ToObject(typeof(ConfigFileOpenEncodingOption), active);
		if (((int)activeOption) > ((int)ConfigFileOpenEncodingOption.Specific)) //Positions higher than specific are always specific too
			activeOption = ConfigFileOpenEncodingOption.Specific;

		Base.Config.FileOpenEncodingOption = activeOption;
		/* If encoding option is specific, encodingOption=Specific and encoding holds the encoding name */
		if (activeOption == ConfigFileOpenEncodingOption.Specific) {
			EncodingDescription chosenEncoding = fileOpenEncoding.ChosenEncoding;
			if (!chosenEncoding.Equals(EncodingDescription.Empty)) {
				Base.Config.FileOpenEncodingFixed = chosenEncoding.Name;
			}
		}
		else {
			/* If encoding is current locale, encoding holds current locale too, otherwise it just holds auto detect */
			ConfigFileOpenEncoding encodingToStore = ConfigFileOpenEncoding.AutoDetect;
			if (activeOption == ConfigFileOpenEncodingOption.CurrentLocale) {
				encodingToStore = ConfigFileOpenEncoding.CurrentLocale;
			}
			Base.Config.FileOpenEncoding = encodingToStore;
		}
	}

	private void OnDefaultsFileOpenFallbackEncodingChanged (object o, EventArgs args) {
		if (fileOpenFallbackEncoding.IsChosenCurrentLocale)
			Base.Config.FileOpenFallbackEncoding = ConfigFileOpenFallbackEncoding.CurrentLocale;
		else {
			EncodingDescription chosenEncoding = fileOpenFallbackEncoding.ChosenEncoding;
			if (!chosenEncoding.Equals(EncodingDescription.Empty)) {
				Base.Config.FileOpenFallbackEncodingFixed = chosenEncoding.Name;
			}
		}
	}

	private void OnDefaultsFileSaveEncodingChanged (object o, EventArgs args) {
		int active = fileSaveEncoding.ActiveSelection;
		ConfigFileSaveEncodingOption activeOption = (ConfigFileSaveEncodingOption)Enum.ToObject(typeof(ConfigFileSaveEncodingOption), active);
		if (((int)activeOption) > ((int)ConfigFileSaveEncodingOption.Specific)) //Positions higher than specific are always specific too
			activeOption = ConfigFileSaveEncodingOption.Specific;

		Base.Config.FileSaveEncodingOption = activeOption;
		/* If encoding is specific, encodingOption=Specific and encoding holds the encoding name */
		if (activeOption == ConfigFileSaveEncodingOption.Specific) {
			EncodingDescription chosenEncoding = fileSaveEncoding.ChosenEncoding;
			if (!chosenEncoding.Equals(EncodingDescription.Empty)) {
				Base.Config.FileSaveEncodingFixed = chosenEncoding.Name;
			}
		}
		else {
			/* If encoding option is current locale, encoding holds current locale too, otherwise it just holds keep existing */
			ConfigFileSaveEncoding encodingToStore = ConfigFileSaveEncoding.KeepExisting;
			if (activeOption == ConfigFileSaveEncodingOption.CurrentLocale) {
				encodingToStore = ConfigFileSaveEncoding.CurrentLocale;
			}
			Base.Config.FileSaveEncoding = encodingToStore;
		}
	}

	private void OnDefaultsFileSaveFormatChanged (object o, EventArgs args) {
		int active = fileSaveFormat.ActiveSelection;
		ConfigFileSaveFormatOption activeOption = (ConfigFileSaveFormatOption)Enum.ToObject(typeof(ConfigFileSaveFormatOption), active);
		if (((int)activeOption) > ((int)ConfigFileSaveFormatOption.Specific)) //Positions higher than specific are always specific too
			activeOption = ConfigFileSaveFormatOption.Specific;

		Base.Config.FileSaveFormatOption = activeOption;
		/* If format is specific, formatOption=Specific and format holds the format name */
		if (activeOption == ConfigFileSaveFormatOption.Specific) {
			SubtitleType chosenFormat = fileSaveFormat.ChosenSubtitleType;
			if (!chosenFormat.Equals(SubtitleType.Unknown)) {
				Base.Config.FileSaveFormatFixed = chosenFormat;
			}
		}
		else {
			/* If format option is keep existing or remember last, use keep existing */
			Base.Config.FileSaveFormat = ConfigFileSaveFormat.KeepExisting;
		}
	}

	private void OnDefaultsFileSaveNewlineChanged (object o, EventArgs args) {
		int active = fileSaveNewline.ActiveSelection;
		ConfigFileSaveNewlineOption activeOption = (ConfigFileSaveNewlineOption)Enum.ToObject(typeof(ConfigFileSaveNewlineOption), active);
		if (((int)activeOption) > ((int)ConfigFileSaveNewlineOption.Specific)) //Positions higher than specific are always specific too
			activeOption = ConfigFileSaveNewlineOption.Specific;

		Base.Config.FileSaveNewlineOption = activeOption;
		/* If newline is specific, newlineOption=Specific and newline holds the newline type name */
		if (activeOption == ConfigFileSaveNewlineOption.Specific) {
			NewlineType chosenNewlineType = fileSaveNewline.ChosenNewlineType;
			if (!chosenNewlineType.Equals(NewlineType.Unknown)) {
				Base.Config.FileSaveNewline = chosenNewlineType;
			}
		}
		else {
			/* If newline option is remember last, use the system default */
			Base.Config.FileSaveNewline = Core.Util.GetSystemNewlineType();
		}
	}

	private void OnVideoAutoChooseFileToggled (object o, EventArgs args) {
		Base.Config.VideoAutoChooseFile = videoAutoChooseFileCheckButton.Active;
	}

	private void OnTranslationSaveAllToggled (object o, EventArgs args) {
		Base.Config.FileTranslationSaveAll = translationSaveAllCheckButton.Active;
	}

	private void OnAutoBackupToggled (object o, EventArgs args) {
		bool isActive = (o as CheckButton).Active;

		Base.Config.BackupAuto = isActive;
		autoBackupTimeSpinButton.Sensitive = isActive;

		Base.Backup.ReCheck();
	}

	private void OnAutoBackupTimeSpinButtonValueChanged (object o, EventArgs args) {
		Base.Config.BackupTime = (o as SpinButton).ValueAsInt * 60; //seconds
		Base.Backup.ReCheck();
	}

	private void OnReactionDelayToggled (object o, EventArgs args) {
		bool isActive = (o as CheckButton).Active;

		Base.Config.VideoApplyReactionDelay = isActive;
		reactionDelaySpinButton.Sensitive = isActive;
	}

	private void OnReactionDelaySpinButtonValueChanged (object o, EventArgs args) {
		Base.Config.VideoReactionDelay = (o as SpinButton).ValueAsInt;
	}

	private void OnVideoSeekToggled (object o, EventArgs args) {
		bool isActive = (o as CheckButton).Active;

		Base.Config.VideoSeekOnChange = isActive;
		videoSeekRewindSpinButton.Sensitive = isActive;
	}

	private void OnVideoSeekRewindSpinButtonValueChanged (object o, EventArgs args) {
		Base.Config.VideoSeekOnChangeRewind = (o as SpinButton).ValueAsInt;
	}

	private void OnSubtitleSplitSpinButtonValueChanged (object o, EventArgs args) {
		Base.Config.TimingsTimeBetweenSubtitles = (o as SpinButton).ValueAsInt;
	}

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Cancel) {
			ResetDialogToDefaults();
			return true;
		}
		else
			return false;
	}

}

}
