/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2019 Pedro Castro
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

using GLib;
using SubLib.Util;

namespace GnomeSubtitles.Core {

public class ConfigBackendGSettings : IConfigBackend {

	/* Schema */
	private const string Schema = "org.gnome.GnomeSubtitles";

	private Settings settings = null;

	public ConfigBackendGSettings () {
		if (!isSchemaAvailable(Schema)) {
			Logger.Error("GSettings schema {0} is unavailable.", Schema);
			throw new ConfigBackendUnavailableException();
		}
	
		settings = new Settings(Schema);
	}
	
	public string GetString (string key) {
		return settings.GetString(key);
	}

	public bool GetBoolean (string key) {
		return settings.GetBoolean(key);
	}

	public int GetInt (string key) {
		return settings.GetInt(key);
	}

	public string[] GetStrings (string key) {
		return settings.GetStrv(key);
	}


	public void SetStrings (string key, string[] values) {
		settings.SetStrv(key, values);
	}

	public void SetString(string key, string value) {
		settings.SetString(key, value);
	}

	public void SetBoolean(string key, bool value) {
		settings.SetBoolean(key, value);
	}

	public void SetInt(string key, int value) {
		settings.SetInt(key, value);
	}
	
	/* Private members */
	
	private bool isSchemaAvailable (string schema) {
		foreach (string installedSchema in Settings.ListSchemas()) {
			if (installedSchema == schema) {
				return true;
			}
		}
		
		return false;
	}

}

}