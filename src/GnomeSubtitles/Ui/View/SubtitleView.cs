/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2019 Pedro Castro
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
using Gtk;
using Mono.Unix;
using System;
using SubLib.Core.Domain;

namespace GnomeSubtitles.Ui.View {

/* Delegates */
public delegate void SubtitleCountChangedHandler (int count);

public class SubtitleView {
	private Subtitles subtitles = null; //A reference to Global.Subtitles, kept here because it's often accessed by CellRenderers.

	private TreeView tree = null;
	private SubtitleSelection selection = null;
	private Search search = null;

	private TreeViewColumn numberCol = null;
	private TreeViewColumn startCol = null;
	private TreeViewColumn endCol = null;
	private TreeViewColumn durationCol = null;
	private TreeViewColumn textCol = null;
	private TreeViewColumn translationCol = null;


	public SubtitleView() {
		tree = Base.GetWidget(WidgetNames.SubtitleView) as TreeView;
		selection = new SubtitleSelection(tree);
		search = new Search();

		CreateColumns();
		SetEmptyModel();

		Base.InitFinished += OnBaseInitFinished;
    }

	/* Events */

	public event SubtitleCountChangedHandler SubtitleCountChanged;

	/* Public properties */

    public SubtitleSelection Selection {
    	get { return selection; }
    }

    public Search Search {
    	get { return search; }
    }

    /* Public methods */

	public void SetAutoSelectSubtitles (bool active) {
		if (active)
			Base.Ui.Video.Tracker.SubtitlePulse += OnSubtitlePulse;
		else
			Base.Ui.Video.Tracker.SubtitlePulse -= OnSubtitlePulse;
	}


	/// <summary>Instructs the <see cref="TreeView" /> to redraw a row.</summary>
	/// <remarks>This is useful when a row changes its width, for instance.</remarks>
	public void RedrawPath (TreePath path) {
		subtitles.Model.EmitRowChanged(path, TreeIter.Zero);
	}

	public void RedrawPaths (TreePath[] paths) {
		foreach (TreePath path in paths)
			RedrawPath(path);
	}

	/// <summary>Refreshes the view.</summary>
	/// <remarks>This is currently limited to a <see cref="TreeView.QueueDraw()" />.</remarks>
    public void Refresh () {
	    tree.QueueDraw();
    }

	/// <summary>Inserts a new subtitle after the specified <see cref="TreePath" />.</summary>
	/// <param name="path">The path after which the new subtitle will be inserted.</param>
	/// <remarks>The new subtitle's timings are based on the subtitle at the specified path.</remarks>
	/// <returns>Whether insertion was successful.</returns>
	public bool InsertNewAfter (TreePath path) {
		int index = Util.PathToInt(path);
		if (!subtitles.Collection.Contains(index)) //The index isn't valid
			return false;

		subtitles.AddNewAfter(index);
		TreePath newPath = Util.PathNext(path);
		selection.Select(newPath, true, true);
		return true;
	}

	/// <summary>Inserts a new subtitle before the specified <see cref="TreePath" />.</summary>
	/// <param name="path">The path before which the new subtitle will be inserted.</param>
	/// <remarks>The new subtitle's timings are based on the subtitle at the specified path.</remarks>
	/// <returns>Whether insertion was successful.</returns>
	public bool InsertNewBefore (TreePath path) {
		int index = Util.PathToInt(path);
		if (!subtitles.Collection.Contains(index)) //The index isn't valid
			return false;

		subtitles.AddNewBefore(index);
		selection.Select(path, true, true); //The path now points to the new subtitle
		return true;
	}

	/// <summary>Inserts a new subtitle at the specified <see cref="TreePath" />.</summary>
	/// <param name="path">The path at which the new subtitle will be inserted.</param>
	/// <returns>Whether insertion was successful.</returns>
	public bool InsertNewAt (TreePath path) {
		int index = Util.PathToInt(path);
		if (!(subtitles.Collection.Contains(index) || (index == subtitles.Count)))
			return false;

		subtitles.AddNewAt(index);
		selection.Select(path, true, true);
		return true;
	}

	/// <summary>Inserts a new subtitle at the specified <see cref="TreePath" />, with the specified start time.</summary>
	/// <param name="path">The path at which the new subtitle will be inserted.</param>
	/// <param name="start">The time at which the new subtitle should start.</para>
	/// <returns>Whether insertion was successful.</returns>
	public bool InsertNewAt (TreePath path, TimeSpan start) {  //TODO merge nicely with existing subtitles (subtract times accordingly)
		int index = Util.PathToInt(path);
		if (!(subtitles.Collection.Contains(index) || (index == subtitles.Count)))
			return false;

		subtitles.AddNewAt(index, start);
		selection.Select(path, true, true);
		return true;
	}

	/// <summary>Inserts a subtitle in the specified path.</summary>
	public void Insert (Subtitle subtitle, TreePath path) {
		int index = Util.PathToInt(path);
		subtitles.Add(subtitle, index);
		selection.Select(path, true, true);
	}

	/// <summary>Inserts a collection of subtitles in the specified paths.</summary>
	/// <param name="subtitles">The subtitles to insert.</param>
	/// <param name="paths">The paths corresponding to each of the subtitles. There must be the same number of paths as there are for subtitles.</param>
	/// <param name="focus">The path that should be given the focus.</param>
	public void Insert (Subtitle[] subtitles, TreePath[] paths, TreePath focus) {
		for (int index = 0 ; index < subtitles.Length ; index++) {
			Subtitle subtitle = subtitles[index];
			TreePath path = paths[index];
			int pathIndex = Util.PathToInt(path);
			this.subtitles.Add(subtitle, pathIndex);
		}
		selection.Select(paths, focus, true);
	}

	public void Insert (Subtitle[] subtitles, TreePath firstPath, TreePath focus) {
		for (int index = 0, pathIndex = Util.PathToInt(firstPath) ; index < subtitles.Length  ; index++, pathIndex++) {
			Subtitle subtitle = subtitles[index];
			this.subtitles.Add(subtitle, pathIndex);
		}
		TreePath lastPath = Util.IntToPath(Util.PathToInt(firstPath) + subtitles.Length - 1);
		TreePath[] pathRange = new TreePath[]{firstPath, lastPath};
		selection.SelectRange(pathRange, focus, true);
	}

	/// <summary>Removes the subtitle at the specified path and selects the next or previous subtitle afterwards.</summary>
	/// <param name="path">The path to remove.</param>
	/// <param name="selectNext">Whether to select the next path after deletion, if possible.
	/// Otherwise, the previous will be selected, if possible.</param>
	/// <returns>Whether removal succeeded.</returns>
	public bool Remove (TreePath path, bool selectNext) {
		int index = Util.PathToInt(path);
		if (!subtitles.Remove(index))
			return false; // The subtitle could not be removed

		int indexToSelect = FindNearIndex(index, selectNext);
		if (indexToSelect != -1) {
			TreePath pathToSelect = Util.IntToPath(indexToSelect);
			selection.Select(pathToSelect, true, true);
		}
		return true;
	}

	/// <summary>Removes the subtitles at the specified paths and selects the next subtitle afterwards.</summary>
	/// <param name="paths">The paths to remove.</param>
	/// <returns>Whether removal succeeded.</returns>
	public bool Remove (TreePath[] paths) {
		return Remove(paths, true);
	}

	/// <summary>Removes the subtitles at the specified paths and selects the next or previous subtitle afterwards.</summary>
	/// <param name="paths">The paths to remove.</param>
	/// <param name="selectNext">Whether to select the next path after deletion, if possible.
	/// Otherwise, the previous will be selected, if possible.</param>
	/// <returns>Whether removal succeeded.</returns>
	public bool Remove (TreePath[] paths, bool selectNext) {
		if ((paths == null) || (paths.Length == 0))
			return true;

		if (!subtitles.Remove(paths))
			return false; // The subtitles could not be removed

		int firstIndex = Util.PathToInt(paths[0]);
		int indexToSelect = FindNearIndex(firstIndex, selectNext);
		if (indexToSelect != -1) {
			TreePath pathToSelect = Util.IntToPath(indexToSelect);
			selection.Select(pathToSelect, true, true);
		}
		return true;
	}

	/// <summary>Selects the subtitle below of current selected</summary>
	public bool SelectNextSubtitle () {
		selection.SelectNext();
		return true;
	}

    /* Private members */

    /// <summary>Loads the subtitles</summary>
    private void Load (Subtitles subtitles) {
   		this.subtitles = subtitles;
   		tree.Model = subtitles.Model;
    	Refresh();

    	tree.Model.RowInserted += OnModelRowInserted;
    	tree.Model.RowDeleted += OnModelRowDeleted;

		EmitSubtitleCountChangedEvent();
    }

    private void CreateColumns() {
    	/* Number column */
    	numberCol = Util.CreateTreeViewColumn(Catalog.GetString("No."), Util.ColumnWidth(tree, "0000"), new CellRendererText(), RenderNumberCell);

    	/* Start, end and duration columns */
    	int timeWidth = Util.ColumnWidthForTimeValue(tree);
    	startCol = Util.CreateTreeViewColumn(Catalog.GetString("Start"), timeWidth, new CellRendererText(), RenderStartCell);
    	endCol = Util.CreateTreeViewColumn(Catalog.GetString("End"), timeWidth, new CellRendererText(), RenderEndCell);
    	durationCol = Util.CreateTreeViewColumn(Catalog.GetString("Duration"), timeWidth, new CellRendererText(), RenderDurationCell);

    	/* Text column */
    	int textWidth = Util.ColumnWidth(tree, "0123456789012345678901234567890123456789");
    	CellRendererText textCellRenderer = new CellRendererText();
    	textCellRenderer.Alignment = Pango.Alignment.Center;
    	textCol = Util.CreateTreeViewColumn(Catalog.GetString("Text"), textWidth, textCellRenderer, RenderSubtitleTextCell);

    	/* Translation column */
    	CellRendererText translationCellRenderer = new CellRendererText();
    	translationCellRenderer.Alignment = Pango.Alignment.Center;
    	translationCol = Util.CreateTreeViewColumn(Catalog.GetString("Translation"), textWidth, translationCellRenderer, RenderTranslationTextCell);
    	SetTranslationVisible(false);

    	tree.AppendColumn(numberCol);
    	tree.AppendColumn(startCol);
    	tree.AppendColumn(endCol);
    	tree.AppendColumn(durationCol);
    	tree.AppendColumn(textCol);
    	tree.AppendColumn(translationCol);
    	tree.AppendColumn(new TreeViewColumn()); //Appending to leave empty space to the right
    }

	private void SetTranslationVisible (bool visible) {
		translationCol.Visible = visible;
	}


	/// <summary>Finds the index that's near a row that has been removed at the specified index.</summary>
	/// <param name="removedIndex">The index of the removed row.</param>
	/// <param name="findNext">Whether to find the next or the previous row.</param>
	/// <returns>The found index, or -1 if there are now remaining indexes.</returns>
	private int FindNearIndex (int removedIndex, bool findNext) {
		if (subtitles.Count == 0)
			return -1;

		if (findNext)
			return (subtitles.Collection.Contains(removedIndex) ? removedIndex : subtitles.Count - 1);
		else
			return (removedIndex == 0 ? removedIndex : removedIndex - 1);
	}

	/* Cell Renderers */

	private void RenderNumberCell (TreeViewColumn column, CellRenderer cell, ITreeModel treeModel, TreeIter iter) {
		(cell as CellRendererText).Text = (Util.PathToInt(treeModel.GetPath(iter)) + 1).ToString();
	}

	private void RenderStartCell (TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
		CellRendererText renderer = cell as CellRendererText;
		if (Base.TimingModeIsFrames)
			renderer.Text = subtitles[iter].Frames.Start.ToString();
		else
			renderer.Text = Util.TimeSpanToText(subtitles[iter].Times.Start);
	}

	private void RenderEndCell (TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
		CellRendererText renderer = cell as CellRendererText;
		if (Base.TimingModeIsFrames)
			renderer.Text = subtitles[iter].Frames.End.ToString();
		else
			renderer.Text = Util.TimeSpanToText(subtitles[iter].Times.End);
	}

	private void RenderDurationCell (TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
		CellRendererText renderer = cell as CellRendererText;
		if (Base.TimingModeIsFrames)
			renderer.Text = subtitles[iter].Frames.Duration.ToString();
		else
			renderer.Text = Util.TimeSpanToText(subtitles[iter].Times.Duration);
	}

	private void RenderSubtitleTextCell (TreeViewColumn column, CellRenderer cell, ITreeModel treeModel, TreeIter iter) {
		Subtitle subtitle = subtitles[iter];
		RenderTextCell(cell as CellRendererText, iter, subtitle.Text, subtitle.Style);
	}

	private void RenderTranslationTextCell (TreeViewColumn column, CellRenderer cell, ITreeModel treeModel, TreeIter iter) {
		Subtitle subtitle = subtitles[iter];
		if (subtitle.HasTranslation) {
			RenderTextCell(cell as CellRendererText, iter, subtitle.Translation, subtitle.Style);
		} else {
			RenderEmptyTextCell(cell as CellRendererText);
		}
	}
	
	private void RenderTextCell (CellRendererText renderer, TreeIter iter, SubtitleText subtitleText, SubLib.Core.Domain.Style subtitleStyle) {

		/* If there's no text, return empty text without line count */
		if (subtitleText.IsEmpty) {
			RenderEmptyTextCell(renderer);
			return;
		}

		string textMarkup = String.Empty;
		string stylePrefix = String.Empty;
		string styleSuffix = String.Empty;
		GetStyleMarkup(subtitleStyle, ref stylePrefix, ref styleSuffix);

		bool first = true;
		bool viewLineLengths = Base.Config.ViewLineLengths;
		foreach (string line in subtitleText) {
			textMarkup += (first ? String.Empty : "\n") + stylePrefix + GLib.Markup.EscapeText(line) + styleSuffix + (viewLineLengths ? " <span size=\"small\"><sup>(" + line.Length + ")</sup></span>" : String.Empty);
			if (first)
				first = false;
		}

		renderer.Markup = textMarkup;
	}
	
	private void RenderEmptyTextCell (CellRendererText renderer) {
		renderer.Text = String.Empty;
	}

	private void GetStyleMarkup (SubLib.Core.Domain.Style subtitleStyle, ref string prefix, ref string suffix) {
		if (subtitleStyle.Italic) {
			prefix += "<i>";
			suffix = "</i>" + suffix;
		}

		if (subtitleStyle.Bold) {
			prefix += "<b>";
			suffix = "</b>" + suffix;
		}

		if (subtitleStyle.Underline) {
			prefix += "<u>";
			suffix = "</u>" + suffix;
		}
	}


	/* Event members */

	private void OnBaseInitFinished () {
		Base.DocumentLoaded += OnBaseDocumentLoaded;
		Base.DocumentUnloaded += OnBaseDocumentUnloaded;
		Base.TranslationLoaded += OnBaseTranslationLoaded;
		Base.TranslationUnloaded += OnBaseTranslationUnloaded;
		Base.TimingModeChanged += OnBaseTimingModeChanged;

		(Base.Ui.Menus.GetMenuItem(WidgetNames.ViewLineLengths) as CheckMenuItem).Toggled += OnViewLineLengthsToggled;
	}

	private void OnBaseDocumentLoaded (Document document) {
   		tree.Sensitive = true;
    	Load(document.Subtitles);
    }

    private void OnBaseDocumentUnloaded (Document document) {
    	if (document == null)
    		return;

    	tree.Sensitive = false;
    	search.Clear();
   		SetTranslationVisible(false);
   		SetEmptyModel();

    	tree.Model.RowInserted -= OnModelRowInserted;
		tree.Model.RowDeleted -= OnModelRowDeleted;
    }

	private void OnModelRowInserted (object o, RowInsertedArgs args) {
		EmitSubtitleCountChangedEvent();
	}

	private void OnModelRowDeleted (object o, RowDeletedArgs args) {
		EmitSubtitleCountChangedEvent();
	}


    private void OnBaseTranslationLoaded () {
    	SetTranslationVisible(true);
    	Refresh();
    }

    private void OnBaseTranslationUnloaded () {
    	SetTranslationVisible(false);
    }

    private void OnBaseTimingModeChanged (TimingMode timingMode) {
    	Refresh();
    }

	private void EmitSubtitleCountChangedEvent () {
		if (SubtitleCountChanged != null)
			SubtitleCountChanged(subtitles.Count);
	}

	private void OnSubtitlePulse (int subtitleIndex) {
		selection.Select(subtitleIndex, false, false, true);
	}

    private void SetEmptyModel () {
    	tree.Model = new ListStore(typeof(Subtitle));
    }

    private void OnViewLineLengthsToggled (object o, EventArgs args) {
    	Refresh();
	}

}

}
