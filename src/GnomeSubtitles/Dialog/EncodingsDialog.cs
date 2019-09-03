/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2019 Pedro Castro
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

namespace GnomeSubtitles.Dialog {

public class EncodingsDialog : BaseDialog {
	private string[] chosenCodes = new string[0];

	/* Constant integers */
	const int RegionColumnNum = 0; //The number of the Region column
	const int NameColumnNum = 1; //The number of the Name column
	const int CodeColumnNum = 2; //The number of the hidden Code column

	/* Widgets */

	private TreeView availableTreeView;
	private TreeView shownTreeView;
	private Button buttonAdd;
	private Button buttonRemove;

	public EncodingsDialog (Window parent) : base() {
		base.Init(BuildDialog(parent));
	}

	/* Public properties */

	public string[] ChosenCodes {
		get { return chosenCodes; }
	}

	/* Private members */

	private Gtk.Dialog BuildDialog (Window parent) {
		Gtk.Dialog dialog = new Gtk.Dialog(Catalog.GetString("Character Encodings"), parent, DialogFlags.Modal | DialogFlagsUseHeaderBar);

		dialog.DefaultWidth = WidgetStyles.DialogWidthMedium;
		dialog.DefaultHeight = WidgetStyles.DialogHeightMedium;

		Grid grid = new Grid();
		grid.RowSpacing = WidgetStyles.RowSpacingMedium;
		grid.ColumnSpacing = WidgetStyles.ColumnSpacingLarge;
		grid.BorderWidth = WidgetStyles.BorderWidthMedium;
		grid.ColumnHomogeneous = true;

		/* Left part: Available VBox */

		Label availLabel = new Label("<b>" + Catalog.GetString("Available Encodings") + "</b>");
		availLabel.UseMarkup = true;
		availLabel.Halign = Align.Start;
		grid.Attach(availLabel, 0, 0, 1, 1);

		ScrolledWindow availScrolledWindow = new ScrolledWindow();
		availScrolledWindow.ShadowType = ShadowType.EtchedIn;
		availScrolledWindow.Expand = true;
		availableTreeView = new TreeView();
		availScrolledWindow.Add(availableTreeView);
		grid.Attach(availScrolledWindow, 0, 1, 1, 1);

		buttonAdd = new Button("gtk-add");
		grid.Attach(buttonAdd, 0, 2, 1, 1);

		/* Right part: Shown VBox */

		Label shownLabel = new Label("<b>" + Catalog.GetString("Chosen Encodings") + "</b>");
		shownLabel.UseMarkup = true;
		shownLabel.Halign = Align.Start;
		grid.Attach(shownLabel, 1, 0, 1, 1);

		ScrolledWindow shownScrolledWindow = new ScrolledWindow();
		shownScrolledWindow.ShadowType = ShadowType.EtchedIn;
		shownScrolledWindow.Expand = true;
		shownTreeView = new TreeView();
		shownScrolledWindow.Add(shownTreeView);
		grid.Attach(shownScrolledWindow, 1, 1, 1, 1);

		buttonRemove = new Button("gtk-remove");
		grid.Attach(buttonRemove, 1, 2, 1, 1);

		FillAvailableEncodings();
		FillShownEncodings();

		dialog.ContentArea.Add(grid);
		dialog.ContentArea.ShowAll();

		ConnectSignals();

		return dialog;
	}

	private void FillAvailableEncodings () {
		SetColumns(availableTreeView);

		ListStore store = new ListStore(typeof(string), typeof(string), typeof(string));
		foreach (EncodingDescription desc in Encodings.All) {
			store.AppendValues(desc.Region, desc.Name, desc.Code);
		}

		SetModel(availableTreeView, store);
	}

	private void FillShownEncodings () {
		SetColumns(shownTreeView);

		chosenCodes = Base.Config.FileEncodingsShownInMenu;

		ListStore store = new ListStore(typeof(string), typeof(string), typeof(string));
		foreach (string shownEncodingCode in chosenCodes) {
			EncodingDescription desc = EncodingDescription.Empty;
			if (Encodings.Find(shownEncodingCode, ref desc)) {
				store.AppendValues(desc.Region, desc.Name, desc.Code);
			}
		}

		SetModel(shownTreeView, store);
	}

	private void SetColumns (TreeView tree) {
		TreeViewColumn descriptionColumn = new TreeViewColumn(Catalog.GetString("Description"), new CellRendererText(), "text", RegionColumnNum);
		descriptionColumn.SortColumnId = RegionColumnNum;
		tree.AppendColumn(descriptionColumn);

		TreeViewColumn nameColumn = new TreeViewColumn(Catalog.GetString("Encoding"), new CellRendererText(), "text", NameColumnNum);
		nameColumn.SortColumnId = NameColumnNum;
		tree.AppendColumn(nameColumn);
		
		TreeViewColumn codeColumn = new TreeViewColumn("-", new CellRendererText(), "text", CodeColumnNum);
		codeColumn.Visible = false;
		tree.AppendColumn(codeColumn);
	}

	private void SetModel (TreeView tree, ListStore store) {
		TreeModelSort storeSort = new TreeModelSort(store);
		storeSort.SetSortColumnId(RegionColumnNum, SortType.Ascending);
		tree.Model = storeSort;
	}

	private void AddSelectedAvailableEncoding () {
		TreePath path = GetSelectedPath(availableTreeView);
		if (path == null)
			return;

		int encodingNumber = Core.Util.PathToInt(path);
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
		if (!IsAlreadyShown(desc.Code)) {
			GetListStore(shownTreeView).AppendValues(desc.Region, desc.Name, desc.Code);
			UpdateShownEncodingsPrefs();
		}
	}

	private string[] GetShownCodes () {
		ListStore store = GetListStore(shownTreeView);
		int count = store.IterNChildren();

		string[] codes = new string[count];
		int rowNumber = 0;
		foreach (object[] row in store) {
			codes[rowNumber] = row[CodeColumnNum] as string;
			rowNumber++;
		}
		return codes;
	}

	private bool IsAlreadyShown (string code) {
		ListStore store = GetListStore(shownTreeView);
		foreach (object[] row in store) {
			if ((row == null) || (row.Length < CodeColumnNum + 1)) {
				continue;
			}

			string rowCode = row[CodeColumnNum] as string;
			if (rowCode == code) {
				return true;
			}
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
		chosenCodes = GetShownCodes();
		Base.Config.FileEncodingsShownInMenu = chosenCodes;
	}


	/* Event related members */

	private void ConnectSignals () {
		availableTreeView.Selection.Changed += OnAvailableSelectionChanged;
		availableTreeView.RowActivated += OnAvailableRowActivated;
		buttonAdd.Clicked += OnAdd;

		shownTreeView.Selection.Changed += OnShownSelectionChanged;
		shownTreeView.RowActivated += OnShownRowActivated;
		buttonRemove.Clicked += OnRemove;
	}

	private void OnAvailableSelectionChanged (object o, EventArgs args) {
		bool sensitive = (availableTreeView.Selection.CountSelectedRows() == 1);
		buttonAdd.Sensitive = sensitive;
	}

	private void OnShownSelectionChanged (object o, EventArgs args) {
		bool sensitive = (shownTreeView.Selection.CountSelectedRows() == 1);
		buttonRemove.Sensitive = sensitive;
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