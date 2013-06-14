/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2011 Pedro Castro
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
using System.Collections;

namespace GnomeSubtitles.Core.Command {

public class MergeSubtitlesCommand : MultipleSelectionCommand {
	private static string description = Catalog.GetString("Merging subtitles");
	private Subtitle[] subtitlesBefore = null;
	private Subtitle subtitleAfter = null;

	public MergeSubtitlesCommand () : base(description, false, SelectionIntended.Range, null) {
	}

	public override bool Execute () {
		GnomeSubtitles.Ui.View.Subtitles subtitles = Base.Document.Subtitles;
		int firstPathInt = Util.PathToInt(this.FirstPath);
		int lastPathInt = Util.PathToInt(this.LastPath);

		/* Store selected subtitles */
		int subtitleCount = lastPathInt - firstPathInt + 1;
		this.subtitlesBefore = new Subtitle[subtitleCount];
		this.subtitlesBefore[0] = subtitles[firstPathInt].Clone(subtitles.Properties); //only the first needs to be cloned, the rest won't be changed
		for (int index = 1, currentPath = firstPathInt + 1 ; index < subtitleCount ; index++, currentPath++) {
			this.subtitlesBefore[index] = subtitles[currentPath];
		}

		/* Merge subtitles */
		MergeOperator mergeOperator = new MergeOperator(Base.Document.Subtitles);
		if (!mergeOperator.Merge(firstPathInt, lastPathInt))
			return false;
		else {
			TreePath secondPath = Util.IntToPath(firstPathInt + 1);
			subtitles.RemoveRange(secondPath, this.LastPath);
			Base.Ui.View.RedrawPath(this.FirstPath);
			Base.Ui.View.Selection.Select(this.FirstPath, true, true);
			PostProcess();
			return true;
		}
	}

	public override void Undo () {
		if (this.subtitleAfter == null) {
			this.subtitleAfter = Base.Document.Subtitles[this.FirstPath];
		}
		Base.Document.Subtitles.Remove(this.FirstPath);
		Base.Ui.View.Insert(this.subtitlesBefore, this.FirstPath, this.FirstPath);
		PostProcess();
	}

	public override void Redo () {
		Base.Document.Subtitles.RemoveRange(this.FirstPath, this.LastPath);
		Base.Ui.View.Insert(this.subtitleAfter, this.FirstPath);
		PostProcess();
	}

	/* Protected members */

	protected void PostProcess () {
		Base.Ui.Video.SeekToSelection(true);
	}

}

}
