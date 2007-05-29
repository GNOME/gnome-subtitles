/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007 Pedro Castro
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

namespace GnomeSubtitles {

public class Status {
	private Statusbar messageStatus = null;
	private Statusbar positionStatus = null;
	private Statusbar overwriteStatus = null;

	public Status () {
		messageStatus = Global.GetWidget(WidgetNames.MessageStatusbar) as Gtk.Statusbar;
		positionStatus = Global.GetWidget(WidgetNames.PositionStatusbar) as Gtk.Statusbar;
		overwriteStatus = Global.GetWidget(WidgetNames.OverwriteStatusbar) as Gtk.Statusbar;
	}

	/* Public properties */

	public bool Overwrite {
		set {
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
	
	public void SetPosition (int line, int column) {
		string message = Catalog.GetString("Ln") + " " + line + ", " + Catalog.GetString("Col") + " " + column;
		ClearStatus(positionStatus);
		positionStatus.Push(0, message);
	}
	
	/* Private methods */
	
	private void ClearStatus (Statusbar statusBar) {
		statusBar.Pop(0);
	}

}

}