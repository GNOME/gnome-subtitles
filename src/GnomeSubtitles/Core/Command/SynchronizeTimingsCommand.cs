/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2009-2019 Pedro Castro
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

using GnomeSubtitles.Ui.View;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using SubLib.Core.Timing;

namespace GnomeSubtitles.Core.Command {

public class SynchronizeTimingsCommand : FixedMultipleSelectionCommand {
	private static string description = Catalog.GetString("Synchronizing timings");

	private SyncPoints syncPoints = null;
	private bool toSyncAll = false;
	private Times[] lastTimes = null;

	public SynchronizeTimingsCommand (SyncPoints syncPoints, bool toSyncAll, SelectionIntended selectionIntended, TreePath[] pathRange) : base(description, false, selectionIntended, pathRange, true) {
		this.syncPoints = syncPoints;
		this.toSyncAll = toSyncAll;
	}

	protected override bool ChangeValues () {
		Ui.View.Subtitles subtitles = Base.Document.Subtitles;

		if (lastTimes == null) {
			int[] subtitleRange = GetSubtitleRange(subtitles);
			Times[] timesToStore = GetCurrentTimes(subtitleRange, subtitles);
			SynchronizeOperator syncOp = new SynchronizeOperator(subtitles);
			if (!syncOp.Sync(syncPoints.Collection, toSyncAll))
				return false;

			lastTimes = timesToStore;
		}
		else {
			int[] subtitleRange = GetSubtitleRange(subtitles);
			Times[] timesToStore = GetCurrentTimes(subtitleRange, subtitles);

			if (subtitleRange == null)
				return false;

			int subtitleFrom = subtitleRange[0];
			int subtitleTo = subtitleRange[1];
			for (int index = subtitleFrom; index <= subtitleTo ; index++) {
				Subtitle subtitle = subtitles[index];
				Times timesToUse = lastTimes[index - subtitleFrom];
				subtitle.Times.Start = timesToUse.Start;
				subtitle.Times.End = timesToUse.End;
			}
			lastTimes = timesToStore;
		}
		return true;
	}

	private Times[] GetCurrentTimes (int[] subtitleRange, Ui.View.Subtitles subtitles) {
		int subtitleFrom = subtitleRange[0];
		int subtitleTo = subtitleRange[1];
		Times[] currentTimes = new Times[subtitleTo - subtitleFrom + 1];

		for (int index = subtitleFrom; index <= subtitleTo ; index++) {
			Subtitle subtitle = subtitles[index];
			currentTimes[index - subtitleFrom] = subtitle.Times.Clone();
		}

		return currentTimes;
	}

	private int[] GetSubtitleRange (Ui.View.Subtitles subtitles) {
		if (SelectionType == SelectionType.Range) {
			TreePath[] paths = Paths;
			int pathFrom = Util.PathToInt(paths[0]);
			int pathTo = Util.PathToInt(paths[1]);
			return new int[] {pathFrom, pathTo};
		}
		else if (SelectionType == SelectionType.All) {
			if (subtitles.Count < 2)
				return null;

			int pathFrom = 0;
			int pathTo = subtitles.Count - 1;
			return new int[] {pathFrom, pathTo};
		}
		return null;
	}

}

}
