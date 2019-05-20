/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2019 Pedro Castro
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

using SubLib.Core.Domain;
using SubLib.Util;
using System;

namespace GnomeSubtitles.Core {

/* Enumerations */
public enum ConfigFileOpenEncodingOption { AutoDetect = 0, RememberLastUsed = 1, CurrentLocale = 3, Specific = 4 }; //Values match ordering where the options are used
public enum ConfigFileOpenEncoding { AutoDetect = 0, CurrentLocale = 2, Fixed = 3 };
public enum ConfigFileOpenFallbackEncoding { CurrentLocale = 0, Fixed = 1 };
public enum ConfigFileSaveEncodingOption { KeepExisting = 0, RememberLastUsed = 1, CurrentLocale = 3, Specific = 4 }; //Values match ordering where the options are used
public enum ConfigFileSaveEncoding { KeepExisting = -1, CurrentLocale = 0, Fixed = 1 }; //KeepExisting=-1 because it doesn't appear
public enum ConfigFileSaveFormatOption { KeepExisting = 0, RememberLastUsed = 1, Specific = 3 }; //Values match ordering where the options are used
public enum ConfigFileSaveFormat { KeepExisting = -1, Fixed = 0 }; //KeepExisting=-1 because it doesn't appear
public enum ConfigFileSaveNewlineOption { RememberLastUsed = 0, Specific = 2 }; //Values match ordering where the options are used

/* Note: the GSettings backend will be used if the GnomeSubtitles schema is installed. Otherwise an in-memory
 * backend will be used. The later is only meant to be used for development, as no schema may have been installed.
 * When changing the schema file, in order to run the application from the development workspace,
 * the new schema needs to be installed to a dir recognized by GSettings. The XML file needs to be placed
 * inside /usr/share/glib-2.0/schemas (location may vary - the glib-2.0 dir must be found inside one of
 * the $XDG_DATA_DIRS) and 'glib-compile-schemas .' must be run on that dir. This will update the system-wide
 * 'gschemas.compiled' file and make the new schema recognizable.
 */
public class Config {

	/* Keys */
	private const string KeyFileEncodingsShownInMenu = "file-encodings-shown-in-menu";
	private const string KeyFileOpenEncodingOption = "file-open-encoding-option";
	private const string KeyFileOpenEncoding = "file-open-encoding";
	private const string KeyFileOpenFallbackEncoding = "file-open-fallback";
	private const string KeyFileSaveEncodingOption = "file-save-encoding-option";
	private const string KeyFileSaveEncoding = "file-save-encoding";
	private const string KeyFileSaveFormatOption = "file-save-format-option";
	private const string KeyFileSaveFormat = "file-save-format";
	private const string KeyFileSaveNewlineOption = "file-save-newline-option";
	private const string KeyFileSaveNewline = "file-save-newline";
	private const string KeyFileTranslationSaveAll = "file-translation-save-all";

	private const string KeyVideoAutoChooseFile = "video-auto-choose-file";
	private const string KeyVideoApplyReactionDelay = "video-apply-reaction-delay";
	private const string KeyVideoReactionDelay = "video-reaction-delay";
	private const string KeyVideoSeekOnChange = "video-seek-on-change";
	private const string KeyVideoSeekOnChangeRewind = "video-seek-on-change-rewind";

	private const string KeyViewLineLengths = "view-line-lengths";
	private const string KeyViewWindowHeight = "view-window-height";
	private const string KeyViewWindowWidth = "view-window-width";

	private const string KeySpellCheckTextLanguage = "spellcheck-text-language";
	private const string KeySpellCheckTranslationLanguage = "spellcheck-translation-language";
	private const string KeySpellCheckAuto = "spellcheck-auto";

	private const string KeyBackupAuto = "backup-auto";
	private const string KeyBackupTime = "backup-time";

	private const string KeyTimingsTimeStep = "timings-time-step"; //Not editable in the Preferences dialog
	private const string KeyTimingsFramesStep = "timings-frames-step"; //Not editable in the Preferences dialog
	private const string KeyTimingsTimeBetweenSubtitles = "timings-time-between-subs";

	/* Constant default values */
	private const string DefaultEncoding = "ISO-8859-15";

	private IConfigBackend settings = null;


	public Config () {
		try {
			settings = new ConfigBackendGSettings();
		} catch (ConfigBackendUnavailableException) {
			Logger.Error("Unable to initialize the GSettings configuration. This means the schema was not properly installed "
				+ "in this distro and the gnome-subtitles package should be fixed. This is ok if you're trying the application without "
				+ "installing it, for example in a development branch.");
		} catch(Exception e) {
			Logger.Error(e, "Unable to initialize the GSettings config backend. Reverting to an in-memory backend (no settings will be saved!).");
		}
		
		if (settings == null) {
			Logger.Error("Reverting to an in-memory configuration backend. No settings will be persisted when the application closes.");
			settings = new ConfigBackendInMemory();
		}
	}

	/* Public properties */

	public string[] FileEncodingsShownInMenu {
		get {
			string[] defaultValue = { DefaultEncoding };
			return GetStrings(KeyFileEncodingsShownInMenu, defaultValue);
		}
		set { SetStrings(KeyFileEncodingsShownInMenu, value); }
	}

	public ConfigFileOpenEncodingOption FileOpenEncodingOption {
		get { return (ConfigFileOpenEncodingOption)GetEnumValue(KeyFileOpenEncodingOption, ConfigFileOpenEncodingOption.AutoDetect); }
		set { Set(KeyFileOpenEncodingOption, value.ToString()); }
	}

	public ConfigFileOpenEncoding FileOpenEncoding {
		get { return (ConfigFileOpenEncoding)GetEnumValueFromSuperset(KeyFileOpenEncoding, ConfigFileOpenEncoding.Fixed); }
		set { Set(KeyFileOpenEncoding, value.ToString()); }
	}

	/* Uses the same key as FileOpenEncoding but is used when there's a specific encoding set */
	public string FileOpenEncodingFixed {
		get { return GetString(KeyFileOpenEncoding, DefaultEncoding); }
		set { Set(KeyFileOpenEncoding, value); }
	}

	public ConfigFileOpenFallbackEncoding FileOpenFallbackEncoding {
		get { return (ConfigFileOpenFallbackEncoding)GetEnumValueFromSuperset(KeyFileOpenFallbackEncoding, ConfigFileOpenFallbackEncoding.Fixed); }
		set { Set(KeyFileOpenFallbackEncoding, value.ToString()); }
	}

	/* Uses the same key as FileOpenFallbackEncoding but is used when there's a specific encoding set */
	public string FileOpenFallbackEncodingFixed {
		get { return GetString(KeyFileOpenFallbackEncoding, DefaultEncoding); }
		set { Set(KeyFileOpenFallbackEncoding, value); }
	}

	public ConfigFileSaveEncodingOption FileSaveEncodingOption {
		get { return (ConfigFileSaveEncodingOption)GetEnumValue(KeyFileSaveEncodingOption, ConfigFileSaveEncodingOption.KeepExisting); }
		set { Set(KeyFileSaveEncodingOption, value.ToString()); }
	}

	public ConfigFileSaveEncoding FileSaveEncoding {
		get { return (ConfigFileSaveEncoding)GetEnumValueFromSuperset(KeyFileSaveEncoding, ConfigFileSaveEncoding.Fixed); }
		set { Set(KeyFileSaveEncoding, value.ToString()); }
	}

	/* Uses the same key as FileSaveEncoding but is used when there's a specific encoding set */
	public string FileSaveEncodingFixed {
		get { return GetString(KeyFileSaveEncoding, DefaultEncoding); }
		set { Set(KeyFileSaveEncoding, value); }
	}

	public ConfigFileSaveFormatOption FileSaveFormatOption {
		get { return (ConfigFileSaveFormatOption)GetEnumValue(KeyFileSaveFormatOption, ConfigFileSaveFormatOption.KeepExisting); }
		set { Set(KeyFileSaveFormatOption, value.ToString()); }
	}

	public ConfigFileSaveFormat FileSaveFormat {
		get { return (ConfigFileSaveFormat)GetEnumValueFromSuperset(KeyFileSaveFormat, ConfigFileSaveFormat.Fixed); }
		set { Set(KeyFileSaveFormat, value.ToString()); }
	}

	/* Uses the same key as FileSaveFormat but is used when there's a specific format set */
	public SubtitleType FileSaveFormatFixed {
		get { return (SubtitleType)GetEnumValueFromSuperset(KeyFileSaveFormat, SubtitleType.SubRip); }
		set { Set(KeyFileSaveFormat, value.ToString()); }
	}

	public ConfigFileSaveNewlineOption FileSaveNewlineOption {
		get { return (ConfigFileSaveNewlineOption)GetEnumValue(KeyFileSaveNewlineOption, ConfigFileSaveNewlineOption.Specific); }
		set { Set(KeyFileSaveNewlineOption, value.ToString()); }
	}

	public NewlineType FileSaveNewline {
		get { return (NewlineType)GetEnumValue(KeyFileSaveNewline, NewlineType.Windows); }
		set { Set(KeyFileSaveNewline, value.ToString()); }
	}

	public bool FileTranslationSaveAll {
		get { return GetBool(KeyFileTranslationSaveAll, true); }
		set { Set(KeyFileTranslationSaveAll, value); }
	}

	public bool VideoAutoChooseFile {
		get { return GetBool(KeyVideoAutoChooseFile, true); }
		set { Set(KeyVideoAutoChooseFile, value); }
	}

	public bool VideoApplyReactionDelay {
		get {
			return GetBool(KeyVideoApplyReactionDelay, false);
		}
		set {
			Set(KeyVideoApplyReactionDelay, value);
		}
	}

	public int VideoReactionDelay {
		get {
			return GetInt(KeyVideoReactionDelay, 200, 0, true, 2000, true);
		}
		set {
			Set(KeyVideoReactionDelay, value);
		}
	}

	public bool VideoSeekOnChange {
		get {
			return GetBool(KeyVideoSeekOnChange, true);
		}
		set {
			Set(KeyVideoSeekOnChange, value);
		}
	}

	public int VideoSeekOnChangeRewind {
		get {
			return GetInt(KeyVideoSeekOnChangeRewind, 200, 0, true, 2000, true);
		}
		set {
			Set(KeyVideoSeekOnChangeRewind, value);
		}
	}

	public bool ViewLineLengths {
		get {
			return GetBool(KeyViewLineLengths, true);
		}
		set {
			Set(KeyViewLineLengths, value);
		}
	}

	public int ViewWindowHeight {
		get { return GetInt(KeyViewWindowHeight, 600, 200, true, 0, false); }
		set { Set(KeyViewWindowHeight, value); }
	}

	public int ViewWindowWidth {
		get { return GetInt(KeyViewWindowWidth, 690, 200, true, 0, false); }
		set { Set(KeyViewWindowWidth, value); }
	}

	public string SpellCheckTextLanguage {
		get { return GetString(KeySpellCheckTextLanguage, String.Empty); }
		set { Set(KeySpellCheckTextLanguage, value); }
	}

	public string SpellCheckTranslationLanguage {
		get { return GetString(KeySpellCheckTranslationLanguage, String.Empty); }
		set { Set(KeySpellCheckTranslationLanguage, value); }
	}

	public bool SpellCheckAuto {
		get { return GetBool(KeySpellCheckAuto, false); }
		set { Set(KeySpellCheckAuto, value); }
	}

	//Even though the default in gconf schema is true, if gconf is not working we're using false
	public bool BackupAuto {
		get { return GetBool(KeyBackupAuto, false); }
		set { Set(KeyBackupAuto, value); }
	}

	public int BackupTime {
		get { return GetInt(KeyBackupTime, 120, 30, true, 0, false); }
		set { Set(KeyBackupTime, value); }
	}

	/* Time in milliseconds */
	public int TimingsTimeStep {
		get {
			return GetInt(KeyTimingsTimeStep, 100, 1, true, 2000, true);
		}
		set {
			Set(KeyTimingsTimeStep, value);
		}
	}

	public int TimingsFramesStep {
		get {
			return GetInt(KeyTimingsFramesStep, 2, 1, true, 60, true);
		}
		set {
			Set(KeyTimingsFramesStep, value);
		}
	}

	public int TimingsTimeBetweenSubtitles {
		get {
			return GetInt(KeyTimingsTimeBetweenSubtitles, 100, 0, true, 2000, true);
		}
		set {
			Set(KeyTimingsTimeBetweenSubtitles, value);
		}
	}


	/* Private members */

	private string GetString (string key, string defaultValue) {
		try {
			return settings.GetString(key);
		}
		catch (Exception e) {
			Logger.Error(e);
			return defaultValue;
		}
	}

	private bool GetBool (string key, bool defaultValue) {
		try {
			return settings.GetBoolean(key);
		}
		catch (Exception e) {
			Logger.Error(e);
			return defaultValue;
		}
	}

	private int GetInt (string key, int defaultValue, int lowerLimit, bool useLowerLimit, int upperLimit, bool useUpperLimit) {
		try {
			int number = settings.GetInt(key);
			if (useLowerLimit && (number < lowerLimit))
				return defaultValue;

			if (useUpperLimit && (number > upperLimit))
				return defaultValue;

			return number;
		}
		catch (Exception e) {
			Logger.Error(e);
			return defaultValue;
		}
	}

	private string[] GetStrings (string key, string[] defaultValue) {
		try {
			string[] strings = settings.GetStrings(key);
			if ((strings.Length == 1) && (strings[0] == String.Empty))
				return new string[0];
			else
				return strings;
		}
		catch (Exception e) {
			Logger.Error(e);
			return defaultValue;
		}
	}

	/* Gets an enum value from a field which can hold a value not included in the enum (basically assumes an exception can occur). */
	private Enum GetEnumValueFromSuperset (string key, Enum defaultValue) {
		try {
			string stringValue = settings.GetString(key);
			return (Enum)Enum.Parse(defaultValue.GetType(), stringValue);
		}
		catch (Exception) {
			return defaultValue;
		}
	}

	private Enum GetEnumValue (string key, Enum defaultValue) {
		try {
			string stringValue = settings.GetString(key);
			return (Enum)Enum.Parse(defaultValue.GetType(), stringValue);
		}
		catch (Exception e) {
			Logger.Error(e);
			return defaultValue;
		}
	}

	private void SetStrings (string key, string[] values) {
		try {
			settings.SetStrings(key, values);
		}
		catch (Exception e) {
			Logger.Error(e);
		}
	}

	private void Set(string key, string value) {
		try {
			settings.SetString(key, value);
		}
		catch (Exception e) {
			Logger.Error(e);
		}
	}

	private void Set(string key, bool value) {
		try {
			settings.SetBoolean(key, value);
		}
		catch (Exception e) {
			Logger.Error(e);
		}
	}

	private void Set(string key, int value) {
		try {
			settings.SetInt(key, value);
		}
		catch (Exception e) {
			Logger.Error(e);
		}
	}

}

}