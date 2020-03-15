/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2019 Pedro Castro
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
using System;

namespace GnomeSubtitles.Dialog.Message {

public abstract class ErrorDialog : MessageDialog {

	/// <summary>Creates a new instance of the <see cref="ErrorDialog" /> class.</summary>
	/// <remarks><see cref="SetText" /> can be used to set the dialog text, and <see cref="AddButtons" /> overridden to add buttons.</remarks>
	public ErrorDialog () : base(MessageType.Error) {
	}

	public ErrorDialog (string primary, string secondary) : base(MessageType.Error, primary, secondary) {
	}


	#region Protected methods

	protected string GetGeneralExceptionErrorMessage (Exception exception) {
		return Catalog.GetString("An unknown error has occurred. Please report a bug and include this error name:") + " \"" + exception.GetType() + "\".";
	}

	#endregion

}

}
