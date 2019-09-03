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

using System;

namespace GnomeSubtitles.Core {

public struct EncodingDescription : IComparable {
	private int codePage;
	private string code;
	private string name;
	private string region;
	private static EncodingDescription emptyEncodingDescription = new EncodingDescription(-1, "-1", "-1");

	public EncodingDescription (int codePage, string code, string region, string name) {
		this.codePage = codePage;
		this.code = code;
		this.region = region;
		this.name = name;
	}
	
	//Use the code as name too
	public EncodingDescription (int codePage, string code, string region)
		: this(codePage, code, region, code) {
	}

	public int CodePage {
		get { return codePage; }
	}
	
	public string Code {
		get { return code; }
	}

	public string Region {
		get { return region; }
	}
	
	public string Name {
		get { return name; }
	}

	public int CompareTo (object obj) {
		if (!(obj is EncodingDescription)) {
			throw new ArgumentException("Object is not EncodingDescription");
		}

		EncodingDescription obj2 = (EncodingDescription)obj;
		int result = this.region.CompareTo(obj2.region);
		if (result != 0) {
			return result;
		}
		
		return this.name.CompareTo(obj2.name);
	}

	/* Static members */

	public static EncodingDescription Empty {
		get { return emptyEncodingDescription; }
	}

}

}