/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2019 Pedro Castro
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
using SubLib.Core.Search;
using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace GnomeSubtitles.Core.Command {

public class ReplaceAllCommand : MultipleSelectionCommand {
	private static string description = Catalog.GetString("Replacing All");

	private ArrayList replacedSubtitles = null;
	private Regex regex = null;
	private string replacement = String.Empty;

	public ReplaceAllCommand (Regex regex, string replacement) : base(description, false, SelectionIntended.Simple, null, false) {
		this.regex = regex;
		this.replacement = replacement;
	}


	public override bool Execute () {
		SearchOperator searchOp = new SearchOperator(Base.Document.Subtitles);
		replacedSubtitles = searchOp.ReplaceAll(regex, replacement);
		if (replacedSubtitles.Count  == 0)
			return false;

		TreePath[] paths = null;
		CommandTarget target = CommandTarget.Normal;
		GetCommandData(out paths, out target);

		SetCommandTarget(target);
		Paths = paths;
		if (Paths.Length > 0)
			Focus = Paths[0];

		Base.Ui.View.Selection.Select(Paths, Focus, true);
		return true;
	}


	public override void Undo () {
		ArrayList newReplacedSubtitles = new ArrayList();

		/* Get values before replacing */
		foreach (SubtitleReplaceResult replacedSubtitle in replacedSubtitles) {
			Subtitle subtitle = Base.Document.Subtitles[replacedSubtitle.Number];
			string oldText = (replacedSubtitle.Text != null ? subtitle.Text.Get() : null);
			string oldTranslation = (replacedSubtitle.Translation != null ? subtitle.Translation.Get() : null);
			newReplacedSubtitles.Add(new SubtitleReplaceResult(replacedSubtitle.Number, oldText, oldTranslation));
		}

		/* Replace the values */
		foreach (SubtitleReplaceResult replacedSubtitle in replacedSubtitles) {
			Subtitle subtitle = Base.Document.Subtitles[replacedSubtitle.Number];

			if (replacedSubtitle.Text != null)
				subtitle.Text.Set(replacedSubtitle.Text);

			if (replacedSubtitle.Translation != null)
				subtitle.Translation.Set(replacedSubtitle.Translation);
		}

		replacedSubtitles = newReplacedSubtitles;
		Base.Ui.View.Selection.Select(Paths, Focus, true);
	}

	public override void Redo () {
		Undo();
	}
	
	public override void ClearTarget (CommandTarget target) {
		if (target == CommandTarget.Translation) {
			ArrayList newReplacedSubtitles = new ArrayList();
			
			foreach (SubtitleReplaceResult replacedSubtitle in replacedSubtitles) {
				if (replacedSubtitle.Text != null) {
					newReplacedSubtitles.Add(new SubtitleReplaceResult(replacedSubtitle.Number, replacedSubtitle.Text, null));
				}
			}
			
			replacedSubtitles = newReplacedSubtitles;
		}
	}


	/* Private members */

	private void GetCommandData (out TreePath[] paths, out CommandTarget target) {
		ArrayList foundPaths = new ArrayList();
		bool foundText = false;
		bool foundTranslation = false;

		foreach (SubtitleReplaceResult replacedSubtitle in replacedSubtitles) {
			foundPaths.Add(Util.IntToPath(replacedSubtitle.Number));

			if ((!foundText) && (replacedSubtitle.Text != null))
				foundText = true;

			if ((!foundTranslation) && (replacedSubtitle.Translation != null))
				foundTranslation = true;
		}
		paths = foundPaths.ToArray(typeof(TreePath)) as TreePath[];

		if (foundText && foundTranslation)
			target = CommandTarget.NormalAndTranslation;
		else if (foundText)
			target = CommandTarget.Normal;
		else
			target = CommandTarget.Translation;
	}

}

}