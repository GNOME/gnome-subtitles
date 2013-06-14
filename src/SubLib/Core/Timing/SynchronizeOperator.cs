/*
 * This file is part of SubLib.
 * Copyright (C) 2008-2010 Pedro Castro
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
		if (!AreSyncArgsValid(pointsToUse)) {
			return false;
		}
		SyncPoint previous = pointsToUse[0];
		for (int index = 1 ; index < pointsToUse.Count ; index++) {
			bool syncLast = (index == pointsToUse.Count - 1);
			SyncPoint current = pointsToUse[index];
			SyncUtil.Sync(subtitles, previous, current, syncLast);

			previous = current;
		}

		return true;
	}


	/* Private members */

	private SyncPoints AdaptForOperation (SyncPoints syncPoints, bool toSyncAll) {
		if ((syncPoints == null) || (!toSyncAll) || (toSyncAll && (syncPoints.Count < 2)))
			return syncPoints;

		SyncPoints adapted = syncPoints.Clone();

		/* Add the first subtitle if possible */
		int firstSubtitleNumber = 0;
		if ((subtitles.Collection.Count > 1) && (!adapted.Contains(firstSubtitleNumber))) {

			/* Calculate sync shift and factor using the last 2 sync points */
			SyncPoint firstSyncPoint = adapted[0];
			SyncPoint secondSyncPoint = adapted[1];
			Subtitle firstSyncPointSubtitle = subtitles.Collection[firstSyncPoint.SubtitleNumber];
			Subtitle secondSyncPointSubtitle = subtitles.Collection[secondSyncPoint.SubtitleNumber];
			TimeSpan shift = firstSyncPoint.Correct.Time - firstSyncPointSubtitle.Times.PreciseStart;
			double factor = (secondSyncPoint.Correct.Time - firstSyncPoint.Correct.Time).TotalMilliseconds / (secondSyncPointSubtitle.Times.PreciseStart - firstSyncPointSubtitle.Times.PreciseStart).TotalMilliseconds;

			/* Calculate new time */
			Subtitle firstSubtitle = subtitles.Collection[firstSubtitleNumber];
			TimeSpan firstSubtitleNewTime = firstSubtitle.Times.Start + shift; //Apply shift
			firstSubtitleNewTime = SyncUtil.Scale(firstSubtitleNewTime, TimeSpan.Zero, factor);
			if (firstSubtitleNewTime < TimeSpan.Zero) { //Can't have negative start
				firstSubtitleNewTime = TimeSpan.Zero;
			}
			int firstSubtitleNewFrame = (int)TimingUtil.TimeToFrames(firstSubtitleNewTime, subtitles.Properties.CurrentFrameRate);
			Domain.Timing firstSubtitleNewTiming = new Domain.Timing(firstSubtitleNewFrame, firstSubtitleNewTime);
			Domain.Timing firstSubtitleCurrentTiming = new Domain.Timing(firstSubtitle.Frames.Start, firstSubtitle.Times.Start);
			SyncPoint newFirstSyncPoint = new SyncPoint(firstSubtitleNumber, firstSubtitleCurrentTiming, firstSubtitleNewTiming);
			adapted.Add(newFirstSyncPoint);
		}

		/* Add last subtitle if possible */
		int lastSubtitleNumber = subtitles.Collection.Count - 1;
		if ((subtitles.Collection.Count > 1) && (!adapted.Contains(lastSubtitleNumber))) {

			/* Calculate sync shift and factor using the last 2 sync points */
			SyncPoint penultSyncPoint = adapted[adapted.Count - 2];
			SyncPoint lastSyncPoint = adapted[adapted.Count - 1];
			Subtitle penultSyncPointSubtitle = subtitles.Collection[penultSyncPoint.SubtitleNumber];
			Subtitle lastSyncPointSubtitle = subtitles.Collection[lastSyncPoint.SubtitleNumber];
			TimeSpan shift = penultSyncPoint.Correct.Time - penultSyncPointSubtitle.Times.PreciseStart;
			double factor = (lastSyncPoint.Correct.Time - penultSyncPoint.Correct.Time).TotalMilliseconds / (lastSyncPointSubtitle.Times.PreciseStart - penultSyncPointSubtitle.Times.PreciseStart).TotalMilliseconds;

			/* Calculate new time */
			Subtitle lastSubtitle = subtitles.Collection[lastSubtitleNumber];
			TimeSpan lastSubtitleNewTime = lastSubtitle.Times.Start + shift; //Apply shift
			lastSubtitleNewTime = SyncUtil.Scale(lastSubtitleNewTime, penultSyncPoint.Correct.Time, factor);
			int lastSubtitleNewFrame = (int)TimingUtil.TimeToFrames(lastSubtitleNewTime, subtitles.Properties.CurrentFrameRate);
			Domain.Timing lastSubtitleNewTiming = new Domain.Timing(lastSubtitleNewFrame, lastSubtitleNewTime);
			Domain.Timing lastSubtitleCurrentTiming = new Domain.Timing(lastSubtitle.Frames.Start, lastSubtitle.Times.Start);
			SyncPoint newLastSyncPoint = new SyncPoint(lastSubtitleNumber, lastSubtitleCurrentTiming, lastSubtitleNewTiming);
			adapted.Add(newLastSyncPoint);
		}

		return adapted;
	}

	private bool AreSyncArgsValid (SyncPoints syncPoints) {
		if ((syncPoints == null) || (syncPoints.Count < 2) || (syncPoints[syncPoints.Count - 1].SubtitleNumber > subtitles.Collection.Count))
			return false;

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
