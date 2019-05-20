/*
 * This file is part of SubLib.
 * Copyright (C) 2007-2019 Pedro Castro
 *
 * SubLib is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * SubLib is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using SubLib.Core.Domain;

namespace SubLib.Core.Search {

/// <summary>Represents the results of a search operation.</summary>
public class SubtitleSearchResults {
	private int subtitle = -1;
	private SubtitleTextType textType = SubtitleTextType.Text;
	private int index = -1;
	private int length = -1;

	/// <summary>Creates a new instance of the <see cref="SubtitleSearchResults" /> class.</summary>
	/// <param name="subtitle">The zero-based number of the subtitle where the text was found.</param>
	/// <param name="textType">The type of text content where the text was found.</param>
	/// <param name="index">The zero-based position where the text was found, within a subtitle.</param>
	/// <param name="length">The length of the found text.</param>
	public SubtitleSearchResults (int subtitle, SubtitleTextType textType, int index, int length) {
		this.subtitle = subtitle;
		this.textType = textType;
		this.index = index;
		this.length = length;
	}

	/* Public properties */

	/// <summary>The subtitle number.</summary>
	public int Subtitle {
		get { return subtitle; }
	}

	/// <summary>The type of the text content.</summary>
	public SubtitleTextType TextType {
		get { return textType; }
	}

	/// <summary>The text index.</summary>
	public int Index {
		get { return index; }
	}

	/// <summary>The text length.</summary>
	public int Length {
		get { return length; }
	}

}

}
