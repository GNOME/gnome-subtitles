/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2021 Pedro Castro
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
using Mono.Unix;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Core.Command {

public abstract class InsertSubtitleCommand : SingleSelectionCommand {
	private static string description = Catalog.GetString("Inserting Subtitle");
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
		subtitle = Base.Document.Subtitles[newPath].Clone(Base.Document.Subtitles.Properties);
		return true;
	}

	public override void Undo () {
		bool selectNext = (Path != null) && (Path.Compare(newPath) == 1);
		Base.Ui.View.Remove(newPath, selectNext);
	}

	public override void Redo () {
		Base.Ui.View.Insert(subtitle, newPath);
	}

	/* Methods to be extended */

	protected abstract TreePath GetNewPath ();
	protected abstract void InsertNew ();

}

public class InsertSubtitleAfterCommand : InsertSubtitleCommand {

	public InsertSubtitleAfterCommand () : base(Base.Ui.View.Selection.LastPath) {
	}

	protected override TreePath GetNewPath () {
		return Util.PathNext(Path);
	}

	protected override void InsertNew () {
		Base.Ui.View.InsertNewAfter(Path);
	}

}

public class InsertSubtitleBeforeCommand : InsertSubtitleCommand {

	public InsertSubtitleBeforeCommand () : base(Base.Ui.View.Selection.FirstPath) {
	}

	protected override TreePath GetNewPath () {
		return Path;
	}

	protected override void InsertNew () {
		Base.Ui.View.InsertNewBefore(Path);
	}

}

public class InsertFirstSubtitleCommand : InsertSubtitleCommand {

	public InsertFirstSubtitleCommand () : base(null) {
	}

	protected override TreePath GetNewPath () {
		return TreePath.NewFirst();
	}

	protected override void InsertNew () {
		Base.Ui.View.InsertNewAt(NewPath);
	}

}

public class InsertLastSubtitleCommand : InsertSubtitleCommand {

	public InsertLastSubtitleCommand () : base(Util.IntToPath(Base.Document.Subtitles.Count - 1)) {
	}

	protected override TreePath GetNewPath () {
		return Util.IntToPath(Base.Document.Subtitles.Count);
	}

	protected override void InsertNew () {
		Base.Ui.View.InsertNewAfter(Path);
	}

}

public class InsertSubtitleAtVideoPositionCommand : InsertSubtitleCommand {
	private TimeSpan subtitleTime = TimeSpan.Zero;

	public InsertSubtitleAtVideoPositionCommand () : base(null) {
	}

	protected override TreePath GetNewPath () {
		subtitleTime = Base.Ui.Video.Position.CurrentTime;
		if (Base.Ui.Video.IsStatusPlaying && Base.Config.VideoApplyReactionDelay) {
			subtitleTime -= TimeSpan.FromMilliseconds(Base.Config.VideoReactionDelay);
		}

		if (Base.Document.Subtitles.Count == 0)
			return TreePath.NewFirst();

		int index = Base.Ui.Video.Tracker.FindSubtitleNearPosition(subtitleTime);
		Subtitle nearestSubtitle = Base.Document.Subtitles[index];
		if (subtitleTime < nearestSubtitle.Times.Start)
			return Util.IntToPath(index);
		else
			return Util.PathNext(Util.IntToPath(index));
	}

	protected override void InsertNew () {
		Base.Ui.View.InsertNewAt(NewPath, subtitleTime);
	}
}

}