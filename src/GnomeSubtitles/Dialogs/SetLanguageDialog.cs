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
using System;

namespace GnomeSubtitles {

public class SetLanguageDialog : GladeDialog {
	private ListStore store = null;
	private int colNum = 0;

	/* Constant strings */
	private const string gladeFilename = "SetLanguageDialog.glade";

	/* Widgets */
	
	[WidgetAttribute] private TreeView languagesTreeView;


	public SetLanguageDialog () : base(gladeFilename) {
		FillAvailableLanguages();
		SelectActiveLanguage();
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
	
	private void SelectActiveLanguage () {
		int count = store.IterNChildren();
		if (count == 0)
			return;
		
		int activeLanguageIndex = GetActiveLanguageIndex(count);
		
		
		TreePath path = Util.IntToPath(activeLanguageIndex);
		languagesTreeView.ScrollToCell(path, null, true, 0.5f, 0.5f);
   		languagesTreeView.SetCursor(path, null, false);
	}
	
	private int GetActiveLanguageIndex (int count) {
		int activeLanguageIndex = Global.SpellLanguages.ActiveLanguageIndex;
		/* Set active language to the first if invalid */
		if ((activeLanguageIndex == -1) || (activeLanguageIndex >= count))
			activeLanguageIndex = 0;
			
		return activeLanguageIndex;
	}
	
	private void SetSpellLanguage () {
		string activeLanguage = GetSelectedLanguage();
		Global.SpellLanguages.ActiveLanguage = activeLanguage;
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
	
	/* Event handlers */

	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok)
			SetSpellLanguage();
	
		Close();
	}


}

}
