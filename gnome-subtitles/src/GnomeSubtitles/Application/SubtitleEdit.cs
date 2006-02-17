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

using Gtk;
using System;
using System.Collections;
using SubLib.Domain;

namespace GnomeSubtitles {

public class SubtitleEdit : GladeWidget {
	private SpinButton startSpinButton = null;
	private SpinButton endSpinButton = null;
	private SpinButton durationSpinButton = null;
	private TextView textView = null;
	private TextTag scaleTag = new TextTag("scale");
	private TextTag boldTag = new TextTag("bold");
	private TextTag italicTag = new TextTag("italic");
	private TextTag underlineTag = new TextTag("underline");
	private ArrayList subtitleTags = new ArrayList(4); //4 not to resize when full
	private Subtitle subtitle = null;

	public SubtitleEdit(GUI gui, Glade.XML glade) : base(gui, glade){
		startSpinButton = (SpinButton)GetWidget(WidgetNames.StartSpinButton);
		endSpinButton = (SpinButton)GetWidget(WidgetNames.EndSpinButton);
		durationSpinButton = (SpinButton)GetWidget(WidgetNames.DurationSpinButton);
		textView = (TextView)GetWidget(WidgetNames.SubtitleTextView);
    		startSpinButton.WidthRequest = SpinButtonWidth();
    		startSpinButton.Alignment = 0.5f;
    		endSpinButton.Alignment = 0.5f;
    		durationSpinButton.Alignment = 0.5f;    	
    		scaleTag.Scale = Pango.Scale.XLarge;
    		boldTag.Weight = Pango.Weight.Bold;
    		italicTag.Style = Pango.Style.Italic;
    		underlineTag.Underline = Pango.Underline.Single;
    		textView.Buffer.TagTable.Add(scaleTag);
    		textView.Buffer.TagTable.Add(boldTag);
    		textView.Buffer.TagTable.Add(italicTag);
    		textView.Buffer.TagTable.Add(underlineTag);
    }
    
    
    public void Show () {
		Widget hBox = GetWidget(WidgetNames.SubtitleEditHBox);
    		hBox.Sensitive = true;
    		hBox.Visible = true;
    		UpdateTimingMode();
    }
    
    public void UpdateTimingMode () {
    		if (GUI.Core.Subtitles.Properties.TimingMode == TimingMode.Frames) {
	    		SetFramesMode(startSpinButton);
	    		SetFramesMode(endSpinButton);
	    		SetFramesMode(durationSpinButton);
	    	}
	    	else {
	    		SetTimesMode(startSpinButton);
	    		SetTimesMode(endSpinButton);
	    		SetTimesMode(durationSpinButton);
	    	}
	    	ShowTimings();
    }

    public void ShowSubtitle (Subtitle subtitle) {
    		this.subtitle = subtitle;

   		textView.Buffer.Changed -= OnBufferChanged;
    		ShowTimings();
    		ShowText();
    		textView.Buffer.Changed += OnBufferChanged;
    }
    
    private void ShowTimings () {
    		if (subtitle == null)
    			return;

    		if (GUI.Core.Subtitles.Properties.TimingMode == TimingMode.Frames) {
			startSpinButton.Value = subtitle.Frames.Start;
			endSpinButton.Value = subtitle.Frames.End;
			durationSpinButton.Value = subtitle.Frames.Duration;
		}
		else {
			startSpinButton.Value = subtitle.Times.Start.TotalMilliseconds;
			endSpinButton.Value = subtitle.Times.End.TotalMilliseconds;
			durationSpinButton.Value = subtitle.Times.Duration.TotalMilliseconds;
		}
    }
    
    private void ShowText () {
    		subtitleTags.Clear();
    		if (subtitle.Style.Bold)
    			subtitleTags.Add(boldTag);
    		if (subtitle.Style.Italic)
    			subtitleTags.Add(italicTag);
    		if (subtitle.Style.Underline)
    			subtitleTags.Add(underlineTag);

    		textView.Buffer.Text = subtitle.Text.Get();    		
		ApplyTags();    
    }
    
    private void ApplyTags () {
    		TextBuffer buffer = textView.Buffer;
    		TextIter start = buffer.StartIter;
    		TextIter end = buffer.EndIter;
    		buffer.ApplyTag(scaleTag, start, end);
    		foreach (TextTag tag in subtitleTags)
    			buffer.ApplyTag(tag, start, end);
    }

    private int SpinButtonWidth () {
    		const int margins = 25;
		return Utility.TextWidth(startSpinButton, "00:00:00,000", margins);
    }
    
    	private int TimeTextToMilliseconds (string text) {
		return (int)TimeSpan.Parse(text).TotalMilliseconds;	
	}
	
	private string MillisecondsToTimeText (int milliseconds) {
		return Utility.TimeSpanToText(TimeSpan.FromMilliseconds(milliseconds));
	}
    
    private void SetTimesMode (SpinButton spinButton) {
    		spinButton.Input += OnInput;
		spinButton.Output += OnOutput;
		spinButton.Adjustment.StepIncrement = 100;
		spinButton.Adjustment.Upper = 86399999;
	}
	
	private void SetFramesMode (SpinButton spinButton) {
    		spinButton.Input -= OnInput;
    		spinButton.Output -= OnOutput;
    		spinButton.Adjustment.StepIncrement = 1;
    		spinButton.Adjustment.Upper = 3000000;
	}
    
	private void OnInput (object o, InputArgs args) {
		args.NewValue = TimeTextToMilliseconds((o as SpinButton).Text);
		args.RetVal = 1;
	}
	
	private void OnOutput (object o, OutputArgs args) {
		SpinButton spinButton = (SpinButton)o;
		spinButton.Numeric = false;
		spinButton.Text = MillisecondsToTimeText((int)spinButton.Value);
		spinButton.Numeric = true;
		args.RetVal = 1;
	}
	
	private void OnBufferChanged (object o, EventArgs args) {
		ApplyTags();
		subtitle.Text.Set((o as TextBuffer).Text);
		GUI.SubtitleView.UpdateSelectedRow();
	}

}

}
