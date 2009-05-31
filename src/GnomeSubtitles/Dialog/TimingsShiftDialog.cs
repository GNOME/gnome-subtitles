/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2009 Pedro Castro
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
using GnomeSubtitles.Ui.View;
using Glade;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Dialog {

//TODO set spinButton limits according to selection type
public class TimingsShiftDialog : GladeDialog {
	private TimingMode timingMode = TimingMode.Frames;

	/* Constant strings */
	private const string gladeFilename = "TimingsShiftDialog.glade";

	/* Widgets */
	[WidgetAttribute] private Label timingModeLabel = null;
	[WidgetAttribute] private SpinButton spinButton = null;
	[WidgetAttribute] private RadioButton allSubtitlesRadioButton = null;
	[WidgetAttribute] private RadioButton selectedSubtitlesRadioButton = null;
	[WidgetAttribute] private RadioButton fromFirstSubtitleToSelectionRadioButton = null;
	[WidgetAttribute] private RadioButton fromSelectionToLastSubtitleRadioButton = null;

	public TimingsShiftDialog () : base(gladeFilename){
		InitSpinButton();
		UpdateContents(true);
	}

	/* Overriden members */

	public override DialogScope Scope {
		get { return DialogScope.Document; }
	}

	/* Methods */

	public override void Show () {
		UpdateContents(false);
		base.Show();		
	}
	
	/* Private methods */
	
	private void UpdateContents (bool initializing) {
		UpdateFromTimingMode(Base.TimingMode, initializing);
		UpdateFromSelection();
		UpdateSpinButtonValue();
	}
	
	private void InitSpinButton () {
		spinButton.WidthRequest = Core.Util.SpinButtonTimeWidth(spinButton);
		spinButton.Alignment = 0.5f;
	}

	private void UpdateFromTimingMode (TimingMode newTimingMode, bool initializing) {
		if ((!initializing) && (newTimingMode == timingMode))
			return;
			
		timingMode = newTimingMode;	
		Core.Util.SetSpinButtonTimingMode(spinButton, timingMode);
		Core.Util.SetSpinButtonMaxAdjustment(spinButton, timingMode, true);
		
		string label = (timingMode == TimingMode.Times ? Catalog.GetString("Time") : Catalog.GetString("Frames"));
		string markup = "<b>" + label + "</b>";
		timingModeLabel.Markup = markup;
	}
	
	private void UpdateFromSelection () {
		bool sensitive = (Core.Base.Ui.View.Selection.Count == 1);
		fromFirstSubtitleToSelectionRadioButton.Sensitive = sensitive;
		fromSelectionToLastSubtitleRadioButton.Sensitive = sensitive;
		
		if ((!sensitive) && (!allSubtitlesRadioButton.Active) && (!selectedSubtitlesRadioButton.Active))
			selectedSubtitlesRadioButton.Active = true;
	}
	
	private void UpdateSpinButtonValue () {
		if (!Core.Base.Ui.Video.IsLoaded) {
			SetSpinButtonValue(0);
			return;
		}
		
		TreePath path = Core.Base.Ui.View.Selection.FirstPath;
		Subtitle subtitle = Base.Document.Subtitles[path];

		double subtitlePosition = 0;
		double videoPosition = 0;
		if (Base.TimingModeIsTimes) {
			subtitlePosition = subtitle.Times.Start.TotalMilliseconds;
			videoPosition = Core.Base.Ui.Video.Position.CurrentTime.TotalMilliseconds;
		}
		else {
			subtitlePosition = subtitle.Frames.Start;
			videoPosition = Core.Base.Ui.Video.Position.CurrentFrames;
		}

		double difference = videoPosition - subtitlePosition;
		SetSpinButtonValue(difference);
	}
	
	private void SetSpinButtonValue (double newValue) {
		spinButton.Value = newValue;
	}
	
	private SelectionIntended GetSelectionIntended () {
		if (allSubtitlesRadioButton.Active)
			return SelectionIntended.All;
		else if (selectedSubtitlesRadioButton.Active)
			return SelectionIntended.Simple;
		else if (fromFirstSubtitleToSelectionRadioButton.Active)
			return SelectionIntended.SimpleToFirst;
		else
			return SelectionIntended.SimpleToLast;
	}

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnClear (object o, EventArgs args) {
		SetSpinButtonValue(0);
	}

	protected override bool ProcessResponse (ResponseType response) {
		if ((response == ResponseType.Ok) && (spinButton.Value != 0)) {
			SelectionIntended selectionIntended = GetSelectionIntended();
			
			if (timingMode == TimingMode.Times) {
				TimeSpan time = TimeSpan.FromMilliseconds(spinButton.Value);
				Base.CommandManager.Execute(new ShiftTimingsCommand(time, selectionIntended));
			}
			else {
				int frames = (int)spinButton.Value;
				Base.CommandManager.Execute(new ShiftTimingsCommand(frames, selectionIntended));
			}
		}
		return false;
	}

}

}
