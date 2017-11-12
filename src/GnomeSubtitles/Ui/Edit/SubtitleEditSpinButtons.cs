/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2017 Pedro Castro
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
using GnomeSubtitles.Core.Command;
using Gtk;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Ui.Edit {

public class SubtitleEditSpinButtons {
	private TimingMode timingMode = TimingMode.Frames; //Need to store to prevent from connecting more than 1 handler to the spin buttons. Default is Frames because it's going to be set to Times in the constructor.

	/* Widgets */
	private SpinButton startSpinButton = null;
	private SpinButton endSpinButton = null;
	private SpinButton durationSpinButton = null;

	/* Constants */
	private const int maxTime = 86399999; //milliseconds
	private const int maxFrames = 3000000;


	public SubtitleEditSpinButtons () {
		/* Assign */
		startSpinButton = Base.GetWidget(WidgetNames.StartSpinButton) as SpinButton;
		endSpinButton = Base.GetWidget(WidgetNames.EndSpinButton) as SpinButton;
		durationSpinButton = Base.GetWidget(WidgetNames.DurationSpinButton) as SpinButton;

		/* Initialize */
		startSpinButton.WidthChars = Util.SpinButtonTimeWidthChars;
		endSpinButton.WidthChars = Util.SpinButtonTimeWidthChars;
		durationSpinButton.WidthChars = Util.SpinButtonTimeWidthChars;

    	/* Set timing mode to Times */
    	SetTimingMode(TimingMode.Times); //Initial timing mode is Times

    	Base.InitFinished += OnBaseInitFinished;
	}

	/* Public methods */

	public void LoadTimings () {
		Subtitle subtitle = Base.Ui.View.Selection.Subtitle;
    	if (subtitle == null)
    		return;

		LoadStartTiming(subtitle);
		LoadEndTiming(subtitle);
		LoadDurationTiming(subtitle);
    }

    public void GetWidgets (out SpinButton startSpinButton, out SpinButton endSpinButton, out SpinButton durationSpinButton) {
    	startSpinButton = this.startSpinButton;
    	endSpinButton = this.endSpinButton;
    	durationSpinButton = this.durationSpinButton;
    }

    public void StartSpinButtonIncreaseStep () {
    	startSpinButton.Spin(SpinType.StepForward, 0); //0 uses the defined stepIncrement
    }

    public void StartSpinButtonDecreaseStep () {
    	startSpinButton.Spin(SpinType.StepBackward, 0); //0 uses the defined stepIncrement
    }

    public void EndSpinButtonIncreaseStep () {
    	endSpinButton.Spin(SpinType.StepForward, 0); //0 uses the defined stepIncrement
    }

    public void EndSpinButtonDecreaseStep () {
    	endSpinButton.Spin(SpinType.StepBackward, 0); //0 uses the defined stepIncrement
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
		SetFramesMode(startSpinButton, false);
	    SetFramesMode(endSpinButton, false);
	    SetFramesMode(durationSpinButton, true);
	}

	private void SetTimesMode () {
	    SetTimesMode(startSpinButton, false);
	    SetTimesMode(endSpinButton, false);
	    SetTimesMode(durationSpinButton, true);
	}

    private void SetTimesMode (SpinButton spinButton, bool allowNegatives) {
		Util.SetSpinButtonTimingMode(spinButton, TimingMode.Times);

		spinButton.Adjustment.StepIncrement = Base.Config.TimingsTimeStep;
		spinButton.Adjustment.Upper = maxTime;
		spinButton.Adjustment.Lower = (allowNegatives ? -maxTime : 0);
	}

	//TODO use Util.SetSpinButtonAdjustment
	private void SetFramesMode (SpinButton spinButton, bool allowNegatives) {
		Util.SetSpinButtonTimingMode(spinButton, TimingMode.Frames);

    	spinButton.Adjustment.StepIncrement = Base.Config.TimingsFramesStep;
    	spinButton.Adjustment.Upper = maxFrames;
    	spinButton.Adjustment.Lower = (allowNegatives ? -maxFrames : 0);
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

	private void ClearFields () {
        DisconnectSpinButtonsChangedSignals();
		startSpinButton.Text = String.Empty;
		endSpinButton.Text = String.Empty;
		durationSpinButton.Text = String.Empty;
    	ConnectSpinButtonsChangedSignals();
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
		if (Base.TimingModeIsFrames)
			Base.CommandManager.Execute(new ChangeStartCommand((int)startSpinButton.Value, true));
		else
			Base.CommandManager.Execute(new ChangeStartCommand(TimeSpan.FromMilliseconds(startSpinButton.Value), true));
	}

	private void OnEndValueChanged (object o, EventArgs args) {
		if (Base.TimingModeIsFrames)
			Base.CommandManager.Execute(new ChangeEndCommand((int)endSpinButton.Value, true));
		else
			Base.CommandManager.Execute(new ChangeEndCommand(TimeSpan.FromMilliseconds(endSpinButton.Value), true));
	}

	private void OnDurationValueChanged (object o, EventArgs args) {
		if (Base.TimingModeIsFrames)
			Base.CommandManager.Execute(new ChangeDurationCommand((int)durationSpinButton.Value, true));
		else
			Base.CommandManager.Execute(new ChangeDurationCommand(TimeSpan.FromMilliseconds(durationSpinButton.Value), true));
	}

	private void OnBaseInitFinished () {
		Base.TimingModeChanged += OnBaseTimingModeChanged;
		Base.Ui.View.Selection.Changed += OnSubtitleSelectionChanged;
	}

	private void OnBaseTimingModeChanged (TimingMode newTimingMode) {
    	if (timingMode == newTimingMode)
			return;

		SetTimingMode(newTimingMode);
		LoadTimings();
    }

    private void OnSubtitleSelectionChanged (TreePath[] paths, Subtitle subtitle) {
    	if (subtitle != null)
    		LoadTimings();
    	else
    		ClearFields();
    }

}

}