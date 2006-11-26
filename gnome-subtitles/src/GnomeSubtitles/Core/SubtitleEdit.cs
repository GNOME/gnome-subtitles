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
using SubLib;

namespace GnomeSubtitles {

public class SubtitleEdit {
	private SpinButton startSpinButton = null;
	private SpinButton endSpinButton = null;
	private SpinButton durationSpinButton = null;
	private TextView textView = null;
	private	HBox hBox = null;
	
	private TextTag scaleTag = new TextTag("scale");
	private TextTag boldTag = new TextTag("bold");
	private TextTag italicTag = new TextTag("italic");
	private TextTag underlineTag = new TextTag("underline");
	private ArrayList subtitleTags = new ArrayList(4); //4 not to resize with 3 items
	
	private Subtitle subtitle = null;

	public SubtitleEdit() {
		startSpinButton = Global.GetWidget(WidgetNames.StartSpinButton) as SpinButton;
		endSpinButton = Global.GetWidget(WidgetNames.EndSpinButton) as SpinButton;
		durationSpinButton = Global.GetWidget(WidgetNames.DurationSpinButton) as SpinButton;
		textView = Global.GetWidget(WidgetNames.SubtitleTextView) as TextView;
		hBox = Global.GetWidget(WidgetNames.SubtitleEditHBox) as HBox;
		
    	startSpinButton.WidthRequest = Util.SpinButtonTimeWidth(startSpinButton);
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
    	
    	SetTimesMode(); //Initial timing mode is Times
    }
    
	/* Public properties */

    public bool Enabled {
    	get { return hBox.Sensitive; }
    	set {
			if (hBox.Sensitive == value)
				return;

			if (value == false)
				ClearFields();

			hBox.Sensitive = value;
    	}
    }
    
    /// <summary>The text that is currently selected, or <see cref="Selection.Empty" /> if no text is selected.</summary>
    public string TextSelection {
    	get {
    		if (!Enabled)
    			return String.Empty;

    		TextIter start, end;
    		if (textView.Buffer.GetSelectionBounds(out start, out end)) //has selection
    			return textView.Buffer.GetText(start, end, false);
    		else
    			return String.Empty;
    	}
    }
    
    
    /// <summary>Whether the <see cref="TextView" /> is the widget with focus.</summary>
    public bool TextIsFocus {
    	get { return textView.IsFocus; }    
    }
    
	/// <summary>The index of the cursor within the <see cref="TextView" />.</summary>
    public int TextCursorIndex {
    	get {
    		if (!Enabled)
    			return -1;
    		else
    			return textView.Buffer.GetIterAtMark(textView.Buffer.InsertMark).Offset;
    	}
    }
    
    /* Public methods */

    public void GetEditableWidgets (out SpinButton startSpinButton, out SpinButton endSpinButton,
    		out SpinButton durationSpinButton, out TextView textView) {
    	
    	startSpinButton = this.startSpinButton;
    	endSpinButton = this.endSpinButton;
    	durationSpinButton = this.durationSpinButton;
    	textView = this.textView;   	
    }
    
    public void NewDocument (bool wasLoaded) {
    	if (!wasLoaded)
    		hBox.Sensitive = true;
    }
    
    public void BlankStartUp () {
    	ClearFields();
    }
    
    public void ToggleTimingMode (TimingMode mode) { 	
    	SetTimingMode(mode);
	   	LoadTimings(mode);
    }

    public void UpdateFromSelection (Subtitle subtitle) {
	   	this.Enabled = true;
    	this.subtitle = subtitle;
		LoadTimings(Global.TimingMode);
		LoadTags();
    	LoadText();
    }

    public void TextGrabFocus () {
    	textView.GrabFocus();
    }
    
    public void TextFocusOnSelection (int startIndex, int endIndex) {
    	TextGrabFocus();
		TextIter start = textView.Buffer.GetIterAtOffset(startIndex);
		TextIter end = textView.Buffer.GetIterAtOffset(endIndex);
		textView.Buffer.SelectRange(start, end);		
    }
    
    /* Private Methods */
    
    private void ClearFields () {
   		DisconnectTextBufferChangedSignal();
    	textView.Buffer.Text = String.Empty;
    	ConnectTextBufferChangedSignal();
    	
    	DisconnectSpinButtonsChangedSignals();
		startSpinButton.Text = String.Empty;
		endSpinButton.Text = String.Empty;
		durationSpinButton.Text = String.Empty;
    	ConnectSpinButtonsChangedSignals();
    }
    
	private void SetTag (TextTag tag, TextIter start, TextIter end, bool activate) {
		if (activate)
			textView.Buffer.ApplyTag(tag, start, end);
		else
			textView.Buffer.RemoveTag(tag, start, end);
    }
    
    private void LoadText () {
    	textView.Buffer.Changed -= OnBufferChanged; 
    	textView.Buffer.Text = subtitle.Text.Get();    		
		ApplyLoadedTags();   
		textView.Buffer.Changed += OnBufferChanged; 
    }
    
    private void LoadTags () {
    	subtitleTags.Clear();
    	if (subtitle.Style.Bold)
    		subtitleTags.Add(boldTag);
    	if (subtitle.Style.Italic)
    		subtitleTags.Add(italicTag);
    	if (subtitle.Style.Underline)
    		subtitleTags.Add(underlineTag);
    }
    
    private void ApplyLoadedTags () {
    	TextBuffer buffer = textView.Buffer;
    	TextIter start = buffer.StartIter;
    	TextIter end = buffer.EndIter;
    	buffer.ApplyTag(scaleTag, start, end);
    	foreach (TextTag tag in subtitleTags)
		SetTag(tag, start, end, true);
    }
    
    private void ConnectTextBufferChangedSignal () {
		textView.Buffer.Changed += OnBufferChanged; 
    }
    
    private void DisconnectTextBufferChangedSignal () {
		textView.Buffer.Changed -= OnBufferChanged; 
    }
    
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
   	
   	private void SetTimingMode(TimingMode mode) {
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
	
	private void LoadTimings (TimingMode mode) {
    	if (subtitle == null)
    		return;
    			
		LoadStartTiming(mode);
		LoadEndTiming(mode);
		LoadDurationTiming(mode);
    }
	
	private void LoadStartTiming (TimingMode mode) {
    	startSpinButton.ValueChanged -= OnStartValueChanged;
    		
    	if (mode == TimingMode.Frames)
    		startSpinButton.Value = subtitle.Frames.Start;
    	else
    		startSpinButton.Value = subtitle.Times.Start.TotalMilliseconds;

   		startSpinButton.ValueChanged += OnStartValueChanged;
   	}
   	
	private void LoadEndTiming (TimingMode mode) {
   		endSpinButton.ValueChanged -= OnEndValueChanged;
    		
    	if (mode == TimingMode.Frames)
    		endSpinButton.Value = subtitle.Frames.End;
    	else
    		endSpinButton.Value = subtitle.Times.End.TotalMilliseconds;

   		endSpinButton.ValueChanged += OnEndValueChanged;
	}
	
	private void LoadDurationTiming (TimingMode mode) {
    	durationSpinButton.ValueChanged -= OnDurationValueChanged;

    	if (mode == TimingMode.Frames)
    		durationSpinButton.Value = subtitle.Frames.Duration;
    	else
    		durationSpinButton.Value = subtitle.Times.Duration.TotalMilliseconds;

    	durationSpinButton.ValueChanged += OnDurationValueChanged;
    }
	
	/* Event Handlers */
    
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
	
	private void OnBufferChanged (object o, EventArgs args) {
		ApplyLoadedTags();
		Global.CommandManager.Execute(new ChangeTextCommand((o as TextBuffer).Text));
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

}

}
