/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2008 Pedro Castro
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

using Gtk;
using SubLib;
using System;

namespace GnomeSubtitles.Ui.SubtitleEdit {

public class SubtitleEditSpinButtons {
	private TimingMode timingMode = TimingMode.Frames; //Need to store to prevent from connecting more than 1 handler to the spin buttons. Default is Frames because it's going to be set to Times in the constructor.

	/* Widgets */
	private SpinButton startSpinButton = null;
	private SpinButton endSpinButton = null;
	private SpinButton durationSpinButton = null;


	public SubtitleEditSpinButtons () {
		/* Assign */
		startSpinButton = Global.GetWidget(WidgetNames.StartSpinButton) as SpinButton;
		endSpinButton = Global.GetWidget(WidgetNames.EndSpinButton) as SpinButton;
		durationSpinButton = Global.GetWidget(WidgetNames.DurationSpinButton) as SpinButton;
		
		/* Initialize */
		startSpinButton.WidthRequest = Util.SpinButtonTimeWidth(startSpinButton); //Only need to set one of the spin buttons' width
    	startSpinButton.Alignment = 0.5f;
    	endSpinButton.Alignment = 0.5f;
    	durationSpinButton.Alignment = 0.5f; 
    	
    	/* Set timing mode to Times */
    	SetTimingMode(TimingMode.Times); //Initial timing mode is Times
	}
	
	/* Public methods */
	
	public void UpdateFromTimingMode (TimingMode mode, Subtitle subtitle) {
		if (mode == timingMode)
			return;

		SetTimingMode(mode);
		LoadTimings(subtitle);
	}

	public void LoadTimings (Subtitle subtitle) {
    	if (subtitle == null)
    		return;

		LoadStartTiming(subtitle);
		LoadEndTiming(subtitle);
		LoadDurationTiming(subtitle);
    }
    
    public void ClearFields () {
        DisconnectSpinButtonsChangedSignals();
		startSpinButton.Text = String.Empty;
		endSpinButton.Text = String.Empty;
		durationSpinButton.Text = String.Empty;
    	ConnectSpinButtonsChangedSignals();
    }
    
    public void GetWidgets (out SpinButton startSpinButton, out SpinButton endSpinButton, out SpinButton durationSpinButton) {
    	startSpinButton = this.startSpinButton;
    	endSpinButton = this.endSpinButton;
    	durationSpinButton = this.durationSpinButton;
    }


	/* Private methods */
	
	private void SetTimingMode (TimingMode mode) {
   		if (mode == timingMode) //Only set if it's not already set
   			return;
   			
   		timingMode = mode;   	
   		if (mode == TimingMode.Frames)
   			SetFramesMode();
   		else
   			SetTimesMode();
   	}
	
	private void SetFramesMode () {
		SetFramesMode(startSpinButton);
	    SetFramesMode(endSpinButton);
	    SetFramesMode(durationSpinButton);
	}
	
	private void SetTimesMode () {
	    SetTimesMode(startSpinButton);
	    SetTimesMode(endSpinButton);
	    SetTimesMode(durationSpinButton);
	}
    
    private void SetTimesMode (SpinButton spinButton) {
    	spinButton.Input += OnTimeInput;
		spinButton.Output += OnTimeOutput;
		spinButton.Adjustment.StepIncrement = 100;
		spinButton.Adjustment.Upper = 86399999;
	}
	
	private void SetFramesMode (SpinButton spinButton) {
		spinButton.Input -= OnTimeInput;
    	spinButton.Output -= OnTimeOutput;
    	spinButton.Adjustment.StepIncrement = 1;
    	spinButton.Adjustment.Upper = 3000000;
	}
	
	private void LoadStartTiming (Subtitle subtitle) {
    	startSpinButton.ValueChanged -= OnStartValueChanged;
    		
    	if (timingMode == TimingMode.Frames)
    		startSpinButton.Value = subtitle.Frames.Start;
    	else
    		startSpinButton.Value = subtitle.Times.Start.TotalMilliseconds;

   		startSpinButton.ValueChanged += OnStartValueChanged;
   	}
   	
	private void LoadEndTiming (Subtitle subtitle) {
   		endSpinButton.ValueChanged -= OnEndValueChanged;
    		
    	if (timingMode == TimingMode.Frames)
    		endSpinButton.Value = subtitle.Frames.End;
    	else
    		endSpinButton.Value = subtitle.Times.End.TotalMilliseconds;

   		endSpinButton.ValueChanged += OnEndValueChanged;
	}
	
	private void LoadDurationTiming (Subtitle subtitle) {
    	durationSpinButton.ValueChanged -= OnDurationValueChanged;

    	if (timingMode == TimingMode.Frames)
    		durationSpinButton.Value = subtitle.Frames.Duration;
    	else
    		durationSpinButton.Value = subtitle.Times.Duration.TotalMilliseconds;

    	durationSpinButton.ValueChanged += OnDurationValueChanged;
    }
    
    /* Event methods */
    
	private void ConnectSpinButtonsChangedSignals () {
    	startSpinButton.ValueChanged += OnStartValueChanged;
    	endSpinButton.ValueChanged += OnEndValueChanged;
    	durationSpinButton.ValueChanged += OnDurationValueChanged;
    }
    
    private void DisconnectSpinButtonsChangedSignals () {
    	startSpinButton.ValueChanged -= OnStartValueChanged;
   		endSpinButton.ValueChanged -= OnEndValueChanged;
   		durationSpinButton.ValueChanged -= OnDurationValueChanged;
   	}
   	
	private void OnStartValueChanged (object o, EventArgs args) {
		if (Global.TimingModeIsFrames)
			Global.CommandManager.Execute(new ChangeStartCommand((int)startSpinButton.Value));
		else
			Global.CommandManager.Execute(new ChangeStartCommand(TimeSpan.FromMilliseconds(startSpinButton.Value)));
	}
	
	private void OnEndValueChanged (object o, EventArgs args) {
		if (Global.TimingModeIsFrames)
			Global.CommandManager.Execute(new ChangeEndCommand((int)endSpinButton.Value));
		else
			Global.CommandManager.Execute(new ChangeEndCommand(TimeSpan.FromMilliseconds(endSpinButton.Value)));
	}
	
	private void OnDurationValueChanged (object o, EventArgs args) {
		if (Global.TimingModeIsFrames)
			Global.CommandManager.Execute(new ChangeDurationCommand((int)durationSpinButton.Value));
		else
			Global.CommandManager.Execute(new ChangeDurationCommand(TimeSpan.FromMilliseconds(durationSpinButton.Value)));
	}
	
	private void OnTimeInput (object o, InputArgs args) {
		SpinButton spinButton = o as SpinButton;
		try {
			args.NewValue = Util.TimeTextToMilliseconds(spinButton.Text);
		}
		catch (Exception) {
			args.NewValue = spinButton.Value;
		}
		args.RetVal = 1;
	}
	
	private void OnTimeOutput (object o, OutputArgs args) {
		SpinButton spinButton = o as SpinButton;
		spinButton.Numeric = false;
		spinButton.Text = Util.MillisecondsToTimeText((int)spinButton.Value);
		spinButton.Numeric = true;
		args.RetVal = 1;
	}

}

}