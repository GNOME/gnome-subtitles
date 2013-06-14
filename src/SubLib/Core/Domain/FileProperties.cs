/*
 * This file is part of SubLib.
 * Copyright (C) 2007-2010 Pedro Castro
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
using System.Text;

namespace SubLib.Core.Domain {

/// <summary>Represents the properties of a file.</summary>
/// <remarks>This includes all that's necessary to work with files, in relation to <see cref="Subtitles" />.</remarks>
public class FileProperties : ICloneable {
	/* Used for reading and writing */
	private string path = String.Empty; //The path of the file
	private Encoding encoding = null; //The character coding of the file
	private SubtitleType subtitleType = SubtitleType.Unknown; //The subtitle type of the file
	private TimingMode timingMode = TimingMode.Times; //The timing mode used in the file (some subtitle types support both)

	/* Used for writing only */
	private NewlineType newlineType = NewlineType.Unknown;

	/// <summary>Creates a new instance of the <see cref="FileProperties" /> class.</summary>
	/// <remarks>All properties are initialized to defaults.</remarks>
	public FileProperties () {
	}

	/// <summary>Creates a new instance of the <see cref="FileProperties" /> class, given its properties.</summary>
	/// <param name="path">The file's path.</param>
	/// <param name="encoding">The file's character coding.</param>
	/// <param name="subtitleType">The file's subtitle type.</param>
	/// <param name="timingMode">The file's timing mode. This is more useful for the subtitle types that support both the time and frame modes.</param>
	/// <param name="newlineType">The file's newline type.</param>
	public FileProperties (string path, Encoding encoding, SubtitleType subtitleType, TimingMode timingMode, NewlineType newlineType) {
		this.path = path;
		this.encoding = encoding;
		this.subtitleType = subtitleType;
		this.timingMode = timingMode;
		this.newlineType = newlineType;
	}

	/// <summary>Creates a new instance of the <see cref="FileProperties" /> class, given its properties.</summary>
	/// <param name="path">The file's path.</param>
	/// <param name="encoding">The file's character coding.</param>
	/// <param name="subtitleType">The file's subtitle type.</param>
	/// <param name="timingMode">The file's timing mode. This is more useful for the subtitle types that support both the time and frame modes.</param>
	public FileProperties (string path, Encoding encoding, SubtitleType subtitleType, TimingMode timingMode)
		: this(path, encoding, subtitleType, timingMode, NewlineType.Unknown) {
	}

	/// <summary>Creates a new instance of the <see cref="FileProperties" /> class, given its properties.</summary>
	/// <param name="path">The file's path.</param>
	/// <param name="encoding">The file's character coding.</param>
	/// <param name="timingMode">The file's timing mode. This is more useful for the subtitle types that support both the time and frame modes.</param>
	public FileProperties(string path, Encoding encoding, TimingMode timingMode)
		: this(path, encoding, SubtitleType.Unknown, timingMode) {
	}

	/// <summary>Creates a new instance of the <see cref="FileProperties" /> class, given the file's path.</summary>
	/// <param name="path">The file's path.</param>
	public FileProperties (string path) : this(path, null, SubtitleType.Unknown, TimingMode.Times) {
	}


	/* Public properties */

	/// <summary>The file's path.</summary>
	public string Path {
		get { return path; }
		set { path = System.IO.Path.GetFullPath(value); }
	}

	/// <summary>The file's filename.</summary>
	/// <remarks>See <see cref="System.IO.Path.GetFileName" /> for more information.</remarks>
	public string Filename {
		get { return System.IO.Path.GetFileName(path); }
	}

	/// <summary>The file's filename without its extension.</summary>
	/// <remarks>See <see cref="System.IO.Path.GetFileNameWithoutExtension" /> for more information.</remarks>
	public string FilenameWithoutExtension {
		get { return System.IO.Path.GetFileNameWithoutExtension(path); }
	}

	/// <summary>The file's directory.</summary>
	/// <remarks>See <see cref="System.IO.Path.GetDirectoryName" /> for more information.</remarks>
	public string Directory {
		get { return System.IO.Path.GetDirectoryName(path); }
	}

	/// <summary>Whether the path is rooted.</summary>
	/// <remarks>See <see cref="System.IO.Path.IsPathRooted" /> for more information.</remarks>
	public bool IsPathRooted {
		get { return System.IO.Path.IsPathRooted(path); }
	}

	/// <summary>The character coding used in the file.</summary>
	public Encoding Encoding {
		get { return encoding; }
		set { encoding = value; }
	}

	/// <summary>The timing mode used in the file.</summary>
	/// <remarks>This is more useful for the subtitle types that support both the time and frame modes.</remarks>
	public TimingMode TimingMode {
		get { return timingMode; }
		set { timingMode = value; }
	}

	/// <summary>The type of the subtitles.</summary>
	public SubtitleType SubtitleType {
		get { return subtitleType; }
		set { subtitleType = value; }
	}

	/// <summary>The type of newline used in the file.</summary>
	public NewlineType NewlineType {
		get { return newlineType; }
		set { newlineType = value; }
	}


	/* Public methods */

	public object Clone () {
		return this.MemberwiseClone();
	}

}

}