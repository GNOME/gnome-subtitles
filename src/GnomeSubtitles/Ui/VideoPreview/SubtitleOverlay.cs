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

using Gdk;
using GnomeSubtitles.Core;
using Gtk;
using SubLib.Core.Domain;
using System;
using System.Runtime.InteropServices;

namespace GnomeSubtitles.Ui.VideoPreview {

//TODO: draw text with Cairo, replacing the current Label in a Gtk Overlay. Example: https://github.com/reboot/obs-text-pango/blob/master/text-pango.c
public class SubtitleOverlay {
	private Label label = null;

	/* Current subtitle */
	private Subtitle subtitle = null;
	private bool toShowText = true;

	public SubtitleOverlay () {
		label = new Label(); //We create it here because Glade doesn't currently allow to set the overlay widget in GtkOverlays

		label.Halign = Align.Center;
		label.Valign = Align.End;
		label.Justify = Justification.Center;
		//label.LineWrap = true;
		//label.LineWrapMode = Pango.WrapMode.WordChar; //We cannot do this anymore. Doing this, a gray rectangle is painted above the subtitle after entering some characters, on top of the video.

		//Yellow text color
		RGBA labelColor = new RGBA();
		labelColor.Red = 1;
		labelColor.Green = 1;
		labelColor.Blue = 0;
		labelColor.Alpha = 1;
		label.OverrideColor(StateFlags.Normal, labelColor);
		label.Expand = false;

		//Black label background.
		RGBA labelBGColor = new RGBA();
		labelBGColor.Red = 0;
		labelBGColor.Green = 0;
		labelBGColor.Blue = 0;
		labelBGColor.Alpha = 1;
		label.OverrideBackgroundColor(StateFlags.Normal, labelBGColor);

		//We don't cast to Overlay because GTK# doesn't support it
		Bin bin = Base.GetWidget(WidgetNames.VideoImageOverlay) as Bin;
		gtk_overlay_add_overlay(bin.Handle, label.Handle);

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
		UpdateOverlayText();
	}

	private void UnloadSubtitle () {
		subtitle = null;
		UpdateOverlayText();
	}

	private void UpdateOverlayText() {
		if (subtitle == null) {
			SetText(String.Empty);
			return;
		}
		
		if (toShowText)
			SetText(subtitle.Text.Get());
		else
			SetText(subtitle.Translation.Get());
	}

	//Ref: https://developer.gnome.org/pango/stable/PangoMarkupFormat.html
	private void SetText (string text) {
		if (text == String.Empty) {
			label.Visible = false;
			label.Text = String.Empty;
			return;
		}

		string markup = "<span size=\"x-large\"";

		if (subtitle.Style.Bold)
			markup += " weight=\"bold\"";

		if (subtitle.Style.Italic)
			markup += " style=\"italic\"";

		if (subtitle.Style.Underline)
			markup += " underline=\"single\"";

		markup += ">" + GLib.Markup.EscapeText(text) + "</span>";
		
		label.Markup = markup;
		label.Visible = true;
	}


	/* Event members */

	private void OnBaseInitFinished () {
		//We do this because the label is initialized as visible (when added to the gtk overlay in the constructor)
		label.Visible = false;

		Base.Ui.Video.Tracker.SubtitlePulse += OnSubtitlePulse;
	}

	private void OnSubtitlePulse (int indexSubtitle) {
		if (indexSubtitle == -1)
			UnloadSubtitle();
		else
			LoadSubtitle(indexSubtitle);
	}

	/* External Imports */

	[DllImport("libgtk")]
	static extern void gtk_overlay_add_overlay (IntPtr overlay, IntPtr widget);

}

}
