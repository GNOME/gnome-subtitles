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

namespace GnomeSubtitles.Core.Command {

public class DeleteSubtitlesCommand : MultipleSelectionCommand {
	private static string description = Catalog.GetString("Deleting Subtitles");
	private Subtitle[] subtitles = null;

	public DeleteSubtitlesCommand () : base(description, false, SelectionIntended.Simple, null) {
	}


	public override bool Execute () {
		//Store subtitles to be deleted
		int count = Paths.Length;
		subtitles = new Subtitle[count];
		for (int index = 0 ; index < count ; index++) {
			TreePath path = Paths[index];
			subtitles[index] = Base.Document.Subtitles[path];
		}
		
		//If translations are loaded, our command affects both the normal document and translations
		SetCommandTarget(Base.Document.IsTranslationLoaded ? CommandTarget.NormalAndTranslation : CommandTarget.Normal);
	
		Base.Ui.View.Remove(Paths);
		return true;
	}

	public override void Undo () {
		Base.Ui.View.Insert(subtitles, Paths, Focus);
	}

	public override void Redo () {
		Execute();
	}
	
	public override void ClearTarget (CommandTarget target) {
		if (target == CommandTarget.Translation) {
			foreach (Subtitle subtitle in subtitles) {
				if (subtitle.HasTranslation) {
					subtitle.Translation.Clear();
				}
			}
		}
	}

}

}
