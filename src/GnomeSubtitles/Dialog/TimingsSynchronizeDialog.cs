/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008-2009 Pedro Castro
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
using GnomeSubtitles.Core;
using GnomeSubtitles.Core.Command;
using GnomeSubtitles.Ui.View;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;
using System.Collections;

namespace GnomeSubtitles.Dialog {

public class TimingsSynchronizeDialog : GladeDialog {
	private TimingMode timingMode = TimingMode.Times; 
	private GnomeSubtitles.Core.SyncPoints syncPoints = new GnomeSubtitles.Core.SyncPoints();
	private TreeViewColumn numberCol = null;
	private TreeViewColumn currentStartCol = null;
	private TreeViewColumn correctStartCol = null;
		
	/* Constant strings */
	private const string gladeFilename = "TimingsSynchronizeDialog.glade";
	
	/* Widgets */
	[WidgetAttribute] private TreeView syncPointsTree = null;
	[WidgetAttribute] private Button buttonAdd = null;
	[WidgetAttribute] private Button buttonRemove = null;
	[WidgetAttribute] private Button buttonSynchronize = null;
	[WidgetAttribute] private CheckButton syncAllSubtitlesCheckButton = null;
	[WidgetAttribute] private Label statusMessageLabel = null;

	public TimingsSynchronizeDialog () : base(gladeFilename, true){
		this.timingMode = Base.TimingMode;
	
		CreateColumns();
		SetModel();
		InitWidgetSensitivity();
		
		ConnectHandlers();
		
		UpdateStatusMessage();
	}
	
	public override void Destroy () {
		Base.Ui.View.Selection.Changed -= OnUiViewSelectionChanged;
		base.Destroy();
	}
	
	/* Private methods */
	
	private void CreateColumns() {
    	/* Number column */
    	numberCol = Core.Util.CreateTreeViewColumn(Catalog.GetString("Subtitle No."), -1, new CellRendererText(), RenderSubtitleNumberCell);
    	
    	/* Start (current and correct) columns */
    	currentStartCol = Core.Util.CreateTreeViewColumn(Catalog.GetString("Current Start"), -1, new CellRendererText(), RenderCurrentStartCell);
    	correctStartCol = Core.Util.CreateTreeViewColumn(Catalog.GetString("Correct Start"), -1, new CellRendererText(), RenderCorrectStartCell);

    	syncPointsTree.AppendColumn(numberCol);
    	syncPointsTree.AppendColumn(currentStartCol);
    	syncPointsTree.AppendColumn(correctStartCol);

    	syncPointsTree.AppendColumn(new TreeViewColumn()); //Appending to leave empty space to the right
    }
    
    private void SetModel () {
		syncPointsTree.Model = syncPoints.Model;
	}
	
	private void UpdateFromSyncPointCountChanged () {
		UpdateStatusMessage();
		UpdateSynchronizeButtonSensitivity();
	}
	
	private void UpdateStatusMessage () {
		String message = String.Empty;
		switch (syncPoints.Collection.Count) {
			case 0:
				message = Catalog.GetString("Add sync points by selecting subtitles and adjusting the video to their correct position. At least 2 points are needed.");
				break;
			case 1:
				message = Catalog.GetString("Add 1 more sync point to start synchronizing. Adding more points will improve accuracy.");
				break;
			default:
				String allSubtitlesSyncMessage = Catalog.GetString("Synchronization is ready. All subtitles will be synchronized.");
				if (syncAllSubtitlesCheckButton.Active)
					message = allSubtitlesSyncMessage;
				else {
					ArrayList intervals = GetOutOfRangeIntervals();
					switch (intervals.Count) {
						case 0:
							message = allSubtitlesSyncMessage;
							break;
						case 1:
							message = String.Format(Catalog.GetString("Synchronization is ready. The following subtitles will not be synchronized: {0}."), intervals[0]);
							break;
						case 2:
							message = String.Format(Catalog.GetString("Synchronization is ready. The following subtitles will not be synchronized: {0} and {1}."), intervals[0], intervals[1]);
							break;
						default:
							break;
					}
				}
				break;
		}
		statusMessageLabel.Text = message;
	}
	
	private void UpdateSynchronizeButtonSensitivity () {
		buttonSynchronize.Sensitive = (syncPoints.Collection.Count >= 2);
	}
	
	private void SelectPath (TreePath path) {
		syncPointsTree.SetCursor(path, null, false);
	}
	
	private void InitWidgetSensitivity () {
		buttonAdd.Sensitive = (Base.Ui.View.Selection.Count == 1);
	}
	
	private ArrayList GetOutOfRangeIntervals () {
		Ui.View.Subtitles subtitles = Base.Document.Subtitles;
		ArrayList intervals = new ArrayList();
		if (syncPoints.Collection.Count == 0)
			return intervals;
		
		SyncPoint first = syncPoints.Collection.Get(0);
		if (first.SubtitleNumber > 0) {
			String firstInterval = "1" + (first.SubtitleNumber > 1 ? "-" + first.SubtitleNumber : String.Empty);
			intervals.Add(firstInterval);
		}
		
		SyncPoint last = syncPoints.Collection.Get(syncPoints.Collection.Count - 1);
		int lastSubtitleNumber = subtitles.Count - 1;
		if (last.SubtitleNumber < lastSubtitleNumber) {
			String lastInterval = (last.SubtitleNumber < lastSubtitleNumber - 1 ? (last.SubtitleNumber + 2) + "-" : String.Empty) + (lastSubtitleNumber + 1);
			intervals.Add(lastInterval);
		}
		
		return intervals;
	}
    
    /* Cell Renderers */

	private void RenderSubtitleNumberCell (TreeViewColumn column, CellRenderer cell, TreeModel treeModel, TreeIter iter) {
		(cell as CellRendererText).Text = (syncPoints[iter].SubtitleNumber + 1).ToString();
	}

	private void RenderCurrentStartCell (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
		CellRendererText renderer = cell as CellRendererText;
		if (timingMode == TimingMode.Frames)
			renderer.Text = syncPoints[iter].Current.Frame.ToString();
		else
			renderer.Text = Core.Util.TimeSpanToText(syncPoints[iter].Current.Time);
	}
	
	private void RenderCorrectStartCell (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
		CellRendererText renderer = cell as CellRendererText;
		if (timingMode == TimingMode.Frames)
			renderer.Text = syncPoints[iter].Correct.Frame.ToString();
		else
			renderer.Text = Core.Util.TimeSpanToText(syncPoints[iter].Correct.Time);
	}
	
	/* Event members */
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void ConnectHandlers () {
		syncPointsTree.Selection.Changed += OnSelectionChanged;
		
		/* External event handlers */
		Base.Ui.View.Selection.Changed += OnUiViewSelectionChanged;
	}
	
	private void OnSelectionChanged (object o, EventArgs args) {
		TreeSelection selection = (o as TreeSelection);
		buttonRemove.Sensitive = (selection.CountSelectedRows() > 0); 
	}
	
	private void OnAdd (object o, EventArgs args) {
		/* Check if document and video are loaded */
		if (!(Base.IsDocumentLoaded && Base.Ui.Video.IsLoaded))
			return;
	
		/* Get selected subtitle */
		TreePath path = Base.Ui.View.Selection.Path;
		if (path == null)
			return;
		
		int subtitleNumber = Core.Util.PathToInt(path);
		Subtitle subtitle = Base.Ui.View.Selection.Subtitle;
		
		/* Get current start */
		Timing currentTiming = new Timing(subtitle.Frames.Start, subtitle.Times.Start);
		
		/* Get correct start from video */
		Timing correctTiming = new Timing(Base.Ui.Video.Position.CurrentFrames, Base.Ui.Video.Position.CurrentTime);

		/* Create and add the sync point */
		SyncPoint syncPoint = new SyncPoint(subtitleNumber, currentTiming, correctTiming);
		int syncPointIndex = syncPoints.Add(syncPoint);
		TreePath syncPointPath = Core.Util.IntToPath(syncPointIndex);
		SelectPath(syncPointPath);		
		
		UpdateFromSyncPointCountChanged();
	}
	
	private void OnRemove (object o, EventArgs args) {
		TreePath[] paths = syncPointsTree.Selection.GetSelectedRows();
		if (paths.Length == 0)
			return;

		syncPoints.Remove(paths);
		int syncPointCount = syncPoints.Collection.Count;
		if (syncPointCount != 0) {
			int firstDeletedSubtitle = Core.Util.PathToInt(paths[0]);
			int subtitleToDelete = (firstDeletedSubtitle < syncPointCount ? firstDeletedSubtitle : syncPointCount - 1);
			syncPointsTree.SetCursor(Core.Util.IntToPath(subtitleToDelete), null, false);
		}
		
		UpdateFromSyncPointCountChanged();
	}
	
	private void OnSynchronizeAllSubtitlesToggled (object o, EventArgs args) {
		UpdateStatusMessage();
	}
	
	private void OnUiViewSelectionChanged (TreePath[] paths, Subtitle subtitle) {
		buttonAdd.Sensitive = (subtitle != null);
	}

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			bool toSyncAll = syncAllSubtitlesCheckButton.Active;
			SelectionIntended selectionIntended = (toSyncAll ? SelectionIntended.All : SelectionIntended.Range);
			
			TreePath[] pathRange = null;
			if (selectionIntended == SelectionIntended.Range) {
				pathRange = new TreePath[2];
				pathRange[0] = Core.Util.IntToPath(syncPoints.Collection[0].SubtitleNumber);
				pathRange[1] = Core.Util.IntToPath(syncPoints.Collection[syncPoints.Collection.Count - 1].SubtitleNumber);
			}
			
			Base.CommandManager.Execute(new SynchronizeTimingsCommand(syncPoints, toSyncAll, selectionIntended, pathRange));
			return true;
		}
		else
			return false;
	}

}

}
