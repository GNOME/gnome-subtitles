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

using Gdk;
using Gtk;
using System;
using SubLib.Domain;

namespace GnomeSubtitles {

/* Renders a spin button which can hold a number (frames) or text (time) */
public class CellRendererSpinButton : CellRendererText {
	private string path = String.Empty;
	private TimingMode timingMode = TimingMode.Times;
	private event EditedHandler edited;	

	public CellRendererSpinButton (TimingMode timingMode) : base() {
		this.timingMode = timingMode;
	}

	public new EditedHandler Edited {
		get { return edited; }
		set { edited = value; }	
	}
	
	public TimingMode TimingMode {
		get { return timingMode; }
		set { timingMode = value; }
	}
	
	public void SetText(TimeSpan time) {
		this.Text = TimeSpanToText(time);		
	}
	
	public void SetText(int frames) {
		this.Text = frames.ToString();
	}
	
	 
	public override void GetSize (Widget widget, ref Rectangle cellArea, out int xOffset,
			out int yOffset, out int width, out int height) {
		
		base.GetSize(widget, ref cellArea, out xOffset, out yOffset, out width, out height);
		width += 20;	 //allow space for the spinbuttons
		height += 4;
	}
	
	public override CellEditable StartEditing (Gdk.Event evnt, Widget widget, string path,
			Rectangle backgroundArea, Rectangle cellArea, CellRendererState flags) {

		this.path = path;
		int valueLimit = (timingMode == TimingMode.Frames ? 3000000 : 86399999); 
		int step = (timingMode == TimingMode.Frames ? 1 : 100); 
		SpinButton spinButton = new SpinButton(0, valueLimit, step);
		spinButton.Alignment = 0.5f;
		spinButton.UpdatePolicy = SpinButtonUpdatePolicy.IfValid;
		spinButton.EditingDone += OnEditingDone;
		spinButton.ButtonPressEvent += OnButtonPress;
		if (timingMode == TimingMode.Times) {
			spinButton.Input += OnInput;
			spinButton.Output += OnOutput;
			spinButton.Value = TextToMilliseconds(this.Text);
		}
		else {
			spinButton.Value = Convert.ToInt32(this.Text);
		}
		spinButton.GrabFocus();
		spinButton.ShowAll();
		return spinButton;
	}
	

	
	private void OnEditingDone (object o, EventArgs args) {
		EditedArgs editedArgs = new EditedArgs();
		editedArgs.Args = new string[]{ path, (o as SpinButton).Text };
		if (this.Edited != null)
			Edited(this, editedArgs);
	}
	
	private void OnButtonPress (object o, ButtonPressEventArgs args) {
		args.RetVal = true;
	}

	private void OnInput (object o, InputArgs args) {
		args.NewValue = TextToMilliseconds((o as SpinButton).Text);
		args.RetVal = 1;
	}
	
	private void OnOutput (object o, OutputArgs args) {
		SpinButton spinButton = (SpinButton)o;
		spinButton.Numeric = false;
		spinButton.Text = MillisecondsToText((int)spinButton.Value);
		spinButton.Numeric = true;
		args.RetVal = 1;
	}
	
	private int TextToMilliseconds (string text) {
		return (int)TimeSpan.Parse(text).TotalMilliseconds;	
	}
	
	private string MillisecondsToText (int milliseconds) {
		return TimeSpanToText(TimeSpan.FromMilliseconds(milliseconds));
	}
	
	private string TimeSpanToText (TimeSpan time) {
		return time.Hours.ToString("D2") + ":" + time.Minutes.ToString("D2") +
				":" + time.Seconds.ToString("D2") + "." + time.Milliseconds.ToString("D3");
	}
	
	
	
}


}