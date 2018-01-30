/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2018 Pedro Castro
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
using GnomeSubtitles.Ui;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;
using System.Text;

namespace GnomeSubtitles.Dialog {

//TODO check if we still need all these properties, and whether others should be added (e.g., video/translation file)
public class FilePropertiesDialog : BaseDialog {

	public FilePropertiesDialog () : base() {
		Init(BuildDialog());
	}

	/* Private methods */

	private Gtk.Dialog BuildDialog() {
		FileProperties properties = Base.Document.TextFile;

		Gtk.Dialog dialog = new Gtk.Dialog(Catalog.GetString("File Properties"), Base.Ui.Window, DialogFlags.Modal | DialogFlagsUseHeaderBar);

		Grid grid = new Grid();
		grid.BorderWidth = WidgetStyles.BorderWidthLarge;
		grid.ColumnSpacing = WidgetStyles.ColumnSpacingLarge;
		grid.RowSpacing = WidgetStyles.RowSpacingLarge;

		grid.Attach(CreateFieldLabel(Catalog.GetString("File Name")), 0, 0, 1, 1);
		grid.Attach(CreateValueLabel(properties.Filename), 1, 0, 1, 1);

		grid.Attach(CreateFieldLabel(Catalog.GetString("Path")), 0, 1, 1, 1);
		grid.Attach(CreateValueLabel(properties.Directory), 1, 1, 1, 1);

		grid.Attach(CreateFieldLabel(Catalog.GetString("Character Encoding")), 0, 2, 1, 1);
		grid.Attach(CreateValueLabel(GetEncoding(properties.Encoding)), 1, 2, 1, 1);

		grid.Attach(CreateFieldLabel(Catalog.GetString("Subtitle Format")), 0, 3, 1, 1);
		grid.Attach(CreateValueLabel(GetSubtitleFormat(properties.SubtitleType)), 1, 3, 1, 1);

		grid.Attach(CreateFieldLabel(Catalog.GetString("Timing Mode")), 0, 4, 1, 1);
		grid.Attach(CreateValueLabel(properties.TimingMode.ToString()), 1, 4, 1, 1);

		dialog.ContentArea.Add(grid);
		dialog.ContentArea.ShowAll();
		return dialog;
	}

	private Label CreateFieldLabel (string text) {
		Label label = new Label();
		label.Markup = "<b>" + text + "</b>";
		label.SetAlignment(0f, 0.5f);
		return label;
	}

	private Label CreateValueLabel (string text) {
		Label label = new Label();
		label.Text = (String.IsNullOrEmpty(text) ? "-" : text);
		label.SetAlignment(0f, 0.5f);
		return label;
	}

	private string GetEncoding (Encoding encoding) {
		if (encoding == null) {
			return null;
		}

		return Encodings.GetEncodingName(encoding.CodePage);
	}

	private string GetSubtitleFormat (SubtitleType type) {
		if (type == SubtitleType.Unknown) {
			return null;
		}

		SubtitleTypeInfo typeInfo = Subtitles.GetAvailableType(type);
		return typeInfo.Name;
	}

}

}