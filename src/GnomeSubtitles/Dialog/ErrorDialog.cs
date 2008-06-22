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

using GnomeSubtitles.Dialog;
using Gtk;
using Mono.Unix;
using System;

namespace GnomeSubtitles.Dialog {

public abstract class ErrorDialog : GladeDialog {
	protected new MessageDialog dialog = null;
	
	/* Strings */
	private const string gladeFilename = "ErrorDialog.glade";

	/// <summary>Creates a new instance of the <see cref="ErrorDialog" /> class.</summary>
	/// <remarks><see cref="SetText" /> can be used to set the dialog text, and <see cref="AddButtons" /> overriden to add buttons.</remarks>
	public ErrorDialog () : base(gladeFilename) {
		dialog = base.dialog as MessageDialog;
		
		AddButtons();
	}
	
	public ErrorDialog (string primary, string secondary) : this() {
		SetText(primary, secondary);
	}
	
	/* Protected methods */
	
	protected void SetText (string primary, string secondary) {
		string text = "<span weight=\"bold\" size=\"larger\">" + primary + "</span>\n\n" + secondary;
		dialog.Markup = text; //Markup has to be used as the Text property is only available from GTK# 2.10	
	}
	
	protected string GetGeneralExceptionErrorMessage (Exception exception) {
		return Catalog.GetString("An unknown error has occured. Please report a bug and include this error name:") + " \"" + exception.GetType() + "\".";
	}
	
	/* Abstract methods */
	
	protected abstract void AddButtons ();

	/* Event members */
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		ResponseType response = args.ResponseId;
		if (response == ResponseType.Accept) {
			actionDone = true;
		}
		Close();
	}

}

}
