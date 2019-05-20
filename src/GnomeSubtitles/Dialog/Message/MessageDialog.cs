/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008-2019 Pedro Castro
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
using Gtk;

namespace GnomeSubtitles.Dialog.Message {

public abstract class MessageDialog : BaseDialog {
		protected Gtk.MessageDialog dialog = null;

		public MessageDialog (MessageType messageType) : base() {
			Init(messageType, null, null);
		}

		public MessageDialog (MessageType messageType, string primaryText, params object[]primaryTextArgs) : base() {
			Init(messageType, primaryText, null, primaryTextArgs) ;
		}

		public MessageDialog (MessageType messageType, string primaryText, string secondaryText) : base() {
			Init(messageType, primaryText, secondaryText);
		}

		public MessageDialog (MessageType messageType, string primaryText, string secondaryText, params object[]primaryTextArgs) : base() {
			Init(messageType, primaryText, secondaryText, primaryTextArgs);
		}


		/* Protected methods */

		protected void SetText (string primaryText, string secondaryText, params object[] primaryTextArgs) {
			SetPrimaryText(primaryText, primaryTextArgs);
			SetSecondaryText(secondaryText);
		}

		protected void SetText (string primaryText, string secondaryText) {
			SetPrimaryText(primaryText);
			SetSecondaryText(secondaryText);
		}

		protected void SetPrimaryText (string text, params object[] textArgs) {
			if (text != null) {
				string primaryText = Core.Util.GetFormattedText(text, textArgs);
				dialog.Markup = GetMarkupPrimaryText(primaryText);
			}
		}

		protected void SetPrimaryText (string text) {
			SetPrimaryText(text, null);
		}

		protected void SetSecondaryText (string text, params object[] textArgs) {
			if (text != null)
				dialog.SecondaryText = Core.Util.GetFormattedText(text, textArgs);
		}

		protected void SetSecondaryText (string text) {
			SetSecondaryText(text, null);
		}


		/* Private members */

		private void Init (MessageType messageType, string primaryText, string secondaryText, params object[]primaryTextArgs) {
			string formattedPrimaryText = GetMarkupPrimaryText(primaryText);

			dialog = new Gtk.MessageDialog(Base.Ui.Window, DialogFlags.Modal, messageType, ButtonsType.None, formattedPrimaryText, primaryTextArgs);
			base.Init(dialog);

			SetSecondaryText(secondaryText);
			AddButtons();
		}

		private string GetMarkupPrimaryText (string primaryText) {
			return "<span weight=\"bold\" size=\"larger\">" + primaryText + "</span>";
		}


		/* Abstract methods */

		protected abstract void AddButtons ();


		/* Event members */

		protected override bool ProcessResponse (ResponseType response) {
			if (response == ResponseType.Accept) {
				SetReturnValue(true);
			}
			return false;
		}

	}


}
