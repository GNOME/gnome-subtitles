/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2017-2019 Pedro Castro
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

public class SplitSubtitlesCommand : MultipleSelectionCommand {
	private static string description = Catalog.GetString("Splitting subtitles");
	private Subtitle[] subtitlesBefore = null;
	private TreePath[] pathsAfter = null;
	private Subtitle[] subtitlesAfter = null;

	public SplitSubtitlesCommand () : base(description, false, SelectionIntended.Simple, null) {
	}

	public override bool Execute () {
		Ui.View.Subtitles subtitles = Base.Document.Subtitles;
		ArrayList pathsBefore = new ArrayList();
		ArrayList subtitlesBefore = new ArrayList();
		ArrayList pathsAfter = new ArrayList();

		SplitOperator splitOperator = new SplitOperator(subtitles, Base.Config.TimingsTimeBetweenSubtitles);

		foreach (TreePath path in Paths) {
			int subtitlesThatHaveBeenAdded = pathsAfter.Count - pathsBefore.Count; //number of subtitles that have been added ever since, in this loop
			int subtitleIndex = Util.PathToInt(path) + subtitlesThatHaveBeenAdded;
			Subtitle subtitle = subtitles[subtitleIndex];
			
			Subtitle[] newSubtitles = splitOperator.Split(subtitle);
			if (newSubtitles != null) {
				pathsBefore.Add(path);
				subtitlesBefore.Add(subtitle);
				
				subtitles.Remove(subtitleIndex);
				for (int i = 0 ; i < newSubtitles.Length ; i++) {
					pathsAfter.Add(Util.IntToPath(subtitleIndex + i));
					subtitles.Add(newSubtitles[i], subtitleIndex + i);
				}
			}
		}

		if (subtitlesBefore.Count == 0) {
			return false;
		}
		
		this.subtitlesBefore = (Subtitle [])subtitlesBefore.ToArray(typeof(Subtitle));
		this.Paths = (TreePath [])pathsBefore.ToArray(typeof(TreePath));
		this.pathsAfter = (TreePath [])pathsAfter.ToArray(typeof(TreePath));
		Base.Ui.View.RedrawPaths(this.pathsAfter);
		Base.Ui.View.Selection.Select(this.pathsAfter, this.pathsAfter [0], true);
		PostProcess();
		return true;
	}

	public override void Undo () {
		if (this.subtitlesAfter == null) {
			this.subtitlesAfter = GetSubtitlesAfter(Base.Document.Subtitles, this.pathsAfter);
		}
		Base.Document.Subtitles.Remove(this.pathsAfter);
		Base.Ui.View.Insert(this.subtitlesBefore, this.Paths, this.FirstPath);
		PostProcess();
	}

	public override void Redo () {
		Base.Document.Subtitles.Remove(this.Paths);
		Base.Ui.View.Insert(this.subtitlesAfter, this.pathsAfter, this.pathsAfter[0]);
		PostProcess();
	}

	/* Protected members */

	protected void PostProcess () {
		Base.Ui.Video.SeekToSelection(true);
	}


	/* Private members */

	private Subtitle[] GetSubtitlesAfter (GnomeSubtitles.Ui.View.Subtitles subtitles, TreePath[] pathsAfter) {
		Subtitle[] subtitlesAfter = new Subtitle[pathsAfter.Length];
		for (int index = 0 ; index < pathsAfter.Length ; index++) {
			TreePath path = pathsAfter[index];
			int subtitleIndex = Util.PathToInt(path);
			subtitlesAfter[index] = subtitles[subtitleIndex];
		}
		return subtitlesAfter;
	}

}

}
