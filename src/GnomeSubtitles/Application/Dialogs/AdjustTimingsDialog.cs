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


public class AdjustTimingsDialog : GladeDialog {
	private TimingMode timingMode;
	
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
	

	public AdjustTimingsDialog (GUI gui) : base(gui, WidgetNames.AdjustTimingsDialog){
		timingMode = gui.Core.TimingMode; 
		SetUpSpinButtons();
		UpdateForTimingMode();
		SetApplyToSelectionSensitivity();
		SetApplyToAll();
	}
	
	private void SetUpSpinButtons () {
		int width = Utility.SpinButtonTimeWidth(firstSubtitleNewStartSpinButton);
		firstSubtitleNewStartSpinButton.WidthRequest = width;
		lastSubtitleNewStartSpinButton.WidthRequest = width;
	}

	private void UpdateForTimingMode () {
		if (timingMode == TimingMode.Times) {
			string startLabel = "Start Time:";
			firstSubtitleStartLabel.Text = startLabel;
			lastSubtitleStartLabel.Text = startLabel;
		}
		Utility.SetSpinButtonTimingMode(firstSubtitleNewStartSpinButton, timingMode, false);
		Utility.SetSpinButtonTimingMode(lastSubtitleNewStartSpinButton, timingMode, false);
	}	

	private void SetApplyToAll () {
		SubtitleCollection collection = GUI.Core.Subtitles.Collection;
		
		int firstNo = 1;
		int lastNo = collection.Count;		
		UpdateInputValues(firstNo, lastNo);
	}
	
	private void SetApplyToSelection () {
		TreePath firstPath = GUI.SubtitleView.FirstSelectedPath;
		TreePath lastPath = GUI.SubtitleView.LastSelectedPath;
		
		int firstNo = firstPath.Indices[0] + 1;
		int lastNo = lastPath.Indices[0] + 1;
		
		UpdateInputValues (firstNo, lastNo);
	}
	
	private void SetApplyToSelectionSensitivity () {
		int selectionCount = GUI.SubtitleView.SelectedPathCount;
		if (selectionCount < 2)
			selectedSubtitlesRadioButton.Sensitive = false;
	}

	private void UpdateInputValues (int firstNo, int lastNo) {
		SubtitleCollection collection = GUI.Core.Subtitles.Collection;
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
			firstSubtitleStartInputLabel.Text = Utility.TimeSpanToText(firstSubtitle.Times.Start);
			firstSubtitleNewStartSpinButton.Value = firstSubtitle.Times.Start.TotalMilliseconds;
			lastSubtitleStartInputLabel.Text = Utility.TimeSpanToText(lastSubtitle.Times.Start);
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
			bool applyToAll = allSubtitlesRadioButton.Active;
			
			if (timingMode == TimingMode.Times) {
				TimeSpan firstTime = TimeSpan.Parse(firstSubtitleNewStartSpinButton.Text);
				TimeSpan lastTime = TimeSpan.Parse(lastSubtitleNewStartSpinButton.Text);
				GUI.Core.CommandManager.Execute(new AdjustTimingsCommand(GUI, firstTime, lastTime, applyToAll));
			}
			else {
				int firstFrame = (int)firstSubtitleNewStartSpinButton.Value;
				int lastFrame = (int)lastSubtitleNewStartSpinButton.Value;
				GUI.Core.CommandManager.Execute(new AdjustTimingsCommand(GUI, firstFrame, lastFrame, applyToAll));
			}
		}
		CloseDialog();
	}

}

}