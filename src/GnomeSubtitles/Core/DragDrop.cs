/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2009 Pedro Castro
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

using Gdk;
using GnomeSubtitles.Ui;
using Gtk;

namespace GnomeSubtitles.Core {

public class DragDrop {
	private TargetEntry targetUriList = new TargetEntry("text/uri-list", 0, DragDropTargetUriList);

	/* Public constants */
	public const uint DragDropTargetUriList = 0;


	public DragDrop () {
		Base.InitFinished += OnBaseInitFinished;
	}


	/* Private members */

	private void SetDropTargets () {
		TargetEntry[] targetEntries = new TargetEntry[] { targetUriList };
		Gtk.Drag.DestSet(Base.GetWidget(WidgetNames.SubtitleAreaVBox), DestDefaults.All, targetEntries, DragAction.Copy);
		Gtk.Drag.DestSet(Base.GetWidget(WidgetNames.VideoAreaHBox), DestDefaults.All, targetEntries, DragAction.Copy);
	}

	private void OnBaseInitFinished () {
		SetDropTargets();
    }

}

}
