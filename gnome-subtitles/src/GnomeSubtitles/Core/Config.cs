/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007 Pedro Castro
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
using System;

namespace GnomeSubtitles {

public class Config {
	private Client client = null;
	
	/* Constant prefix strings */
	private const string keyPrefix = "/apps/gnome-subtitles/";
	private const string keyPrefs = keyPrefix + "preferences/";
	private const string keyPrefsVideo = keyPrefs + "video/";
	private const string keyPrefsEncodings = keyPrefs + "encodings/";
	
	/* Constant key strings */
	private const string keyPrefsVideoAutoChooseFile = keyPrefsVideo + "auto_choose_file";
	private const string keyPrefsEncodingsShownInMenu = keyPrefsEncodings + "shown_in_menu";

	public Config () {
		client = new Client();
	}
	
	/* Public properties */
	
	public bool PrefsVideoAutoChooseFile {
		get { return GetBool(keyPrefsVideoAutoChooseFile, true); }
		set { Set(keyPrefsVideoAutoChooseFile, value); }
	}
	
	public string[] PrefsEncodingsShownInMenu {
		get {
			string[] defaultValue = { "ISO-8859-15" };
			return GetStrings(keyPrefsEncodingsShownInMenu, defaultValue);
		}
		set { SetStrings(keyPrefsEncodingsShownInMenu, value); }
	}
	
	/* Private members */
	
	private bool GetBool (string key, bool defaultValue) {
		try {
			return (bool)client.Get(key);
		}
		catch (Exception) {
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
			System.Console.WriteLine(e);
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
			System.Console.WriteLine(e);
		}
	}

}

}