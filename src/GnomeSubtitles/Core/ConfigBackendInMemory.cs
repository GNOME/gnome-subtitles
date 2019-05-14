/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2018-2019 Pedro Castro
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

using System.Collections.Generic;

namespace GnomeSubtitles.Core {

public class ConfigBackendInMemory : IConfigBackend {

	private Dictionary<string, object> map = new Dictionary<string, object>();

	public ConfigBackendInMemory () {
	}
	
	public string GetString (string key) {
		return (string)map[key];
	}

	public bool GetBoolean (string key) {
		return (bool)map[key];
	}

	public int GetInt (string key) {
		return (int)map[key];
	}

	public string[] GetStrings (string key) {
		return (string[])map[key];
	}


	public void SetStrings (string key, string[] values) {
		map[key] = values;
	}

	public void SetString(string key, string value) {
		map[key] = value;
	}

	public void SetBoolean(string key, bool value) {
		map[key] = value;
	}

	public void SetInt(string key, int value) {
		map[key] = value;
	}
	
}

}