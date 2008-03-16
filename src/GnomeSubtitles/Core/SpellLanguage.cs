/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008 Pedro Castro
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

using Mono.Unix;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GnomeSubtitles {

public class SpellLanguage : IComparable {
	private string id = null;
	private string name = null;
	private string prefixUnknown = Catalog.GetString("Unknown");
	
	/* Static variables */
	private static string langGroupName = "lang";
	private static string countryGroupName = "country";
	private static Regex regex = new Regex(@"(?<" + langGroupName + @">[a-zA-Z0-9]+)([^a-zA-Z0-9]+(?<" + countryGroupName + @">[a-zA-Z0-9]+))?$", RegexOptions.IgnoreCase);
		
	public SpellLanguage (String id) {
		this.id = id;
		this.name = GetNameFromID(id);
	}
	
	/* Properties */
	
	public string ID {
		get { return id; }
	}
	
	public string Name {
		get { return name; }
	}
	
	/* Public methods */
	
	public override bool Equals (object o) {
		return ((o is SpellLanguage) && ((o as SpellLanguage).Name == this.name));
	}
	
	public override int GetHashCode () {
		return this.name.GetHashCode();
	}
	
	public int CompareTo (object o) {
		if (!(o is SpellLanguage))
			throw new ArgumentException();
		
		return this.name.CompareTo((o as SpellLanguage).Name);
	} 
	
	/* Private members */
	
	private string GetNameFromID (string id) {
		System.Console.WriteLine("ID: " + id);
		string lang = null;
		string country = null;
		bool parsed = ParseID(id, ref lang, ref country);
		System.Console.WriteLine("Parsed? " + parsed);
		string builtID = null;
		if (parsed) {
			builtID = lang;
			if ((country != null) && (country != String.Empty))
				builtID += "-" + country;
		}
		else
			builtID = id;
		
		CultureInfo info = null;
		try {
			info = new CultureInfo(builtID);
		}
		catch (Exception e) {
			System.Console.WriteLine(e);
			return prefixUnknown + " (" + id + ")";
		}
		return info.EnglishName;
	}
	
	private bool ParseID (string id, ref string lang, ref string country) {
		Match match = regex.Match(id);
		if (!match.Success)
			return false;
		
		System.Console.WriteLine("Match was successful");
		Group langGroup = match.Groups[langGroupName];
		if (!langGroup.Success)
			return false;
		
		lang = langGroup.Value;
		System.Console.WriteLine("Lang group was successful: " + lang);
		
		Group countryGroup = match.Groups[countryGroupName];
		country = (countryGroup.Success ? countryGroup.Value : null);
		System.Console.WriteLine("Country group was successful: " + country);
		return true;
	}

}

}
