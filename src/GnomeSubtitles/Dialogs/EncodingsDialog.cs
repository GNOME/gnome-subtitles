/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007 Pedro Castro
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

public class EncodingsDialog : GladeDialog {
	private string[] chosenNames = new string[0];

	/* Constant strings */
	private const string gladeFilename = "EncodingsDialog.glade";

	/* Constant integers */
	const int descColumnNum = 0; //The number of the Description column
	const int nameColumnNum = 1; //The number of the Name column

	/* Widgets */
	
	[WidgetAttribute] private TreeView availableTreeView;
	[WidgetAttribute] private TreeView shownTreeView;
	[WidgetAttribute] private Button buttonAdd;
	[WidgetAttribute] private Button buttonRemove;


	public EncodingsDialog () : base(gladeFilename) {
		FillAvailableEncodings();
		FillShownEncodings();
		
		ConnectSignals();
	}
	
	/* Public properties */
	
	public string[] ChosenNames {
		get { return chosenNames; }
	}
	
	/* Private members */
	
	private void FillAvailableEncodings () {
		SetColumns(availableTreeView);
		
		ListStore store = new ListStore(typeof(string), typeof(string));
		foreach (EncodingDescription desc in Encodings.All) {
			store.AppendValues(desc.Description, desc.Name);
		}

		SetModel(availableTreeView, store);
	}
	
	private void FillShownEncodings () {
		SetColumns(shownTreeView);
		
		chosenNames = Global.Config.PrefsEncodingsShownInMenu;
		
		ListStore store = new ListStore(typeof(string), typeof(string));
		foreach (string shownEncoding in chosenNames) {
			EncodingDescription desc = new EncodingDescription();
			if (Encodings.Find(shownEncoding, ref desc))
				store.AppendValues(desc.Description, desc.Name);
		}

		SetModel(shownTreeView, store);
	}
	
	private void SetColumns (TreeView tree) {
		TreeViewColumn descriptionColumn = new TreeViewColumn("Description", new CellRendererText(), "text", descColumnNum);
		descriptionColumn.SortColumnId = descColumnNum;
		tree.AppendColumn(descriptionColumn);
		
		TreeViewColumn nameColumn = new TreeViewColumn("Encoding", new CellRendererText(), "text", nameColumnNum);
		nameColumn.SortColumnId = nameColumnNum;
		tree.AppendColumn(nameColumn);
	}
	
	private void SetModel (TreeView tree, ListStore store) {
		TreeModelSort storeSort = new TreeModelSort(store);
		storeSort.SetSortColumnId(descColumnNum, SortType.Ascending);
		tree.Model = storeSort;
	}
	
	private void AddSelectedAvailableEncoding () {
		TreePath path = GetSelectedPath(availableTreeView);
		if (path == null)
			return;
		
		int encodingNumber = Util.PathToInt(path);
		EncodingDescription desc = Encodings.All[encodingNumber];
		
		AddToShownEncodings(desc);	
	}
	
	private void RemoveSelectedShownEncoding () {
		TreeIter iter = new TreeIter();
		if (GetSelectedIter(shownTreeView, ref iter)) {
			GetListStore(shownTreeView).Remove(ref iter);
			UpdateShownEncodingsPrefs();
		}
	}
	
	private void AddToShownEncodings (EncodingDescription desc) {
		if (!IsAlreadyShown(desc.Name)) {
			GetListStore(shownTreeView).AppendValues(desc.Description, desc.Name);
			UpdateShownEncodingsPrefs();
		}
	}
	
	private string[] GetShownNames () {
		ListStore store = GetListStore(shownTreeView);
		int count = store.IterNChildren();

		string[] names = new string[count];
		int rowNumber = 0;
		foreach (object[] row in store) {
			names[rowNumber] = row[nameColumnNum] as string;
			rowNumber++;
		}
		return names;	
	}
	
	private bool IsAlreadyShown (string name) {
		ListStore store = GetListStore(shownTreeView);
		foreach (object[] row in store) {
			if ((row == null) || (row.Length != 2))
				continue;

			string rowName = row[nameColumnNum] as string;
			if (rowName == name)
				return true;
		}
		return false;
	}
	
	private ListStore GetListStore (TreeView tree) {
		return (tree.Model as TreeModelSort).Model as ListStore;
	}
	
	private bool GetSelectedIter (TreeView tree, ref TreeIter iter) {
		TreePath path = GetSelectedPath(tree);
		if (path == null)
			return false;
		
		return GetListStore(tree).GetIter(out iter, path);
	}
	
	private TreePath GetSelectedPath (TreeView tree) {
		TreePath[] paths = tree.Selection.GetSelectedRows();
		if ((paths == null) || (paths.Length != 1))
			return null;

		TreePath selected = paths[0];
		return (tree.Model as TreeModelSort).ConvertPathToChildPath(selected);	
	}
		
	private void UpdateShownEncodingsPrefs () {
		chosenNames = GetShownNames();
		Global.Config.PrefsEncodingsShownInMenu = chosenNames;
	}
	
	
	/* Event related members */
	
	private void ConnectSignals () {
		availableTreeView.Selection.Changed += OnAvailableSelectionChanged;
		shownTreeView.Selection.Changed += OnShownSelectionChanged;
	}
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnAvailableSelectionChanged (object o, EventArgs args) {
		bool sensitive = (availableTreeView.Selection.CountSelectedRows() == 1);
		buttonAdd.Sensitive = sensitive;
	}
	
	private void OnShownSelectionChanged (object o, EventArgs args) {
		bool sensitive = (shownTreeView.Selection.CountSelectedRows() == 1);
		buttonRemove.Sensitive = sensitive;
	}
	
	private void OnResponse (object o, ResponseArgs args) {
		CloseDialog();
	}
	
	private void OnAvailableRowActivated (object o, RowActivatedArgs args) {
		AddSelectedAvailableEncoding();
	}
		
	private void OnAdd (object o, EventArgs args) {
		AddSelectedAvailableEncoding();
	}
	
	private void OnRemove (object o, EventArgs args) {
		RemoveSelectedShownEncoding();
	}
	
	private void OnShownRowActivated (object o, RowActivatedArgs args) {
		RemoveSelectedShownEncoding();
	}
	
}

}
