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

using Gdk;
using Gtk;
using System;
using SubLib.Domain;

namespace GnomeSubtitles {

public class SubtitleView : GladeWidget {
	private const string widgetName = "subtitleListView";
	private TreeView widget = null;
	private Widget scrollArea = null;
	
	private Subtitles subtitles = null;
	private TreeViewColumn numberCol = new TreeViewColumn();
	private TreeViewColumn startCol = new TreeViewColumn();
	private TreeViewColumn endCol = new TreeViewColumn();
	private TreeViewColumn durationCol = new TreeViewColumn();
	private TreeViewColumn textCol = new TreeViewColumn();

	
	public SubtitleView(GUI gui, Glade.XML glade) : base(gui, glade){
		widget = (TreeView)GetWidget(widgetName);
		scrollArea = widget.Parent;
		CreateColumns();
    }
    
    //NOT WORKING
    public int Width {
    		get { return numberCol.Width + startCol.Width + endCol.Width + durationCol.Width + textCol.Width; }
    }
    
    private void CreateColumns() {
    		CellRendererText numberCell = new CellRendererText();
    		numberCell.Xalign = 0.5f;
    		numberCol.Title = "Num.";
    		numberCol.Alignment = 0.5f;
    		numberCol.PackStart(numberCell, true);
    		numberCol.SetCellDataFunc(numberCell, RenderNumberCell);
    		
    		CellRendererSpinButton startCell = new CellRendererSpinButton(TimingMode.Times);
    		startCell.Xalign = 0.5f;
    		startCell.Editable = true;
    		startCell.Edited = OnStartCellEdited;
    		startCol.Title = "From";
    		startCol.Alignment = 0.5f;
    		startCol.PackStart(startCell, true);
    		startCol.SetCellDataFunc(startCell, RenderStartCell);

    		CellRendererSpinButton endCell = new CellRendererSpinButton(TimingMode.Times);
    		endCell.Xalign = 0.5f;
    		endCell.Editable = true;
    		endCell.Edited = OnEndCellEdited;
    		endCol.Title = "To";
    		endCol.Alignment = 0.5f;
    		endCol.PackStart(endCell, true);
    		endCol.SetCellDataFunc(endCell, RenderEndCell);
    		
    		CellRendererSpinButton durationCell = new CellRendererSpinButton(TimingMode.Times);
    		durationCell.Xalign = 0.5f;
    		durationCell.Editable = true;
    		durationCell.Edited = OnDurationCellEdited;
    		durationCol.Title = "During";
    		durationCol.Alignment = 0.5f;
    		durationCol.PackStart(durationCell, true);
    		durationCol.SetCellDataFunc(durationCell, RenderDurationCell);

    		CellRendererText textCell = new CellRendererText();
    		textCell.Editable = true;
    		textCol.Title = "Text";
    		textCol.PackStart(textCell, true);
    		textCol.SetCellDataFunc(textCell, RenderTextCell);
    		
    		widget.AppendColumn(numberCol);
    		widget.AppendColumn(startCol);
    		widget.AppendColumn(endCol);
    		widget.AppendColumn(durationCol);
    		widget.AppendColumn(textCol);
    }

  
    public void NewDocument () {
	    	subtitles = GUI.Core.Subtitles;
	    widget.Model = subtitles.Model;
	    scrollArea.Sensitive = true;
	    scrollArea.Visible = true;	
	    UpdateTimingMode();
    }
    
    public void UpdateTimingMode () {
	    TimingMode timingMode = GUI.Core.Subtitles.Properties.TimingMode;
	    (startCol.CellRenderers[0] as CellRendererSpinButton).TimingMode = timingMode;
	    (endCol.CellRenderers[0] as CellRendererSpinButton).TimingMode = timingMode;
	    (durationCol.CellRenderers[0] as CellRendererSpinButton).TimingMode = timingMode;
	    Refresh();
    }
    
    public void Refresh () {
    		widget.QueueDraw();
    }


	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void RenderNumberCell (TreeViewColumn column, CellRenderer cell, TreeModel treeModel, TreeIter iter) {
		(cell as CellRendererText).Text = (treeModel.GetPath(iter).Indices[0] + 1).ToString();
	}

	private void RenderStartCell (TreeViewColumn column, CellRenderer cell, TreeModel treeModel, TreeIter iter) {
		CellRendererSpinButton cellRenderer = (CellRendererSpinButton)cell;
		if (cellRenderer.TimingMode == TimingMode.Frames)
			cellRenderer.SetText(subtitles.GetSubtitle(iter).Frames.Start);
		else
			cellRenderer.SetText(subtitles.GetSubtitle(iter).Times.Start);
	}	
	
	private void RenderEndCell (TreeViewColumn column, CellRenderer cell, TreeModel treeModel, TreeIter iter) {
		CellRendererSpinButton cellRenderer = (CellRendererSpinButton)cell;
		if (cellRenderer.TimingMode == TimingMode.Frames)
			cellRenderer.SetText(subtitles.GetSubtitle(iter).Frames.End);
		else
			cellRenderer.SetText(subtitles.GetSubtitle(iter).Times.End);
	}
	
	private void RenderDurationCell (TreeViewColumn column, CellRenderer cell, TreeModel treeModel, TreeIter iter) {
		CellRendererSpinButton cellRenderer = (CellRendererSpinButton)cell;
		if (cellRenderer.TimingMode == TimingMode.Frames)
			cellRenderer.SetText(subtitles.GetSubtitle(iter).Frames.Duration);
		else
			cellRenderer.SetText(subtitles.GetSubtitle(iter).Times.Duration);
	}
	
	private void RenderTextCell (TreeViewColumn column, CellRenderer cellRenderer, TreeModel treeModel, TreeIter iter) {
		CellRendererText cell = (CellRendererText)cellRenderer;
		Subtitle subtitle = subtitles.GetSubtitle(iter);
		cell.Text = subtitle.Text.Get();
		
		if (subtitle.Style.Italic)
			cell.Style = Pango.Style.Italic;
		else
			cell.Style = Pango.Style.Normal;
			
		if (subtitle.Style.Bold)
			cell.Weight = (int)Pango.Weight.Bold;
		else
			cell.Weight = (int)Pango.Weight.Normal;

		if (subtitle.Style.Underline)
			cell.Underline = Pango.Underline.Single;
		else
			cell.Underline = Pango.Underline.None;
	}
	
	private void OnStartCellEdited (object o, EditedArgs args) {
		if ((o as CellRendererSpinButton).TimingMode == TimingMode.Frames)
			subtitles.GetSubtitle(args.Path).Frames.Start = Convert.ToInt32(args.NewText);
		else
			subtitles.GetSubtitle(args.Path).Times.Start = TimeSpan.Parse(args.NewText);
    }
    
    private void OnEndCellEdited (object o, EditedArgs args) {
		if ((o as CellRendererSpinButton).TimingMode == TimingMode.Frames)
			subtitles.GetSubtitle(args.Path).Frames.End = Convert.ToInt32(args.NewText);
		else
			subtitles.GetSubtitle(args.Path).Times.End = TimeSpan.Parse(args.NewText);
    }
    
     private void OnDurationCellEdited (object o, EditedArgs args) {
		if ((o as CellRendererSpinButton).TimingMode == TimingMode.Frames)
			subtitles.GetSubtitle(args.Path).Frames.Duration = Convert.ToInt32(args.NewText);
		else
			subtitles.GetSubtitle(args.Path).Times.Duration = TimeSpan.Parse(args.NewText);
    }
    
}

}