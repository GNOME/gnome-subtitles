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
using System;
using System.Collections;
using System.Collections.Generic;

namespace GnomeSubtitles.Core.Command {

public class SplitSubtitlesCommand : MultipleSelectionCommand {
	private static string description = Catalog.GetString("Splitting subtitles");

	private Subtitle[] subtitlesBefore = null;
	private Dictionary<int, int[]> mapping = new Dictionary<int, int[]>(); //Maps the indices of the subtitles that were split <originalIndex, resultingIndices[]>
	private TreePath[] pathsAfter = null;
	bool translationCleared = false; //Translation was cleared, so if we are undoing, we need to use the new translation text if present

	public SplitSubtitlesCommand () : base(description, false, SelectionIntended.Simple, null) {
	}
	
	public override bool Execute () {
		SetCommandTarget(Base.Document.IsTranslationLoaded ? CommandTarget.NormalAndTranslation : CommandTarget.Normal);

		if (!Split()) {
			return false;
		}

		PostProcess();
		return true;
	}

	public override void Undo () {
		/*
		 * If the command originally applied to translations and they have changed, we need to use the new translations because we didn't have them
		 * when this command was first executed. The same goes if the command didn't originally have translations but it now has.
		 */
		bool hadTranslation = (this.Target == CommandTarget.NormalAndTranslation);
		bool updateTranslation = Base.Document.IsTranslationLoaded && ((hadTranslation && translationCleared) || !hadTranslation);
		
		if (updateTranslation) {
			for (int i = 0; i < subtitlesBefore.Length; i++) {
				Subtitle subtitle = subtitlesBefore[i];
				int subtitleIndex = Util.PathToInt(this.Paths[i]);

				ArrayList translations = new ArrayList();
				foreach (int subtitleIndexAfter in this.mapping[subtitleIndex]) {
					string translation = Base.Document.Subtitles[subtitleIndexAfter].Translation.Get();
					if (!String.IsNullOrEmpty(translation)) {
						translations.Add(translation);
					}
				}
				
				string mergedTranslation = String.Join("\n", (string[])translations.ToArray(typeof(string)));
				subtitle.Translation.Set(mergedTranslation);
			}
			translationCleared = false;
		}

		SetCommandTarget(Base.Document.IsTranslationLoaded ? CommandTarget.NormalAndTranslation : CommandTarget.Normal);
		

		Base.Document.Subtitles.Remove(this.pathsAfter);
		Base.Ui.View.Insert(this.subtitlesBefore, this.Paths, this.FirstPath);
		PostProcess();
	}


	/*
	 * In Redo, we just run the Split command again, so we don't reuse old subtitles. Because of that, if a new translation
	 * has been loaded on the meanwhile, there's no problem, the current translation text will be used.
	 */
	public override void Redo () {
		SetCommandTarget(Base.Document.IsTranslationLoaded ? CommandTarget.NormalAndTranslation : CommandTarget.Normal);
	
		if (!Split()) {
			return;
		}

		PostProcess();
	}
	
	public override void ClearTarget (CommandTarget target) {
		if (target == CommandTarget.Translation) {
			if (subtitlesBefore != null) {
				foreach (Subtitle subtitle in subtitlesBefore) {
					if (subtitle.HasTranslation) {
						subtitle.Translation.Clear();
					}
				}
			}

			translationCleared = true;			
		}
	}


	/* Protected members */

	protected void PostProcess () {
		Base.Ui.Video.SeekToSelection(true);
	}


	/* Private members */

	private bool Split () {
		Ui.View.Subtitles subtitles = Base.Document.Subtitles;
		ArrayList pathsBefore = new ArrayList();
		ArrayList subtitlesBefore = new ArrayList();
		ArrayList pathsAfter = new ArrayList();
		Dictionary<int, int[]> mapping = new Dictionary<int, int[]>();

		SplitOperator splitOperator = new SplitOperator(subtitles, Base.Config.TimingsTimeBetweenSubtitles);

		foreach (TreePath path in Paths) {
			int originalSubtitleIndex = Util.PathToInt(path);
			
			int subtitlesThatHaveBeenAdded = pathsAfter.Count - pathsBefore.Count; //number of subtitles that have been added ever since, in this loop
			int subtitleIndex = originalSubtitleIndex + subtitlesThatHaveBeenAdded;
			Subtitle subtitle = subtitles[subtitleIndex];
			
			Subtitle[] newSubtitles = splitOperator.Split(subtitle);
			if (newSubtitles != null) {
				pathsBefore.Add(path);
				subtitlesBefore.Add(subtitle);
				
				subtitles.Remove(subtitleIndex);
				int[] newSubtitleIndices = new int[newSubtitles.Length];
				for (int i = 0 ; i < newSubtitles.Length ; i++) {
					int newSubtitleIndex = subtitleIndex + i;
					pathsAfter.Add(Util.IntToPath(newSubtitleIndex));
					subtitles.Add(newSubtitles[i], newSubtitleIndex);
					newSubtitleIndices[i] = newSubtitleIndex;
				}
				
				mapping.Add(originalSubtitleIndex, newSubtitleIndices);
			}
		}

		if (subtitlesBefore.Count == 0) {
			return false;
		}
		
		this.subtitlesBefore = (Subtitle [])subtitlesBefore.ToArray(typeof(Subtitle));
		this.Paths = (TreePath [])pathsBefore.ToArray(typeof(TreePath));
		this.pathsAfter = (TreePath [])pathsAfter.ToArray(typeof(TreePath));
		this.mapping = mapping;
		
		Base.Ui.View.RedrawPaths(this.pathsAfter);
		Base.Ui.View.Selection.Select(this.pathsAfter, this.pathsAfter[0], true);
		PostProcess();
		return true;
	}
}

}