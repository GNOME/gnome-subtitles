/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2011 Pedro Castro
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
using System.Collections;

namespace SubLib.Core.Domain {

/// <summary>A container that represents all the subtitles.</summary>
public class SubtitleCollection {
	private ArrayList subtitles = new ArrayList();


	/* Public properties */

	/// <summary>The number of subtitles in the collection.</summary>
	public int Count {
		get { return subtitles.Count; }
	}

	/* Indexers */

	public Subtitle this [int index] {
		get {
			try {
				return subtitles[index] as Subtitle;
			}
			catch (ArgumentOutOfRangeException) {
				return null;
			}
		}
	}


	/* Public methods */

	public SubtitleCollection Clone (SubtitleProperties propertiesClone) {
		SubtitleCollection collectionClone = new SubtitleCollection();
		foreach (Subtitle subtitle in this.subtitles) {
			Subtitle subtitleClone = subtitle.Clone(propertiesClone);
			collectionClone.Add(subtitleClone);
		}
		return collectionClone;
	}

	/// <summary>Returns an enumerator that can iterate through the collection.</summary>
	/// <returns>An <see cref="IEnumerator" /> for the entire <see cref="SubtitleCollection" />.</returns>
	public IEnumerator GetEnumerator () {
		return subtitles.GetEnumerator();
	}

	/// <summary>Returns the subtitle at the specified index.</summary>
	/// <param name="index">The zero-based subtitle's index.</param>
	/// <returns>The subtitle at the specified index, or null in case the index is invalid.</returns>
	public Subtitle Get (int index) {
		if ((index >= 0) && (index < Count))
			return (Subtitle)subtitles[index];
		else
			return null;
	}

	/// <summary>Adds a subtitle to the end of the collection.</summary>
	/// <param name="subtitle">The subtitle to add.</param>
	public void Add (Subtitle subtitle){
		subtitles.Add(subtitle);
	}

	/// <summary>Adds a subtitle to the collection, inserting it at the specified index.</summary>
	/// <param name="subtitle">The subtitle to add.</param>
	/// <param name="index">The zero-based index at which the subtitle should be inserted.</param>
	public void Add (Subtitle subtitle, int index){
		subtitles.Insert(index, subtitle);
	}

	/// <summary>Creates a subtitle based on the subtitle at the specified index and adds it to the
	/// collection, inserting it right before that index.</summary>
	/// <remarks>The newly created subtitle's times will be based on the specified subtitle. Its end
	/// time will be the start time of the existing subtitle minus <see cref="SubtitleConstants.MinTimeBetweenSubtitles" />.
	/// Its duration will be <see cref="SubtitleConstants.MaxSingleLineSubtitleDuration" />. Both times will be wrapped to
	/// zero if they are less than zero.</remarks>
	/// <param name="index">The zero-based index before which the subtitle should be inserted.</param>
	/// <param name="subtitleProperties">The SubtitleProperties of the subtitles.</param>
	/// <returns>True if the subtitle could be added, false otherwise.</returns>
	public bool AddNewBefore (int index, SubtitleProperties subtitleProperties) {
		return AddNewBefore(index, subtitleProperties, (int)(SubtitleConstants.MinTimeBetweenSubtitles*1000));
	}

	/// <summary>Creates a subtitle based on the subtitle at the specified index and adds it to the
	/// collection, inserting it right before that index.</summary>
	/// <remarks>The newly created subtitle's times will be based on the specified subtitle. Its duration
	/// will be <see cref="SubtitleConstants.MaxSingleLineSubtitleDuration" />. Both the start and end times
	/// times will be wrapped to zero if they are negative.</remarks>
	/// <param name="index">The zero-based index before which the subtitle should be inserted.</param>
	/// <param name="subtitleProperties">The SubtitleProperties of the subtitles.</param>
	/// <param name="timeBetweenSubtitles">The gap to keep before the existing subtitle, in milliseconds.</param>
	/// <returns>True if the subtitle could be added, false otherwise.</returns>
	public bool AddNewBefore (int index, SubtitleProperties subtitleProperties, int timeBetweenSubtitles) {
		Subtitle existing = Get(index);
		if (existing == null)
			return false;

		TimeSpan subtitleEnd = existing.Times.Start - TimeSpan.FromMilliseconds(timeBetweenSubtitles);
		if (subtitleEnd < TimeSpan.Zero)
			subtitleEnd = TimeSpan.FromSeconds(0);

		TimeSpan subtitleStart = subtitleEnd - TimeSpan.FromSeconds(SubtitleConstants.AverageSubtitleDuration);
		if (subtitleStart < TimeSpan.Zero)
			subtitleStart = TimeSpan.FromSeconds(0);

		Subtitle subtitle = new Subtitle(subtitleProperties, subtitleStart, subtitleEnd);
		Add(subtitle, index);
		return true;
	}

	/// <summary>Creates a subtitle based on the subtitle at the specified index and adds it to the
	/// collection, inserting it right after that index.</summary>
	/// <remarks>The newly created subtitle's times will be based on the specified subtitle. Its start
	/// time will be the end time of the existing subtitle plus <see cref="SubtitleConstants.MinTimeBetweenSubtitles" />.
	/// Its duration will be <see cref="SubtitleConstants.MaxSingleLineSubtitleDuration" />.</remarks>
	/// <param name="index">The zero-based index after which the subtitle should be inserted.</param>
	/// <param name="subtitleProperties">The SubtitleProperties of the subtitles.</param>
	/// <returns>True if the subtitle could be added, false otherwise.</returns>
	public bool AddNewAfter (int index, SubtitleProperties subtitleProperties) {
		return AddNewAfter(index, subtitleProperties, (int)(SubtitleConstants.MinTimeBetweenSubtitles*1000));
	}

	/// <summary>Creates a subtitle based on the subtitle at the specified index and adds it to the
	/// collection, inserting it right after that index.</summary>
	/// <remarks>The newly created subtitle's times will be based on the specified subtitle.
	/// Its duration will be <see cref="SubtitleConstants.MaxSingleLineSubtitleDuration" />.</remarks>
	/// <param name="index">The zero-based index after which the subtitle should be inserted.</param>
	/// <param name="subtitleProperties">The SubtitleProperties of the subtitles.</param>
	/// <param name="timeBetweenSubtitles">The gap to keep after the existing subtitle, in milliseconds.</param>
	/// <returns>True if the subtitle could be added, false otherwise.</returns>
	public bool AddNewAfter (int index, SubtitleProperties subtitleProperties, int timeBetweenSubtitles) {
		Subtitle existing = Get(index);
		if (existing == null)
			return false;

		TimeSpan subtitleStart = existing.Times.End + TimeSpan.FromMilliseconds(timeBetweenSubtitles);
		TimeSpan subtitleEnd = subtitleStart + TimeSpan.FromSeconds(SubtitleConstants.AverageSubtitleDuration);
		Subtitle subtitle = new Subtitle(subtitleProperties, subtitleStart, subtitleEnd);
		Add(subtitle, index + 1);
		return true;
	}

	/// <summary>Creates a subtitle and adds it to the collection, inserting it at the specified index.</summary>
	/// <remarks>The newly created subtitle's start time will be zero and its duration will be
	/// <see cref="SubtitleConstants.MaxSingleLineSubtitleDuration" />.</remarks>
	/// <param name="index">The zero-based index at which the subtitle should be inserted.</param>
	/// <param name="subtitleProperties">The SubtitleProperties of the subtitles.</param>
	/// <returns>True if the subtitle could be added, false otherwise.</returns>
	public bool AddNewAt (int index, SubtitleProperties subtitleProperties) {
		if ((index < 0) || (index > Count))
			return false;

		TimeSpan subtitleStart = TimeSpan.FromSeconds(0);
		TimeSpan subtitleEnd = TimeSpan.FromSeconds(SubtitleConstants.AverageSubtitleDuration);
		Subtitle subtitle = new Subtitle(subtitleProperties, subtitleStart, subtitleEnd);
		Add(subtitle, index);
		return true;
	}

	/// <summary>Creates a subtitle and adds it to the collection, inserting it at the specified index
	/// and with the specified start time.</summary>
	/// <remarks>The newly created subtitle's duration will be
	/// <see cref="SubtitleConstants.MaxSingleLineSubtitleDuration" />.</remarks>
	/// <param name="index">The zero-based index at which the subtitle should be inserted.</param>
	/// <param name="subtitleProperties">The SubtitleProperties of the subtitles.</param>
	/// <param name="start">The time at which the new subtitle will start</para>
	/// <returns>True if the subtitle could be added, false otherwise.</returns>
	public bool AddNewAt (int index, SubtitleProperties subtitleProperties, TimeSpan start) {
		if ((index < 0) || (index > Count))
			return false;

		TimeSpan subtitleStart = start;
		TimeSpan subtitleEnd = subtitleStart + TimeSpan.FromSeconds(SubtitleConstants.AverageSubtitleDuration);
		Subtitle subtitle = new Subtitle(subtitleProperties, subtitleStart, subtitleEnd);
		Add(subtitle, index);
		return true;
	}

	/// <summary>Checks whether a subtitle with the specified index exists in the collection.</summary>
	/// <param name="index">The zero-based index.</param>
	/// <returns>Whether the index is contained within the collection.</returns>
	public bool Contains (int index) {
		return (index >= 0) && (index < Count);
	}

	/// <summary>Removes a subtitle from the collection, given its index.</summary>
	/// <param name="index">The zero-based index of the subtitle to be removed.</param>
	public void Remove (int index) {
		subtitles.RemoveAt(index);
	}

	public override string ToString(){
		string result = "\t* SUBTITLE LIST *\n";
		foreach(Subtitle subtitle in subtitles){
			result += subtitle.ToString();
		}
		return result;
	}


	/* Internal methods */

	internal void SetPropertiesForAll (SubtitleProperties properties) {
		foreach (Subtitle subtitle in subtitles)
			subtitle.Properties = properties;
	}

}

}
