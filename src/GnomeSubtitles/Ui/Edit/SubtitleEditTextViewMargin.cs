/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2011-2017 Pedro Castro
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

using Cairo;
using GnomeSubtitles.Core;
using Gtk;
using System;

namespace GnomeSubtitles.Ui.Edit {

public class SubtitleEditTextViewMargin {
	/* Constants */
	private const int marginSpace = 4; //pixels
	private const int marginMinDigits = 2; //the minimum number of digits for margin width (1 would make the margin adjust with more than 9 chars, so it's better to keep a minimum of 2 to avoid constant adjustment

	/* Set on base init */
	private Gdk.RGBA marginBGColor;
	private Gdk.RGBA marginLineColor;
	private Gdk.RGBA marginTextColor;
	private Pango.Layout textLayout;

	/* Widgets */
	private TextView textView = null;

	private int marginCharWidth = -1; //pixels
	private int marginDigitCount = 2;

	public SubtitleEditTextViewMargin (TextView textView) {
		this.textView = textView;

		Base.InitFinished += OnBaseInitFinished;
	}

	/* Private methods */

	public void DrawMargin (TextView textView, Context cr) {

    	/* Get char count info  */
    	int[,] info;
    	GetCharCountDrawInfo(textView, out info);

    	/* Set margin and window on the right */
    	int marginWidth = (marginSpace * 2) + (this.marginDigitCount * this.marginCharWidth);
    	textView.SetBorderWindowSize(TextWindowType.Right, marginWidth);

		/* Get margin window and cairo context */
		Gdk.Window marginWindow = textView.GetWindow(TextWindowType.Right);
		Cairo.Context marginCR = Gdk.CairoHelper.Create(marginWindow);

		/* Clear the background */
		Gdk.CairoHelper.SetSourceRgba(marginCR, marginBGColor);
		marginCR.Paint();

    	/* Draw line */
		Gdk.CairoHelper.SetSourceRgba(marginCR, marginLineColor);
		marginCR.MoveTo(0, 0);
		marginCR.LineTo(0, marginWindow.Height); //We don't use marginWindow.Height because sometimes (e.g., on initialization) its height is 1px
		marginCR.Stroke();

    	/* Draw text */
    	int infoCount = info.GetLength(0);
    	for (int i = 0 ; i < infoCount ; i++) {
    		int charCount = info[i, 0];
    		int y = info[i, 1];

    		this.textLayout.SetText(charCount.ToString());
    		int textLayoutWidth, textLayoutHeight;
    		this.textLayout.GetPixelSize(out textLayoutWidth, out textLayoutHeight);
			marginCR.MoveTo(marginSpace, y - textLayoutHeight / 2);

			Gdk.CairoHelper.SetSourceRgba(marginCR, marginTextColor);
			Pango.CairoHelper.ShowLayout(marginCR, this.textLayout);
		}

		marginCR.Dispose();
		cr.Dispose();
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
    	int endLineCharCount = endIter.CharsInLine - (endLine == textView.Buffer.LineCount - 1 ? 0 : 1); //subtract newline if this isn't the last line in the buffer
    	info[lineCount-1, 0] = endLineCharCount;
    	Gdk.Rectangle endIterLocation = textView.GetIterLocation(endIter);
    	info[lineCount-1, 1] = endIterLocation.Bottom - (endIterLocation.Height/2) - minVisibleY; //Y
    }

	private int CalcDigitCount (TextBuffer buffer, int marginMinDigits) {
		int maxChars = -1;
		int lineCount = buffer.LineCount;
		for (int line = 0 ; line < lineCount; line++) {
			TextIter iter = buffer.GetIterAtLine(line);
			int chars = iter.CharsInLine - (line == lineCount - 1 ? 0 : 1); //Subtract 1 for newline (except for the last line)
			if (chars > maxChars) {
				maxChars = chars;
			}
		}

		int digitCount = CountDigitsInNumber(maxChars);
		return Math.Max(digitCount, marginMinDigits);
	}

    private int CountDigitsInNumber (int number) {
    	return (number == 0 ? 1 : (int)Math.Floor(Math.Log10(number)) + 1); //assuming the number is positive, otherwise would need to use abs() too
    }

    private void Refresh () {
    	textView.QueueDraw();
    }

    private void HideMarginWindow () {
    	this.textView.SetBorderWindowSize(TextWindowType.Right, 0);
    }

    private void Enable () {
		textView.Drawn += OnWidgetDrawn;
		textView.Buffer.Changed += OnBufferChanged; //To calculate margin digit count (based on the largest line char count)
		textView.StateChanged += OnStateChanged;

		Refresh();
    }

    private void Disable () {
		textView.Drawn -= OnWidgetDrawn;
		textView.Buffer.Changed -= OnBufferChanged; //To calculate margin digit count (based on the largest line char count)
		textView.StateChanged -= OnStateChanged;

		HideMarginWindow();
    }

	private void SetColors () {
		marginTextColor = Base.Ui.Window.StyleContext.GetColor(StateFlags.Active);
		marginBGColor = Base.Ui.Window.StyleContext.GetBackgroundColor(StateFlags.Active);
		marginLineColor = marginBGColor;
		marginLineColor.Red -= (marginLineColor.Red <= 0.1 ? 0 : 0.1);
		marginLineColor.Green -= (marginLineColor.Green <= 0.1 ? 0 : 0.1);
		marginLineColor.Blue -= (marginLineColor.Blue <= 0.1 ? 0 : 0.1);
	}


	/* Event members */

	private void OnWidgetDrawn (object o, DrawnArgs args) {
		TextView textView = o as TextView;
		if (textView.State != StateType.Insensitive) {
			DrawMargin(textView, args.Cr);
		}
	}

	private void OnBaseInitFinished () {
		/* Colors */
		SetColors();

		/* Layout */
		this.textLayout = new Pango.Layout(textView.PangoContext);
		this.textLayout.FontDescription = Pango.FontDescription.FromString("sans 10");

		/* Margin char width */
		this.textLayout.SetText("8"); //To calculate a character's width
		int marginCharHeight;
		this.textLayout.GetPixelSize(out this.marginCharWidth, out marginCharHeight);

		/* Events */
		textView.StyleSet += OnStyleSet; //To update colors if the style is changed
		(Base.Ui.Menus.GetMenuItem(WidgetNames.ViewLineLengths) as CheckMenuItem).Toggled += OnViewLineLengthsToggled;
		if (Base.Config.ViewLineLengths) {
			Enable();
		}
	}

	private void OnBufferChanged (object o, EventArgs args) {
		this.marginDigitCount = CalcDigitCount(o as TextBuffer, marginMinDigits);
	}

	private void OnStyleSet (object o, StyleSetArgs args) {
		SetColors();
	}

	private void OnStateChanged (object o, StateChangedArgs args) {
		TextView textView = o as TextView;
		if (textView.State == StateType.Insensitive) {
			HideMarginWindow();
		}
	}

	private void OnViewLineLengthsToggled (object o, EventArgs args) {
    	CheckMenuItem menuItem = o as CheckMenuItem;
    	if (menuItem.Active)
    		Enable();
    	else
    		Disable();
	}


}

}

