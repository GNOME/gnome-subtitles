/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2011 Pedro Castro
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
using System;
using SubLib.Core.Domain;

namespace GnomeSubtitles.Ui.View {

/* Delegates */
public delegate void SubtitleSelectionChangedHandler (TreePath[] paths, Subtitle subtitle);

public class SubtitleSelection {

	private TreeView tree = null;
	private TreeSelection selection = null;
	//TODO use this subtitle, and don't store it in individual widgets
	private Subtitle subtitle = null; //The selected subtitle, if only 1 row is selected

	public SubtitleSelection (TreeView tree) {
		this.tree = tree;
		this.selection = tree.Selection;

		selection.Mode = SelectionMode.Multiple;

		Base.InitFinished += OnBaseInitFinished;
	}


	/* Events */

	public SubtitleSelectionChangedHandler Changed;

	/* Public properties */

    /// <summary>An array containing the selected paths.</summary>
    public TreePath[] Paths {
    	get { return selection.GetSelectedRows(); }
    }

    /// <summary>The first selected path, or null if no path is selected.</summary>
    public TreePath FirstPath {
    	get {
    		TreePath[] paths = Paths;
    		if (paths.Length == 0)
    			return null;
    		else
    			return paths[0];
    	}
    }

    /// <summary>The last selected path, or null if no path is selected.</summary>
    public TreePath LastPath {
    	get {
    		TreePath[] paths = Paths;
    		int pathCount = paths.Length;
    		if (pathCount == 0)
    			return null;
    		else
    			return paths[pathCount - 1];
    	}
    }

    /// <summary>The selected path.</summary>
    /// <remarks>If there is more than one path selected, null is returned.</remarks>
	public TreePath Path {
    	get {
    		TreePath[] paths = Paths;
    		if (paths.Length == 1)
    			return paths[0];
    		else
    			return null;
    	}
    }

	/// <summary>The range of selected subtitles, an array with 2 positions containing the first and last paths of the selection.</summary>
	/// <remarks>If only 1 subtitle is selected, the range will start and end on that subtitle. If the selection is empty, null is returned.</remarks>
	public TreePath[] Range {
		get {
			TreePath[] paths = Paths;
    		if (paths.Length == 0)
	    		return null;
    		else {
    			TreePath[] range = new TreePath[2];
    			range[0] = paths[0];
    			range[1] = paths[paths.Length - 1];
    			return range;
    		}
    	}
	}

	/// <summary>The range of paths, starting at the first path (even if not selected), and ending at the last selected path.</summary>
	public TreePath[] PathsToFirst {
		get {
			TreePath lastPath = LastPath;
			if (lastPath == null)
				return null;

			TreePath[] range = new TreePath[2];
    		range[0] = TreePath.NewFirst();
    		range[1] = lastPath;
    		return range;
		}
	}

	/// <summary>The range of paths, starting at the first selected path, and ending at the last path (even if not selected).</summary>
	public TreePath[] PathsToLast {
		get {
			TreePath firstPath = FirstPath;
			if (firstPath == null)
				return null;

			TreePath[] range = new TreePath[2];
    		range[0] = firstPath;

    		int count = Base.Document.Subtitles.Count;
    		range[1] = Util.IntToPath(count - 1);
    		return range;
		}
	}

    public Subtitle Subtitle {
    	get { return subtitle; }
    }

    /// <summary>The selected subtitle. If there is more than one selected, the first is returned.</summary>
    public Subtitle FirstSubtitle {
    	get {
    		TreePath path = this.FirstPath;
    		if (path != null)
    			return Base.Document.Subtitles[path];
    		else
    			return null;
    	}
    }

	/// <summary>The last selected subtitle. If there is only one selected, the first is returned.</summary>
    public Subtitle LastSubtitle {
    	get {
    		TreePath path = this.LastPath;
    		if (path != null)
    			return Base.Document.Subtitles[path];
    		else
    			return null;
    	}
    }

    /// <summary>The number of selected paths.</summary>
    public int Count {
    	get { return selection.CountSelectedRows(); }
    }

    /// <summary>The selected path that currently has the focus.<summary>
    /// <remarks>If none of the selected paths have focus, the <see cref="FirstPath" /> is returned. The first path,
    /// on the other hand, can be null if no path is selected. If there isn't a focused row, null is returned.</remarks>
    public TreePath Focus {
    	get {
    		if (Count == 0)
    			return null;

    		TreePath path;
    		TreeViewColumn column;
    		tree.GetCursor(out path, out column);
			if (selection.PathIsSelected(path))
				return path;
			else
    			return FirstPath;
    	}

    }

    /* Public methods */

	/// <summary>Selects the specified paths and possibly gives focus to the specified path.</summary>
	/// <param name="paths">The paths to select. If it is null or has zero length, all subtitles will be unselected.</param>
	/// <param name="focus">The path to give input focus to. It must be one of the specified paths or null, in which case no focus will be given.</param>
	/// <param name="align">Whether to align the selected path to the center if the path isn't visible and scrolling is needed.</param>
	/// <returns>Whether selection was successful.</returns>
	public bool Select (TreePath[] paths, TreePath focus, bool align) {
		if ((paths == null) || (paths.Length == 0)) {
   			UnselectAll();
   			return true;
   		}
   		Select(paths, SelectionType.Simple, focus, align);
   		return true;
	}

	/// <summary>Selects a range of paths and possibly gives focus to the specified path.</summary>
	/// <param name="paths">The range of paths, with length 2 containing the first and last paths of the range to select.
	/// If it is null or has zero length, all subtitles will be unselected.</param>
	/// <param name="focus">The path to give input focus to. It must be one of the specified paths or null, in which case no focus will be given.</param>
	/// <param name="align">Whether to align the selected path to the center if the path isn't visible and scrolling is needed.</param>
	/// <returns>Whether selection was successful.</returns>
	public bool SelectRange (TreePath[] paths, TreePath focus, bool align) {
		if ((paths == null) || (paths.Length == 0)) {
   			UnselectAll();
   			return true;
   		}
   		else if (paths.Length != 2)
   			return false;
   		else {
   			Select(paths, SelectionType.Range, focus, align);
   			return true;
   		}
	}

    /// <summary>Selects the specified path, possibly aligning it to the center and/or reselecting it.</summary>
    /// <param name="path">The path to select. If it's null, all paths will be unselected.</param>
    /// <param name="align">Whether to align the selected path to the center if the path isn't visible and scrolling is needed.</param>
    /// <param name="reselect">Whether to reselect the path if it's already the only selected path.</param>
    /// <param name="checkAllPaths">If multiple paths are selected and reselect isn't required, check if the current path is contained in the selection.
    /// If the subtitle is within the selection but we want to guarantee that only that single subtitle is selected, set this to false.</param>
    /// <remarks>The input focus will be placed on the path.</remarks>
    public void Select (TreePath path, bool align, bool reselect, bool checkAllPaths) {
   		if (path == null) {
   			UnselectAll();
   			return;
   		}

   		if ((!reselect) && (checkAllPaths || (Count == 1)) && (selection.PathIsSelected(path))) //No reselection is required and path is already the only selected path
   			return;

		SetFocus(path, align);
	}

	public void Select (TreePath path, bool align, bool reselect) {
		Select(path, align, reselect, false);
	}

	/// <summary>Selects the specified index, possibly aligning it to the center and/or reselecting it.</summary>
	/// <param name="index">The index of the subtitle to select.</param>
    /// <param name="align">Whether to align the selected path to the center if the path isn't visible and scrolling is needed.</param>
    /// <param name="reselect">Whether to reselect the path if it's already the only selected path.</param>
    /// <remarks>The subtitle is only selected if it exists. The input focus will be placed on the path.</remarks>
    public void Select (int index, bool align, bool reselect, bool checkAllPaths) {
    	if ((index >= 0) && (index < Base.Document.Subtitles.Count))
    		Select(Util.IntToPath(index), align, reselect, checkAllPaths);
	}

	public void Select (int index, bool align, bool reselect) {
		Select(index, align, reselect, false);
	}

	/// <summary>Selects a <see cref="TreePath" />, activates it and selects text in the subtitle it refers to.</summary>
	/// <param name="path">The path to select. If it's null, all paths will be unselected.</param>
    /// <param name="align">Whether to align the selected path to the center if the path isn't visible and scrolling is needed.</param>
    /// <param name="reselect">Whether to reselect the path if it's already the only selected path.</param>
	/// <param name="start">The index to start the text selection with. This is also where the cursor is positioned.</param>
	/// <param name="end">The index to start the text selection with. This is also where the cursor is positioned.</param>
	/// <param name="textType">The type of text content to select.</param>
	public void Select (TreePath path, bool align, bool reselect, int start, int end, SubtitleTextType textType) {
		if (path == null) {
			UnselectAll();
			return;
		}
		Select(path, align, reselect);
		Core.Base.Ui.Edit.TextFocusOnSelection(start, end, textType);
	}

    /// <summary>Selects the first subtitle.</summary>
    /// <remarks>The subtitle is only selected if it exists.</remarks>
    public void SelectFirst () {
    	if (Base.Document.Subtitles.Count > 0)
    		Select(TreePath.NewFirst(), false, false);
    }

    /// <summary>Selects the last subtitle.</summary>
    /// <remarks>The subtitle is only selected if it exists.</remarks>
    public void SelectLast () {
    	int count = Base.Document.Subtitles.Count;
    	if (count > 0)
    		Select(Util.IntToPath(count - 1), false, false);
    }

    /// <summary>Selects the next path. If multiple paths are currently selected, the one after the
    /// last selected path is selected. If no paths are selected, the first one is selected.</summary>
    public void SelectNext () {
    	if (Count == 0) {
    		SelectFirst();
    		return;
    	}
    	TreePath path = LastPath;
    	if (path.Indices[0] == (Base.Document.Subtitles.Count - 1)) //this is the last existing path
    		return;

    	TreePath next = Util.PathNext(path);
    	Select(next, false, true);
    }

    /// <summary>Selects the previous path. If multiple paths are currently selected, the one before the
    /// first selected path is selected. If no paths are selected, the first one is selected.</summary>
    public void SelectPrevious () {
    	if (Count == 0) {
    		SelectFirst();
    		return;
    	}
    	TreePath path = FirstPath;
    	if (path.Indices[0] == 0) //this is the first existing path
    		return;

    	TreePath previous = Util.PathPrevious(path);
    	Select(previous, false, true);
    }

	/// <summary>Selects all subtitles.</summary>
	public void SelectAll () {
		selection.SelectAll();
	}

	/// <summary>Needed for the selection-dependent widgets to refresh when the contents of a selection are changed
	/// while the selection itself isn't. Example: applying styles to a single selected subtitle</summary>
    public void Reselect () {
    	EmitChangedEvent();
    }

	/// <summary>Scrolls to the specified path and optionally aligns it to the center of the <see cref="TreeView" />.</summary>
	/// <remarks>This should only be used to scroll to the input focus.</remarks>
    /// <param name="focus ">The path corresponding to the input focus.</param>
    /// <param name="toAlign">Whether to align the path to the center.</param>
	public void ScrollToFocus (TreePath focus, bool align) {
		Scroll(focus, align);
	}

	/* Private members */

	/// <summary>Selects the specified paths according to the specified <see cref="SelectionType" />
	/// and possibly gives focus to the specified path.</summary>
	/// <param name="paths">The paths to select. If <see cref="SelectionType" /> is Range this must be an array
	/// with length 2 containing the first and last paths in the range. This value must be validated before calling this method.</param>
	/// <param name="focus">The path to give input focus to. It must be one of the specified paths or null, in which case no focus will be given.</param>
	/// <param name="align">Whether to align the focused path to the center if the path isn't visible and scrolling is needed.</param>
	private void Select (TreePath[] paths, SelectionType selectionType, TreePath focus, bool align) {
		DisconnectSelectionChangedSignal();

		if (focus != null)
			SetFocus(focus, align);
		else
   			UnselectAll(); //When not using focus, manual unselection is needed

		if (selectionType == SelectionType.Simple) {
			foreach (TreePath path in paths)
				selection.SelectPath(path);
		}
		else if (selectionType == SelectionType.Range) {
			selection.SelectRange(paths[0], paths[1]);
		}

		ConnectSelectionChangedSignal();
		OnSelectionChanged(this, EventArgs.Empty); //Need to simulate this event because the signal was disabled during selection change
   	}

   	/// <summary>Sets the input to the specified path and selects it, possibly aligning it to the center.</summary>
   	/// <param name="path">The path to place the cursor at.</param>
   	/// <param name="align">Whether to align the path to the center if it isn't visible.</param>
   	/// <remarks>The path is selected and the input </remarks>
   	private void SetFocus (TreePath path, bool align) {
		ScrollToFocus(path, align);
   		tree.SetCursor(path, null, false);
   	}

	/// <summary>Unselects all subtitles.</summary>
	private void UnselectAll () {
		selection.UnselectAll();
	}

	/// <summary>Scrolls to the specified path and optionally aligns it to the center of the <see cref="TreeView" />.</summary>
    /// <param name="path">The path to scroll to.</param>
    /// <param name="toAlign">Whether to align the path to the center.</param>
	private void ScrollToCell (TreePath path, bool align) {
		tree.ScrollToCell(path, null, align, 0.5f, 0.5f);
	}

	/// <summary>Scrolls to the specified path, in case it isn't currently visible, centering the row on the center of the <see cref="TreeView" /></summary>
    private void Scroll (TreePath path, bool align) {
    	TreePath startPath, endPath;

    	bool arePathsValid = tree.GetVisibleRange(out startPath, out endPath);
    	if ((!arePathsValid) || (!Util.IsPathValid(startPath)) || (!Util.IsPathValid(endPath))) {
    		ScrollToCell(path, align);
    		return;
    	}

		int startIndice = startPath.Indices[0];
		int endIndice = endPath.Indices[0];
		int pathIndice = path.Indices[0];
		if ((pathIndice >= startIndice) && (pathIndice <= endIndice)) //path is already visible
			return;
		else
			ScrollToCell(path, align);
    }

	public void Activate (TreePath path, bool align, bool reselect) {
		Select(path, align, reselect);
		tree.ActivateRow(path, null);
	}

	/* Event members */

	private void OnBaseInitFinished () {
		ConnectSelectionChangedSignal();
	}

	private void ConnectSelectionChangedSignal () {
		selection.Changed += OnSelectionChanged;
	}

	private void DisconnectSelectionChangedSignal () {
		selection.Changed -= OnSelectionChanged;
	}

	private void OnSelectionChanged (object o, EventArgs args) {
		subtitle = (this.Count == 1 ? Base.Document.Subtitles[Path] : null);
		EmitChangedEvent();
	}

	private void EmitChangedEvent () {
		if (Changed != null)
			Changed(Paths, subtitle);
	}

}

}
