/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008-2018 Pedro Castro
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
using GnomeSubtitles.Core.Command;
using GnomeSubtitles.Ui;
using GnomeSubtitles.Ui.View;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;
using System.Collections;

namespace GnomeSubtitles.Dialog {

public class TimingsSynchronizeDialog : BaseDialog {
	private TimingMode timingMode = TimingMode.Times;
	private Core.SyncPoints syncPoints = new Core.SyncPoints();

	/* Widgets */
	private TreeView syncPointsTree = null;
	private Button buttonAdd = null;
	private Button buttonRemove = null;
	private RadioButton allSubtitlesRadioButton = null;
	private RadioButton selectedSubtitlesRadioButton = null;
	private Label statusMessageLabel = null;


	public TimingsSynchronizeDialog () : base(){
		this.timingMode = Base.TimingMode;

		Init(BuildDialog());

		UpdateSynchronizeButtonSensitivity();
		UpdateStatusMessage();
		
		ConnectEventHandlers();
	}

	/* Overridden members */

	public override DialogScope Scope {
		get { return DialogScope.Document; }
	}

	public override void Destroy () {
		DisconnectEventHandlers();
		base.Destroy();
	}


	/* Private methods */

	private Gtk.Dialog BuildDialog () {
		Gtk.Dialog dialog = new Gtk.Dialog(Catalog.GetString("Synchronize Timings"), Base.Ui.Window, DialogFlags.DestroyWithParent,
			Util.GetStockLabel("gtk-close"), ResponseType.Cancel, Catalog.GetString("_Apply"), ResponseType.Ok);

		dialog.DefaultResponse = ResponseType.Ok;
		dialog.DefaultWidth = WidgetStyles.DialogWidthXSmall;
		dialog.DefaultHeight = WidgetStyles.DialogHeightMedium;

		Box box = new Box(Orientation.Vertical, WidgetStyles.BoxSpacingMedium);
		box.BorderWidth = WidgetStyles.BorderWidthMedium;
		
		//Sync Points frame

		Box syncVBox = new Box(Orientation.Vertical, WidgetStyles.BoxSpacingMedium);
		syncVBox.MarginLeft = WidgetStyles.FrameContentSpacingMedium;
		syncVBox.Spacing = WidgetStyles.BoxSpacingMedium;

		ScrolledWindow syncWindow = new ScrolledWindow();
		syncWindow.ShadowType = ShadowType.EtchedIn;
		syncWindow.Expand = true;
		syncPointsTree = new TreeView();
		CreateColumns(syncPointsTree);
		syncPointsTree.Model = syncPoints.Model;
		syncPointsTree.Selection.Changed += OnSelectionChanged;
		syncWindow.Add(syncPointsTree);
		syncVBox.Add(syncWindow);
		
		Box syncButtonsHBox = new Box(Orientation.Horizontal, WidgetStyles.BoxSpacingMedium);
		buttonAdd = new Button("gtk-add");
		UpdateAddButtonSensitivity();
		buttonAdd.Clicked += OnAdd;
		syncButtonsHBox.Add(buttonAdd);
		
		buttonRemove = new Button("gtk-remove");
		buttonRemove.Sensitive = false;
		buttonRemove.Clicked += OnRemove;
		syncButtonsHBox.Add(buttonRemove);
		
		syncVBox.Add(syncButtonsHBox);
		Frame syncFrame = Util.CreateFrameWithContent(Catalog.GetString("Sync Points"), syncVBox);
		box.Add(syncFrame);

		
		//Apply To frame

		Box applyToFrameVBox = new Box(Orientation.Vertical, WidgetStyles.BoxSpacingMedium);
		applyToFrameVBox.MarginLeft = WidgetStyles.FrameContentSpacingMedium;
		applyToFrameVBox.Spacing = WidgetStyles.BoxSpacingMedium;
		allSubtitlesRadioButton = new RadioButton(DialogStrings.ApplyToAllSubtitles);
		selectedSubtitlesRadioButton = new RadioButton(allSubtitlesRadioButton, Catalog.GetString("Subtitles between sync points"));
		applyToFrameVBox.Add(allSubtitlesRadioButton);
		applyToFrameVBox.Add(selectedSubtitlesRadioButton);
		
		Frame applyToFrame = Util.CreateFrameWithContent(Catalog.GetString("Apply To"), applyToFrameVBox);
		box.Add(applyToFrame);
		
		
		//Status frame

		Box statusFrameVBox = new Box(Orientation.Vertical, WidgetStyles.BoxSpacingMedium); //Used so we can have a border
		statusFrameVBox.MarginLeft = WidgetStyles.FrameContentSpacingMedium;
		statusFrameVBox.Spacing = WidgetStyles.BoxSpacingMedium;
		statusMessageLabel = new Label();
		statusMessageLabel.Wrap = true;
		statusFrameVBox.Add(statusMessageLabel);
		
		Frame statusFrame = Util.CreateFrameWithContent(Catalog.GetString("Info"), statusFrameVBox);
		box.Add(statusFrame);
		
		dialog.ContentArea.Add(box);
		dialog.ContentArea.ShowAll();

		return dialog;
	}

	private void CreateColumns(TreeView treeView) {
    	/* Number column */
    	TreeViewColumn numberCol = Core.Util.CreateTreeViewColumn(Catalog.GetString("Subtitle No."), -1, new CellRendererText(), RenderSubtitleNumberCell);

    	/* Start (current and correct) columns */
    	int timeWidth = Util.ColumnWidthForTimeValue(treeView);
    	TreeViewColumn currentStartCol = Core.Util.CreateTreeViewColumn(Catalog.GetString("Start"), timeWidth, new CellRendererText(), RenderCurrentStartCell);
    	TreeViewColumn correctStartCol = Core.Util.CreateTreeViewColumn(Catalog.GetString("New Start"), timeWidth, new CellRendererText(), RenderCorrectStartCell);

    	treeView.AppendColumn(numberCol);
    	treeView.AppendColumn(currentStartCol);
    	treeView.AppendColumn(correctStartCol);

    	treeView.AppendColumn(new TreeViewColumn()); //Appending to leave empty space to the right
    }

	private void UpdateFromSyncPointCountChanged () {
		UpdateStatusMessage();
		UpdateSynchronizeButtonSensitivity();
	}

	private void UpdateStatusMessage () {
		string message = String.Empty;
		switch (syncPoints.Collection.Count) {
			case 0:
				message = Catalog.GetString("Add sync points by selecting subtitles and adjusting the video to their correct position. At least 2 points are needed.");
				break;
			case 1:
				message = Catalog.GetString("Add 1 more sync point to start synchronizing. Adding more points will improve accuracy.");
				break;
			default:
				string allSubtitlesSyncMessage = Catalog.GetString("Synchronization is ready. All subtitles will be synchronized.");
				if (allSubtitlesRadioButton.Active) {
					message = allSubtitlesSyncMessage;
				} else {
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
		
		statusMessageLabel.Markup = "<i>" + message + "</i>";
	}

	private void UpdateSynchronizeButtonSensitivity () {
		Button syncButton = Dialog.GetWidgetForResponse((int)ResponseType.Ok) as Button;
		syncButton.Sensitive = (syncPoints.Collection.Count >= 2);
	}
	
	private void UpdateAddButtonSensitivity () {
		buttonAdd.Sensitive = (Base.Ui.View.Selection.Count == 1) && Base.Ui.Video.IsLoaded;
	}

	private void SelectPath (TreePath path) {
		syncPointsTree.SetCursor(path, null, false);
	}

	private ArrayList GetOutOfRangeIntervals () {
		Ui.View.Subtitles subtitles = Base.Document.Subtitles;
		ArrayList intervals = new ArrayList();
		if (syncPoints.Collection.Count == 0)
			return intervals;

		SyncPoint first = syncPoints.Collection.Get(0);
		if (first.SubtitleNumber > 0) {
			string firstInterval = "1" + (first.SubtitleNumber > 1 ? "-" + first.SubtitleNumber : String.Empty);
			intervals.Add(firstInterval);
		}

		SyncPoint last = syncPoints.Collection.Get(syncPoints.Collection.Count - 1);
		int lastSubtitleNumber = subtitles.Count - 1;
		if (last.SubtitleNumber < lastSubtitleNumber) {
			string lastInterval = (last.SubtitleNumber < lastSubtitleNumber - 1 ? (last.SubtitleNumber + 2) + "-" : String.Empty) + (lastSubtitleNumber + 1);
			intervals.Add(lastInterval);
		}

		return intervals;
	}

	private bool CanSynchronize () {
		return (syncPoints.Collection.Count > 0)
			&& (syncPoints.Collection.Get(syncPoints.Collection.Count - 1).SubtitleNumber < Base.Document.Subtitles.Count);
	}

    /* Cell Renderers */

	private void RenderSubtitleNumberCell (TreeViewColumn column, CellRenderer cell, ITreeModel treeModel, TreeIter iter) {
		(cell as CellRendererText).Text = (syncPoints[iter].SubtitleNumber + 1).ToString();
	}

	private void RenderCurrentStartCell (TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
		CellRendererText renderer = cell as CellRendererText;
		if (timingMode == TimingMode.Frames)
			renderer.Text = syncPoints[iter].Current.Frame.ToString();
		else
			renderer.Text = Core.Util.TimeSpanToText(syncPoints[iter].Current.Time);
	}

	private void RenderCorrectStartCell (TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
		CellRendererText renderer = cell as CellRendererText;
		if (timingMode == TimingMode.Frames)
			renderer.Text = syncPoints[iter].Correct.Frame.ToString();
		else
			renderer.Text = Core.Util.TimeSpanToText(syncPoints[iter].Correct.Time);
	}
	
	private void UpdateFromTimingMode () {
		syncPointsTree.QueueDraw();
	}

	/* Event members */

	private void ConnectEventHandlers () {
		Base.Ui.View.Selection.Changed += OnUiViewSelectionChanged;
		Base.VideoLoaded += OnBaseVideoLoaded;
		Base.VideoUnloaded += OnBaseVideoUnloaded;
		Base.TimingModeChanged += OnBaseTimingModeChanged;
	}
	
	private void DisconnectEventHandlers () {
		Base.Ui.View.Selection.Changed -= OnUiViewSelectionChanged;
		Base.VideoLoaded -= OnBaseVideoLoaded;
		Base.VideoUnloaded -= OnBaseVideoUnloaded;
		Base.TimingModeChanged -= OnBaseTimingModeChanged;
	}
	
	private void OnBaseTimingModeChanged (TimingMode newTimingMode) {
    	if (timingMode == newTimingMode) {
			return;
		}
		
		timingMode = newTimingMode;
		UpdateFromTimingMode();
    }
	
	private void OnBaseVideoLoaded (Uri videoUri) {
    	UpdateAddButtonSensitivity();
	}

	private void OnBaseVideoUnloaded () {
		UpdateAddButtonSensitivity();
	}

	private void OnSelectionChanged (object o, EventArgs args) {
		TreeSelection selection = (o as TreeSelection);
		buttonRemove.Sensitive = (selection.CountSelectedRows() > 0);
	}

	private void OnRowActivated (object o, RowActivatedArgs args) {
		SyncPoint syncPoint = syncPoints[args.Path];
		int subtitleNumber = syncPoint.SubtitleNumber;
		if (subtitleNumber < Base.Document.Subtitles.Count) {
			Base.Ui.View.Selection.Select(Core.Util.IntToPath(syncPoint.SubtitleNumber), true, true);
			Base.Ui.Video.Seek(syncPoint.Correct.Time);
		}
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
		UpdateAddButtonSensitivity();
	}

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			if (CanSynchronize()) {
				bool toSyncAll = allSubtitlesRadioButton.Active;
				SelectionIntended selectionIntended = (toSyncAll ? SelectionIntended.All : SelectionIntended.Range);

				TreePath[] pathRange = null;
				if (selectionIntended == SelectionIntended.Range) {
					pathRange = new TreePath[2];
					pathRange[0] = Core.Util.IntToPath(syncPoints.Collection[0].SubtitleNumber);
					pathRange[1] = Core.Util.IntToPath(syncPoints.Collection[syncPoints.Collection.Count - 1].SubtitleNumber);
				}

				Base.CommandManager.Execute(new SynchronizeTimingsCommand(syncPoints, toSyncAll, selectionIntended, pathRange));
			}
			return true;
		}
		
		return false;
	}

}

}