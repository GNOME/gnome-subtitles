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


public class TimingsAdjustDialog : BaseDialog {
	private TimingMode timingMode;

	/* Widgets */

	private Label firstSubtitleStartInputLabel = null;
	private Label firstSubtitleNoInputLabel = null;
	private SpinButton firstSubtitleNewStartSpinButton = null;
	
	private Label lastSubtitleStartInputLabel = null;
	private Label lastSubtitleNoInputLabel = null;
	private SpinButton lastSubtitleNewStartSpinButton = null;
	
	private RadioButton allSubtitlesRadioButton = null;
	private RadioButton selectedSubtitlesRadioButton = null;
	private RadioButton fromFirstSubtitleToSelectionRadioButton = null;
	private RadioButton fromSelectionToLastSubtitleRadioButton = null;


	public TimingsAdjustDialog () : base() {
		timingMode = Base.TimingMode;
		
		Init(BuildDialog());
		
		UpdateFromTimingMode();
		UpdateApplyToOptionFromSubtitleSelection();
		ConnectEventHandlers();
	}
	
	/* Overridden members */

	public override DialogScope Scope {
		get { return DialogScope.Document; }
	}

	public override void Show () {
		UpdateApplyToOptionFromSubtitleSelection();
		base.Show();
	}
	
	public override void Destroy () {
		DisconnectEventHandlers();
		base.Destroy();
	}
	
	
	/* Private members */
	
	private Gtk.Dialog BuildDialog () {
		Gtk.Dialog dialog = new Gtk.Dialog(Catalog.GetString("Adjust Timings Between 2 Points"), Base.Ui.Window, DialogFlags.DestroyWithParent,
			Util.GetStockLabel("gtk-close"), ResponseType.Cancel, Catalog.GetString("_Apply"), ResponseType.Ok);

		dialog.DefaultResponse = ResponseType.Ok;

		Grid grid = new Grid();
		grid.BorderWidth = WidgetStyles.BorderWidthMedium;
		grid.RowSpacing = WidgetStyles.RowSpacingLarge;
		grid.ColumnSpacing = WidgetStyles.ColumnSpacingLarge;
		
		
		//First Subtitle frame
		
		Grid firstSubtitleGrid = new Grid();
		firstSubtitleGrid.RowSpacing = WidgetStyles.RowSpacingMedium;
		firstSubtitleGrid.ColumnSpacing = WidgetStyles.ColumnSpacingMedium;
		firstSubtitleGrid.MarginLeft = WidgetStyles.FrameContentSpacingMedium;
		Label firstSubtitleNoLabel = CreateAlignedLabel(Catalog.GetString("Subtitle No.:"));
		firstSubtitleGrid.Attach(firstSubtitleNoLabel, 0, 0, 1, 1);
		firstSubtitleNoInputLabel = CreateAlignedLabel();
		firstSubtitleGrid.Attach(firstSubtitleNoInputLabel, 1, 0, 1, 1);
		Label firstSubtitleStartLabel = CreateAlignedLabel(Catalog.GetString("Start:"));
		firstSubtitleGrid.Attach(firstSubtitleStartLabel, 0, 1, 1, 1);
		firstSubtitleStartInputLabel = CreateAlignedLabel();
		firstSubtitleGrid.Attach(firstSubtitleStartInputLabel, 1, 1, 1, 1);
		Label firstSubtitleNewStartLabel = CreateAlignedLabel(Catalog.GetString("New Start:"));
		firstSubtitleNewStartLabel.SetAlignment(0, 0.5f);
		firstSubtitleGrid.Attach(firstSubtitleNewStartLabel, 0, 2, 1, 1);
		firstSubtitleNewStartSpinButton = new SpinButton(new Adjustment(0, 0, 0, 1, 10, 0), 0, 0);
		firstSubtitleNewStartSpinButton.WidthChars = Core.Util.SpinButtonTimeWidthChars;
		firstSubtitleNewStartSpinButton.Alignment = 0.5f;
		firstSubtitleGrid.Attach(firstSubtitleNewStartSpinButton, 1, 2, 1, 1);

		Frame firstSubtitleFrame = Util.CreateFrameWithContent(Catalog.GetString("First Point"), firstSubtitleGrid);
		grid.Attach(firstSubtitleFrame, 0, 0, 1, 1);

	
		//Second Subtitle frame
		
		Grid lastSubtitleGrid = new Grid();
		lastSubtitleGrid.RowSpacing = WidgetStyles.RowSpacingMedium;
		lastSubtitleGrid.ColumnSpacing = WidgetStyles.ColumnSpacingMedium;
		lastSubtitleGrid.MarginLeft = WidgetStyles.FrameContentSpacingMedium;
		Label lastSubtitleNoLabel = CreateAlignedLabel(Catalog.GetString("Subtitle No.:"));
		lastSubtitleGrid.Attach(lastSubtitleNoLabel, 0, 0, 1, 1);
		lastSubtitleNoInputLabel = CreateAlignedLabel();
		lastSubtitleGrid.Attach(lastSubtitleNoInputLabel, 1, 0, 1, 1);
		Label lastSubtitleStartLabel = CreateAlignedLabel(Catalog.GetString("Start:"));
		lastSubtitleGrid.Attach(lastSubtitleStartLabel, 0, 1, 1, 1);
		lastSubtitleStartInputLabel = CreateAlignedLabel();
		lastSubtitleGrid.Attach(lastSubtitleStartInputLabel, 1, 1, 1, 1);
		Label lastSubtitleNewStartLabel = CreateAlignedLabel(Catalog.GetString("New Start:"));
		lastSubtitleGrid.Attach(lastSubtitleNewStartLabel, 0, 2, 1, 1);
		lastSubtitleNewStartSpinButton = new SpinButton(new Adjustment(0, 0, 0, 1, 10, 0), 0, 0);
		lastSubtitleNewStartSpinButton.WidthChars = Core.Util.SpinButtonTimeWidthChars;
		lastSubtitleNewStartSpinButton.Alignment = 0.5f;
		lastSubtitleGrid.Attach(lastSubtitleNewStartSpinButton, 1, 2, 1, 1);

		Frame lastSubtitleFrame = Util.CreateFrameWithContent(Catalog.GetString("Second Point"), lastSubtitleGrid);
		grid.Attach(lastSubtitleFrame, 1, 0, 1, 1);
		

		//Apply To frame
		
		allSubtitlesRadioButton = new RadioButton(DialogStrings.ApplyToAllSubtitles);
		selectedSubtitlesRadioButton = new RadioButton(allSubtitlesRadioButton, DialogStrings.ApplyToSelection);
		fromFirstSubtitleToSelectionRadioButton = new RadioButton(allSubtitlesRadioButton, DialogStrings.ApplyToFirstToSelection);
		fromSelectionToLastSubtitleRadioButton = new RadioButton(allSubtitlesRadioButton, DialogStrings.ApplyToSelectionToLast);

		allSubtitlesRadioButton.Toggled += OnToggleRadioButton;
		selectedSubtitlesRadioButton.Toggled += OnToggleRadioButton;
		fromFirstSubtitleToSelectionRadioButton.Toggled += OnToggleRadioButton;
		fromSelectionToLastSubtitleRadioButton.Toggled += OnToggleRadioButton;
		
		Box applyToFrameVBox = new Box(Orientation.Vertical, WidgetStyles.BoxSpacingMedium);
		applyToFrameVBox.MarginLeft = WidgetStyles.FrameContentSpacingMedium;
		applyToFrameVBox.Add(allSubtitlesRadioButton);
		applyToFrameVBox.Add(selectedSubtitlesRadioButton);
		applyToFrameVBox.Add(fromFirstSubtitleToSelectionRadioButton);
		applyToFrameVBox.Add(fromSelectionToLastSubtitleRadioButton);

		Frame applyToFrame = Util.CreateFrameWithContent(Catalog.GetString("Apply To"), applyToFrameVBox);
		grid.Attach(applyToFrame, 0, 1, 2, 2);
		
		dialog.ContentArea.Add(grid);
		dialog.ContentArea.ShowAll();
		
		return dialog;
	}
	
	private Label CreateAlignedLabel () {
		Label label = new Label();
		label.SetAlignment(0, 0.5f);
		return label;
	}
	
	private Label CreateAlignedLabel (string text) {
		Label label = CreateAlignedLabel();
		label.Text = text;
		return label;
	}

	private void UpdateFromTimingMode () {
		Core.Util.SetSpinButtonTimingMode(firstSubtitleNewStartSpinButton, timingMode);
		Core.Util.SetSpinButtonMaxAdjustment(firstSubtitleNewStartSpinButton, timingMode, false);
		Core.Util.SetSpinButtonTimingMode(lastSubtitleNewStartSpinButton, timingMode);
		Core.Util.SetSpinButtonMaxAdjustment(lastSubtitleNewStartSpinButton, timingMode, false);
		
		UpdateInputValuesAccordingToApplyToOption();
	}
	
	private void UpdateApplyToOptionFromSubtitleSelection () {
		int selectionCount = Core.Base.Ui.View.Selection.Count;
		if (selectionCount > 1) {
			if (selectedSubtitlesRadioButton.Active) {
				UpdateInputValuesAccordingToApplyToOption(); //It's already selected, need to call update directly
			} else {
				selectedSubtitlesRadioButton.Active = true;
			}
		} else {
			if (allSubtitlesRadioButton.Active) {
				UpdateInputValuesAccordingToApplyToOption(); //It's already selected, need to call update directly
			} else {
				allSubtitlesRadioButton.Active = true;
			}
		}
	}
	
	private void UpdateInputValuesAccordingToApplyToOption () {
		if (allSubtitlesRadioButton.Active) {
			UpdateInputValuesForApplyToAll();
		} else if (selectedSubtitlesRadioButton.Active) {
			UpdateInputValuesForApplyToSelection();
		} else if (fromFirstSubtitleToSelectionRadioButton.Active) {
			UpdateInputValuesForApplyToFirstToSelection();
		} else if (fromSelectionToLastSubtitleRadioButton.Active) {
			UpdateInputValuesForApplyToSelectionToLast();
		}
	}

	private void UpdateInputValuesForApplyToAll () {
		int subtitleCount = Base.Document.Subtitles.Collection.Count;

		int firstNo = (subtitleCount > 0 ? 1 : -1);
		int lastNo = (subtitleCount > 1 ? subtitleCount : -1);
		UpdateInputValues(firstNo, lastNo);
	}

	private void UpdateInputValuesForApplyToSelection () {
		int selectionCount = Core.Base.Ui.View.Selection.Count;
	
		int firstNo = (selectionCount > 0 ? Core.Base.Ui.View.Selection.FirstPath.Indices[0] + 1 : -1);
		int lastNo = (selectionCount > 1 ? Core.Base.Ui.View.Selection.LastPath.Indices[0] + 1 : -1);
		UpdateInputValues(firstNo, lastNo);
	}
	
	private void UpdateInputValuesForApplyToFirstToSelection () {
		int subtitleCount = Base.Document.Subtitles.Collection.Count;
		int firstNo = (subtitleCount > 0 ? 1 : -1);

		int lastNo = -1;
		if (subtitleCount > 1) {
			TreePath path = Core.Base.Ui.View.Selection.LastPath;
			if (path != null) {
				int lastIndex = path.Indices[0];
				if (lastIndex > 0) {
					lastNo = lastIndex + 1;
				}
			}
		}
		
		UpdateInputValues(firstNo, lastNo);
	}
	
	private void UpdateInputValuesForApplyToSelectionToLast () {
		int subtitleCount = Base.Document.Subtitles.Collection.Count;
		int lastNo = (subtitleCount > 0 ? subtitleCount : -1);
		
		int firstNo = -1;
		if (subtitleCount > 1) {
			TreePath path = Core.Base.Ui.View.Selection.FirstPath;
			if (path != null) {
				int firstIndex = path.Indices[0];
				if (firstIndex < subtitleCount - 1) {
					firstNo = firstIndex + 1;
				}
			}
		}
		
		UpdateInputValues(firstNo, lastNo);
	}

	private void UpdateInputValues (int firstNo, int lastNo) {
	
		//Handle the first subtitle
		if (firstNo == -1) {
			firstSubtitleNoInputLabel.Text = "-";
			firstSubtitleStartInputLabel.Text = "-";
			firstSubtitleNewStartSpinButton.Value = 0;
		} else {
			Subtitle firstSubtitle = Base.Document.Subtitles.Collection.Get(firstNo - 1);
			firstSubtitleNoInputLabel.Text = firstNo.ToString();

			if (timingMode == TimingMode.Frames) {			
				firstSubtitleStartInputLabel.Text = firstSubtitle.Frames.Start.ToString();
				firstSubtitleNewStartSpinButton.Value = firstSubtitle.Frames.Start;
			} else {
				firstSubtitleStartInputLabel.Text = Core.Util.TimeSpanToText(firstSubtitle.Times.Start);
				firstSubtitleNewStartSpinButton.Value = firstSubtitle.Times.Start.TotalMilliseconds;
			}
		}
		
		//Handle the last subtitle
		if (lastNo == -1) {
			lastSubtitleNoInputLabel.Text = "-";
			lastSubtitleStartInputLabel.Text = "-";
			lastSubtitleNewStartSpinButton.Value = 0;
		} else {
			Subtitle lastSubtitle = Base.Document.Subtitles.Collection.Get(lastNo - 1);
			lastSubtitleNoInputLabel.Text = lastNo.ToString();

			if (timingMode == TimingMode.Frames) {			
				lastSubtitleStartInputLabel.Text = lastSubtitle.Frames.Start.ToString();
				lastSubtitleNewStartSpinButton.Value = lastSubtitle.Frames.Start;
			} else {
				lastSubtitleStartInputLabel.Text = Core.Util.TimeSpanToText(lastSubtitle.Times.Start);
				lastSubtitleNewStartSpinButton.Value = lastSubtitle.Times.Start.TotalMilliseconds;
			}
		}
		
		//Update spin buttons sensitivity
		bool hasBothPoints = (firstNo != -1) && (lastNo != -1);
		firstSubtitleNewStartSpinButton.Sensitive = hasBothPoints;
		lastSubtitleNewStartSpinButton.Sensitive = hasBothPoints;
		
		//Update apply button sensitivity
		Button applyButton = Dialog.GetWidgetForResponse((int)ResponseType.Ok) as Button;
		applyButton.Sensitive = hasBothPoints;
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


	/* Event members */

	private void OnToggleRadioButton (object o, EventArgs args) {
		if ((o as RadioButton).Active) {
			UpdateInputValuesAccordingToApplyToOption();
		}
	}

	private void ConnectEventHandlers () {
		Base.TimingModeChanged += OnBaseTimingModeChanged;
		Base.Ui.View.Selection.Changed += OnSubtitleViewSelectionChanged;
	}
	
	private void DisconnectEventHandlers () {
		Base.TimingModeChanged -= OnBaseTimingModeChanged;
		Base.Ui.View.Selection.Changed -= OnSubtitleViewSelectionChanged;
	}
	
	private void OnBaseTimingModeChanged (TimingMode newTimingMode) {
    	if (timingMode == newTimingMode) {
			return;
		}

		timingMode = newTimingMode;
		UpdateFromTimingMode();
    }
    
    private void OnSubtitleViewSelectionChanged (TreePath[] paths, Subtitle subtitle) {
    	UpdateInputValuesAccordingToApplyToOption();
    }

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			SelectionIntended selectionIntended = GetSelectionIntended();
		
			if (timingMode == TimingMode.Times) {
				TimeSpan firstTime = TimeSpan.Parse(firstSubtitleNewStartSpinButton.Text);
				TimeSpan lastTime = TimeSpan.Parse(lastSubtitleNewStartSpinButton.Text);
				Base.CommandManager.Execute(new AdjustTimingsCommand(firstTime, lastTime, selectionIntended));
			}
			else {
				int firstFrame = (int)firstSubtitleNewStartSpinButton.Value;
				int lastFrame = (int)lastSubtitleNewStartSpinButton.Value;
				Base.CommandManager.Execute(new AdjustTimingsCommand(firstFrame, lastFrame, selectionIntended));
			}
			
			return true;
		}
		
		return false;
	}

    
}

}
