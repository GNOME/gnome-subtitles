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

using GnomeSubtitles.Core;
using GnomeSubtitles.Core.Command;
using GnomeSubtitles.Ui.View;
using Glade;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Dialog {


public class TimingsAdjustDialog : GladeDialog {
	private TimingMode timingMode;
	
	/* Constant strings */
	private const string gladeFilename = "TimingsAdjustDialog.glade";
	
	/* Widgets */
	
	[WidgetAttribute] private Label firstSubtitleStartLabel = null;
	[WidgetAttribute] private Label firstSubtitleNoInputLabel = null;
	[WidgetAttribute] private Label firstSubtitleStartInputLabel = null;
	[WidgetAttribute] private SpinButton firstSubtitleNewStartSpinButton = null;
	[WidgetAttribute] private Label lastSubtitleStartLabel = null;
	[WidgetAttribute] private Label lastSubtitleNoInputLabel = null;
	[WidgetAttribute] private Label lastSubtitleStartInputLabel = null;
	[WidgetAttribute] private SpinButton lastSubtitleNewStartSpinButton = null;
	[WidgetAttribute] private RadioButton allSubtitlesRadioButton = null;	
	[WidgetAttribute] private RadioButton selectedSubtitlesRadioButton = null;
	

	public TimingsAdjustDialog () : base(gladeFilename){
		timingMode = Base.TimingMode; 
		SetSpinButtons();
		UpdateForTimingMode();
		SetApplyToSelectionSensitivity();
		SetApplyToAll();
	}
	
	private void SetSpinButtons () {
		int width = Core.Util.SpinButtonTimeWidth(firstSubtitleNewStartSpinButton);
		firstSubtitleNewStartSpinButton.WidthRequest = width;
		lastSubtitleNewStartSpinButton.WidthRequest = width;
	}

	private void UpdateForTimingMode () {
		if (timingMode == TimingMode.Times) {
			string startLabel = Catalog.GetString("Start Time:");
			firstSubtitleStartLabel.Text = startLabel;
			lastSubtitleStartLabel.Text = startLabel;
		}
		Core.Util.SetSpinButtonTimingMode(firstSubtitleNewStartSpinButton, timingMode, false);
		Core.Util.SetSpinButtonTimingMode(lastSubtitleNewStartSpinButton, timingMode, false);
	}	

	private void SetApplyToAll () {
		SubtitleCollection collection = Base.Document.Subtitles.Collection;
		
		int firstNo = 1;
		int lastNo = collection.Count;		
		UpdateInputValues(firstNo, lastNo);
	}
	
	private void SetApplyToSelection () {
		TreePath firstPath = Core.Base.Ui.View.Selection.FirstPath;
		TreePath lastPath = Core.Base.Ui.View.Selection.LastPath;
		
		int firstNo = firstPath.Indices[0] + 1;
		int lastNo = lastPath.Indices[0] + 1;
		
		UpdateInputValues (firstNo, lastNo);
	}
	
	private void SetApplyToSelectionSensitivity () {
		int selectionCount = Core.Base.Ui.View.Selection.Count;
		if (selectionCount < 2)
			selectedSubtitlesRadioButton.Sensitive = false;
	}

	private void UpdateInputValues (int firstNo, int lastNo) {
		SubtitleCollection collection = Base.Document.Subtitles.Collection;
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
			firstSubtitleStartInputLabel.Text = Core.Util.TimeSpanToText(firstSubtitle.Times.Start);
			firstSubtitleNewStartSpinButton.Value = firstSubtitle.Times.Start.TotalMilliseconds;
			lastSubtitleStartInputLabel.Text = Core.Util.TimeSpanToText(lastSubtitle.Times.Start);
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
			SelectionIntended selectionIntended = (allSubtitlesRadioButton.Active ? SelectionIntended.All : SelectionIntended.Range);
			
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
		}
		Close();
	}

}

}
