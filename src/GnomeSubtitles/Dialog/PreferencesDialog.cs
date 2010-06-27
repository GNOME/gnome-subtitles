/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2010 Pedro Castro
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

using Glade;
using GnomeSubtitles.Core;
using GnomeSubtitles.Ui.Component;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Dialog {

public class PreferencesDialog : GladeDialog {

	/* Constant strings */
	private const string gladeFilename = "PreferencesDialog.glade";

	/* Components */
	private EncodingComboBox fileOpenEncoding = null;
	private EncodingComboBox fileOpenFallbackEncoding = null;
	private EncodingComboBox fileSaveEncoding = null;
	private SubtitleFormatComboBox fileSaveFormat = null;
	private NewlineTypeComboBox fileSaveNewline = null;

	/* Widgets */
	[WidgetAttribute] private CheckButton translationSaveAllCheckButton = null;
	[WidgetAttribute] private CheckButton videoAutoChooseFileCheckButton = null;
	[WidgetAttribute] private CheckButton autoBackupCheckButton = null;
	[WidgetAttribute] private ComboBox fileOpenEncodingComboBox = null;
	[WidgetAttribute] private ComboBox fileOpenFallbackEncodingComboBox = null;
	[WidgetAttribute] private ComboBox fileSaveEncodingComboBox = null;
	[WidgetAttribute] private ComboBox fileSaveFormatComboBox = null;
	[WidgetAttribute] private ComboBox fileSaveNewlineComboBox = null;
	[WidgetAttribute] private SpinButton autoBackupTimeSpinButton = null;


	public PreferencesDialog () : base(gladeFilename, false) {
		LoadValues();
		Autoconnect();
	}

	/* Private members */
	
	private void LoadValues () {
		/* Translation Save All */
		translationSaveAllCheckButton.Active = Base.Config.PrefsTranslationSaveAll;

		/* Defaults */
		SetDefaultsFileOpenEncoding();
		SetDefaultsFileOpenFallbackEncoding();
		SetDefaultsFileSaveEncoding();
		SetDefaultsFileSaveFormat();
		SetDefaultsFileSaveNewline();

		/* Video Auto choose file */
		videoAutoChooseFileCheckButton.Active = Base.Config.PrefsVideoAutoChooseFile;
		
		/* Auto Backup */
		SetAutoBackup();
	}

	private void SetDefaultsFileOpenEncoding () {
		string[] additionalActions = { Catalog.GetString("Remember Last Used") };
		int fixedEncoding = -1;
		ConfigFileOpenEncodingOption fileOpenEncodingOption = Base.Config.PrefsDefaultsFileOpenEncodingOption;
		if (fileOpenEncodingOption == ConfigFileOpenEncodingOption.Specific) {
			string encodingName = Base.Config.PrefsDefaultsFileOpenEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingName, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		fileOpenEncoding = new EncodingComboBox(fileOpenEncodingComboBox, true, additionalActions, fixedEncoding);
		if (fileOpenEncodingOption != ConfigFileOpenEncodingOption.Specific) {
			fileOpenEncoding.ActiveSelection = (int)fileOpenEncodingOption;
		}
		fileOpenEncoding.SelectionChanged += OnDefaultsFileOpenEncodingChanged;
	}

	private void SetDefaultsFileOpenFallbackEncoding () {
		int fixedEncoding = -1;
		ConfigFileOpenFallbackEncoding fileOpenFallbackEncodingConfig = Base.Config.PrefsDefaultsFileOpenFallbackEncoding;
		if (fileOpenFallbackEncodingConfig == ConfigFileOpenFallbackEncoding.Fixed) {
			string encodingName = Base.Config.PrefsDefaultsFileOpenFallbackEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingName, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		fileOpenFallbackEncoding = new EncodingComboBox(fileOpenFallbackEncodingComboBox, false, null, fixedEncoding);
		fileOpenFallbackEncoding.SelectionChanged += OnDefaultsFileOpenFallbackEncodingChanged;
	}

	private void SetDefaultsFileSaveEncoding () {
		string[] additionalActions = { Catalog.GetString("Keep Existing"), Catalog.GetString("Remember Last Used") }; //TODO change label
		int fixedEncoding = -1;
		ConfigFileSaveEncodingOption fileSaveEncodingOption = Base.Config.PrefsDefaultsFileSaveEncodingOption;
		if (fileSaveEncodingOption == ConfigFileSaveEncodingOption.Specific) {
			string encodingName = Base.Config.PrefsDefaultsFileSaveEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingName, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		fileSaveEncoding = new EncodingComboBox(fileSaveEncodingComboBox, false, additionalActions, fixedEncoding);
		if (fileSaveEncodingOption != ConfigFileSaveEncodingOption.Specific) {
			fileSaveEncoding.ActiveSelection = (int)fileSaveEncodingOption;
		}
		fileSaveEncoding.SelectionChanged += OnDefaultsFileSaveEncodingChanged;
	}

	private void SetDefaultsFileSaveFormat () {
		string[] additionalActions = { Catalog.GetString("Keep Existing"), Catalog.GetString("Remember Last Used") }; //TODO change label
		SubtitleType fixedFormat = SubtitleType.Unknown;
		ConfigFileSaveFormatOption fileSaveFormatOption = Base.Config.PrefsDefaultsFileSaveFormatOption;
		if (fileSaveFormatOption == ConfigFileSaveFormatOption.Specific) {
			fixedFormat = Base.Config.PrefsDefaultsFileSaveFormatFixed;
		}

		fileSaveFormat = new SubtitleFormatComboBox(fileSaveFormatComboBox, fixedFormat, additionalActions);
		if (fileSaveFormatOption != ConfigFileSaveFormatOption.Specific) {
			fileSaveFormat.ActiveSelection = (int)fileSaveFormatOption;
		}
		fileSaveFormat.SelectionChanged += OnDefaultsFileSaveFormatChanged;
	}

	private void SetDefaultsFileSaveNewline () {
		string[] additionalActions = { Catalog.GetString("Remember Last Used") }; //TODO change label
		NewlineType newlineTypeToSelect = NewlineType.Unknown;
		ConfigFileSaveNewlineOption fileSaveNewlineOption = Base.Config.PrefsDefaultsFileSaveNewlineOption;
		if (fileSaveNewlineOption == ConfigFileSaveNewlineOption.Specific) {
			newlineTypeToSelect = Base.Config.PrefsDefaultsFileSaveNewline;
		}

		fileSaveNewline = new NewlineTypeComboBox(fileSaveNewlineComboBox, newlineTypeToSelect, additionalActions);
		if (fileSaveNewlineOption != ConfigFileSaveNewlineOption.Specific) {
			fileSaveNewline.ActiveSelection = (int)fileSaveNewlineOption;
		}
		fileSaveNewline.SelectionChanged += OnDefaultsFileSaveNewlineChanged;
	}
	
	private void SetAutoBackup () {
		bool autoBackupEnabled = Base.Config.PrefsBackupAutoBackup;
		autoBackupCheckButton.Active = autoBackupEnabled;
		autoBackupTimeSpinButton.Sensitive = autoBackupEnabled;
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
	}

	
	/* Event members */

	#pragma warning disable 169		//Disables warning about handlers not being used

	private void OnDefaultsFileOpenEncodingChanged (object o, EventArgs args) {
		int active = fileOpenEncoding.ActiveSelection;
		ConfigFileOpenEncodingOption activeOption = (ConfigFileOpenEncodingOption)Enum.ToObject(typeof(ConfigFileOpenEncodingOption), active);
		if (((int)activeOption) > ((int)ConfigFileOpenEncodingOption.Specific)) //Positions higher than specific are always specific too
			activeOption = ConfigFileOpenEncodingOption.Specific;

		Base.Config.PrefsDefaultsFileOpenEncodingOption = activeOption;
		/* If encoding option is specific, encodingOption=Specific and encoding holds the encoding name */
		if (activeOption == ConfigFileOpenEncodingOption.Specific) {
			EncodingDescription chosenEncoding = fileOpenEncoding.ChosenEncoding;
			if (!chosenEncoding.Equals(EncodingDescription.Empty)) {
				Base.Config.PrefsDefaultsFileOpenEncodingFixed = chosenEncoding.Name;
			}
		}
		else {
			/* If encoding is current locale, encoding holds current locale too, otherwise it just holds auto detect */
			ConfigFileOpenEncoding encodingToStore = ConfigFileOpenEncoding.AutoDetect;
			if (activeOption == ConfigFileOpenEncodingOption.CurrentLocale) {
				encodingToStore = ConfigFileOpenEncoding.CurrentLocale;
			}
			Base.Config.PrefsDefaultsFileOpenEncoding = encodingToStore;
		}
	}

	private void OnDefaultsFileOpenFallbackEncodingChanged (object o, EventArgs args) {
		if (fileOpenFallbackEncoding.IsChosenCurrentLocale)
			Base.Config.PrefsDefaultsFileOpenFallbackEncoding = ConfigFileOpenFallbackEncoding.CurrentLocale;
		else {
			EncodingDescription chosenEncoding = fileOpenFallbackEncoding.ChosenEncoding;
			if (!chosenEncoding.Equals(EncodingDescription.Empty)) {
				Base.Config.PrefsDefaultsFileOpenFallbackEncodingFixed = chosenEncoding.Name;
			}
		}
	}

	private void OnDefaultsFileSaveEncodingChanged (object o, EventArgs args) {
		int active = fileSaveEncoding.ActiveSelection;
		ConfigFileSaveEncodingOption activeOption = (ConfigFileSaveEncodingOption)Enum.ToObject(typeof(ConfigFileSaveEncodingOption), active);
		if (((int)activeOption) > ((int)ConfigFileSaveEncodingOption.Specific)) //Positions higher than specific are always specific too
			activeOption = ConfigFileSaveEncodingOption.Specific;

		Base.Config.PrefsDefaultsFileSaveEncodingOption = activeOption;
		/* If encoding is specific, encodingOption=Specific and encoding holds the encoding name */
		if (activeOption == ConfigFileSaveEncodingOption.Specific) {
			EncodingDescription chosenEncoding = fileSaveEncoding.ChosenEncoding;
			if (!chosenEncoding.Equals(EncodingDescription.Empty)) {
				Base.Config.PrefsDefaultsFileSaveEncodingFixed = chosenEncoding.Name;
			}
		}
		else {
			/* If encoding option is current locale, encoding holds current locale too, otherwise it just holds keep existing */
			ConfigFileSaveEncoding encodingToStore = ConfigFileSaveEncoding.KeepExisting;
			if (activeOption == ConfigFileSaveEncodingOption.CurrentLocale) {
				encodingToStore = ConfigFileSaveEncoding.CurrentLocale;
			}
			Base.Config.PrefsDefaultsFileSaveEncoding = encodingToStore;
		}
	}

	private void OnDefaultsFileSaveFormatChanged (object o, EventArgs args) {
		int active = fileSaveFormat.ActiveSelection;
		ConfigFileSaveFormatOption activeOption = (ConfigFileSaveFormatOption)Enum.ToObject(typeof(ConfigFileSaveFormatOption), active);
		if (((int)activeOption) > ((int)ConfigFileSaveFormatOption.Specific)) //Positions higher than specific are always specific too
			activeOption = ConfigFileSaveFormatOption.Specific;

		Base.Config.PrefsDefaultsFileSaveFormatOption = activeOption;
		/* If format is specific, formatOption=Specific and format holds the format name */
		if (activeOption == ConfigFileSaveFormatOption.Specific) {
			SubtitleType chosenFormat = fileSaveFormat.ChosenSubtitleType;
			if (!chosenFormat.Equals(SubtitleType.Unknown)) {
				Base.Config.PrefsDefaultsFileSaveFormatFixed = chosenFormat;
			}
		}
		else {
			/* If format option is keep existing or remember last, use keep existing */
			Base.Config.PrefsDefaultsFileSaveFormat = ConfigFileSaveFormat.KeepExisting;
		}
	}

	private void OnDefaultsFileSaveNewlineChanged (object o, EventArgs args) {
		int active = fileSaveNewline.ActiveSelection;
		ConfigFileSaveNewlineOption activeOption = (ConfigFileSaveNewlineOption)Enum.ToObject(typeof(ConfigFileSaveNewlineOption), active);
		if (((int)activeOption) > ((int)ConfigFileSaveNewlineOption.Specific)) //Positions higher than specific are always specific too
			activeOption = ConfigFileSaveNewlineOption.Specific;

		Base.Config.PrefsDefaultsFileSaveNewlineOption = activeOption;
		/* If newline is specific, newlineOption=Specific and newline holds the newline type name */
		if (activeOption == ConfigFileSaveNewlineOption.Specific) {
			NewlineType chosenNewlineType = fileSaveNewline.ChosenNewlineType;
			if (!chosenNewlineType.Equals(NewlineType.Unknown)) {
				Base.Config.PrefsDefaultsFileSaveNewline = chosenNewlineType;
			}
		}
		else {
			/* If newline option is remember last, use the system default */
			Base.Config.PrefsDefaultsFileSaveNewline = Core.Util.GetSystemNewlineType();
		}
	}

	private void OnVideoAutoChooseFileToggled (object o, EventArgs args) {
		Base.Config.PrefsVideoAutoChooseFile = videoAutoChooseFileCheckButton.Active;
	}

	private void OnTranslationSaveAllToggled (object o, EventArgs args) {
		Base.Config.PrefsTranslationSaveAll = translationSaveAllCheckButton.Active;
	}
	
	private void OnAutoBackupToggled (object o, EventArgs args) {
		bool isActive = (o as CheckButton).Active;
		
		Base.Config.PrefsBackupAutoBackup = isActive;
		autoBackupTimeSpinButton.Sensitive = isActive;
		
		Base.Backup.ReCheck();
	}
	
	private void OnAutoBackupTimeSpinButonValueChanged (object o, EventArgs args) {
		Base.Config.PrefsBackupBackupTime = (o as SpinButton).ValueAsInt * 60; //seconds
		Base.Backup.ReCheck();
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
