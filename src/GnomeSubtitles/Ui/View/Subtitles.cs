/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2017 Pedro Castro
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

using GnomeSubtitles.Core;
using Gtk;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Ui.View {

public class Subtitles : SubLib.Core.Domain.Subtitles {
	private ListStore model = new ListStore(typeof(Subtitle));


	public Subtitles (SubLib.Core.Domain.Subtitles subtitles) : base(subtitles.Collection, subtitles.Properties) {
		LoadModelFromCollection();
	}


	/* Indexers */

	public Subtitle this [int index] {
		get { return Collection[index]; }
	}

	public Subtitle this [TreeIter iter] {
		get { return model.GetValue(iter, 0) as Subtitle; }
	}

	public Subtitle this [TreePath path] {
		get { return this[path.Indices[0]]; }
	}

	/* Public properties */

	public ListStore Model {
		get { return model; }
	}

	public int Count {
		get { return Collection.Count; }
	}

	/// <summary>Appends a subtitle to the end of the list.</summary>
	public void Add (Subtitle subtitle) {
		Collection.Add(subtitle);
		model.AppendValues(subtitle);
	}

	/// <summary>Adds a subtitle to the specified position on the list.</summary>
	public void Add (Subtitle subtitle, int index) {
		Collection.Add(subtitle, index);
		model.SetValue(model.Insert(index), 0, subtitle);
	}

	/// <summary>Creates a subtitle and adds it after the specified position.</summary>
	/// <remarks>The timings of the new subtitle will be based on the subtitle that precedes it.</remarks>
	public void AddNewAfter (int index) {
		Collection.AddNewAfter(index, Properties, Base.Config.TimingsTimeBetweenSubtitles);
		int newIndex = index + 1;
		Subtitle newSubtitle = this[newIndex];
		model.SetValue(model.Insert(newIndex), 0, newSubtitle);
	}

	/// <summary>Creates a subtitle and adds it before the specified position.</summary>
	/// <remarks>The timings of the new subtitle will be based on the subtitle that succeeds it.</remarks>
	public void AddNewBefore (int index) {
		Collection.AddNewBefore(index, Properties, Base.Config.TimingsTimeBetweenSubtitles);
		Subtitle newSubtitle = this[index];
		model.SetValue(model.Insert(index), 0, newSubtitle);
	}

	/// <summary>Creates a subtitle and adds it to the specified position of the list.</summary>
	public void AddNewAt (int index) {
		Collection.AddNewAt(index, Properties);
		Subtitle newSubtitle = this[index];
		model.SetValue(model.Insert(index), 0, newSubtitle);
	}

	public void AddNewAt (int index, TimeSpan start) {
		Collection.AddNewAt(index, Properties, start);
		Subtitle newSubtitle = this[index];
		model.SetValue(model.Insert(index), 0, newSubtitle);
	}

	/// <summary>Removes a subtitle from the collection, given its <see cref="TreePath" />.</summary>
	/// <returns>Whether the subtitle could be removed.</returns>
	public bool Remove (TreePath path) {
		int index = Util.PathToInt(path);
		if (!Collection.Contains(index))
			return false;

		TreeIter iter;
		model.GetIter(out iter, path);

		Collection.Remove(index);
		model.Remove(ref iter);
		return true;
	}

	/// <summary>Removes a subtitle from the list, given its index.</summary>
	/// <returns>Whether the subtitle could be removed.</returns>
	public bool Remove (int index) {
		if (!Collection.Contains(index))
			return false;

		TreeIter iter;
		model.GetIterFromString(out iter, index.ToString());

		Collection.Remove(index);
		model.Remove(ref iter);
		return true;
	}

	/// <summary>Removes a collection of subtitles from the subtitle collection, given their multiple <see cref="TreePath" />.</summary>
	/// <param name="paths">The collection of paths corresponding to the subtitles to be removed. Its elements must be ordered without repetition.</param>
	/// <returns>Whether the subtitles could be removed. This method removes all of the subtitles or none.</returns>
	public bool Remove (TreePath[] paths) { //TODO seems to perform worse than before, for no visible reason. Check out. Maybe has to due with event being thrown on count change?
		if ((paths == null) || (paths.Length == 0))
			return true;

		/* Check if the last member is within the valid range */
		int lastIndex = Util.PathToInt(paths[paths.Length - 1]);
		if (!Collection.Contains(lastIndex))
			return false;

		for (int index = 0 ; index < paths.Length ; index++) {
			TreePath path = paths[index];
			int subtitleIndex = Util.PathToInt(path) - index; //Subtract pathIndex because indexes decrement as subtitles are removed.
			Remove(subtitleIndex);
		}
		return true;
	}

	public bool RemoveRange (TreePath firstPath, TreePath lastPath) {
		if ((firstPath == null) || (lastPath == null))
			return false;

		int firstSubtitleNumber = Util.PathToInt(firstPath);
		int lastSubtitleNumber = Util.PathToInt(lastPath);
		if ((firstSubtitleNumber < 0) || (firstSubtitleNumber > lastSubtitleNumber) || (lastSubtitleNumber >= Collection.Count))
			return false;

		for (int index = firstSubtitleNumber ; index <= lastSubtitleNumber ; index++) {
			if (!Remove(firstSubtitleNumber)) //the index is constant as subtitles are removed
				return false;
		}
		return true;
	}

	/// <summary>Loads possible extra subtitles at the end of the model.</summary>
	/// <remarks>Extra subtitles exist if subtitles were added to the base collection.</remarks>
	public void AddExtra (int extraCount) {
		if (extraCount <= 0)
			return;

		int lastIndex = Count + extraCount - 1;

		if (Count == 0)
			AddNewAt(0);

		for (int index = Count - 1 ; index < lastIndex ; index++)
			AddNewAfter(index);
	}


	/* Private members */

	/// <summary>Loads the <see cref="ListModel" /> from the subtitle collection.</summary>
	private void LoadModelFromCollection () {
		model.Clear();
		foreach (Subtitle subtitle in Collection) {
			model.AppendValues(subtitle);
		}
	}

}

}
