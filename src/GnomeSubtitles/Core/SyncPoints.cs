/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008-2009 Pedro Castro
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

using Gtk;
using SubLib.Core.Domain;
using System.Collections;

namespace GnomeSubtitles.Core {

public class SyncPoints {
	private ListStore model = new ListStore(typeof(SyncPoint));
	private SubLib.Core.Domain.SyncPoints collection = new SubLib.Core.Domain.SyncPoints();


	public SyncPoints () : base() {
		LoadModelFromCollection();
	}

	/* Indexers */

	public SyncPoint this [TreeIter iter] {
		get { return model.GetValue(iter, 0) as SyncPoint; }
	}

	public SyncPoint this [TreePath path] {
		get { return collection[path.Indices[0]]; }
	}

	/* Public properties */

	public ListStore Model {
		get { return model; }
	}

	public SubLib.Core.Domain.SyncPoints Collection {
		get { return collection; }
	}

	/* Public methods */

	public int Add (SyncPoint syncPoint) {
		bool didReplace = collection.Add(syncPoint);
		int index = collection.IndexOf(syncPoint);
		if (didReplace) {
			Replace(index, syncPoint); //Replace existing
			return index;
		}
		else if (collection[collection.Count - 1].SubtitleNumber == syncPoint.SubtitleNumber) {
			Append(syncPoint); //Append to the end
			return collection.Count - 1;
		}
		else {
			Insert(index, syncPoint); //Insert in position, not replacing
			return index;
		}
	}

	public void Remove (TreePath[] paths) {
		foreach (TreePath path in paths) {
			TreeIter iter;
			model.GetIter(out iter, path);
			model.Remove(ref iter);

			collection.Remove(Util.PathToInt(path));
		}
	}

	public IEnumerator GetEnumerator () {
		return collection.GetEnumerator();
	}


	/* Private members */

	private void LoadModelFromCollection () {
		model.Clear();
		foreach (SyncPoint syncPoint in collection) {
			model.AppendValues(syncPoint);
		}
	}


	private void Insert (int index, SyncPoint syncPoint) {
		model.SetValue(model.Insert(index), 0, syncPoint);
	}

	private void Replace (int index, SyncPoint syncPoint) {
		TreeIter iter;
		model.GetIterFromString(out iter, index.ToString());
		model.SetValue(iter, 0, syncPoint);
	}

	private void Append (SyncPoint syncPoint) {
		model.AppendValues(syncPoint);
	}

}

}
