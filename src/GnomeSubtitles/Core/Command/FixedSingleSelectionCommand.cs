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

namespace GnomeSubtitles.Core.Command {

public abstract class FixedSingleSelectionCommand : SingleSelectionCommand {
	private bool reselect = false;

	public FixedSingleSelectionCommand (string description, bool canGroup, bool reselect) : base(description, canGroup) {
		this.reselect = reselect;
	}

	public override bool Execute () {
		bool completed = ChangeValues();
		if (!completed)
			return false;

		if (reselect)
			Base.Ui.View.Selection.Select(Path, true, true);
		else {
			Base.Ui.View.Selection.ScrollToFocus(Path, true);
			Base.Ui.View.Refresh();
		}

		PostProcess();
		return true;
	}

	public override void Undo () {
		ChangeValues();
		Base.Ui.View.Selection.Select(Path, true, true);
		PostProcess();
	}

	/* Methods to be extended */

	protected virtual bool ChangeValues () {
		return true;
	}

	protected virtual void PostProcess () {
		return;
	}

}

}
