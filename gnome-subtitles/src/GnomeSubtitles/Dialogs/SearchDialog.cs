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

using Glade;
using Gtk;
using System;
using SubLib;

namespace GnomeSubtitles {

internal enum SearchDialogResponse { Find = 1, Replace, ReplaceAll, Close };

public class SearchDialog : GladeDialog {

	/* TODO: are these really needed? */
	private Subtitles subtitles = null;
	private SubtitleView subtitleView = null;
	private SubtitleEdit subtitleEdit = null;
	
	/* Constant strings */
	private const string dialogName = "searchDialog";

	/* Widgets */
	
	[WidgetAttribute]
	private Entry searchForEntry;
	/*[WidgetAttribute]
	private Entry replaceWithEntry;*/
	[WidgetAttribute]
	private CheckButton matchCaseCheckButton;	
	/*[WidgetAttribute]
	private CheckButton searchBackwardsCheckButton;*/
	[WidgetAttribute]
	private CheckButton wrapAroundCheckButton;

	public SearchDialog (bool showReplace) : base(dialogName) {
		 subtitles = Global.Subtitles;
		 subtitleView = Global.GUI.View;
		 subtitleEdit = Global.GUI.Edit;
		 
		 SetTextToFind();
	}
	
	//if text is selected, uses it. if not, uses history.
	private void SetTextToFind () {
		string selection = subtitleEdit.TextSelection;
		if (selection != String.Empty) {
			TextToFind = selection;				
		}
		SelectSearchForText();
	}
	
	private bool MatchCase {
		get { return matchCaseCheckButton.Active; }
	}
	/*
	private bool SearchBackwards {
		get { return searchBackwardsCheckButton.Active; }
	}*/
	
	private bool WrapAround {
		get { return wrapAroundCheckButton.Active; }
	}
	
	private string TextToFind {
		get { return searchForEntry.Text; }
		set { searchForEntry.Text = value; }
	}
	/*
	private string TextToReplace {
		get { return replaceWithEntry.Text; }
		set { replaceWithEntry.Text = value; }
	}*/
	
	private void SelectSearchForText () {
		searchForEntry.SelectRegion(0, TextToFind.Length);
	}
	
	private void GetPositions (out int subtitle, out int index) {
		subtitle = 0;
		index = 0;
		
		if (subtitleView.Selection.Count > 0) {
			subtitle = Util.PathToInt(subtitleView.Selection.FirstPath);
			if (subtitleEdit.TextIsFocus)
				index = subtitleEdit.TextCursorIndex;
		}
	}

	private void Find () {
		int startSubtitle, startIndex;
		GetPositions(out startSubtitle, out startIndex);
		
		int foundIndex;
		int foundSubtitle = subtitles.Find(TextToFind, startSubtitle, startIndex, WrapAround,
			MatchCase, out foundIndex);
		
		System.Console.WriteLine("found: " + foundSubtitle + " " + foundIndex);

		/*if (!SearchBackwards) {
			subtitles.Find(TextToFind, 
			subtitles.find
		}*/
	
	}
	
	/* Private members */
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		SearchDialogResponse response = (SearchDialogResponse)args.ResponseId;
		switch (response) {
			case SearchDialogResponse.Find:
				System.Console.WriteLine("Find");
				Find();
				break;
			case SearchDialogResponse.Replace:
				System.Console.WriteLine("Replace");
				break;
			case SearchDialogResponse.ReplaceAll:
				System.Console.WriteLine("ReplaceAll");
				break;
			case SearchDialogResponse.Close:
				System.Console.WriteLine("Close");
				CloseDialog();
				break;
		}
	}

}

}