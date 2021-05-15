/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008-2021 Pedro Castro
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
using GnomeSubtitles.Ui;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Dialog {

public class VideoSeekToDialog : BaseDialog {
	private TimingMode timingMode = TimingMode.Frames;

	/* Widgets */
	private SpinButton spinButton = null;

	public VideoSeekToDialog () : base(){
		Init(BuildDialog());
	}

	/* Overridden members */

	public override DialogScope Scope {
		get { return DialogScope.Video; }
	}

	public override void Show () {
		SetSpinButtonFromTimingMode();
		
		spinButton.SelectRegion(0, spinButton.Text.Length);
		
		base.Show ();
	}


	/* Private methods */

	private Gtk.Dialog BuildDialog () {
		Gtk.Dialog dialog = new Gtk.Dialog(Catalog.GetString("Seek Video To"), Base.Ui.Window, DialogFlags.Modal | DialogFlagsUseHeaderBar,
			Util.GetStockLabel("gtk-cancel"), ResponseType.Cancel, Catalog.GetString("_Seek"), ResponseType.Ok);

		dialog.DefaultResponse = ResponseType.Ok;
		dialog.Resizable = false;
		
		Box hbox = new Box(Orientation.Horizontal, WidgetStyles.BoxSpacingMedium);
		hbox.BorderWidth = WidgetStyles.BorderWidthMedium;
		hbox.Spacing = WidgetStyles.BoxSpacingMedium;
		hbox.Add(new Label(Catalog.GetString("Seek _video to:")));
		
		spinButton = new SpinButton(new Adjustment(0, 0, 0, 1, 10, 0), 0, 0);
		spinButton.WidthChars = Core.Util.SpinButtonTimeWidthChars;
		spinButton.Alignment = 0.5f;
		spinButton.ActivatesDefault = true;
		hbox.Add(spinButton);
		
		dialog.ContentArea.Add(hbox);
		dialog.ShowAll();
		
		return dialog;
	}

	private void SetSpinButtonFromTimingMode () {
		if (this.timingMode == Base.TimingMode) {
			return;
		}

		this.timingMode = Base.TimingMode;
		Core.Util.SetSpinButtonTimingMode(spinButton, timingMode);

		if (timingMode == TimingMode.Times) {
			Core.Util.SetSpinButtonAdjustment(spinButton, Base.Ui.Video.Position.Duration, false);
			SetSpinButtonValue(Base.Ui.Video.Position.CurrentTime.TotalMilliseconds);
		} else {
			Core.Util.SetSpinButtonAdjustment(spinButton, Base.Ui.Video.Position.DurationInFrames, false);
			SetSpinButtonValue(Base.Ui.Video.Position.CurrentFrames);
		}
	}

	private void SetSpinButtonValue (double newValue) {
		spinButton.Value = newValue;
	}

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			if (timingMode == TimingMode.Times) {
				TimeSpan position = TimeSpan.FromMilliseconds(spinButton.Value);
				Base.Ui.Video.Seek(position);
			}
			else {
				Base.Ui.Video.Seek(Convert.ToInt32(spinButton.Value));
			}
		}
		return false;
	}

}

}
