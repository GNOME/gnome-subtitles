/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2019 Pedro Castro
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

using System;

namespace SubLib.Core.Domain {

/// <summary>Represents a text style, including Bold, Italic and Underline.</summary>
public class Style : ICloneable {
	private bool bold = false;
	private bool italic = false;
	private bool underline = false;

	/// <summary>Initializes a new instance of the <see cref="SubLib.Style" /> class.</summary>
	public Style () {}

	/// <summary>
	/// Initializes a new instance of the <see cref="Style" /> class, given the
	/// specified style values.</summary>
	/// <param name="bold">Whether the text is bold.</param>
	/// <param name="italic">Whether the text is italic.</param>
	/// <param name="underline">Whether the text is underlined.</param>
	public Style (bool bold, bool italic, bool underline) {
		this.bold = bold;
		this.italic = italic;
		this.underline = underline;
	}


	/* Public properties */

	/// <summary>Whether the style is bold.</summary>
	public bool Bold {
		get { return bold; }
		set { bold = value; }
	}

	/// <summary>Whether the style is italic.</summary>
	public bool Italic {
		get { return italic; }
		set { italic = value; }
	}

	/// <summary>Whether the style is underlined.</summary>
	public bool Underline {
		get { return underline; }
		set { underline = value; }
	}

	/// <summary>Whether any of the style values is enabled.</summary>
	public bool Enabled {
		get { return Bold || Italic || Underline; }
	}

	public override string ToString() {
  		string result = String.Empty;
  		if (Bold)
  			result += " bold";
  		if (Italic)
  			result += " italic";
  		if (Underline)
  			result += " underline";
  		return result;
	}


	/* Public methods */

	public object Clone() {
		return this.MemberwiseClone();
	}

}

}
