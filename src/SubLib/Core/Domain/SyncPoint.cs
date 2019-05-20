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

namespace SubLib.Core.Domain {

public class SyncPoint {
	private int subtitleNumber = 0;
	private Timing current = null;
	private Timing correct = null;

	public SyncPoint (int subtitleNumber, Timing current, Timing correct) {
		this.subtitleNumber = subtitleNumber;
		this.current = current;
		this.correct = correct;
	}

	/* Properties */

	public int SubtitleNumber {
		get { return subtitleNumber; }
	}

	public Timing Current {
		get { return current; }
	}

	public Timing Correct {
		get { return correct; }
	}

	public int CompareCurrentTo (SyncPoint otherSyncPoint) {
		return current.CompareTo(otherSyncPoint.Current);
	}


}

}
