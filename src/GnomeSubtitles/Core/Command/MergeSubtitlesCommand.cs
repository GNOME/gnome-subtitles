/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2011-2019 Pedro Castro
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

using System;
using GnomeSubtitles.Ui.View;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using SubLib.Core.Timing;

namespace GnomeSubtitles.Core.Command {

public class MergeSubtitlesCommand : MultipleSelectionCommand {
	private static string description = Catalog.GetString("Merging subtitles");
	private Subtitle[] subtitlesBefore = null;
	bool translationCleared = false; //Translation was cleared, so if we are undoing, we need to use the new translation text if present

	public MergeSubtitlesCommand () : base(description, false, SelectionIntended.Range, null) {
	}

	public override bool Execute () {
		SetCommandTarget(Base.Document.IsTranslationLoaded ? CommandTarget.NormalAndTranslation : CommandTarget.Normal);

		if (!Merge()) {
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
			int firstSubtitleIndex = Util.PathToInt(this.FirstPath);
			Subtitle subtitle = Base.Document.Subtitles[firstSubtitleIndex];
			if (!subtitle.Translation.IsEmpty) {
				string[] translationLines = subtitle.Translation.GetLines();

				for (int i = 0; i < this.subtitlesBefore.Length; i++) {
					Subtitle subtitleBefore = this.subtitlesBefore[i];
				
					//If we don't have more translation lines, set it to empty
					if (i >= translationLines.Length) {
						subtitleBefore.Translation.Clear();
						continue;
					}

					//If it's the last subtitle, check if we need to merge all remaining translations into it
					if ((i == this.subtitlesBefore.Length - 1) && (this.subtitlesBefore.Length < translationLines.Length)) {
						string lastLinesMerged = String.Join("\n", translationLines, i, translationLines.Length - this.subtitlesBefore.Length + 1);
						subtitleBefore.Translation.Set(lastLinesMerged);
						continue;
					}

					//Normal stuff, just set the translation line
					subtitleBefore.Translation.Set(translationLines[i]);
				}
			}
			translationCleared = false;
		}

		SetCommandTarget(Base.Document.IsTranslationLoaded ? CommandTarget.NormalAndTranslation : CommandTarget.Normal);
		
		Base.Document.Subtitles.Remove(this.FirstPath);
		Base.Ui.View.Insert(this.subtitlesBefore, this.FirstPath, this.FirstPath);
		PostProcess();
	}

	/*
	 * In Redo, we just run the Split command again, so we don't reuse old subtitles. Because of that, if a new translation
	 * has been loaded on the meanwhile, there's no problem, the current translation text will be used.
	 */
	public override void Redo () {
		SetCommandTarget(Base.Document.IsTranslationLoaded ? CommandTarget.NormalAndTranslation : CommandTarget.Normal);
	
		if (!Merge()) {
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

	private bool Merge () {
		Ui.View.Subtitles subtitles = Base.Document.Subtitles;
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
		MergeOperator mergeOperator = new MergeOperator(subtitles);
		if (!mergeOperator.Merge(firstPathInt, lastPathInt)) {
			return false;
		}

		TreePath secondPath = Util.IntToPath(firstPathInt + 1);
		subtitles.RemoveRange(secondPath, this.LastPath);
		Base.Ui.View.RedrawPath(this.FirstPath);
		Base.Ui.View.Selection.Select(this.FirstPath, true, true);
		return true;
	}

}

}