/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008-2009 Pedro Castro
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

using Glade;
using GnomeSubtitles.Core;
using Gtk;
using Mono.Unix;
using SubLib.Core;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Dialog {

public class VideoSeekToDialog : GladeDialog {
	private TimingMode timingMode = TimingMode.Frames;

	/* Constant strings */
	private const string gladeFilename = "VideoSeekToDialog.glade";

	/* Widgets */
	[WidgetAttribute] private SpinButton spinButton = null;
	[WidgetAttribute] private Label positionLabel = null;

	public VideoSeekToDialog () : base(gladeFilename){
		this.timingMode = Base.TimingMode;
	
		InitSpinButton();
	}

	/* Public properties */

	public override DialogScope Scope {
		get { return DialogScope.Video; }
	}
	
	/* Private methods */

	private void InitSpinButton () {
		spinButton.WidthRequest = Core.Util.SpinButtonTimeWidth(spinButton);
		spinButton.Alignment = 0.5f;
		
		if (timingMode == TimingMode.Times) {
			Core.Util.SetSpinButtonTimingMode(spinButton, Base.TimingMode);
			Core.Util.SetSpinButtonAdjustment(spinButton, Base.Ui.Video.Position.Duration, false);
			SetSpinButtonValue(Base.Ui.Video.Position.CurrentTime.TotalMilliseconds);
		}
		else {
			Core.Util.SetSpinButtonTimingMode(spinButton, Base.TimingMode);
			Core.Util.SetSpinButtonAdjustment(spinButton, Base.Ui.Video.Position.DurationInFrames, false);
			SetSpinButtonValue(Base.Ui.Video.Position.CurrentFrames);
			
			positionLabel.TextWithMnemonic = Catalog.GetString("Seek to _frame:");
		}
		
		spinButton.SelectRegion(0, spinButton.Text.Length);
	}
	
	private void SetSpinButtonValue (double newValue) {
		spinButton.Value = newValue;
	}

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnClear (object o, EventArgs args) {
		SetSpinButtonValue(0);
	}

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			if (timingMode == TimingMode.Times) {
				TimeSpan position = TimeSpan.FromMilliseconds(spinButton.Value); //TODO move to Util
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
