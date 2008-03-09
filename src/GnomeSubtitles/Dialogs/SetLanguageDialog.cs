/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008 Pedro Castro
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
using SubLib;
using System;

namespace GnomeSubtitles {

public class SetLanguageDialog : GladeDialog {
	private ListStore store = null;
	private int colNum = 0;
	private SubtitleTextType textType;

	/* Constant strings */
	private const string gladeFilename = "SetLanguageDialog.glade";
	
	/* Strings */
	private string dialogTitleText = Catalog.GetString("Set Text Language");
	private string dialogTitleTranslation = Catalog.GetString("Set Translation Language");
	private string introLabelText = Catalog.GetString("Select the text _language of the current subtitles.");
	private string introLabelTranslation = Catalog.GetString("Select the translation _language of the current subtitles.");

	/* Widgets */
	
	[WidgetAttribute] private TreeView languagesTreeView;
	[WidgetAttribute] private Label introLabel;


	public SetLanguageDialog (SubtitleTextType textType) : base(gladeFilename) {
		this.textType = textType;
	
		SetDialogTitle(textType);
		SetIntroLabel(textType);
		FillAvailableLanguages();
		SelectActiveLanguage(textType);
	}

	/* Private members */
	
	private void FillAvailableLanguages () {
		TreeViewColumn col = new TreeViewColumn("col", new CellRendererText(), "text", colNum);
		languagesTreeView.AppendColumn(col);
	
		store = new ListStore(typeof(string));
		foreach (string language in Global.SpellLanguages.Languages) {
			store.AppendValues(language);
		}
		
		languagesTreeView.Model = store;
	}
	
	private void SelectActiveLanguage (SubtitleTextType textType) {
		int count = store.IterNChildren();
		if (count == 0)
			return;
		
		int activeLanguageIndex = GetActiveLanguageIndex(textType, count);
		
		TreePath path = Util.IntToPath(activeLanguageIndex);
		languagesTreeView.ScrollToCell(path, null, true, 0.5f, 0.5f);
   		languagesTreeView.SetCursor(path, null, false);
	}
	
	private int GetActiveLanguageIndex (SubtitleTextType textType, int count) {
		int activeLanguageIndex = Global.SpellLanguages.GetActiveLanguageIndex(textType);
		/* Set active language to the first if invalid */
		if ((activeLanguageIndex == -1) || (activeLanguageIndex >= count))
			activeLanguageIndex = 0;
			
		return activeLanguageIndex;
	}
	
	private void SetSpellLanguage () {
		string activeLanguage = GetSelectedLanguage();
		Global.SpellLanguages.SetActiveLanguage(textType, activeLanguage);
	}
	
	private string GetSelectedLanguage () {
		int count = languagesTreeView.Selection.CountSelectedRows();
		if (count != 1)
			return String.Empty;
			
		TreePath path = GetSelectedPath(languagesTreeView);
		if (path == null)
			return String.Empty;
		
		TreeIter iter;
		languagesTreeView.Model.GetIter(out iter, path);
		return languagesTreeView.Model.GetValue(iter, colNum) as string;
	}
	
	private TreePath GetSelectedPath (TreeView tree) {
		TreePath[] paths = tree.Selection.GetSelectedRows();
		if ((paths == null) || (paths.Length != 1))
			return null;

		TreePath selected = paths[0];
		return selected;
	}
	
	private void SetDialogTitle (SubtitleTextType textType) {
		dialog.Title = (textType == SubtitleTextType.Text ? dialogTitleText : dialogTitleTranslation);
	}
	
	private void SetIntroLabel (SubtitleTextType textType) {
		introLabel.TextWithMnemonic = (textType == SubtitleTextType.Text ? introLabelText : introLabelTranslation);
	}
	
	/* Event handlers */

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok)
			SetSpellLanguage();
	
		Close();
	}
	
	private void OnLanguageRowActivated (object o, RowActivatedArgs args) {
		SetSpellLanguage();
		Close();
	}


}

}
