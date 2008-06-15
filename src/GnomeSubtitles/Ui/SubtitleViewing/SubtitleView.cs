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

using Gtk;
using Mono.Unix;
using System;
using SubLib;

namespace GnomeSubtitles.Ui.SubtitleView {

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
		tree = Global.GetWidget(WidgetNames.SubtitleView) as TreeView;
		selection = new SubtitleSelection(tree);
		search = new Search();

		CreateColumns();
    }

	/* Public properties */

    public SubtitleSelection Selection {
    	get { return selection; }
    }
    
    public Search Search {
    	get { return search; }
    }
    
    /* Public methods */ 
    
    /// <summary>Used in a blank startup. A blank startup refers to when no document is loaded.</summary>
    public void BlankStartUp () {
    	tree.Model = new ListStore(typeof(Subtitle));
    }
    
	public void UpdateFromNewDocument (bool wasLoaded) {
    	if (!wasLoaded)
    		tree.Sensitive = true;
    	else {
    		search.Clear();
    		SetTranslationVisible(false);
    	}

    	Load(Global.Document.Subtitles);
    }
    
    public void UpdateFromNewTranslationDocument () {
    	SetTranslationVisible(true);
    	Refresh();
    }
    
    public void UpdateFromCloseTranslation () {
    	SetTranslationVisible(false);
    }
    
	public void UpdateFromTimingMode (TimingMode mode) {
		Refresh();
	}
	
	/// <summary>Instructs the <see cref="TreeView" /> to redraw a row.</summary>
	/// <remarks>This is useful when a row changes its width, for instance.</remarks>
	public void RedrawPath (TreePath path) {
		subtitles.Model.EmitRowChanged(path, TreeIter.Zero);
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
	
	/// <summary>Inserts a subtitle in the specified path.</summary>
	public void Insert (Subtitle subtitle, TreePath path) {
		int index = Util.PathToInt(path);
		subtitles.Add(subtitle, index);
		selection.Select(path, true, true);	
	}
	
	/// <summary>Inserts a collection of subtitles in the specified paths.</summary>
	/// <param name="subtitles">The subtitles to insert.</param>
	/// <param name="paths">The paths corresponding to each of the subtitles. There must be the same number of paths as there are for subtitles.</param>
	/// <param name="focus">The tree that should be given the focus.</param>
	public void Insert (Subtitle[] subtitles, TreePath[] paths, TreePath focus) {
		for (int index = 0 ; index < subtitles.Length ; index++) {
			Subtitle subtitle = subtitles[index];
			TreePath path = paths[index];
			int pathIndex = Util.PathToInt(path);
			this.subtitles.Add(subtitle, pathIndex);
		}
		selection.Select(paths, focus, true);
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

    
    /* Private members */
    
    /// <summary>Loads the subtitles</summary>
    private void Load (Subtitles subtitles) {
   		this.subtitles = subtitles;
   		tree.Model = subtitles.Model;
    	Refresh();
    }
	
    private void CreateColumns() {
    	numberCol = CreateColumn(Catalog.GetString("No."), ColumnWidth("000"), new CellRendererText(), RenderNumberCell);
    		
    	int timeWidth = ColumnWidth("00:00:00.000");
    	startCol = CreateColumn(Catalog.GetString("From"), timeWidth, new CellRendererText(), RenderStartCell);
    	endCol = CreateColumn(Catalog.GetString("To"), timeWidth, new CellRendererText(), RenderEndCell);
    	durationCol = CreateColumn(Catalog.GetString("During"), timeWidth, new CellRendererText(), RenderDurationCell);
    	
    	int textWidth = ColumnWidth("0123456789012345678901234567890123456789");
    	textCol = CreateColumn(Catalog.GetString("Text"), textWidth, new CellRendererCenteredText(), RenderTextCell);
    	translationCol = CreateColumn(Catalog.GetString("Translation"), textWidth, new CellRendererCenteredText(), RenderTranslationCell);
    	SetTranslationVisible(false);

    	tree.AppendColumn(numberCol);
    	tree.AppendColumn(startCol);
    	tree.AppendColumn(endCol);
    	tree.AppendColumn(durationCol);
    	tree.AppendColumn(textCol);
    	tree.AppendColumn(translationCol);
    	tree.AppendColumn(new TreeViewColumn()); //Appending to leave empty space to the right 
    }

	private TreeViewColumn CreateColumn (string title, int width, CellRenderer cell, TreeCellDataFunc dataFunction) {
		cell.Xalign = 0.5f;
		cell.Yalign = 0;
		TreeViewColumn column = new TreeViewColumn();
		column.Alignment = 0.5f;
		column.Title = title;
		column.FixedWidth = width;
		column.Sizing = TreeViewColumnSizing.Fixed;
		column.Resizable = true;
		column.PackStart(cell, true);
		column.SetCellDataFunc(cell, dataFunction);
		return column;
	}

	private int ColumnWidth (string text) {
		const int margins = 10;
		return Util.TextWidth(tree, text, margins);
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

	private void RenderNumberCell (TreeViewColumn column, CellRenderer cell, TreeModel treeModel, TreeIter iter) {
		(cell as CellRendererText).Text = (Util.PathToInt(treeModel.GetPath(iter)) + 1).ToString();
	}

	private void RenderStartCell (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
		CellRendererText renderer = cell as CellRendererText;
		if (Global.TimingModeIsFrames)
			renderer.Text = subtitles[iter].Frames.Start.ToString();
		else
			renderer.Text = Util.TimeSpanToText(subtitles[iter].Times.Start);
	}
	
	private void RenderEndCell (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
		CellRendererText renderer = cell as CellRendererText;
		if (Global.TimingModeIsFrames)
			renderer.Text = subtitles[iter].Frames.End.ToString();
		else
			renderer.Text = Util.TimeSpanToText(subtitles[iter].Times.End);
	}
	
	private void RenderDurationCell (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
		CellRendererText renderer = cell as CellRendererText;
		if (Global.TimingModeIsFrames)
			renderer.Text = subtitles[iter].Frames.Duration.ToString();
		else
			renderer.Text = Util.TimeSpanToText(subtitles[iter].Times.Duration);
	}
	
	private void RenderTextCell (TreeViewColumn column, CellRenderer cell, TreeModel treeModel, TreeIter iter) {
		Subtitle subtitle = subtitles[iter];
		CellRendererText renderer = cell as CellRendererText;		
		renderer.Text = subtitle.Text.GetReplaceEmptyLines(" "); //Replace empty lines with a space so Pango doesn't complain
		SetRendererStyle(renderer, subtitle);
	}
	
	private void RenderTranslationCell (TreeViewColumn column, CellRenderer cell, TreeModel treeModel, TreeIter iter) {
		Subtitle subtitle = subtitles[iter];
		CellRendererText renderer = cell as CellRendererText;		
		renderer.Text = subtitle.Translation.GetReplaceEmptyLines(" "); //Replace empty lines with a space so Pango doesn't complain
		SetRendererStyle(renderer, subtitle);		
	}
	
	private void SetRendererStyle (CellRendererText renderer, Subtitle subtitle) {
		if (subtitle.Style.Italic)
			renderer.Style = Pango.Style.Italic;
		else
			renderer.Style = Pango.Style.Normal;
			
		if (subtitle.Style.Bold)
			renderer.Weight = (int)Pango.Weight.Bold;
		else
			renderer.Weight = (int)Pango.Weight.Normal;

		if (subtitle.Style.Underline)
			renderer.Underline = Pango.Underline.Single;
		else
			renderer.Underline = Pango.Underline.None;
	}
		
}

}
