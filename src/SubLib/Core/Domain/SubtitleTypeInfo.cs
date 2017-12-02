/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2017 Pedro Castro
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

using SubLib.IO.SubtitleFormats;
using System;

namespace SubLib.Core.Domain {

/// <summary>Contains information about a subtitle file type.</summary>
public class SubtitleTypeInfo : IComparable {
	private string name;
	private SubtitleType type;
	private SubtitleMode mode;
	private string[] extensions;


	/// <summary>Initializes a new instance of the <see cref="SubtitleTypeInfo" /> class.</summary>
	/// <param name="name">The name of the subtitle type.</param>
	/// <param name="type">The subtitle type.</param>
	/// <param name="mode">The subtitle mode.</param>
	/// <param name="extensions">The extensions the subtitle type uses.</param>
	public SubtitleTypeInfo (string name, SubtitleType type, SubtitleMode mode, string[] extensions) {
		this.name = name;
		this.type = type;
		this.mode = mode;
		this.extensions = extensions;
	}

	/// <summary>The name of the subtitle type.</summary>
	public string Name {
		get { return name; }
	}

	/// <summary>The subtitle type.</summary>
	public SubtitleType Type {
		get { return type; }
	}

	/// <summary>The subtitle mode.</summary>
	public SubtitleMode Mode {
		get { return mode; }
	}

	/// <summary>The extensions the subtitle type uses.</summary>
	public string[] Extensions {
		get { return extensions; }
	}

	/// <summary>The preferred extension, which is the first on the list.</summary>
	public string PreferredExtension {
		get { return extensions[0]; }
	}

	/// <summary>Checks whether the specified extension is one of the extensions of the <see cref="SubtitleType" /></summary>
	/// <param name="extension">The extension to search for.</param>
	/// <returns>True if the extension was found, False otherwise.</returns>
	public bool HasExtension (string extension) {
		foreach (string typeExtension in extensions) {
			if (typeExtension == extension)
				return true;
		}
		return false;
	}

	/// <summary>Compares this instance with a specified object, based on the object names.
	/// See <see cref="String.CompareTo(object)" /> for more information.</summary>
	/// <param name="obj">The object to compare this class to.</param>
	/// <returns>
	/// <list type="table">
    /// 		<listheader><term>Value</term><description>Condition</description></listheader>
    ///		<item><term>Less than zero</term><description>This instance is less than obj.</description></item>
    ///		<item><term>Zero</term><description>This instance is equal to obj.</description></item>
    ///		<item><term>Greater than zero</term><description>This instance is greater than obj, or obj is a
    ///			null reference.</description></item>
	///	</list>
	///	</returns>
	public int CompareTo (object obj) {
		return type.ToString().CompareTo((obj as SubtitleTypeInfo).Type.ToString());
	}

	/* Internal members */

	internal SubtitleTypeInfo (SubtitleFormat format) : this(format.Name, format.Type, format.Mode, format.Extensions) {
	}

}

}
