/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
 * Copyright (C) 2006 Pedro Castro
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
using System;
using SubLib;

namespace GnomeSubtitles {

public class SubtitleSelection {

	private TreeView tree = null;
	private TreeSelection selection = null;

	public SubtitleSelection (TreeView tree) {
		this.tree = tree;
		this.selection = tree.Selection;
		
		selection.Mode = SelectionMode.Multiple;
		ConnectSelectionChanged();
	}

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
    /// <remarks>If there is more than one path selected, the first is returned.</remarks>
	public TreePath Path {
    	get { return FirstPath; }
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
    
    /// <summary>The selected subtitle. If there is more than one selected, the first is returned.</summary>
    public Subtitle Subtitle {
    	get { return Global.Subtitles[Path]; }
    }

    /// <summary>The number of selected paths.</summary>
    public int Count {
    	get { return selection.CountSelectedRows(); }
    }
    
    /// <summary>The selected path that currently has the focus.<summary>
    /// <remarks>If none of the selected paths have focus, the <see cref="FirstPath" /> is returned. The first path,
    /// on the other hand, can be null if no path is selected.</remarks>
    public TreePath Focus {
    	get {
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
    /// <param name="reselect">Whether to reselect the path if it's already the only selected path.</param
    /// <remarks>The input focus will be placed on the path.</remarks>
    public void Select (TreePath path, bool align, bool reselect) {
   		if (path == null) {
   			UnselectAll();
   			return;
   		}
   		if ((!reselect) && (Count == 1) && (selection.PathIsSelected(path))) //No reselection is required and path is already the only selected path
   			return;
   		
		SetFocus(path, align);
	}
    
    /// <summary>Selects the first subtitle.</summary>
    /// <remarks>The subtitle is only selected if it exists.</remarks>
    public void SelectFirst () {
    	if (Global.Subtitles.Count > 0)
    		Select(TreePath.NewFirst(), false, false);
    }
    
    /// <summary>Selects the next path. If multiple paths are currently selected, the one after the
    /// last selected path is selected. If no paths are selected, the first one is selected.</summary>
    public void SelectNext () {
    	if (Count == 0) {
    		SelectFirst();
    		return;
    	}    
    	TreePath path = LastPath;
    	if (path.Indices[0] == (Global.Subtitles.Count - 1)) //this is the last existing path
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
	
	/// <summary>Activates the selected path.</summary>
	/// <remarks>If there isn't one and only one path selected, nothing is done.</remarks>
	/// <returns>Whether there was only one path to be activated.</returns>
	public bool ActivatePath () {
		if (Count != 1)
			return false;
		
		Global.GUI.Edit.TextGrabFocus();
		return true;
	}
	
	/// <summary>Scrolls to the specified path and optionally aligns it to the center of the <see cref="TreeView" />.</summary>
	/// <remarks>This should only be used to scroll to the input focus.</remarks>
    /// <param name="focus ">The path corresponding to the input focus.</param>
    /// <param name="toAlign">Whether to align the path to the center.</param>
	public void ScrollToFocus (TreePath focus, bool align) {
		Scroll(focus, align);	
	}

    /* Event members */
	
	private void DisconnectSelectionChanged () {
		selection.Changed -= OnSelectionChanged;
	}
	
	private void ConnectSelectionChanged () {
		selection.Changed += OnSelectionChanged;
	}
	
	private void EmitSelectionChanged () {
		OnSelectionChanged(tree.Selection, EventArgs.Empty);
	}
	
	private void OnSelectionChanged (object o, EventArgs args) {
		Global.GUI.UpdateFromSelection();
	}
	
	/* Private members */

	/// <summary>Selects the specified paths according to the specified <see cref="SelectionType" />
	/// and possibly gives focus to the specified path.</summary>
	/// <param name="paths">The paths to select. If <see cref="SelectionType" /> is Range this must be an array 
	/// with length 2 containing the first and last paths in the range. This value must be validated before calling this method.</param>
	/// <param name="focus">The path to give input focus to. It must be one of the specified paths or null, in which case no focus will be given.</param>
	/// <param name="align">Whether to align the selected path to the center if the path isn't visible and scrolling is needed.</param>
	private void Select (TreePath[] paths, SelectionType selectionType, TreePath focus, bool align) {
		DisconnectSelectionChanged();
		
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
		
		ConnectSelectionChanged();		
		EmitSelectionChanged();
   	}
	   	
   	/// <summary>Sets the input to the specified path and selects it, possibly aligning it to the center.</summary>
   	/// <param name="path">The path to place the cursor at.</param>
   	/// <param name="align">Whether to align the path to the center if it isn't visible.</param>
   	/// <remarks>The path is selected and the input </remarks>
   	private void SetFocus (TreePath path, bool align) {
		ScrollToFocus(path, true);
   		tree.SetCursor(path, null, false);
   	}
	
	/// <summary>Unselects all subtitles.</summary>
	private void UnselectAll () {
		selection.UnselectAll();
	}
	
	/// <summary>Scrolls to the specified path and optionally aligns it to the center of the <see cref="TreeView" />.</summary>
	/// <remarks>This is the original Gtk scroll.</remarks>
    /// <param name="path">The path to scroll to.</param>
    /// <param name="toAlign">Whether to align the path to the center.</param>
	/// TODO Scheduled for using as default when Gnome hits 2.18
	private void ScrollToCell (TreePath path, bool align) {
		tree.ScrollToCell(path, null, align, 0.5f, 0.5f);
	}
	
	/// <summary>Scrolls to the specified path, in case it isn't currently visible, centering the row on the center of the <see cref="TreeView" /></summary>
    /// <remarks>This basically does the same as <see cref="TreeView.ScrollToCell" />. It is used because the default one was showing warnings at runtime.
    /// It seems that recent GTK versions don't show those warnings anymore though.</remarks>
	/// TODO Scheduled for removal when Gnome hits 2.18
    private void Scroll (TreePath path, bool align) {
    	TreePath startPath, endPath;
    	bool hasEmptySpace;
    	bool arePathsValid = GetVisibleRange(out startPath, out endPath, out hasEmptySpace);
    	if (!arePathsValid)
    		return;
    	else if (hasEmptySpace) {
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

	/// <summary>Scrolls to the first path if none of the paths is currently visible.</summary>
	/// TODO Scheduled for update when Gnome hits 2.18
	/*private void Scroll (TreePath[] paths, bool align) {
		if (paths.Length == 0)
			return; 
		else if (paths.Length == 1) {
			Scroll(paths[0], align);
			return;
		}
		
		TreePath startPath, endPath;
		bool hasEmptySpace;
		bool arePathsValid = GetVisibleRange(out startPath, out endPath, out hasEmptySpace);
		if (!arePathsValid)
			return;
		else if (hasEmptySpace) {
			Scroll(paths[0], align);
			return;
		}

		int startIndice = startPath.Indices[0];
		int endIndice = endPath.Indices[0];
		
		//Check if any of the paths is currently visible
		foreach (TreePath path in paths) {
			int pathIndice = path.Indices[0];
			if ((pathIndice >= startIndice) && (pathIndice <= endIndice))
				return;
		}
		Scroll(paths[0], align); //No path is currently visible, scroll to the first one
	}*/
	
	/// <summary>Only needed because the one in GTK was showing some warnings,
	/// which don't seem to appear anymore on recent GTK versions.</summary>
	/// TODO Scheduled to be removed when Gnome hits 2.18
	private bool GetVisibleRange (out TreePath start, out TreePath end, out bool hasEmptySpace) {
    	start = null;
    	end = null;
    	hasEmptySpace = false;
    
    	Gdk.Rectangle visible = tree.VisibleRect;
    	int x, top, bottom;
    	tree.TreeToWidgetCoords(0, visible.Top, out x, out top);
    	tree.TreeToWidgetCoords(0, visible.Bottom, out x, out bottom);

    	TreePath startPath, endPath;
    	bool isStartPathValid = tree.GetPathAtPos(0, top, out startPath);
    	bool isEndPathValid = tree.GetPathAtPos(0, bottom, out endPath);
    	
    	if (!isEndPathValid) {
    		hasEmptySpace = true;
    		if (isStartPathValid) {
    			int lastPath = Global.Subtitles.Count - 1;
    			endPath = new TreePath(lastPath.ToString());
    		}
    		else
    			return false;
    	}
    	else if (!isStartPathValid) {
    		hasEmptySpace = true;
    		startPath = new TreePath("0");
    	}
    	start = startPath;
   		end = endPath;
   		return true;
    }
    
}

}