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

using Glade;
using Gtk;
using Mono.Unix;
using System;
using System.Text.RegularExpressions;

namespace GnomeSubtitles.Dialog {

internal enum SearchDialogResponse { Find = 1, Replace, ReplaceAll, Close = -6 };

public class SearchDialog : GladeDialog {
	private string text = String.Empty;		//The text to search for
	private Regex forwardRegex = null;		//The regex that corresponds to the text and the options
	private Regex backwardRegex = null;		//The regex that corresponds to the text and the options
	private bool valuesMayHaveChanged = false;	//Whether the values of the dialog may have been changed since the last Find

	private bool matchCase = false;
	private bool backwards = false;
	private bool useRegex = false;
	private bool wrap = true;

	/* Constant strings */
	private const string gladeFilename = "SearchDialog.glade";

	/* Widgets */
	
	[WidgetAttribute] private Entry findEntry = null;
	[WidgetAttribute] private Entry replaceEntry = null;
	[WidgetAttribute] private Label replaceLabel = null;
	[WidgetAttribute] private Table table = null;
	
	[WidgetAttribute] private CheckButton matchCaseCheckButton = null;	
	[WidgetAttribute] private CheckButton backwardsCheckButton = null;
	[WidgetAttribute] private CheckButton regexCheckButton = null;
	[WidgetAttribute] private CheckButton wrapCheckButton = null;
	
	[WidgetAttribute] private Button buttonReplaceAll = null;
	[WidgetAttribute] private Button buttonReplace = null;
	[WidgetAttribute] private Button buttonFind = null;

	public SearchDialog () : base(gladeFilename, true, true) {
	}
	
	public bool ShowReplace2 {
		set {
			if (value == true) {
				dialog.Title = Catalog.GetString("Replace");
				table.RowSpacing = 12;
			}
			else {
				dialog.Title = Catalog.GetString("Find");
				table.RowSpacing = 0;
			}
		
			replaceEntry.Visible = value;
			replaceLabel.Visible = value;
		
			buttonReplaceAll.Visible = value;
			buttonReplace.Visible = value;
		}
	}
	
	public Regex ForwardRegex {
		get { return forwardRegex; }
	}
	
	public Regex BackwardRegex {
		get { return backwardRegex; }
	}
	
	public string Replacement {
		get { return replaceEntry.Text; }
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
	
	public override void Show () {
		Show(false);
	}
	
	public void Show (bool useReplace) {
		if (useReplace) {
			dialog.Title = Catalog.GetString("Replace");
			table.RowSpacing = 12;
		}
		else {
			dialog.Title = Catalog.GetString("Find");
			table.RowSpacing = 0;
		}
		
		replaceEntry.Visible = useReplace;
		replaceLabel.Visible = useReplace;
		
		buttonReplaceAll.Visible = useReplace;
		buttonReplace.Visible = useReplace;
		
		LoadDialogValues();
		base.Show();
	}
	
	
	/* Private properties */
		
	private bool ValuesHaveChanged {
		get {
			if (!valuesMayHaveChanged)
				return false;
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
		string currentSelection = Global.GUI.Edit.SelectedTextContent;
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
	
	private void HandleValuesChange () {
		bool updateRegex = ValuesHaveChanged; //Need to be before SaveDialogValues, as the values will be changed
		SaveDialogValues();
		if (updateRegex)
			UpdateRegex();
	
	}
	
	private void Find () {
		HandleValuesChange();
		
		bool found = Global.GUI.View.Search.Find();
		if (found)
			buttonReplace.Sensitive = true;
	}
	
	private void Replace () {
		HandleValuesChange();
		
		bool foundNext = Global.GUI.View.Search.Replace();
		if (!foundNext) //No other text was found to replace, after replacing this one
			buttonReplace.Sensitive = false;
	}
	
	private void ReplaceAll () {
		HandleValuesChange();
		
		Global.GUI.View.Search.ReplaceAll();
	}
	
	private void UpdateRegex() {
		RegexOptions options = RegexOptions.Singleline;
		if (!matchCase)
			options |= RegexOptions.IgnoreCase;
				
		string regexText = (useRegex ? text : Regex.Escape(text));
		forwardRegex = new Regex(regexText, options);
		backwardRegex = new Regex(regexText, options | RegexOptions.RightToLeft);
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
				Replace();
				break;
			case SearchDialogResponse.ReplaceAll:
				ReplaceAll();
				break;
			case SearchDialogResponse.Close:
				Hide();
				break;
		}
	}

	private void OnFindTextChanged (object o, EventArgs args) {
		valuesMayHaveChanged = true;

		if (findEntry.Text.Length == 0) { //No text in the entry
			buttonFind.Sensitive = false;
			buttonReplace.Sensitive = false;
			buttonReplaceAll.Sensitive = false;
		}
		else {
			buttonFind.Sensitive = true;
			buttonReplaceAll.Sensitive = true;
		}
	}
	
	private void OnReplaceTextChanged (object o, EventArgs args) {
		valuesMayHaveChanged = true;
	}
	
	private void OnMatchCaseToggled (object o, EventArgs args) {
		valuesMayHaveChanged = true;
	}
	
	private void OnBackwardsToggled (object o, EventArgs args) {
		valuesMayHaveChanged = true;
	}

	private void OnUseRegexToggled (object o, EventArgs args) {
		valuesMayHaveChanged = true;
	}
	
	private void OnWrapToggled (object o, EventArgs args) {
		valuesMayHaveChanged = true;
	}	

}

}
