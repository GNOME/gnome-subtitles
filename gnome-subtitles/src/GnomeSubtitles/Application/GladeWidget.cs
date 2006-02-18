/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
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

namespace GnomeSubtitles {

public class GladeMainWidget {
	private Glade.XML glade = null;

	protected GladeMainWidget () {}

	protected void Init (string gladeMasterFileName, string widgetName) {
		glade = new Glade.XML (gladeMasterFileName, widgetName);
	}
	
	protected GladeMainWidget (Glade.XML glade) {
		this.glade = glade;
	}

	
	protected Glade.XML Glade {
		get { return glade; }
	}
	
	
	protected Widget GetWidget(string widgetName) {
		return glade.GetWidget(widgetName);
	}

}


public class GladeWidget {
	private Glade.XML glade = null;
	private GUI gui = null;
	
	protected GladeWidget () {}
	
	protected GladeWidget (GUI gui, string widgetName) {
		this.gui = gui;
		Init(gui.Core.ExecutionInfo.GladeMasterFileName, widgetName, this);
	}

	protected GladeWidget (GUI gui, Glade.XML glade) {
		this.gui = gui;
		this.glade = glade;
	}
	
	
	protected Glade.XML Glade {
		get { return glade; }
	}
	
	protected GUI GUI {
		get { return gui; }
	}
	
	
	protected void Init (string gladeMasterFileName, string widgetName, object handler) {
		glade = new Glade.XML (gladeMasterFileName, widgetName);
		glade.Autoconnect(handler);
	}
	
	protected Widget GetWidget(string widgetName) {
		return glade.GetWidget(widgetName);
	}
	

}

}