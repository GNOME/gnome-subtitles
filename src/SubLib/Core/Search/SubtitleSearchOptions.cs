/*
 * This file is part of SubLib.
 * Copyright (C) 2007-2008 Pedro Castro
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
using System;
using System.Text.RegularExpressions;

namespace SubLib.Core.Search {

/// <summary>Represents the options used during a search operation.</summary>
public class SubtitleSearchOptions {
	private Regex regex = null;
	private string lineBreak = String.Empty;
	private SubtitleTextType textType = SubtitleTextType.Text;
	private int startSubtitle = 0;
	private int startIndex = 0;
	private bool wrap = false;
	private bool backwards = false;

	/// <summary>Creates a new instance of the <see cref="SubtitleSearchOptions" /> class.</summary>
	/// <param name="regex">The regular expression to use when searching. It must be created with
	/// <see cref="RegexOptions.IgnoreCase" /> to perform a case-insensitive search.</param>
	/// <param name="textType">The type of text content the search is started at.</param>
	/// <param name="lineBreak">The line break to use between multiple lines of text in each subtitle.</param>
	/// <param name="startSubtitle">The zero-based number of the subtitle to start the search at.</param>
	/// <param name="startIndex">The zero-based position within the startSubtitle to start the search at.</param>
	/// <param name="wrap">Whether to continue the search from the beginning when it reaches the end of the subtitles.</param>
	/// <param name="backwards">Whether to search backwards. Note that regex must be constructed with the
	/// <see cref="RegexOptions.RightToLeft" /> option for backwards search to work.</param>
	public SubtitleSearchOptions (Regex regex, SubtitleTextType textType, string lineBreak, int startSubtitle, int startIndex, bool wrap, bool backwards) {
		this.regex = regex;
		this.textType = textType;
		this.lineBreak = lineBreak;
		this.startSubtitle = startSubtitle;
		this.startIndex = startIndex;
		this.wrap = wrap;
		this.backwards = backwards;
	}

	/// <summary>Creates a new instance of the <see cref="SubtitleSearchOptions" /> class.</summary>
	/// <param name="regex">The regular expression to use when searching. It must be created with
	/// <see cref="RegexOptions.IgnoreCase" /> to perform a case-insensitive search.</param>
	/// <param name="textType">The type of text content the search is started at.</param>
	/// <param name="startSubtitle">The zero-based number of the subtitle to start the search at.</param>
	/// <param name="startIndex">The zero-based position within the startSubtitle to start the search at.</param>
	/// <param name="wrap">Whether to continue the search from the beginning when it reaches the end of the subtitles.</param>
	/// <param name="backwards">Whether to search backwards. Note that regex must be constructed with the
	/// <see cref="RegexOptions.RightToLeft" /> option for backwards search to work.</param>
	/// <remarks>The newline character (\n) is used as lineBreak.</remarks>
	public SubtitleSearchOptions (Regex regex, SubtitleTextType textType, int startSubtitle, int startIndex, bool wrap, bool backwards)
		: this(regex, textType, "\n", startSubtitle, startIndex, wrap, backwards) {
	}

	/* Public properties */

	/// <summary>The regular expression.</summary>
	public Regex Regex {
		get { return regex; }
	}

	/// <summary>The linebreak used to use between multiple lines of text.</summary>
	public string LineBreak {
		get { return lineBreak; }
	}

	/// <summary>The type of text content to start the search with.</summary>
	public SubtitleTextType TextType {
		get { return textType; }
	}

	/// <summary>The subtitle to start the search with.</summary>
	public int StartSubtitle {
		get { return startSubtitle; }
	}

	/// <summary>The index of the text to start the search with.</summary>
	public int StartIndex {
		get { return startIndex; }
	}

	/// <summary>Whether to continue the search from the beginning when it reaches the end of the subtitles.</summary>
	public bool Wrap {
		get { return wrap; }
	}

	/// <summary>Whether to search backwards.</summary>
	public bool Backwards {
		get { return backwards; }
	}

}

}
