/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
 * Copyright (C) 2006-2007 Pedro Castro
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
using System.Text;

namespace GnomeSubtitles {

public class SubtitleFileChooserDialog : GladeDialog {

	/* Protected variables */
	protected string chosenFilename = String.Empty;
	protected Encoding chosenEncoding = null;
	protected new FileChooserDialog dialog = null;
	protected FileEncoding[] encodings = {
		new FileEncoding(65001, "Unicode", "UTF-8"),
		new FileEncoding(28605, "Western", "ISO-8859-15"),
		new FileEncoding("Current Locale")
	};

	public SubtitleFileChooserDialog (string filename) : base(filename) {
		dialog = base.dialog as FileChooserDialog;
	}
	
	public string Filename {
		get { return chosenFilename; }
	}
	
	public Encoding Encoding {
		get { return chosenEncoding; }
	}
	
	public bool HasEncoding {
		get { return chosenEncoding != null; }
	}
	
	/* Protected members */
	
	protected void FillEncodingComboBox (ComboBox comboBox) {
		foreach (FileEncoding encoding in encodings)
			comboBox.AppendText(encoding.Description + " (" + encoding.Name + ")");

		comboBox.RowSeparatorFunc = SeparatorFunc;		
		comboBox.Active = 0;
	}
		
	protected bool SeparatorFunc (TreeModel model, TreeIter iter) {
		string text = (string)model.GetValue(iter, 0);
		return ((text != null) && (text.CompareTo("-") == 0));
	}

}

}