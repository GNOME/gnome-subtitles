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
using Glade;
using Gtk;
using SubLib;

namespace GnomeSubtitles {


public class TimingsAdjustDialog : GladeDialog {
	private TimingMode timingMode;
	
	/* Constant strings */
	private const string gladeFilename = "TimingsAdjustDialog.glade";
	
	/* Widgets */
	
	[WidgetAttribute]
	private Label firstSubtitleStartLabel;
	[WidgetAttribute]
	private Label firstSubtitleNoInputLabel;
	[WidgetAttribute]
	private Label firstSubtitleStartInputLabel;
	[WidgetAttribute]
	private SpinButton firstSubtitleNewStartSpinButton;
	[WidgetAttribute]
	private Label lastSubtitleStartLabel;
	[WidgetAttribute]
	private Label lastSubtitleNoInputLabel;
	[WidgetAttribute]
	private Label lastSubtitleStartInputLabel;
	[WidgetAttribute]
	private SpinButton lastSubtitleNewStartSpinButton;
	[WidgetAttribute]
	private RadioButton allSubtitlesRadioButton;	
	[WidgetAttribute]
	private RadioButton selectedSubtitlesRadioButton;
	

	public TimingsAdjustDialog () : base(gladeFilename){
		timingMode = Global.TimingMode; 
		SetSpinButtons();
		UpdateForTimingMode();
		SetApplyToSelectionSensitivity();
		SetApplyToAll();
	}
	
	private void SetSpinButtons () {
		int width = Util.SpinButtonTimeWidth(firstSubtitleNewStartSpinButton);
		firstSubtitleNewStartSpinButton.WidthRequest = width;
		lastSubtitleNewStartSpinButton.WidthRequest = width;
	}

	private void UpdateForTimingMode () {
		if (timingMode == TimingMode.Times) {
			string startLabel = "Start Time:";
			firstSubtitleStartLabel.Text = startLabel;
			lastSubtitleStartLabel.Text = startLabel;
		}
		Util.SetSpinButtonTimingMode(firstSubtitleNewStartSpinButton, timingMode, false);
		Util.SetSpinButtonTimingMode(lastSubtitleNewStartSpinButton, timingMode, false);
	}	

	private void SetApplyToAll () {
		SubtitleCollection collection = Global.Subtitles.Collection;
		
		int firstNo = 1;
		int lastNo = collection.Count;		
		UpdateInputValues(firstNo, lastNo);
	}
	
	private void SetApplyToSelection () {
		TreePath firstPath = Global.GUI.View.Selection.FirstPath;
		TreePath lastPath = Global.GUI.View.Selection.LastPath;
		
		int firstNo = firstPath.Indices[0] + 1;
		int lastNo = lastPath.Indices[0] + 1;
		
		UpdateInputValues (firstNo, lastNo);
	}
	
	private void SetApplyToSelectionSensitivity () {
		int selectionCount = Global.GUI.View.Selection.Count;
		if (selectionCount < 2)
			selectedSubtitlesRadioButton.Sensitive = false;
	}

	private void UpdateInputValues (int firstNo, int lastNo) {
		SubtitleCollection collection = Global.Subtitles.Collection;
		Subtitle firstSubtitle = collection.Get(firstNo - 1);
		Subtitle lastSubtitle = collection.Get(lastNo - 1);
	
		firstSubtitleNoInputLabel.Text = firstNo.ToString();
		lastSubtitleNoInputLabel.Text = lastNo.ToString();
		
		if (timingMode == TimingMode.Frames) {
			firstSubtitleStartInputLabel.Text = firstSubtitle.Frames.Start.ToString();
			firstSubtitleNewStartSpinButton.Value = firstSubtitle.Frames.Start;
			lastSubtitleStartInputLabel.Text = lastSubtitle.Frames.Start.ToString();
			lastSubtitleNewStartSpinButton.Value = lastSubtitle.Frames.Start;
		}
		else {
			firstSubtitleStartInputLabel.Text = Util.TimeSpanToText(firstSubtitle.Times.Start);
			firstSubtitleNewStartSpinButton.Value = firstSubtitle.Times.Start.TotalMilliseconds;
			lastSubtitleStartInputLabel.Text = Util.TimeSpanToText(lastSubtitle.Times.Start);
			lastSubtitleNewStartSpinButton.Value = lastSubtitle.Times.Start.TotalMilliseconds;
		}
	}

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnToggleAllSubtitles (object o, EventArgs args) {
		if ((o as RadioButton).Active)
			SetApplyToAll();
	}
	
	private void OnToggleSelectedSubtitles (object o, EventArgs args) {
		if ((o as RadioButton).Active)
			SetApplyToSelection();
	}
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok) {
			SelectionType selectionType = (allSubtitlesRadioButton.Active ? SelectionType.All : SelectionType.Range);
			
			if (timingMode == TimingMode.Times) {
				TimeSpan firstTime = TimeSpan.Parse(firstSubtitleNewStartSpinButton.Text);
				TimeSpan lastTime = TimeSpan.Parse(lastSubtitleNewStartSpinButton.Text);
				Global.CommandManager.Execute(new AdjustTimingsCommand(firstTime, lastTime, selectionType));
			}
			else {
				int firstFrame = (int)firstSubtitleNewStartSpinButton.Value;
				int lastFrame = (int)lastSubtitleNewStartSpinButton.Value;
				Global.CommandManager.Execute(new AdjustTimingsCommand(firstFrame, lastFrame, selectionType));
			}
		}
		CloseDialog();
	}

}

}