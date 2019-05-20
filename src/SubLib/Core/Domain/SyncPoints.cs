/*
 * This file is part of SubLib.
 * Copyright (C) 2008-2019 Pedro Castro
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

public class SyncPoints {
	protected SortedList syncPoints = null;

	public SyncPoints () {
		this.syncPoints = new SortedList();
	}

	public SyncPoints (SortedList syncPoints) {
		this.syncPoints = syncPoints;
	}

	/* Indexers */

	public SyncPoint this [int index] {
		get { return syncPoints.GetByIndex(index) as SyncPoint; }
	}

	/* Properties */

	public int Count {
		get { return syncPoints.Count; }
	}

	/* Public methods */

	public SyncPoint Get (int index) {
		return syncPoints.GetByIndex(index) as SyncPoint;
	}

	public int IndexOf (SyncPoint syncPoint) {
		return syncPoints.IndexOfKey(syncPoint.SubtitleNumber);
	}

	public bool Contains (int subtitleNumber) {
		return syncPoints.ContainsKey(subtitleNumber);
	}

	public bool Add (SyncPoint syncPoint) {
		bool willReplace = Contains(syncPoint.SubtitleNumber);
		syncPoints[syncPoint.SubtitleNumber] = syncPoint;
		return willReplace;
	}

	public void Remove (int index) {
		syncPoints.RemoveAt(index);
	}

	public IEnumerator GetEnumerator () {
		return syncPoints.GetEnumerator();
	}

	public SyncPoints Clone () {
		return new SyncPoints(syncPoints.Clone() as SortedList);
	}



}

}
