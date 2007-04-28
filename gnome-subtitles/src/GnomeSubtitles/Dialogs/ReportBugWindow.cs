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

using Glade;
using Gtk;
using SubLib;
using System;

namespace GnomeSubtitles {

public class BugReportWindow {
	private Clipboard clipboard = null;

	/* Constant strings */
	private const string gladeFilename = "ReportBugWindow.glade";

	/* Widgets */
	[WidgetAttribute] private Window window;
	[WidgetAttribute] private TextView bugTextView;
	
	public BugReportWindow (Exception exception, string bugInfo) {
		Application.Init();
		Glade.XML glade = new Glade.XML(null, gladeFilename, null, Global.Execution.TranslationDomain);
		glade.Autoconnect(this);

		bugTextView.Buffer.Text = bugInfo;
		clipboard = Clipboard.Get(Gdk.Selection.Clipboard);
		CopyErrorLog();
		
		window.Visible = true;
		
		Application.Run();
	}
		
	/* Private members */
	
	private void CopyErrorLog () {
		clipboard.Text = bugTextView.Buffer.Text;
	}

	
	/* Event handlers */
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnCopy (object o, EventArgs args) {
		CopyErrorLog();
	}
	
	private void OnOpenWebForm (object o, EventArgs args) {
		if (!Util.OpenBugReport())
			Console.Error.WriteLine("Could not open web form");
	}
	
	private void OnClose (object o, EventArgs args) {
		Application.Quit();
	}
	
	private void OnClose (object o, DeleteEventArgs args) {
		OnClose(o, args);
	}

}

}
