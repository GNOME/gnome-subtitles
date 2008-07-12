/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008 Pedro Castro
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
using System;

namespace GnomeSubtitles.Dialog {
	
	public abstract class MessageDialog : BaseDialog {
		protected new Gtk.MessageDialog dialog = null;

		public MessageDialog (MessageType messageType) : this(messageType, null, null, null) {
		}

		public MessageDialog (MessageType messageType, string primaryText, params object[]primaryTextArgs) : this(messageType, primaryText, null, primaryTextArgs) {
		}

		public MessageDialog (MessageType messageType, string primaryText, string secondaryText) : this(messageType, primaryText, secondaryText, null) {
		}
		
		public MessageDialog (MessageType messageType, string primaryText, string secondaryText, params object[]primaryTextArgs) : base() {
			dialog = new Gtk.MessageDialog(Base.Ui.Window, DialogFlags.Modal, messageType, ButtonsType.None, primaryText, primaryTextArgs);
			base.dialog = dialog;
			
			dialog.Response += OnResponse;

			SetSecondaryText(secondaryText);

			Util.SetBaseWindowToUi(dialog);
			AddButtons();
		}
		
		
		#region Protected methods
	
		protected void SetText (string primaryText, string secondaryText, params object[] primaryTextArgs) {
			SetPrimaryText(primaryText, primaryTextArgs);
			SetSecondaryText(secondaryText);
		}
	
		protected void SetText (string primaryText, string secondaryText) {
			SetPrimaryText(primaryText);
			SetSecondaryText(secondaryText);
		}
	
		protected void SetPrimaryText (string text, params object[] textArgs) {
			if (text != null)
				dialog.Text = Core.Util.GetFormattedText(text, textArgs);
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

		#endregion
		
		
		#region Abstract methods
		
		protected abstract void AddButtons ();
		
		#endregion

		
		#region Event members
	
		protected virtual void OnResponse (object o, ResponseArgs args) {
			ResponseType response = args.ResponseId;
			if (response == ResponseType.Accept) {
				returnValue = true;
			}
			Close();
		}
		
		#endregion
		
	}

	
}
