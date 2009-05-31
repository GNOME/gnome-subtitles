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

using Gtk;

namespace GnomeSubtitles.Dialog {

public abstract class BaseDialog {
	
	private Gtk.Dialog dialog = null;
	private bool returnValue = false;

	public BaseDialog () {
	}

	/* Properties */

	public virtual DialogScope Scope {
		get { return DialogScope.Singleton; }
	}

	public virtual bool Visible {
		get { return dialog.Visible; }
		set { 
			if (value)
				Show();
			else
				Hide();
		}
	}

	/* Public Methods */

	public virtual void Show () {
		dialog.Visible = true;
	}

	public virtual void Hide () {
		dialog.Visible = false;
	}

	public virtual void Destroy () {
		dialog.Destroy();
	}

	

	//TODO check if this is needed
	public virtual bool WaitForResponse () {
		dialog.Run();
		return returnValue;
	}

	protected virtual bool ProcessResponse (Gtk.ResponseType response) {
		return false;
	}


	/* Protected members */

	protected void Init (Gtk.Dialog dialog) {
		this.dialog = dialog;
		Util.SetBaseWindowFromUi(dialog);
		dialog.Response += OnResponse;
	}

	protected Gtk.Dialog GetDialog () {
		return dialog;
	}

	protected void SetReturnValue (bool returnValue) {
		this.returnValue = returnValue;
	}

	
	/* Event members */

	protected void OnResponse (object o, ResponseArgs args) {
		bool keepVisible = ProcessResponse(args.ResponseId);
		if (keepVisible && (args.ResponseId != ResponseType.DeleteEvent))
			return;

		if (this.Scope == DialogScope.Singleton)
			Destroy();
		else {
			Hide();
			args.RetVal = true;
		}
	}


}

}
