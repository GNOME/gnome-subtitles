/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2018 Pedro Castro
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
using GnomeSubtitles.Ui;
using Gtk;
using Mono.Unix;
using System;
using System.Text.RegularExpressions;

namespace GnomeSubtitles.Dialog {

internal enum SearchDialogResponse { Find = 1, Replace, ReplaceAll };

public class SearchDialog : BaseDialog {
	private string text = String.Empty;		//The text to search for
	private Regex forwardRegex = null;		//The regex that corresponds to the text and the options
	private Regex backwardRegex = null;		//The regex that corresponds to the text and the options
	private bool valuesMayHaveChanged = false;	//Whether the values of the dialog may have been changed since the last Find

	private bool matchCase = false;
	private bool backwards = false;
	private bool useRegex = false;
	private bool wrap = true;

	/* Widgets */

	private Entry findEntry = null;
	private Entry replaceEntry = null;
	private Label replaceLabel = null;

	private CheckButton matchCaseCheckButton = null;
	private CheckButton backwardsCheckButton = null;
	private CheckButton regexCheckButton = null;
	private CheckButton wrapCheckButton = null;

	private Button buttonReplaceAll = null;
	private Button buttonReplace = null;
	private Button buttonFind = null;

	public SearchDialog () : base() {
		base.Init(BuildDialog());
	}

	/* Overridden members */

	public override DialogScope Scope {
		get { return DialogScope.Document; }
	}

	/* Properties */

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

	/* Methods */

	public override void Show () {
		Show(false);
	}

	public void Show (bool useReplace) {
		Dialog.Title = (useReplace ? Catalog.GetString("Replace") : Catalog.GetString("Find"));

		replaceEntry.Visible = useReplace;
		replaceLabel.Visible = useReplace;

		buttonReplaceAll.Visible = useReplace;
		buttonReplace.Visible = useReplace;

		LoadDialogValues();
		base.Show();
	}

	/* Private methods */

	private Gtk.Dialog BuildDialog() {
		Gtk.Dialog dialog = new Gtk.Dialog();
		dialog.Resizable = false; //This way it automatically resizes according to its content in the 2 display modes: find / replace
		
		//Content area

		Grid grid = new Grid();

		grid.BorderWidth = WidgetStyles.BorderWidthLarge;
		grid.ColumnSpacing = WidgetStyles.ColumnSpacingLarge;
		grid.RowSpacing = WidgetStyles.RowSpacingLarge;

		Label findLabel = new Label(Catalog.GetString("F_ind"));
		findLabel.SetAlignment(1, 0.5f);
		grid.Attach(findLabel, 0, 0, 1, 1);

		findEntry = new Entry();
		findEntry.Changed += OnFindTextChanged;
		findEntry.ActivatesDefault = true;
		grid.Attach(findEntry, 1, 0, 2, 1);

		replaceLabel = new Label(Catalog.GetString("Replace _with"));
		replaceLabel.SetAlignment(1, 0.5f);
		grid.Attach(replaceLabel, 0, 1, 1, 1);

		replaceEntry = new Entry();
		replaceEntry.Changed += OnReplaceTextChanged;
		replaceEntry.ActivatesDefault = true;
		grid.Attach(replaceEntry, 1, 1, 2, 1);

		matchCaseCheckButton = new CheckButton(Catalog.GetString("_Match case"));
		matchCaseCheckButton.Toggled += OnMatchCaseToggled;
		grid.Attach(matchCaseCheckButton, 1, 2, 1, 1);

		backwardsCheckButton = new CheckButton(Catalog.GetString("Search _backwards"));
		backwardsCheckButton.Toggled += OnBackwardsToggled;
		grid.Attach(backwardsCheckButton, 2, 2, 1, 1);

		regexCheckButton = new CheckButton(Catalog.GetString("Regular _expression"));
		regexCheckButton.Toggled += OnUseRegexToggled;
		grid.Attach(regexCheckButton, 1, 3, 1, 1);

		wrapCheckButton = new CheckButton(Catalog.GetString("Wra_p around"));
		wrapCheckButton.Toggled += OnWrapToggled;
		grid.Attach(wrapCheckButton, 2, 3, 1, 1);

		dialog.ContentArea.Add(grid);
		dialog.ContentArea.ShowAll();

		//Action area
		
		buttonReplaceAll = dialog.AddButton(Catalog.GetString("Replace _All"), (int)SearchDialogResponse.ReplaceAll) as Button;
		buttonReplaceAll.Sensitive = false;
		
		buttonReplace = dialog.AddButton(Catalog.GetString("_Replace"), (int)SearchDialogResponse.Replace) as Button;
		buttonReplace.Sensitive = false;
		
		buttonFind = dialog.AddButton(Util.GetStockLabel("gtk-find"), (int)SearchDialogResponse.Find) as Button;
		buttonFind.Sensitive = false;

		dialog.DefaultResponse = (ResponseType)SearchDialogResponse.Find;

		return dialog;
	}

	private bool ValuesHaveChanged () {
		if (!valuesMayHaveChanged) {
			return false;
		}
		
		return (text != findEntry.Text)
		|| (matchCase != matchCaseCheckButton.Active)
		|| (backwards != backwardsCheckButton.Active)
		|| (useRegex != regexCheckButton.Active)
		|| (wrap != wrapCheckButton.Active);
	}

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
		string currentSelection = Core.Base.Ui.Edit.SelectedTextContent;
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
		bool updateRegex = ValuesHaveChanged(); //Need to be before SaveDialogValues, as the values will be changed
		SaveDialogValues();
		if (updateRegex) {
			UpdateRegex();
		}
	}

	private void Find () {
		HandleValuesChange();

		bool found = Core.Base.Ui.View.Search.Find();
		if (found) {
			buttonReplace.Sensitive = true;
		}
	}

	private void Replace () {
		HandleValuesChange();

		bool foundNext = Core.Base.Ui.View.Search.Replace();
		if (!foundNext) { //No other text was found to replace, after replacing this one
			buttonReplace.Sensitive = false;
		}
	}

	private void ReplaceAll () {
		HandleValuesChange();

		Core.Base.Ui.View.Search.ReplaceAll();
	}

	private void UpdateRegex() {
		RegexOptions options = RegexOptions.Singleline;
		if (!matchCase) {
			options |= RegexOptions.IgnoreCase;
		}

		string regexText = (useRegex ? text : Regex.Escape(text));
		forwardRegex = new Regex(regexText, options);
		backwardRegex = new Regex(regexText, options | RegexOptions.RightToLeft);
	}

	/* Event members */

	protected override bool ProcessResponse (ResponseType response) {
		SearchDialogResponse searchResponse = (SearchDialogResponse)response;
		switch (searchResponse) {
			case SearchDialogResponse.Find:
				Find();
				return true;
			case SearchDialogResponse.Replace:
				Replace();
				return true;
			case SearchDialogResponse.ReplaceAll:
				ReplaceAll();
				return true;
			default:
				return false;
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
