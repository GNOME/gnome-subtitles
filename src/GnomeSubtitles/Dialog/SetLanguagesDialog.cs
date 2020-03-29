/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008-2020 Pedro Castro
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
using SubLib.Core.Domain;

namespace GnomeSubtitles.Dialog {

public class SetLanguagesDialog : BaseDialog {

	/* Widgets */
	private TreeView textTreeView = null;
	private TreeView transTreeView = null;


	public SetLanguagesDialog () : base() {
		Init(BuildDialog());
	}

	private Gtk.Dialog BuildDialog () {
		Gtk.Dialog dialog = new Gtk.Dialog(Catalog.GetString("Set Languages"), Base.Ui.Window, DialogFlags.Modal | DialogFlagsUseHeaderBar,
			Util.GetStockLabel("gtk-cancel"), ResponseType.Cancel, Util.GetStockLabel("gtk-apply"), ResponseType.Ok);

		dialog.DefaultResponse = ResponseType.Ok;
		dialog.DefaultWidth = WidgetStyles.DialogWidthMedium;
		dialog.DefaultHeight = WidgetStyles.DialogHeightMedium;

		Grid grid = new Grid();
		grid.RowSpacing = WidgetStyles.RowSpacingMedium;
		grid.ColumnSpacing = WidgetStyles.ColumnSpacingLarge;
		grid.BorderWidth = WidgetStyles.BorderWidthMedium;
		grid.ColumnHomogeneous = true;

		/* Left part: Text Language */

		Label textLabel = new Label("<b>" + Catalog.GetString("Text Language") + "</b>");
		textLabel.UseMarkup = true;
		textLabel.Halign = Align.Start;
		grid.Attach(textLabel, 0, 0, 1, 1);

		textTreeView = CreateTreeView();
		ScrolledWindow textScrolledWindow = CreateLanguagesScrolledWindow(textTreeView);
		SelectActiveLanguage(textTreeView, SubtitleTextType.Text);
		grid.Attach(textScrolledWindow, 0, 1, 1, 1);

		/* Right part: Translation Language */

		Label transLabel = new Label("<b>" + Catalog.GetString("Translation Language") + "</b>");
		transLabel.UseMarkup = true;
		transLabel.Halign = Align.Start;
		grid.Attach(transLabel, 1, 0, 1, 1);

		transTreeView = CreateTreeView();
		ScrolledWindow transScrolledWindow = CreateLanguagesScrolledWindow(transTreeView);
		if (Base.Document.IsTranslationLoaded) {
			SelectActiveLanguage(transTreeView, SubtitleTextType.Translation);
		} else {
			transScrolledWindow.Sensitive = false;
		}
		grid.Attach(transScrolledWindow, 1, 1, 1, 1);
		
		
		/* Bottom: info message */
		string providers = string.Join(", ", Base.SpellLanguages.Providers);
		Label bottomLabel = Util.CreateLabel("<i>" + string.Format(Catalog.GetString("Use your distro's package manager to install additional languages (tip: search for 'spell'). Supported language packs: {0}."), providers) + "</i>", 0, 0);
		bottomLabel.UseMarkup = true;
		bottomLabel.Wrap = true;
		grid.Attach(bottomLabel, 0, 2, 2, 1);

		dialog.ContentArea.Add(grid);
		dialog.ContentArea.ShowAll();

		ConnectSignals();

		return dialog;
	}

	private TreeView CreateTreeView () {
		TreeView treeView = new TreeView();
		treeView.HeadersVisible = false;

		TreeViewColumn col = new TreeViewColumn("col", new CellRendererText(), "text", 0);
		treeView.AppendColumn(col);

		treeView.Model = CreateLanguagesListStore();
		return treeView;
	}

	private ScrolledWindow CreateLanguagesScrolledWindow (TreeView treeView) {
		ScrolledWindow scrolledWindow = new ScrolledWindow();
		scrolledWindow.ShadowType = ShadowType.EtchedIn;
		scrolledWindow.Expand = true;
		scrolledWindow.Add(treeView);
		return scrolledWindow;
	}

	private ListStore CreateLanguagesListStore () {
		ListStore store = new ListStore(typeof(string));

		foreach (SpellLanguage language in Base.SpellLanguages.Languages) {
			store.AppendValues(language.Name);
		}

		return store;
	}

	private void SelectActiveLanguage (TreeView treeView, SubtitleTextType textType) {
		int count = treeView.Model.IterNChildren();
		if (count == 0)
			return;

		int activeLanguageIndex = GetActiveLanguageIndex(textType, count);

		TreePath path = Core.Util.IntToPath(activeLanguageIndex);
		treeView.ScrollToCell(path, null, true, 0.5f, 0.5f);
		treeView.SetCursor(path, null, false);
	}

	private int GetActiveLanguageIndex (SubtitleTextType textType, int count) {
		int activeLanguageIndex = Base.SpellLanguages.GetActiveLanguageIndex(textType);
		/* Set active language to the first if invalid */
		if ((activeLanguageIndex == -1) || (activeLanguageIndex >= count))
			activeLanguageIndex = 0;

		return activeLanguageIndex;
	}

	private void SetSpellLanguages () {
		int selectedTextLanguageIndex = GetSelectedLanguageIndex(textTreeView);
		Base.SpellLanguages.SetActiveLanguageIndex(SubtitleTextType.Text, selectedTextLanguageIndex);

		if (transTreeView.Sensitive) {
			int selectedTransLanguageIndex = GetSelectedLanguageIndex(transTreeView);
			Base.SpellLanguages.SetActiveLanguageIndex(SubtitleTextType.Translation, selectedTransLanguageIndex);
		}
	}

	private int GetSelectedLanguageIndex (TreeView treeView) {
		int count = treeView.Selection.CountSelectedRows();
		if (count != 1)
			return -1;

		TreePath path = GetSelectedPath(treeView);
		if (path == null)
			return -1;

		return Core.Util.PathToInt(path);
	}

	private TreePath GetSelectedPath (TreeView tree) {
		TreePath[] paths = tree.Selection.GetSelectedRows();
		if ((paths == null) || (paths.Length != 1))
			return null;

		TreePath selected = paths[0];
		return selected;
	}

	/* Event members */

	private void ConnectSignals () {
		textTreeView.RowActivated += OnLanguageRowActivated;
		transTreeView.RowActivated += OnLanguageRowActivated;
	}

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			SetSpellLanguages();
			SetReturnValue(true);
		}
		return false;
	}

	private void OnLanguageRowActivated (object o, RowActivatedArgs args) {
		SetSpellLanguages();
		SetReturnValue(true);
		Destroy();
	}


}

}