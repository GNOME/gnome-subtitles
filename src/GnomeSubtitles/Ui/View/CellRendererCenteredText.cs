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

using Gtk;

namespace GnomeSubtitles.Ui.View {

/// <summary>Provides a cell renderer that centers text with multiple lines.</summary>
//TODO It looks like the Alignment property can now be used. It requires GTK# 2.10.
public class CellRendererCenteredText2 : CellRendererText {

	public CellRendererCenteredText2 () : base() {
		//this.Alignment = Pango.Alignment.Center;
	}

	/*protected override void Render (Drawable window, Widget widget, Rectangle backgroundArea,
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

	}*/

}

}
