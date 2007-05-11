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

using System;

namespace GnomeSubtitles {

public struct EncodingDescription : IComparable {
	private int codePage;
	private string name;
	private string description;
	
	public EncodingDescription (int codePage, string name, string description) {
		this.codePage = codePage;
		this.name = name;
		this.description = description;
	}
	
	public int CodePage {
		get { return codePage; }
	}
	
	public string Name {
		get { return name; }
	}
	
	public string Description {
		get { return description; }
	}
	
	public int CompareTo (object obj) {
		if (obj is EncodingDescription) {
			EncodingDescription obj2 = (EncodingDescription)obj;
			int descComparison = this.description.CompareTo(obj2.description);
			if (descComparison != 0)
				return descComparison;
			else
				return this.name.CompareTo(obj2.name);
		}
		else
			throw new ArgumentException("Object is not EncodingDescription");    
	}
	
}

}