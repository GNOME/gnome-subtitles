/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2017 Pedro Castro
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
using System.Collections;
using System;

namespace GnomeSubtitles.Dialog {

public class Dialogs {
	private Hashtable dialogs = new Hashtable();

	public Dialogs () {
		Base.InitFinished += OnBaseInitFinished;
	}

	/* Public methods */

	public BaseDialog Get (Type dialogType, params object[] args) {
		BaseDialog dialog = dialogs[dialogType] as BaseDialog;
		if (dialog == null) {
			object newDialog = Activator.CreateInstance(dialogType, args);
			if (!(newDialog is BaseDialog)) {
				return null;
			}

			dialog = newDialog as BaseDialog;
			if (dialog.Scope != DialogScope.Singleton) {
				dialogs[dialogType] = dialog;
				dialog.Destroyed += OnDialogDestroyed;
			}
		}
		return dialog;
	}

	/* Event members */

	private void OnBaseInitFinished () {
    	Base.DocumentUnloaded += OnBaseDocumentUnloaded;
		Base.VideoUnloaded += OnBaseVideoUnloaded;
    }

	private void OnBaseDocumentUnloaded (Document document) {
		ICollection keyCollection = dialogs.Keys;
		Type[] keys = new Type[keyCollection.Count];
		keyCollection.CopyTo(keys, 0);
		for (int index = 0 ; index < keys.Length ; index++) {
			Type type = keys[index];
			BaseDialog dialog = dialogs[type] as BaseDialog;
			if ((dialog.Scope == DialogScope.Singleton) || (dialog.Scope == DialogScope.Document)) {
				dialog.Destroy();
			}
		}
	}

	private void OnBaseVideoUnloaded () {
		ICollection keyCollection = dialogs.Keys;
		Type[] keys = new Type[keyCollection.Count];
		keyCollection.CopyTo(keys, 0);
		for (int index = 0 ; index < keys.Length ; index++) {
			Type type = keys[index];
			BaseDialog dialog = dialogs[type] as BaseDialog;
			if (dialog.Scope == DialogScope.Video) {
				dialog.Destroy();
			}
		}
	}

	private void OnDialogDestroyed (object o, EventArgs args) {
		dialogs.Remove(o.GetType());
	}

}

}