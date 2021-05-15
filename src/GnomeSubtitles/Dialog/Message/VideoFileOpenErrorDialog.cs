/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2021 Pedro Castro
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

using Mono.Unix;
using Gtk;

namespace GnomeSubtitles.Dialog.Message {

	public class VideoFileOpenErrorDialog : ErrorDialog {

		private string primaryTextStart = Catalog.GetString("Could not play the file '{0}'.");
		private string actionLabel = Catalog.GetString("Open another file");

		public VideoFileOpenErrorDialog(string filename, string error) {
			string primaryText = string.Format(primaryTextStart, filename);
			string secondaryText = error;
			SetText(primaryText, secondaryText);
		}
		
		/* Overridden members */
	
		protected override void AddButtons() {
			Button actionButton = dialog.AddButton(actionLabel, ResponseType.Accept) as Button;
			actionButton.Image = new Image(Stock.Open, IconSize.Button);
			dialog.AddButton(Stock.Ok, ResponseType.Ok);
			
			dialog.DefaultResponse = ResponseType.Accept;
		}

	}
}
