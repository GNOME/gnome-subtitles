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
	[WidgetAttribute]
	private TextView bugTextView;
	
	public BugReportWindow (Exception exception) {
		Console.Error.WriteLine(exception);
	
		Application.Init();
		Glade.XML glade = new Glade.XML(gladeFilename, null);
		glade.Autoconnect(this);

		AddBugText(exception);
		clipboard = Clipboard.Get(Gdk.Selection.Clipboard);
		CopyErrorLog();
		
		Application.Run();
	}
		
	/* Private members */
	
	private void CopyErrorLog () {
		clipboard.Text = bugTextView.Buffer.Text;
	}

	private void AddBugText (Exception exception) {
		string text = String.Empty;
		text += "Gnome Subtitles version: " + ExecutionInfo.Version + "\n";
		text += "SubLib version: " + ExecutionInfo.SubLibVersion + "\n";
		text += "GnomeSharp version: " + ExecutionInfo.GnomeSharpVersion + "\n";
		text += "GtkSharp version: " + ExecutionInfo.GtkSharpVersion + "\n";
		text += "GladeSharp version: " + ExecutionInfo.GladeSharpVersion + "\n\n";
		text += "Stack trace:" + "\n";
		text += exception.ToString();
		
		bugTextView.Buffer.Text = text;
	}
	
	/* Event handlers */
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnCopy (object o, EventArgs args) {
		CopyErrorLog();
	}
	
	private void OnOpenWebForm (object o, EventArgs args) {
		if (!Util.OpenBugReport())
			Console.WriteLine("Could not open web form");
	}
	
	private void OnClose (object o, EventArgs args) {
		Application.Quit();
	}
	
	private void OnClose (object o, DeleteEventArgs args) {
		OnClose(o, args);
	}

}

}