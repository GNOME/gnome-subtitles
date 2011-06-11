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

public class SplitSubtitlesCommand : MultipleSelectionCommand {
	private static string description = Catalog.GetString("Splitting subtitles");
	private Subtitle[] subtitlesBefore = null;
	private TreePath[] pathsAfter = null;
	private Subtitle[] subtitlesAfter = null;
	
	public SplitSubtitlesCommand () : base(description, false, SelectionIntended.Simple, null) {
	}

	public override bool Execute () {
		GnomeSubtitles.Ui.View.Subtitles subtitles = Base.Document.Subtitles;
		ArrayList pathsBefore = new ArrayList();
		ArrayList subtitlesBefore = new ArrayList();
		ArrayList pathsAfter = new ArrayList();
		
		SplitOperator splitOperator = new SplitOperator(subtitles, 100); //FIXME 100
		
		foreach (TreePath path in Paths) {
			int subtitleIndex = Util.PathToInt(path) + subtitlesBefore.Count; //need to account for subtitles already added in this loop
			Subtitle subtitle = subtitles[subtitleIndex];
			Subtitle subtitleClone = subtitle.Clone(subtitles.Properties);
			Subtitle subtitle2 = splitOperator.Split(subtitle);
			if (subtitle2 != null) {
				pathsAfter.Add(Util.IntToPath(subtitleIndex));
				pathsAfter.Add(Util.IntToPath(subtitleIndex + 1));

				pathsBefore.Add(path);
				subtitlesBefore.Add(subtitleClone);

				subtitles.Add(subtitle2, subtitleIndex + 1);
			}
		}
		
		/* If any subtitle was changed, the command was successful */
		if (subtitlesBefore.Count == 0)
			return false;
		else {
			this.subtitlesBefore = (Subtitle[])subtitlesBefore.ToArray(typeof(Subtitle));
			this.Paths = (TreePath[])pathsBefore.ToArray(typeof(TreePath));
			this.pathsAfter = (TreePath[])pathsAfter.ToArray(typeof(TreePath));
			Base.Ui.View.Selection.Select(this.pathsAfter, null, true);
			return true;
		}
	}
	
	public override void Undo () {
		if (this.subtitlesAfter == null) {
			this.subtitlesAfter = GetSubtitlesAfter(Base.Document.Subtitles, this.pathsAfter);
		}
		Base.Document.Subtitles.Remove(this.pathsAfter);
		Base.Ui.View.Insert(this.subtitlesBefore, this.Paths, null);
	}

	public override void Redo () {
		Base.Document.Subtitles.Remove(this.Paths);
		Base.Ui.View.Insert(this.subtitlesAfter, this.pathsAfter, null);
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
