/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006 Pedro Castro
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

using System.Text;

namespace GnomeSubtitles {

public class FileEncoding {
	private int codePage;
	private string description;
	private string name;

	public FileEncoding (int codePage, string description, string name) {
		this.codePage = codePage;
		this.description = description;
		this.name = name;
	}
	
	public FileEncoding (int codePage, string description) : this(codePage, description, Encoding.GetEncoding(codePage).EncodingName) {}
	
	public FileEncoding (string description) : this(Encoding.Default.CodePage, description, Encoding.Default.EncodingName) {}
	
	
	public int CodePage {
		get { return codePage; }
	}
	
	public string Description {
		get { return description; }
	}
	
	public string Name {
		get { return name; }
	}

}

}
