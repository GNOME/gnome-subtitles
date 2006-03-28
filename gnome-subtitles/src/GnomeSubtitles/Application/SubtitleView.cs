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
using SubLib;

namespace GnomeSubtitles {

public class SubtitleView : GladeWidget {
	private TreeView treeView = null;
	
	private Subtitles subtitles = null;
	private TreeViewColumn numberCol = null;
	private TreeViewColumn startCol = null;
	private TreeViewColumn endCol = null;
	private TreeViewColumn durationCol = null;
	private TreeViewColumn textCol = null;

	
	public SubtitleView(GUI gui, Glade.XML glade) : base(gui, glade){
		treeView = (TreeView)GetWidget(WidgetNames.SubtitleView);
		CreateColumns();
    }

    public TreeView Widget {
    		get { return treeView; }
    }
    
    public Subtitle Selected {
    		get {
    			TreeIter iter;
    			treeView.Selection.GetSelected(out iter);
			return subtitles.GetSubtitle(iter);
		}
    }
  
    public void Show () {
	    	subtitles = GUI.Core.Subtitles;
	    	treeView.Selection.Changed -= OnSelected;
	    treeView.Model = subtitles.Model;
	    ScrolledWindow scrollArea = (ScrolledWindow)treeView.Parent;
	    scrollArea.Sensitive = true;
	    scrollArea.Visible = true;
	    scrollArea.ShadowType = ShadowType.In;	
	    UpdateTimingMode();
	    treeView.Selection.Changed += OnSelected;
	    treeView.Selection.SelectPath(TreePath.NewFirst());
	    treeView.GrabFocus();
    }
    
    public void UpdateTimingMode () {
	    Refresh();
    }
    
    public void Refresh () {
	    treeView.QueueDraw();
    }
    
    public void UpdateSelectedRow () {
    		TreeIter iter;
    		treeView.Selection.GetSelected(out iter);
    		subtitles.EmitSubtitleChanged(iter);
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
 	
    private void CreateColumns() {
    		numberCol = CreateColumn("No.", ColumnWidth("000"), new CellRendererText(), RenderNumberCell);
    		
    		int timeWidth = ColumnWidth("00:00:00.000");
    		startCol = CreateColumn("From", timeWidth, new CellRendererText(), RenderStartCell);
    		endCol = CreateColumn("To", timeWidth, new CellRendererText(), RenderEndCell);
    		durationCol = CreateColumn("During", timeWidth, new CellRendererText(), RenderDurationCell);
    		
    		int textWidth = ColumnWidth("0123456789012345678901234567890123456789");
    		textCol = CreateColumn("Text", textWidth, new CellRendererCenteredText(), RenderTextCell);
    		    		
    		treeView.AppendColumn(numberCol);
    		treeView.AppendColumn(startCol);
    		treeView.AppendColumn(endCol);
    		treeView.AppendColumn(durationCol);
    		treeView.AppendColumn(textCol);
    		treeView.AppendColumn(new TreeViewColumn());
    }

	private int ColumnWidth (string text) {
		const int margins = 10;
		return Utility.TextWidth(treeView, text, margins);
	}
	
	private void OnSelected (object o, EventArgs args) {
		GUI.SubtitleSelected(this.Selected);
	}

	private void RenderNumberCell (TreeViewColumn column, CellRenderer cell, TreeModel treeModel, TreeIter iter) {
		(cell as CellRendererText).Text = (treeModel.GetPath(iter).Indices[0] + 1).ToString();
	}

	private void RenderStartCell (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
		CellRendererText cellRenderer = (CellRendererText)cell;
		if (GUI.Core.Subtitles.Properties.TimingMode == TimingMode.Frames)
			cellRenderer.Text = subtitles.GetSubtitle(iter).Frames.Start.ToString();
		else
			cellRenderer.Text = Utility.TimeSpanToText(subtitles.GetSubtitle(iter).Times.Start);
	}
	
	private void RenderEndCell (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
		CellRendererText cellRenderer = (CellRendererText)cell;
		if (GUI.Core.Subtitles.Properties.TimingMode == TimingMode.Frames)
			cellRenderer.Text = subtitles.GetSubtitle(iter).Frames.End.ToString();
		else
			cellRenderer.Text = Utility.TimeSpanToText(subtitles.GetSubtitle(iter).Times.End);
	}
	
	private void RenderDurationCell (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
		CellRendererText cellRenderer = (CellRendererText)cell;
		if (GUI.Core.Subtitles.Properties.TimingMode == TimingMode.Frames)
			cellRenderer.Text = subtitles.GetSubtitle(iter).Frames.Duration.ToString();
		else
			cellRenderer.Text = Utility.TimeSpanToText(subtitles.GetSubtitle(iter).Times.Duration);
	}
	
	private void RenderTextCell (TreeViewColumn column, CellRenderer cellRenderer, TreeModel treeModel, TreeIter iter) {
		CellRendererText cell = (CellRendererText)cellRenderer;
		Subtitle subtitle = subtitles.GetSubtitle(iter);
		if (subtitle.Text.IsEmpty) {
			cell.Text = " ";
			return;
		}
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
		
}


public class CellRendererCenteredText : CellRendererText {

	protected override void Render (Drawable window, Widget widget, Rectangle backgroundArea,
			Rectangle cellArea, Rectangle exposeArea, CellRendererState flags) {

		int xOffset, yOffset, width, height;
		GetSize(widget, ref cellArea, out xOffset, out yOffset, out width, out height);

		StateType state;
		if (!this.Sensitive)
			state = StateType.Insensitive;
		else if ((flags & CellRendererState.Selected) == CellRendererState.Selected)
			state = (widget.HasFocus ? StateType.Selected : StateType.Active);
		else if (((flags & CellRendererState.Prelit) == CellRendererState.Prelit) && (widget.State == StateType.Prelight))
			state = StateType.Prelight;
		else
			state = (widget.State == StateType.Insensitive ? StateType.Insensitive : StateType.Normal);
		
		Pango.Layout layout = widget.CreatePangoLayout(null);
		layout.Alignment = Pango.Alignment.Center;
		Pango.FontDescription fontDescription = new Pango.FontDescription();
		fontDescription.Style = Style;
		fontDescription.Weight = (Pango.Weight)Weight;
		layout.FontDescription = fontDescription;
		
		if (Underline != Pango.Underline.None)
			layout.SetMarkup("<u>" + Text + "</u>");
		else
			layout.SetText(Text);

		Gtk.Style.PaintLayout(widget.Style, window, state, true, cellArea, widget,
			"cellrenderertext", cellArea.X + xOffset, cellArea.Y + yOffset, layout);			
	}

}


}