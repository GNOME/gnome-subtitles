/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2011 Pedro Castro
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
using Gtk;
using System;

namespace GnomeSubtitles.Ui.Edit {

//TODO possible improvements: draw text once with newlines and Spacing
public class SubtitleEditTextViewMargin {
	private int marginCharWidth = -1; //pixels
	private int marginSpace = 4; //pixels
	private int marginDigitCount = 2; //we always show ammount for 2 digits, which should account for "normal" lengths (above 99, lengths are marked with 99+)
	private int marginNumbersWidth = -1;
	private int marginWidth = -1;
	private int marginMaxCharCount = 99;
	private String marginMaxCharCountString = "99.";
	
	/* Cached GCs and Pango Layout */
	private Gdk.GC bgGC = null;
	private Gdk.GC lineGC = null;
	private Gdk.GC textGC = null;
	private Pango.Layout textLayout = null;

	/* Widgets */
	private TextView textView = null;
	
	public SubtitleEditTextViewMargin (TextView textView) {
		this.textView = textView;
		
		Base.InitFinished += OnBaseInitFinished;
	}
	
	/* Private methods */
	
	public void DrawMargin (TextView textView, Gdk.Window window) {
    	/* Get char count info  */
    	int[,] info;
    	GetCharCountDrawInfo(textView, out info);

    	/* Do some calculations */    	
    	int marginNumbersX = textView.Allocation.Width - this.marginSpace - this.marginNumbersWidth;

    	/* Draw line */
    	int marginLineX = textView.Allocation.Width - this.marginWidth;
    	window.DrawLine(this.lineGC, marginLineX, 0, marginLineX, textView.Allocation.Height);
    	
    	/* Draw background area */
    	window.DrawRectangle(this.bgGC, true, marginLineX+1, 0, this.marginWidth-1, textView.Allocation.Height);
    	
    	/* Draw text */
    	int infoCount = info.GetLength(0);
    	for (int i = 0 ; i < infoCount ; i++) {
    		int charCount = info[i, 0];
    		int y = info[i, 1];
    		
    		String charCountText = (charCount > this.marginMaxCharCount ? this.marginMaxCharCountString : charCount.ToString());
    		this.textLayout.SetText(charCountText);
    		Pango.Rectangle layoutRect = GetPangoLayoutRect(this.textLayout);
    		window.DrawLayout(this.textGC, marginNumbersX, y - layoutRect.Height/2, this.textLayout);
		}
    }

    private void GetCharCountDrawInfo (TextView textView, out int[,] info) {
    	if (textView.Buffer.LineCount == 0) {
    		info = null;
    		return; //shouldn't happen, but just to make sure
    	}
    	
    	/* Get visible coordinates */	
    	int minVisibleY = textView.VisibleRect.Top;
   		int maxVisibleY = textView.VisibleRect.Bottom;
   		
   		/* Get visible start and end iters */
   		TextIter startIter, endIter;
   		int lineTop;
    	textView.GetLineAtY(out startIter, minVisibleY, out lineTop);
    	textView.GetLineAtY(out endIter, maxVisibleY, out lineTop);
    	int lineCount = endIter.Line - startIter.Line + 1;
    	int startLine = startIter.Line;
    	int endLine = endIter.Line;

		/* Initializations */
		info = new int[lineCount, 2];
    	
    	/* Process start iter */
    	int startLineCharCount = startIter.CharsInLine - (lineCount > 1 ? 1 : 0); //subtract 1 for newline if there are >1 lines
    	info[0, 0] = startLineCharCount; //Char Count
    	Gdk.Rectangle startIterLocation = textView.GetIterLocation(startIter);
    	info[0, 1] = startIterLocation.Bottom - (startIterLocation.Height/2) - minVisibleY; //Y
    	
    	/* If only 1 line, return */
    	if (lineCount == 1) {
    		return;
    	}

		/* Process middle iters */
    	for (int i = 1, line = startLine + 1 ; line < endLine ; i++, line++) {
    		TextIter iter = textView.Buffer.GetIterAtLine(line);
			int charCount = iter.CharsInLine - 1; //subtract 1 for newline
			info[i, 0] = charCount;
			Gdk.Rectangle iterLocation = textView.GetIterLocation(iter);
    		info[i, 1] = iterLocation.Bottom - (iterLocation.Height/2) - minVisibleY; //Y
    	}
    	
    	/* Process end iter */
    	int endLineCharCount = endIter.CharsInLine; //don't subtract newline because it's the last line
    	info[lineCount-1, 0] = endLineCharCount;
    	Gdk.Rectangle endIterLocation = textView.GetIterLocation(endIter);
    	info[lineCount-1, 1] = endIterLocation.Bottom - (endIterLocation.Height/2) - minVisibleY; //Y
    }
	   
	private Pango.Rectangle GetPangoLayoutRect (Pango.Layout layout) {
		Pango.Rectangle inkRect, logicalRect;
    	layout.GetPixelExtents(out inkRect, out logicalRect);
    	return logicalRect;
	}
    
    private int CountDigitsInNumber (int number) {
    	return (number == 0 ? 1 : (int)Math.Floor(Math.Log10(number)) + 1); //assuming the number is positive, otherwise would need to use abs() too
    }
    
    private void SetGCs () {
    	this.bgGC = Base.Ui.Window.Style.BackgroundGC(StateType.Normal);
		this.lineGC = Base.Ui.Window.Style.BackgroundGC(StateType.Active);
		this.textGC = Base.Ui.Window.Style.TextGC(StateType.Active);
    }
    
    private void Refresh () {
    	textView.QueueDraw();
    }
   
    	
	/* Event members */
	
	private void OnBaseInitFinished () {
	
		/* GCs */
		SetGCs();
		
		/* Layouts */
		this.textLayout = new Pango.Layout(textView.PangoContext);
		this.textLayout.FontDescription = Pango.FontDescription.FromString("sans 10");
		
		/* Margin char width */
		this.textLayout.SetText("0");
		Pango.Rectangle layoutRect = GetPangoLayoutRect(this.textLayout);
		this.marginCharWidth = layoutRect.Width;
		this.marginNumbersWidth = this.marginDigitCount * this.marginCharWidth;
		this.marginWidth = (this.marginSpace * 2) + this.marginNumbersWidth;
		
			
		/* Events */
		textView.ExposeEvent += OnExposeEvent;
		textView.StyleSet += OnStyleSet; //To update colors if the style is changed
		textView.Parent.ExposeEvent += OnScrolledWindowExposeEvent;
	}
	
	private void OnExposeEvent (object o, ExposeEventArgs args) {
		TextView textView = o as TextView;
		if (textView.State != StateType.Insensitive) {
			DrawMargin(textView, args.Event.Window);
		}
	}
	
	private void OnScrolledWindowExposeEvent (object o, ExposeEventArgs args) {
		Refresh(); //Necessary for artifacts not to appear when scrolling
	}
	
	private void OnStyleSet (object o, StyleSetArgs args) {
		SetGCs();
	}

	
}

}

