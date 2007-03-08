/*
 * This file is part of Gnome Subtitles.
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

/* TODO this is deprecated */
public class ShowMessageDialog : GladeDialog {
	
	/* Constant strings */
	private const string gladeFilename = "ShowMessageDialog.glade";
	
	public ShowMessageDialog (MessageType messageType, string primaryText, string secondaryText) : base(gladeFilename) {
		MessageDialog messageDialog = dialog as MessageDialog;
		messageDialog.MessageType = messageType;
		messageDialog.Text = primaryText;
		messageDialog.SecondaryText = secondaryText;	
	}

	/* Event handlers */

	#pragma warning disable 169		//Disables warning about handlers not being used

	private void OnResponse (object o, ResponseArgs args) {
		CloseDialog();
	}
	
	#pragma warning restore 169		//Restore warning about handlers not being used
}

/* TODO this is deprecated */
public class PlayerNotFoundErrorDialog : ShowMessageDialog {

	/* Constant strings */
	private const string primaryText = "Could not open the video player.";
	private const string secondaryText = "Please check that MPlayer is installed.";

	public PlayerNotFoundErrorDialog () : base(MessageType.Error, primaryText, secondaryText) {
	}
}

}
