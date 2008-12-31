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
using GnomeSubtitles.Core;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;

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
	[WidgetAttribute] private TreeView tree = null;
	[WidgetAttribute] private Button buttonRemove = null;

	public TimingsSynchronizeDialog () : base(gladeFilename){
		this.timingMode = Base.TimingMode;
	
		CreateColumns();
		SetModel();
		ConnectHandlers();
	}
	
	/* Private methods */
	
	private void CreateColumns() {
    	/* Number column */
    	numberCol = Core.Util.CreateTreeViewColumn(Catalog.GetString("No."), Core.Util.ColumnWidth(tree, "000"), new CellRendererText(), RenderSubtitleNumberCell);
    	
    	/* Start (current and correct) columns */
    	int timeWidth = Core.Util.ColumnWidth(tree, "00:00:00.000");
    	currentStartCol = Core.Util.CreateTreeViewColumn(Catalog.GetString("Current Start"), timeWidth, new CellRendererText(), RenderCurrentStartCell);
    	correctStartCol = Core.Util.CreateTreeViewColumn(Catalog.GetString("Correct Start"), timeWidth, new CellRendererText(), RenderCorrectStartCell);

    	tree.AppendColumn(numberCol);
    	tree.AppendColumn(currentStartCol);
    	tree.AppendColumn(correctStartCol);

    	tree.AppendColumn(new TreeViewColumn()); //Appending to leave empty space to the right
    }
    
    private void SetModel () {
		tree.Model = syncPoints.Model;
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
		tree.Selection.Changed += OnSelectionChanged;
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
		TreePath[] selectedPaths = Base.Ui.View.Selection.Paths;
		if (selectedPaths.Length != 1)
			return;
		
		TreePath path = selectedPaths[0];
		int subtitleNumber = Core.Util.PathToInt(path);
		Subtitle subtitle = Base.Ui.View.Selection.Subtitle;
		
		/* Get current start */
		Timing currentTiming = new Timing(subtitle.Frames.Start, subtitle.Times.Start);
		
		/* Get correct start from video */
		Timing correctTiming = new Timing(Base.Ui.Video.Position.CurrentFrames, Base.Ui.Video.Position.CurrentTime);

		/* Create and add the sync point */
		SyncPoint syncPoint = new SyncPoint(subtitleNumber, currentTiming, correctTiming);
		syncPoints.InsertSorted(syncPoint);
	}
	
	private void OnRemove (object o, EventArgs args) {
		System.Console.WriteLine("Remove");
	}
	
	private void OnResponse (object o, ResponseArgs args) {
		if (args.ResponseId == ResponseType.Ok) {
		/*	SelectionIntended selectionIntended = (allSubtitlesRadioButton.Active ? SelectionIntended.All : SelectionIntended.Range);
			
			if (timingMode == TimingMode.Times) {
				TimeSpan firstTime = TimeSpan.Parse(firstSubtitleNewStartSpinButton.Text);
				TimeSpan lastTime = TimeSpan.Parse(lastSubtitleNewStartSpinButton.Text);
				Base.CommandManager.Execute(new AdjustTimingsCommand(firstTime, lastTime, selectionIntended));
			}
			else {
				int firstFrame = (int)firstSubtitleNewStartSpinButton.Value;
				int lastFrame = (int)lastSubtitleNewStartSpinButton.Value;
				Base.CommandManager.Execute(new AdjustTimingsCommand(firstFrame, lastFrame, selectionIntended));
			}*/
		}
		else {
			Close();
		}
	}

}

}
