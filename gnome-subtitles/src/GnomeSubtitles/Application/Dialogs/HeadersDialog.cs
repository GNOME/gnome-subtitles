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
using Glade;
using Gtk;
using SubLib;

namespace GnomeSubtitles {


public class HeadersDialog : GladeDialog {
	private SubtitleHeaders subtitleHeaders = null;

	public HeadersDialog (GUI gui) : base(gui, WidgetNames.HeadersDialog) {
		subtitleHeaders = gui.Core.Subtitles.Properties.Headers;
		LoadHeaders();
	}
	
	/* Dialog fields */
	
	[WidgetAttribute]
	private Entry entryMPSubTitle;




	/* Private members */
	
	private void LoadHeaders () {
		LoadMPSubHeaders();	
	}
	
	private void LoadMPSubHeaders () {
		SubtitleHeadersMPSub headers = subtitleHeaders.MPSub;
	
		entryMPSubTitle.Text = headers.Title;	
	}
	
	private void StoreHeaders () {
		StoreMPSubHeaders();	
	}
	
	private void StoreMPSubHeaders () {
		SubtitleHeadersMPSub headers = subtitleHeaders.MPSub;
	
		headers.Title = entryMPSubTitle.Text;	
	}
	
	/* Event handlers */

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok) {
			StoreHeaders();
		}
		CloseDialog();
	}

}

}