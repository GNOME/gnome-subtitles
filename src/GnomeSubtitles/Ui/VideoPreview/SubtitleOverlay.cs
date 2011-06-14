/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2011 Pedro Castro
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
using SubLib.Core.Domain;
using SubLib.Core.Search;
using System;

namespace GnomeSubtitles.Ui.VideoPreview {

public class SubtitleOverlay {
	private Label label = null;	
	
	/* Current subtitle */
	private Subtitle subtitle = null;	
	private bool toShowText = true;
	
	public SubtitleOverlay () {
		EventBox box = Base.GetWidget(WidgetNames.VideoSubtitleLabelEventBox) as EventBox;
		box.ModifyBg(StateType.Normal, box.Style.Black);

		label = Base.GetWidget(WidgetNames.VideoSubtitleLabel) as Label;
		label.ModifyFg(StateType.Normal, new Gdk.Color(255, 255, 0));

		Base.InitFinished += OnBaseInitFinished;
	}

	
	/* Public properties */
	
	public bool ToShowText {
		get { return toShowText; }
		set { this.toShowText = value; }
	}


	/* Public methods */

	public void Close () {
		UnloadSubtitle();
	}
	
	
	/* Private methods */
	
	private void LoadSubtitle (int number) {
		subtitle = Base.Document.Subtitles[number];		
		SetText();
		label.Visible = true;
	}
	
	private void UnloadSubtitle () {
		subtitle = null;		
		ClearText();
		label.Visible = false;
	}
	
	private void SetText () {
		if (toShowText)
			SetText(subtitle.Text.Get());
		else
			SetText(subtitle.Translation.Get());
	}
	
	private void SetText (string text) {
		string markup = "<span size=\"x-large\""; 
	
		if (subtitle.Style.Bold)
			markup += " weight=\"bold\"";

		if (subtitle.Style.Italic)
			markup += " style=\"italic\"";

		if (subtitle.Style.Underline)
			markup += " underline=\"single\"";
		
		markup += ">" + text + "</span>";
		label.Markup = markup;
	}
	
	private void ClearText () {
		label.Text = String.Empty;
	}


	/* Event members */		

	private void OnBaseInitFinished () {
		Base.Ui.Video.Tracker.CurrentSubtitleChanged += OnCurrentSubtitleChanged;		
	}

	private void OnCurrentSubtitleChanged (int indexSubtitle) {
		if (indexSubtitle == -1)
			UnloadSubtitle();
		else
			LoadSubtitle(indexSubtitle);		
	}

}

}
