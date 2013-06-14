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

using System.Collections;

namespace SubLib.Core.Domain {

/// <summary>A container that represents a collection of incomplete subtitles.</summary>
public class IncompleteSubtitleCollection {
	private ArrayList subtitles = new ArrayList();


	/// <summary>The number of subtitles in the collection.</summary>
	public int Count {
		get { return subtitles.Count; }
	}

	/// <summary>Returns an enumerator that can iterate through the collection.</summary>
	/// <returns>An <see cref="IEnumerator" /> for the entire <see cref="IncompleteSubtitleCollection" />.</returns>
	public IEnumerator GetEnumerator () {
		return subtitles.GetEnumerator();
	}

	/// <summary>Returns the subtitle at the specified index.</summary>
	/// <param name="index">The zero-based subtitle's index.</param>
	/// <returns>The subtitle at the specified index.</returns>
	public IncompleteSubtitle Get (int index){
		return (IncompleteSubtitle)subtitles[index];
	}

	/// <summary>Adds an incomplete subtitle to the end of the collection.</summary>
	/// <param name="subtitle">The subtitle to add.</param>
	public void Add (IncompleteSubtitle subtitle){
		subtitles.Add(subtitle);
	}

	/// <summary>Adds an incomplete subtitle to the collection, inserting it at the specified index.</summary>
	/// <param name="subtitle">The subtitle to add.</param>
	/// <param name="index">The zero-based index at which the subtitle should be inserted.</param>
	public void Add (IncompleteSubtitle subtitle, int index){
		subtitles.Insert(index, subtitle);
	}

	public override string ToString(){
		string result = "\t* SUBTITLE LIST *\n";
		foreach(IncompleteSubtitle subtitle in subtitles){
			result += subtitle.ToString();
		}
		return result;
	}

}

}
