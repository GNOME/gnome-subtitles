/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2007 Pedro Castro
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

using Gtk;
using SubLib;

namespace GnomeSubtitles {

public class DeleteSubtitlesCommand : MultipleSelectionCommand {
	private static string description = Cat.Get("Deleting Subtitles");
	private Subtitle[] subtitles = null;
	
	public DeleteSubtitlesCommand () : base(description, false, SelectionIntended.Simple) {
		StoreSubtitles();
	}

	
	public override bool Execute () {
		Global.GUI.View.Remove(Paths);
		return true;
	}
	
	public override void Undo () {
		Global.GUI.View.Insert(subtitles, Paths, Focus);	
	}
	
	public override void Redo () {
		Execute();
	}
		
	/* Private members */

	private void StoreSubtitles () {
		int count = Paths.Length;
		subtitles = new Subtitle[count];
		for (int index = 0 ; index < count ; index++) {
			TreePath path = Paths[index];
			subtitles[index] = Global.Document.Subtitles[path];
		}	
	}

}

}
