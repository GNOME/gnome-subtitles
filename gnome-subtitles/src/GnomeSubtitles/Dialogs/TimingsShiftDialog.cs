/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2007 Pedro Castro
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
using Gtk;
using Mono.Unix;
using SubLib;
using System;

namespace GnomeSubtitles {

public class TimingsShiftDialog : GladeDialog {
	private TimingMode timingMode = TimingMode.Frames;

	/* Constant strings */
	private const string gladeFilename = "TimingsShiftDialog.glade";

	/* Widgets */
	[WidgetAttribute] private Label timingModeLabel;
	[WidgetAttribute] private SpinButton spinButton;
	[WidgetAttribute] private RadioButton allSubtitlesRadioButton;
	[WidgetAttribute] private RadioButton selectedSubtitlesRadioButton;
	[WidgetAttribute] private RadioButton selectedSubtitleToFirstRadioButton;
	[WidgetAttribute] private RadioButton selectedSubtitleToLastRadioButton;

	public TimingsShiftDialog () : base(gladeFilename, true, true){
		InitSpinButton();
		UpdateContents();
	}
	
	/* Protected methods */
	
	public override void Show () {
		UpdateContents();
		base.Show();		
	}
	
	/* Private methods */
	
	private void UpdateContents () {
		UpdateFromTimingMode(Global.TimingMode);
		UpdateFromSelection();
		UpdateSpinButtonValue();
	}
	
	private void InitSpinButton () {
		spinButton.WidthRequest = Util.SpinButtonTimeWidth(spinButton);
		spinButton.Alignment = 0.5f;
	}

	private void UpdateFromTimingMode (TimingMode newTimingMode) {
		if (newTimingMode == timingMode)
			return;
			
		timingMode = newTimingMode;	
		Util.SetSpinButtonTimingMode(spinButton, timingMode, true);
		
		string label = (timingMode == TimingMode.Times ? Catalog.GetString("Time") : Catalog.GetString("Frames"));
		string markup = "<b>" + label + "</b>";
		timingModeLabel.Markup = markup;
	}
	
	private void UpdateFromSelection () {
		bool sensitive = (Global.GUI.View.Selection.Count == 1);
		selectedSubtitleToFirstRadioButton.Sensitive = sensitive;
		selectedSubtitleToLastRadioButton.Sensitive = sensitive;
		
		if ((!sensitive) && (!allSubtitlesRadioButton.Active) && (!selectedSubtitlesRadioButton.Active))
			selectedSubtitlesRadioButton.Active = true;
	}
	
	private void UpdateSpinButtonValue () {
		if (!Global.GUI.Video.IsLoaded) {
			SetSpinButtonValue(0);
			return;
		}
		
		TreePath path = Global.GUI.View.Selection.FirstPath;
		Subtitle subtitle = Global.Document.Subtitles[path];

		double subtitlePosition = 0;
		double videoPosition = 0;
		if (Global.TimingModeIsTimes) {
			subtitlePosition = subtitle.Times.Start.TotalMilliseconds;
			videoPosition = Global.GUI.Video.Position.CurrentTime * 1000; //Times 1000 for milliseconds
		}
		else {
			subtitlePosition = subtitle.Frames.Start;
			videoPosition = Global.GUI.Video.Position.CurrentFrames;
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
		else if (selectedSubtitleToFirstRadioButton.Active)
			return SelectionIntended.SimpleToFirst;
		else
			return SelectionIntended.SimpleToLast;
	}

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnClear (object o, EventArgs args) {
		SetSpinButtonValue(0);
	}
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok) {
			SelectionIntended selectionIntended = GetSelectionIntended();
			
			if (timingMode == TimingMode.Times) {
				TimeSpan time = TimeSpan.Parse(spinButton.Text);
				Global.CommandManager.Execute(new ShiftTimingsCommand(time, selectionIntended));
			}
			else {
				int frames = (int)spinButton.Value;
				Global.CommandManager.Execute(new ShiftTimingsCommand(frames, selectionIntended));
			}
		}
		Hide();
	}

}

}
