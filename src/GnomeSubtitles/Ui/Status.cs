/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2008 Pedro Castro
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

using GnomeSubtitles.Core;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;

namespace GnomeSubtitles.Ui {

public class Status {
	//private Statusbar messageStatus = null;
	private Statusbar positionStatus = null;
	private Statusbar overwriteStatus = null;

	public Status () {
		//messageStatus = Base.GetWidget(WidgetNames.MessageStatusbar) as Gtk.Statusbar;
		positionStatus = Base.GetWidget(WidgetNames.PositionStatusbar) as Gtk.Statusbar;
		overwriteStatus = Base.GetWidget(WidgetNames.OverwriteStatusbar) as Gtk.Statusbar;
	}

	/* Public properties */

	public bool Overwrite {
		set {
			//To translators: OVR and INS correspond to the Overwrite and Insert text editing modes.
			string message = (value == true ? Catalog.GetString("OVR") : Catalog.GetString("INS"));
			ClearStatus(overwriteStatus);
			overwriteStatus.Push(0, message);
		}
	}

	/* Public methods */

	public void ClearEditRelatedStatus () {
		ClearStatus(positionStatus);
		ClearStatus(overwriteStatus);
	}

	public void SetPosition (SubtitleTextType textType, int lineNumber, int columnNumber) {
		//To translators: Trans corresponds to Translation (used here to denote whether text or translation is being edited).
		string type = (textType == SubtitleTextType.Text ? Catalog.GetString("Text") : Catalog.GetString("Trans"));
		//To translators: Ln corresponds to Line
		string line = Catalog.GetString("Ln");
		//To translators: Col corresponds to Column
		string column = Catalog.GetString("Col");

		string message = type + " " + line  + " " + lineNumber + ", " + column + " " + columnNumber;
		ClearStatus(positionStatus);
		positionStatus.Push(0, message);
	}

	/* Private methods */

	private void ClearStatus (Statusbar statusBar) {
		statusBar.Pop(0);
	}

}

}