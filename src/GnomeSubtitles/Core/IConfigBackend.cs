/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2018-2019 Pedro Castro
 *
 * Gnome Subtitles is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * Gnome Subtitles is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General License for more details.
 *
 * You should have received a copy of the GNU General License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

namespace GnomeSubtitles.Core {

interface IConfigBackend {

	string GetString (string key);
	bool GetBoolean (string key);
	int GetInt (string key);
	string[] GetStrings (string key);

	void SetStrings (string key, string[] values);
	void SetString(string key, string value);
	void SetBoolean(string key, bool value);
	void SetInt(string key, int value);

}

}