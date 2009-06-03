/*
 * This file is part of SubLib.
 * Copyright (C) 2008-2009 Pedro Castro
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

namespace SubLib.Core.Timing {

/// <summary>Performs synchronization operations.</summary>
public class SynchronizeOperator {
	private Subtitles subtitles = null;
	
	public SynchronizeOperator (Subtitles subtitles) {
		this.subtitles = subtitles;
	}

	/* Public members */
	
	public bool Sync (SyncPoints syncPoints, bool toSyncAll) {
		SyncPoints pointsToUse = AdaptForOperation(syncPoints, toSyncAll);
		if (!AreSyncArgsValid(pointsToUse))
			return false;

		System.Console.WriteLine("Here");
		SyncPoint previous = pointsToUse[0];
		for (int index = 1 ; index < pointsToUse.Count ; index++) {
			System.Console.WriteLine(index);
			SyncPoint current = pointsToUse[index];
			SyncUtil.Sync(subtitles, previous, current);
		
			previous = current;
		}
		
		return true;
	}
	
	
	/* Private members */
	
	private SyncPoints AdaptForOperation (SyncPoints syncPoints, bool toSyncAll) {
		if ((syncPoints == null) || (!toSyncAll))
			return syncPoints;
		
		SyncPoints adapted = syncPoints.Clone();
		
		/* Add the first subtitle if possible */
		int firstSubtitleNumber = 0;
		if ((subtitles.Collection.Count > 0) && (!adapted.Contains(firstSubtitleNumber))) {
			Subtitle firstSubtitle = subtitles.Collection[firstSubtitleNumber];
			Domain.Timing firstSubtitleTiming = new Domain.Timing(firstSubtitle.Frames.Start, firstSubtitle.Times.Start);
			SyncPoint firstSyncPoint = new SyncPoint(firstSubtitleNumber, firstSubtitleTiming, firstSubtitleTiming);
			adapted.Add(firstSyncPoint);
		}
		
		/* Add last subtitle if possible */
		int lastSubtitleNumber = subtitles.Collection.Count - 1;
		if ((subtitles.Collection.Count > 1) && (!adapted.Contains(lastSubtitleNumber))) {
			Subtitle lastSubtitle = subtitles.Collection[lastSubtitleNumber - 1];
			Domain.Timing lastSubtitleTiming = new Domain.Timing(lastSubtitle.Frames.Start, lastSubtitle.Times.Start);
			SyncPoint lastSyncPoint = new SyncPoint(lastSubtitleNumber, lastSubtitleTiming, lastSubtitleTiming);
			adapted.Add(lastSyncPoint);
		}
		
		return adapted;
	}
	
	private bool AreSyncArgsValid (SyncPoints syncPoints) {
		System.Console.WriteLine(1);
		if ((syncPoints == null) || (syncPoints.Count < 2) || (syncPoints[syncPoints.Count - 1].SubtitleNumber > subtitles.Collection.Count))
			return false;
		System.Console.WriteLine(2);
		SyncPoint previous = syncPoints[0];
		for (int index = 1 ; index < syncPoints.Count ; index++) {
			SyncPoint current = syncPoints[index];
			if (!SyncUtil.AreSyncPointsValid(subtitles, previous, current))
				return false;
		
			previous = current;
		}
		return true;
	}
	
}

}
