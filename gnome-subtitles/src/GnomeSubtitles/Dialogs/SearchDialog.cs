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
using System.Text.RegularExpressions;

namespace GnomeSubtitles {

internal enum SearchDialogResponse { Find = 1, Replace, ReplaceAll, Close };

//TODO validate entry when using a regular expression (seems to be working as it is, though)
//TODO check cases when dialog is opened and a change from find to replace is required
public class SearchDialog : GladeDialog {
	private string text = String.Empty;	//The text to search for
	private Regex regex = null; 		//The regex that corresponds to the text and the options
	private bool valuesCanDiffer = false; //Whether the values of the dialog might have been changed since the last Find

	private bool matchCase = false;
	private bool backwards = false;
	private bool useRegex = false;
	private bool wrap = true;

	/* Constant strings */
	private const string dialogName = "searchDialog";

	/* Widgets */
	
	[WidgetAttribute] private Entry findEntry;
	[WidgetAttribute] private Entry replaceEntry;
	[WidgetAttribute] private Label replaceLabel;
	[WidgetAttribute] private Table table;
	
	[WidgetAttribute] private CheckButton matchCaseCheckButton;	
	[WidgetAttribute] private CheckButton backwardsCheckButton;
	[WidgetAttribute] private CheckButton regexCheckButton;
	[WidgetAttribute] private CheckButton wrapCheckButton;
	
	[WidgetAttribute] private Button buttonClose;
	[WidgetAttribute] private Button buttonReplaceAll;
	[WidgetAttribute] private Button buttonReplace;
	[WidgetAttribute] private Button buttonFind;

	public SearchDialog () : base(dialogName) {
	}
	
	public bool ShowReplace {
		set {
			if (value == true) {
				dialog.Title = "Replace";
				table.RowSpacing = 12;
			}
			else {
				dialog.Title = "Find";
				table.RowSpacing = 0;
			}
		
			replaceEntry.Visible = value;
			replaceLabel.Visible = value;
		
			buttonReplaceAll.Visible = value;
			buttonReplace.Visible = value;
		}
	}
	
	public Regex Regex {
		get { return regex; }
	}
	
	public bool MatchCase {
		get { return matchCaseCheckButton.Active; }
	}

	public bool Backwards {
		get { return backwardsCheckButton.Active; }
	}
	
	public bool UseRegex {
		get { return regexCheckButton.Active; }
	}
	
	public bool Wrap {
		get { return wrapCheckButton.Active; }
	}
	
	public override void ShowDialog() { //TODO add ShowReplace here?
		LoadDialogValues();
		base.ShowDialog();
	}
	
	
	/* Private properties */
		
	private bool ValuesHaveChanged {
		get {
			if (text != findEntry.Text)
				return true;
			if (matchCase != matchCaseCheckButton.Active)
				return true;
			if (backwards != backwardsCheckButton.Active)
				return true;
			if (useRegex != regexCheckButton.Active)
				return true;
			if (wrap != wrapCheckButton.Active)
				return true;
			
			return false;
		}
	}
	
	/* Private methods */
	
	private void LoadDialogValues () {
		SetFindEntryText();
		matchCaseCheckButton.Active = matchCase;
		backwardsCheckButton.Active = backwards;
		regexCheckButton.Active = useRegex;
		wrapCheckButton.Active = wrap;
	}
	
	/// <summary>Sets the text in the Find entry.</summary>
	/// <remarks>Updating the text will select it and grab the focus to the entry.</remarks>
	private void SetFindEntryText () {
		string currentSelection = Global.GUI.Edit.TextSelection;
		string textToUse = (currentSelection != String.Empty ? currentSelection : text);
		findEntry.Text = textToUse;
		findEntry.SelectRegion(0, textToUse.Length);
		findEntry.GrabFocus();
	}

	private void SaveDialogValues () {
		text = findEntry.Text;
		matchCase = matchCaseCheckButton.Active;
		backwards = backwardsCheckButton.Active;
		useRegex = regexCheckButton.Active;
		wrap = wrapCheckButton.Active;	
	}
	
	private void Find () {
		bool updateRegex = (valuesCanDiffer && ValuesHaveChanged);
		SaveDialogValues();
		if (updateRegex)
			UpdateRegex();

		if (backwards)
			Global.GUI.View.Search.FindPrevious();
		else
			Global.GUI.View.Search.FindNext();
	}
	
	private void UpdateRegex() {
		RegexOptions options = RegexOptions.Singleline;
		if (!matchCase)
			options |= RegexOptions.IgnoreCase;
		if (backwards)
			options |= RegexOptions.RightToLeft;
		
		string regexText = (useRegex ? text : Regex.Escape(text));
		regex = new Regex(regexText, options);		
	}

	/* Event members */
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		SearchDialogResponse response = (SearchDialogResponse)args.ResponseId;
		switch (response) {
			case SearchDialogResponse.Find:
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
				HideDialog();
				break;
		}
	}

	private void OnFindTextChanged (object o, EventArgs args) {
		valuesCanDiffer = true;
		if (findEntry.Text.Length == 0)
			buttonFind.Sensitive = false;
		else
			buttonFind.Sensitive = true;
	}
	
	private void OnReplaceTextChanged (object o, EventArgs args) {
		valuesCanDiffer = true;
	}
	
	private void OnMatchCaseToggled (object o, EventArgs args) {
		valuesCanDiffer = true;
	}
	
	private void OnBackwardsToggled (object o, EventArgs args) {
		valuesCanDiffer = true;
	}

	private void OnUseRegexToggled (object o, EventArgs args) {
		valuesCanDiffer = true;
	}
	
	private void OnWrapToggled (object o, EventArgs args) {
		valuesCanDiffer = true;
	}	

}

}