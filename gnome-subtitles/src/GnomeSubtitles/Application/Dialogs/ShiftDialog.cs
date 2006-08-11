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
using Gtk;
using SubLib;

namespace GnomeSubtitles {


public class ShiftDialog : GladeDialog {
	private SpinButton spinButton = null;
	private TimingMode timingMode;

	public ShiftDialog (GUI gui) : base(gui, WidgetNames.ShiftDialog){
		spinButton = GetWidget(WidgetNames.ShiftDialogSpinButton) as SpinButton;
		spinButton.WidthRequest = gui.SubtitleEdit.SpinButtonWidth();
		spinButton.Alignment = 0.5f;
		
		timingMode = gui.Core.Subtitles.Properties.TimingMode;
		UpdateSpinButton(timingMode);
	}

	private void UpdateSpinButton (TimingMode timingMode) {
		if (timingMode == TimingMode.Frames) {
			spinButton.Adjustment.StepIncrement = 1;
    		spinButton.Adjustment.Upper = 3000000;
    		spinButton.Adjustment.Lower = -3000000;
		}
		else {
			spinButton.Input += OnTimeInput;
			spinButton.Output += OnTimeOutput;
			spinButton.Value = 0;
			spinButton.Adjustment.StepIncrement = 100;
			spinButton.Adjustment.Upper = 86399999;
			spinButton.Adjustment.Lower = -86399999;
		}
	}
	
	private void OnTimeInput (object o, InputArgs args) {
		try {
			args.NewValue = Utility.TimeTextToMilliseconds(spinButton.Text);
		}
		catch (Exception) {
			args.NewValue = spinButton.Value;
		}
		args.RetVal = 1;
	}
	
	private void OnTimeOutput (object o, OutputArgs args) {
		spinButton.Numeric = false;
		spinButton.Text = Utility.MillisecondsToTimeText((int)spinButton.Value);
		spinButton.Numeric = true;
		args.RetVal = 1;
	}

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok) {
			RadioButton allSubtitles = GetWidget(WidgetNames.ShiftDialogAllSubtitlesRadioButton) as RadioButton;
			bool applyToAll = allSubtitles.Active; 
			
			if (timingMode == TimingMode.Times) {
				TimeSpan time = TimeSpan.Parse(spinButton.Text);
				GUI.Core.CommandManager.Execute(new ShiftTimingsCommand(GUI, time, applyToAll));
			}
			else {
				int frames = (int)spinButton.Value;
				GUI.Core.CommandManager.Execute(new ShiftTimingsCommand(GUI, frames, applyToAll));
			}
			CloseDialog(); 
		}
		else if (args.ResponseId == ResponseType.Cancel) {
			CloseDialog();
		}
	}

}

}