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

using System;
using Gtk;
using SubLib;

namespace GnomeSubtitles {

public class BugReportWindow : GladeWidget {
	TextView textView = null;
	Clipboard clipboard = null;

	public BugReportWindow (Exception exception) {
		System.Console.Error.WriteLine(exception);
		
		Application.Init();		
		Init(ExecutionInfo.GladeMasterFileName, WidgetNames.BugReportWindow, this);
		
		textView = GetWidget(WidgetNames.BugReportWindowTextView) as TextView;
		textView.Buffer.Text = exception.ToString();
		
		clipboard = Clipboard.Get(Gdk.Selection.Clipboard);
		CopyErrorLog();
		
		Application.Run();
	}
		
	/* Private members */
	
	private void CopyErrorLog () {
		clipboard.Text = textView.Buffer.Text;
	}
	
	/* Event handlers */
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnCopy (object o, EventArgs args) {
		CopyErrorLog();
	}
	
	private void OnOpenWebForm (object o, EventArgs args) {
		Gnome.Url.Show("http://sourceforge.net/tracker/?func=add&group_id=129996&atid=716496");
	}
	
	private void OnClose (object o, EventArgs args) {
		Application.Quit();
	}
	
	private void OnClose (object o, DeleteEventArgs args) {
		Application.Quit();
	}



}

}