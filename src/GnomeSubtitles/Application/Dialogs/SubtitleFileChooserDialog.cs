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
using System.Text;

namespace GnomeSubtitles {

public class SubtitleFileChooserDialog : GladeDialog {
	protected new FileChooserDialog dialog = null;
	protected FileEncoding[] encodings = {
		new FileEncoding(65001, "Unicode", "UTF-8"),
		new FileEncoding(28605, "Western", "ISO-8859-15"),
		new FileEncoding("Current Locale")
	};

	public SubtitleFileChooserDialog (GUI gui, string widgetName) : base(gui, widgetName) {
		dialog = base.dialog as FileChooserDialog;
	}
	
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

public class FileEncoding {
	private int codePage;
	private string description;
	private string name;

	public FileEncoding (int codePage, string description, string name) {
		this.codePage = codePage;
		this.description = description;
		this.name = name;
	}
	
	public FileEncoding (int codePage, string description) : this(codePage, description, Encoding.GetEncoding(codePage).EncodingName) {}
	
	public FileEncoding (string description) : this(Encoding.Default.CodePage, description, Encoding.Default.EncodingName) {}
	
	
	public int CodePage {
		get { return codePage; }
	}
	
	public string Description {
		get { return description; }
	}
	
	public string Name {
		get { return name; }
	}



}

}