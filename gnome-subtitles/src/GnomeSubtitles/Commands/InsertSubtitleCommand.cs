/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006 Pedro Castro
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

public abstract class InsertSubtitleCommand : SingleSelectionCommand {
	private static string description = "Inserting Subtitle";
	private TreePath newPath = null;
	private Subtitle subtitle = null;

	public InsertSubtitleCommand (TreePath path) : base(description, false, path) {
		newPath = GetNewPath();
	}
	
	/* Protected properties */
	
	protected TreePath NewPath {
		get { return newPath; }
	}
	
	/* Public members */

	public override bool Execute () {
		InsertNew();
		subtitle = Global.Document.Subtitles[newPath];
		return true;
	}
	
	public override void Undo () {
		bool selectNext = ((Path != null) && (Path.Compare(newPath) == 1));
		Global.GUI.View.Remove(newPath, selectNext);
	}

	public override void Redo () {
		Global.GUI.View.Insert(subtitle, newPath);
	}
	
	/* Methods to be extended */
	
	protected abstract TreePath GetNewPath ();
	protected abstract void InsertNew ();

}

public class InsertSubtitleAfterCommand : InsertSubtitleCommand {

	public InsertSubtitleAfterCommand () : base(Global.GUI.View.Selection.LastPath) {
	}
	
	protected override TreePath GetNewPath () {
		return Util.PathNext(Path);
	}
	
	protected override void InsertNew () {
		Global.GUI.View.InsertNewAfter(Path);
	}

}

public class InsertSubtitleBeforeCommand : InsertSubtitleCommand {

	public InsertSubtitleBeforeCommand () : base(Global.GUI.View.Selection.FirstPath) {
	}

	protected override TreePath GetNewPath () {
		return Path;
	}
	
	protected override void InsertNew () {
		Global.GUI.View.InsertNewBefore(Path);
	}

}
 
public class InsertFirstSubtitleCommand : InsertSubtitleCommand {
	
	public InsertFirstSubtitleCommand () : base(null) {
	}
	
	protected override TreePath GetNewPath () {
		return TreePath.NewFirst();
	}
	
	protected override void InsertNew () {
		Global.GUI.View.InsertNewAt(NewPath);
	}

}

}
