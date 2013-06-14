/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2008 Pedro Castro
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

/// <summary>Represents an incomplete subtitle.</summary>
/// <remarks>An incomplete subtitle is characterized by its incomplete text and
/// the valid subtitle that precedes it.</remarks>
public class IncompleteSubtitle {
	private int previous = 0;
	private string text = String.Empty;

	/// <summary>Initializes a new instance of the <see cref="IncompleteSubtitle" /> class,
	/// given the index of its preceding valid subtitle and the incomplete text.</summary>
	/// <param name="previous">The index of the preceding valid subtitle.</param>
	/// <param name="text">The subtitle's incomplete text.</param>
	public IncompleteSubtitle (int previous, string text) {
		this.previous = previous;
		this.text = text;
	}

	/// <summary>The index of the preceding valid subtitle.</summary>
	public int Previous {
		get { return previous; }
		set { previous = value; }
	}

	/// <summary>The incomplete subtitle's text.</summary>
	public string Text {
		get { return text; }
		set { text = value; }
	}

 	public override string ToString(){
	  	return "* After " + previous + ": " + text + "\n";
	}

}

}
