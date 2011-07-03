/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2011 Pedro Castro
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

using GConf;
using SubLib.Core.Domain;
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

public class Config {
	private Client client = null;
	
	/* Constant prefix strings */
	private const string keyPrefix = "/apps/gnome-subtitles/";
	private const string keyPrefs = keyPrefix + "preferences/";
	private const string keyPrefsEncodings = keyPrefs + "encodings/";
	private const string keyPrefsSpellCheck = keyPrefs + "spellcheck/";
	private const string keyPrefsVideo = keyPrefs + "video/";
	private const string keyPrefsView = keyPrefs + "view/";
	private const string keyPrefsWindow = keyPrefs + "window/";
	private const string keyPrefsDefaults = keyPrefs + "defaults/";
	private const string keyPrefsTranslation = keyPrefs + "translation/";
	private const string keyPrefsBackup = keyPrefs + "backup/";
	private const string keyPrefsTimings = keyPrefs + "timings/";

	/* Constant key strings */
	private const string keyPrefsEncodingsShownInMenu = keyPrefsEncodings + "shown_in_menu";
	private const string keyPrefsTranslationSaveAll = keyPrefsTranslation + "save_all";
	private const string keyPrefsVideoAutoChooseFile = keyPrefsVideo + "auto_choose_file";
	private const string keyPrefsVideoApplyReactionDelay = keyPrefsVideo + "apply_reaction_delay";
	private const string keyPrefsVideoReactionDelay = keyPrefsVideo + "reaction_delay";
	private const string keyPrefsVideoSeekOnChange = keyPrefsVideo + "seek_on_change"; //FIXME add option to the Preferences Dialog
	private const string keyPrefsVideoSeekOnChangeRewind = keyPrefsVideo + "seek_on_change_rewind"; //FIXME add option to the Preferences Dialog
	private const string keyPrefsViewLineLengths = keyPrefsView + "line_lengths";
	private const string keyPrefsSpellCheckActiveTextLanguage = keyPrefsSpellCheck + "active_text_language";
	private const string keyPrefsSpellCheckActiveTranslationLanguage = keyPrefsSpellCheck + "active_translation_language";
	private const string keyPrefsSpellCheckAutocheck = keyPrefsSpellCheck + "autocheck";
	private const string keyPrefsWindowHeight = keyPrefsWindow + "height";
	private const string keyPrefsWindowWidth = keyPrefsWindow + "width";
	private const string keyPrefsDefaultsFileOpenEncodingOption = keyPrefsDefaults + "file_open_encoding_option";
	private const string keyPrefsDefaultsFileOpenEncoding = keyPrefsDefaults + "file_open_encoding";
	private const string keyPrefsDefaultsFileOpenFallbackEncoding = keyPrefsDefaults + "file_open_fallback";
	private const string keyPrefsDefaultsFileSaveEncodingOption = keyPrefsDefaults + "file_save_encoding_option";
	private const string keyPrefsDefaultsFileSaveEncoding = keyPrefsDefaults + "file_save_encoding";
	private const string keyPrefsDefaultsFileSaveFormatOption = keyPrefsDefaults + "file_save_format_option";
	private const string keyPrefsDefaultsFileSaveFormat = keyPrefsDefaults + "file_save_format";
	private const string keyPrefsDefaultsFileSaveNewlineOption = keyPrefsDefaults + "file_save_newline_option";
	private const string keyPrefsDefaultsFileSaveNewline = keyPrefsDefaults + "file_save_newline";
	private const string keyPrefsBackupAutoBackup = keyPrefsBackup + "auto_backup";
	private const string keyPrefsBackupBackupTime = keyPrefsBackup + "backup_time";
	private const string keyPrefsTimingsTimeStep = keyPrefsTimings + "time_step"; //FIXME add option to the Preferences Dialog
	private const string keyPrefsTimingsFramesStep = keyPrefsTimings + "frames_step"; //FIXME add option to the Preferences Dialog
	private const string keyPrefsTimingsTimeBetweenSubtitles = keyPrefsTimings + "time_between_subs"; //FIXME add option to the Preferences Dialog
	
	/* Cached values */
	private bool isValuePrefsVideoApplyReactionDelayCached = false;
	private bool valuePrefsVideoApplyReactionDelay = false;
	private int valuePrefsVideoReactionDelay = -1;
	private bool isValuePrefsVideoSeekOnChangeCached = false;
	private bool valuePrefsVideoSeekOnChange = false;
	private int valuePrefsVideoSeekOnChangeRewind = -1;
	private int valuePrefsTimingsTimeStep = -1;
	private int valuePrefsTimingsFramesStep = -1;
	private int valuePrefsTimingsTimeBetweenSubtitles = -1;
	

	public Config () {
		client = new Client();
	}
	
	/* Public properties */

	public string[] PrefsEncodingsShownInMenu {
		get {
			string[] defaultValue = { "ISO-8859-15" };
			return GetStrings(keyPrefsEncodingsShownInMenu, defaultValue);
		}
		set { SetStrings(keyPrefsEncodingsShownInMenu, value); }
	}
	
	public string PrefsSpellCheckActiveTextLanguage {
		get { return GetString(keyPrefsSpellCheckActiveTextLanguage, String.Empty); }
		set { Set(keyPrefsSpellCheckActiveTextLanguage, value); }
	}
	
	public string PrefsSpellCheckActiveTranslationLanguage {
		get { return GetString(keyPrefsSpellCheckActiveTranslationLanguage, String.Empty); }
		set { Set(keyPrefsSpellCheckActiveTranslationLanguage, value); }
	}
	
	public bool PrefsSpellCheckAutocheck {
		get { return GetBool(keyPrefsSpellCheckAutocheck, false); }
		set { Set(keyPrefsSpellCheckAutocheck, value); }
	}
	
	public bool PrefsVideoAutoChooseFile {
		get { return GetBool(keyPrefsVideoAutoChooseFile, true); }
		set { Set(keyPrefsVideoAutoChooseFile, value); }
	}
	
	public bool PrefsVideoApplyReactionDelay {
		get {
			if (!isValuePrefsVideoApplyReactionDelayCached) {
				this.valuePrefsVideoApplyReactionDelay = GetBool(keyPrefsVideoApplyReactionDelay, false);
				this.isValuePrefsVideoApplyReactionDelayCached = true;
			}
			return valuePrefsVideoApplyReactionDelay;
		}
		set {
			Set(keyPrefsVideoApplyReactionDelay, value);
			this.valuePrefsVideoApplyReactionDelay = value;
			this.isValuePrefsVideoApplyReactionDelayCached = true;
		}
	}

	public int PrefsVideoReactionDelay {
		get {
			if (this.valuePrefsVideoReactionDelay == -1) {
				this.valuePrefsVideoReactionDelay = GetInt(keyPrefsVideoReactionDelay, 200, 0, true, 2000, true);
			}
			return this.valuePrefsVideoReactionDelay;
		}
		set {
			Set(keyPrefsVideoReactionDelay, value);
			this.valuePrefsVideoReactionDelay = value;
		}
	}
	
	public bool PrefsVideoSeekOnChange {
		get {
			if (!isValuePrefsVideoSeekOnChangeCached) {
				this.valuePrefsVideoSeekOnChange = GetBool(keyPrefsVideoSeekOnChange, true);
				this.isValuePrefsVideoSeekOnChangeCached = true;
			}
			return valuePrefsVideoSeekOnChange;
		}
		set {
			Set(keyPrefsVideoSeekOnChange, value);
			this.valuePrefsVideoSeekOnChange = value;
			this.isValuePrefsVideoSeekOnChangeCached = true;
		}
	}

	public int PrefsVideoSeekOnChangeRewind {
		get {
			if (this.valuePrefsVideoSeekOnChangeRewind == -1) {
				this.valuePrefsVideoSeekOnChangeRewind = GetInt(keyPrefsVideoSeekOnChangeRewind, 200, 0, true, 2000, true);
			}
			return this.valuePrefsVideoSeekOnChangeRewind;
		}
		set {
			Set(keyPrefsVideoSeekOnChangeRewind, value);
			this.valuePrefsVideoSeekOnChangeRewind = value;
		}
	}
	
	public bool PrefsViewLineLengths {
		get { return GetBool(keyPrefsViewLineLengths, true); }
		set { Set(keyPrefsViewLineLengths, value); }
	}
	
	public int PrefsWindowHeight {
		get { return GetInt(keyPrefsWindowHeight, 600, 200, true, 0, false); }
		set { Set(keyPrefsWindowHeight, value); }
	}
	
	public int PrefsWindowWidth {
		get { return GetInt(keyPrefsWindowWidth, 690, 200, true, 0, false); }
		set { Set(keyPrefsWindowWidth, value); }
	}

	public ConfigFileOpenEncodingOption PrefsDefaultsFileOpenEncodingOption {
		get { return (ConfigFileOpenEncodingOption)GetEnumValue(keyPrefsDefaultsFileOpenEncodingOption, ConfigFileOpenEncodingOption.AutoDetect); }
		set { Set(keyPrefsDefaultsFileOpenEncodingOption, value.ToString()); }
	}

	public ConfigFileOpenEncoding PrefsDefaultsFileOpenEncoding {
		get { return (ConfigFileOpenEncoding)GetEnumValueFromSuperset(keyPrefsDefaultsFileOpenEncoding, ConfigFileOpenEncoding.Fixed); }
		set { Set(keyPrefsDefaultsFileOpenEncoding, value.ToString()); }
	}

	/* Uses the same key as PrefsDefaultsFileOpenEncoding but is used when there's a specific encoding set */
	public string PrefsDefaultsFileOpenEncodingFixed {
		get { return GetString(keyPrefsDefaultsFileOpenEncoding, "ISO-8859-15"); }
		set { Set(keyPrefsDefaultsFileOpenEncoding, value); }
	}

	public ConfigFileOpenFallbackEncoding PrefsDefaultsFileOpenFallbackEncoding {
		get { return (ConfigFileOpenFallbackEncoding)GetEnumValueFromSuperset(keyPrefsDefaultsFileOpenFallbackEncoding, ConfigFileOpenFallbackEncoding.Fixed); }
		set { Set(keyPrefsDefaultsFileOpenFallbackEncoding, value.ToString()); }
	}

	/* Uses the same key as PrefsDefaultsFileOpenFallbackEncoding but is used when there's a specific encoding set */
	public string PrefsDefaultsFileOpenFallbackEncodingFixed {
		get { return GetString(keyPrefsDefaultsFileOpenFallbackEncoding, "ISO-8859-15"); }
		set { Set(keyPrefsDefaultsFileOpenFallbackEncoding, value); }
	}

	public ConfigFileSaveEncodingOption PrefsDefaultsFileSaveEncodingOption {
		get { return (ConfigFileSaveEncodingOption)GetEnumValue(keyPrefsDefaultsFileSaveEncodingOption, ConfigFileSaveEncodingOption.KeepExisting); }
		set { Set(keyPrefsDefaultsFileSaveEncodingOption, value.ToString()); }
	}

	public ConfigFileSaveEncoding PrefsDefaultsFileSaveEncoding {
		get { return (ConfigFileSaveEncoding)GetEnumValueFromSuperset(keyPrefsDefaultsFileSaveEncoding, ConfigFileSaveEncoding.Fixed); }
		set { Set(keyPrefsDefaultsFileSaveEncoding, value.ToString()); }
	}

	/* Uses the same key as PrefsDefaultsFileSaveEncoding but is used when there's a specific encoding set */
	public string PrefsDefaultsFileSaveEncodingFixed {
		get { return GetString(keyPrefsDefaultsFileSaveEncoding, "ISO-8859-15"); }
		set { Set(keyPrefsDefaultsFileSaveEncoding, value); }
	}

	public ConfigFileSaveFormatOption PrefsDefaultsFileSaveFormatOption {
		get { return (ConfigFileSaveFormatOption)GetEnumValue(keyPrefsDefaultsFileSaveFormatOption, ConfigFileSaveFormatOption.KeepExisting); }
		set { Set(keyPrefsDefaultsFileSaveFormatOption, value.ToString()); }
	}

	public ConfigFileSaveFormat PrefsDefaultsFileSaveFormat {
		get { return (ConfigFileSaveFormat)GetEnumValueFromSuperset(keyPrefsDefaultsFileSaveFormat, ConfigFileSaveFormat.Fixed); }
		set { Set(keyPrefsDefaultsFileSaveFormat, value.ToString()); }
	}

	/* Uses the same key as PrefsDefaultsFileSaveFormat but is used when there's a specific format set */
	public SubtitleType PrefsDefaultsFileSaveFormatFixed {
		get { return (SubtitleType)GetEnumValueFromSuperset(keyPrefsDefaultsFileSaveFormat, SubtitleType.SubRip); }
		set { Set(keyPrefsDefaultsFileSaveFormat, value.ToString()); }
	}

	public ConfigFileSaveNewlineOption PrefsDefaultsFileSaveNewlineOption {
		get { return (ConfigFileSaveNewlineOption)GetEnumValue(keyPrefsDefaultsFileSaveNewlineOption, ConfigFileSaveNewlineOption.Specific); }
		set { Set(keyPrefsDefaultsFileSaveNewlineOption, value.ToString()); }
	}

	public NewlineType PrefsDefaultsFileSaveNewline {
		get { return (NewlineType)GetEnumValue(keyPrefsDefaultsFileSaveNewline, NewlineType.Windows); }
		set { Set(keyPrefsDefaultsFileSaveNewline, value.ToString()); }
	}

	public bool PrefsTranslationSaveAll {
		get { return GetBool(keyPrefsTranslationSaveAll, true); }
		set { Set(keyPrefsTranslationSaveAll, value); }
	}
	
	//Even though the default in gconf schema is true, if gconf is not working we're using false
	public bool PrefsBackupAutoBackup {
		get { return GetBool(keyPrefsBackupAutoBackup, false); }
		set { Set(keyPrefsBackupAutoBackup, value); }
	}
 
	public int PrefsBackupBackupTime {
		get { return GetInt(keyPrefsBackupBackupTime, 120, 30, true, 0, false); }
		set { Set(keyPrefsBackupBackupTime, value); }
	}
	
	/* Time in milliseconds */
	public int PrefsTimingsTimeStep {
		get {
			if (this.valuePrefsTimingsTimeStep == -1) {
				this.valuePrefsTimingsTimeStep = GetInt(keyPrefsTimingsTimeStep, 100, 1, true, 2000, true);
			}
			return this.valuePrefsTimingsTimeStep;
		}
		set {
			Set(keyPrefsTimingsTimeStep, value);
			this.valuePrefsTimingsTimeStep = value;
		}
	}
	
	public int PrefsTimingsFramesStep {
		get {
			if (this.valuePrefsTimingsFramesStep == -1) {
				this.valuePrefsTimingsFramesStep = GetInt(keyPrefsTimingsFramesStep, 2, 1, true, 60, true);
			}
			return this.valuePrefsTimingsFramesStep;
		}
		set {
			Set(keyPrefsTimingsFramesStep, value);
			this.valuePrefsTimingsFramesStep = value;
		}
	}
	
	public int PrefsTimingsTimeBetweenSubtitles {
		get {
			if (this.valuePrefsTimingsTimeBetweenSubtitles == -1) {
				this.valuePrefsTimingsTimeBetweenSubtitles = GetInt(keyPrefsTimingsTimeBetweenSubtitles, 100, 0, true, 2000, true);
			}
			return this.valuePrefsTimingsTimeBetweenSubtitles;
		}
		set {
			Set(keyPrefsTimingsTimeBetweenSubtitles, value);
			this.valuePrefsTimingsTimeBetweenSubtitles = value;
		}
	}

	
	/* Private members */
	
	private string GetString (string key, string defaultValue) {
		try {
			return (string)client.Get(key);
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
			return defaultValue;
		}
	}
	
	private bool GetBool (string key, bool defaultValue) {
		try {
			return (bool)client.Get(key);
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
			return defaultValue;
		}
	}

	/* private int GetInt (string key, int defaultValue) {
		try {
			return (int)client.Get(key);
		}
		catch (Exception) {
			return defaultValue;
		}
	}*/
	
	private int GetInt (string key, int defaultValue, int lowerLimit, bool useLowerLimit, int upperLimit, bool useUpperLimit) {
		try {
			int number = (int)client.Get(key);
			if (useLowerLimit && (number < lowerLimit))
				return defaultValue;
			
			if (useUpperLimit && (number > upperLimit))
				return defaultValue;
			
			return number;
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
			return defaultValue;
		}
	}
	
	private string[] GetStrings (string key, string[] defaultValue) {
		try {
			string[] strings = client.Get(key) as string[];
			if ((strings.Length == 1) && (strings[0] == String.Empty))
				return new string[0];
			else
				return strings;
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
			return defaultValue;
		}
	}

	/* Gets an enum value from a field which can hold a value not included in the enum (basically assumes an exception can occur). */
	private Enum GetEnumValueFromSuperset (string key, Enum defaultValue) {
		try {
			string stringValue = (string)client.Get(key);
			return (Enum)Enum.Parse(defaultValue.GetType(), stringValue);
		}
		catch (Exception) {
			return defaultValue;
		}
	}

	private Enum GetEnumValue (string key, Enum defaultValue) {
		try {
			string stringValue = (string)client.Get(key);
			return (Enum)Enum.Parse(defaultValue.GetType(), stringValue);
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
			return defaultValue;
		}
	}

	private void SetStrings (string key, string[] values) {
		if (values.Length == 0) {
			string[] newValues = { String.Empty };
			Set(key, newValues);
		}
		else
			Set(key, values);
	}

	private void Set (string key, object val) {
		try {
			client.Set(key, val);
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
		}
	}

}

}