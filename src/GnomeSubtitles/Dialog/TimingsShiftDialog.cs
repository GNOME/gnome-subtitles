/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2018 Pedro Castro
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
using GnomeSubtitles.Ui;
using GnomeSubtitles.Ui.View;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Dialog {

//TODO set spinButton limits according to selection type
//TODO show informative message about the Ctrl+Shift++ shortcuts
public class TimingsShiftDialog : BaseDialog {
	private TimingMode timingMode = TimingMode.Frames;

	/* Widgets */
	private Frame timingModeFrame;
	private SpinButton spinButton = null;
	private RadioButton allSubtitlesRadioButton = null;
	private RadioButton selectedSubtitlesRadioButton = null;
	private RadioButton fromFirstSubtitleToSelectionRadioButton = null;
	private RadioButton fromSelectionToLastSubtitleRadioButton = null;

	public TimingsShiftDialog () : base(){
		Init(BuildDialog());
	}


	/* Methods */
	
	private Gtk.Dialog BuildDialog () {
		Gtk.Dialog dialog = new Gtk.Dialog(Catalog.GetString("Shift Timings"), Base.Ui.Window, DialogFlags.Modal | DialogFlagsUseHeaderBar,
			Util.GetStockLabel("gtk-cancel"), ResponseType.Cancel, Catalog.GetString("_Shift"), ResponseType.Ok);

		dialog.DefaultResponse = ResponseType.Ok;
		dialog.DefaultWidth = 1; //Needed otherwise the tip label will be displayed in a single line making the dialog have a huge width

		Box box = new Box(Orientation.Vertical, WidgetStyles.BoxSpacingLarge);
		box.BorderWidth = WidgetStyles.BorderWidthMedium;

		//Timing Mode frame

		timingModeFrame = new Frame();
		timingModeFrame.ShadowType = ShadowType.None;
		Label timingModeFrameLabel = new Label();
		timingModeFrame.LabelWidget = timingModeFrameLabel;
		
		spinButton = new SpinButton(new Adjustment(0, 0, 0, 1, 10, 0), 0, 0);
		spinButton.WidthChars = Core.Util.SpinButtonTimeWidthChars;
		spinButton.Alignment = 0.5f;
		Button button = new Button("gtk-clear");
		button.Clicked += OnClear;

		Box timingModeHBox = new Box(Orientation.Horizontal, WidgetStyles.BoxSpacingMedium);
		timingModeHBox.BorderWidth = WidgetStyles.BorderWidthMedium;
		timingModeHBox.MarginLeft = 10;
		timingModeHBox.Add(spinButton);
		timingModeHBox.Add(button);
		
		timingModeFrame.Add(timingModeHBox);
		box.Add(timingModeFrame);
		
		
		//Apply To frame
		
		Frame applyToFrame = new Frame();
		applyToFrame.ShadowType = ShadowType.None;
		Label applyToFrameLabel = new Label();
		applyToFrameLabel.Markup = "<b>" + Catalog.GetString("Apply to") + "</b>";
		applyToFrame.LabelWidget = applyToFrameLabel;
		
		allSubtitlesRadioButton = new RadioButton(Catalog.GetString("_All subtitles"));
		selectedSubtitlesRadioButton = new RadioButton(allSubtitlesRadioButton, Catalog.GetString("_Selected subtitles"));
		fromFirstSubtitleToSelectionRadioButton = new RadioButton(allSubtitlesRadioButton, Catalog.GetString("From _first subtitle to selection"));
		fromSelectionToLastSubtitleRadioButton = new RadioButton(allSubtitlesRadioButton, Catalog.GetString("From selection to _last subtitle"));

		Box applyToFrameVBox = new Box(Orientation.Vertical, WidgetStyles.BoxSpacingMedium);
		applyToFrameVBox.BorderWidth = WidgetStyles.BorderWidthMedium;
		applyToFrameVBox.MarginLeft = 10;
		applyToFrameVBox.Add(allSubtitlesRadioButton);
		applyToFrameVBox.Add(selectedSubtitlesRadioButton);
		applyToFrameVBox.Add(fromFirstSubtitleToSelectionRadioButton);
		applyToFrameVBox.Add(fromSelectionToLastSubtitleRadioButton);
		
		applyToFrame.Add(applyToFrameVBox);
		
		box.Add(applyToFrame);
		
		//Tips label
		
		Label label = new Label("<small><i>" + Catalog.GetString("Tip: use Shift+Plus/Minus (on the numpad) to shift timings directly from the main window.") + "</i></small>");
		label.UseMarkup = true;
		label.Wrap = true;
		box.Add(label);

		dialog.ContentArea.Add(box);
		UpdateContents(true);
		dialog.ContentArea.ShowAll();

		return dialog;
	}

	public override void Show () {
		UpdateContents(false);
		base.Show();
	}

	/* Private methods */

	private void UpdateContents (bool initializing) {
		UpdateFromTimingMode(Base.TimingMode, initializing);
		UpdateFromSelection();
		UpdateSpinButtonValue(initializing);
	}

	private void UpdateFromTimingMode (TimingMode newTimingMode, bool initializing) {
		if ((!initializing) && (newTimingMode == timingMode))
			return;

		timingMode = newTimingMode;
		Core.Util.SetSpinButtonTimingMode(spinButton, timingMode);
		Core.Util.SetSpinButtonMaxAdjustment(spinButton, timingMode, true);

		string label = (timingMode == TimingMode.Times ? Catalog.GetString("Time") : Catalog.GetString("Frames"));
		string markup = "<b>" + label + "</b>";
		(timingModeFrame.LabelWidget as Label).Markup = markup;
	}

	private void UpdateFromSelection () {
		int selectionCount = Core.Base.Ui.View.Selection.Count;
		fromFirstSubtitleToSelectionRadioButton.Sensitive = (selectionCount == 1);
		fromSelectionToLastSubtitleRadioButton.Sensitive = (selectionCount == 1);

		if (selectionCount > 0)
			selectedSubtitlesRadioButton.Active = true;
		else
			selectedSubtitlesRadioButton.Sensitive = false;
	}

	private void UpdateSpinButtonValue (bool initializing) {
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

	private void OnClear (object o, EventArgs args) {
		SetSpinButtonValue(0);
	}

	protected override bool ProcessResponse (ResponseType response) {
		if ((response == ResponseType.Ok) && (Math.Abs(spinButton.Value) > float.Epsilon)) {
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