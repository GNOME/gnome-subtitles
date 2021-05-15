/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2009-2021 Pedro Castro
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
using System;
using GnomeSubtitles.Core;

namespace GnomeSubtitles.Dialog {

public abstract class BaseDialog {

	private bool returnValue = false;

	//Hack because gtk# doesn't support this flag yet (as of 2019). Ref: gtk/gtkdialog.h (GtkDialogFlags.GTK_DIALOG_USE_HEADER_BAR)
	protected const DialogFlags DialogFlagsUseHeaderBar = (DialogFlags)4;

	public BaseDialog () {
	}

	/* Events */

	public event EventHandler Destroyed;


	/* Properties */

	public virtual DialogScope Scope {
		get { return DialogScope.Singleton; }
	}

	public virtual bool Visible {
		get { return Dialog.Visible; }
		set {
			if (value)
				Show();
			else
				Hide();
		}
	}

	protected Gtk.Dialog Dialog {
		get; private set;
	}

	/* Public Methods */

	public virtual void Show () {
		Dialog.Visible = true;
	}

	public virtual void Hide () {
		Dialog.Visible = false;
	}

	public virtual void Destroy () {
		Dialog.Destroy();
		EmitDestroyedEvent();
	}

	/**
	 * Used when we want to open a dialog and continue execution based on the dialog response.
	 * This way, the dialog doesn't need to understand who its caller is. It will just set its
	 * response value (in its object) and the caller will obtain it once the dialog returns.
	 */
	public virtual bool WaitForResponse () {
		Dialog.Run();
		return returnValue;
	}

	protected virtual bool ProcessResponse (Gtk.ResponseType response) {
		return false;
	}


	/* Protected members */

	protected void Init (Gtk.Dialog dialog) {
		Dialog = dialog;

		/* If the dialog doesn't specify a parent, we assume it's the main window.
		 * Usually the parent is specified when creating dialogs, but not in all types (e.g., AboutDialog).
		 */
		if (dialog.TransientFor == null) {
			dialog.TransientFor = Base.Ui.Window;
		}
		
		Dialog.Response += OnResponse;
		Dialog.DeleteEvent += OnDeleteEvent;
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

	private void OnDeleteEvent (object o, DeleteEventArgs args) {
		args.RetVal = true;
	}

	private void EmitDestroyedEvent () {
		if (Destroyed != null) {
			Destroyed(this, EventArgs.Empty);
		}
	}

}

}
