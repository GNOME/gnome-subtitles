/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2008 Pedro Castro
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
using System;

namespace GnomeSubtitles.Ui.VideoPreview {

//TODO change name to SubtitleOverlay
public class VideoSubtitle {
	private Label label = null;
	
	/* Current subtitle */
	private Subtitle subtitle = null;
	private TimeSpan subtitleStart = TimeSpan.Zero;
	private TimeSpan subtitleEnd = TimeSpan.Zero;
	private bool toShowText = true;
	
	public VideoSubtitle (VideoPosition position) {
		EventBox box = Base.GetWidget(WidgetNames.VideoSubtitleLabelEventBox) as EventBox;
		box.ModifyBg(StateType.Normal, box.Style.Black);

		label = Base.GetWidget(WidgetNames.VideoSubtitleLabel) as Label;
		label.ModifyFg(StateType.Normal, new Gdk.Color(255, 255, 0));

		position.Changed += OnVideoPositionChanged;
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

	/* Event members */
	
	private void OnVideoPositionChanged (TimeSpan newPosition) {
		if (!(Base.IsDocumentLoaded))
			return;
	
		if (!(IsTimeInCurrentSubtitle(newPosition))) {
			int foundSubtitle = Base.Document.Subtitles.FindWithTime((float)newPosition.TotalSeconds); //TODO write method in SubLib that accepts TimeSpans
			if (foundSubtitle == -1)
				UnloadSubtitle();
			else
				LoadSubtitle(foundSubtitle);
		}
	}
	
	/* Private properties */
	
	private bool IsSubtitleLoaded {
		get { return subtitle != null; }
	}
	
	/* Private methods */
	
	private bool IsTimeInCurrentSubtitle (TimeSpan time) {
		return IsSubtitleLoaded && (time >= subtitleStart) && (time <= subtitleEnd);	
	}
	
	private void LoadSubtitle (int number) {
		subtitle = Base.Document.Subtitles[number];
		subtitleStart = subtitle.Times.Start;
		subtitleEnd = subtitle.Times.End;
		SetText();
		label.Visible = true;
	}
	
	private void UnloadSubtitle () {
		subtitle = null;
		subtitleStart = TimeSpan.Zero;
		subtitleEnd = TimeSpan.Zero;
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
	
}

}
