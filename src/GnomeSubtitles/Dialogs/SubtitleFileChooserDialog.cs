/*
 * This file is part of Gnome Subtitles.
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
using System.Collections;
using System.Text;

namespace GnomeSubtitles {

public class SubtitleFileChooserDialog : GladeDialog {

	/* Protected variables */
	protected new FileChooserDialog dialog = null;
	
	protected string chosenFilename = String.Empty;
	protected EncodingDescription chosenEncoding;
	protected bool hasChosenEncoding = false;
	
	protected EncodingDescription[] encodings = null;

	public SubtitleFileChooserDialog (string filename) : base(filename) {
		dialog = base.dialog as FileChooserDialog;
	}
	
	/* Public properties */
	
	public string Filename {
		get { return chosenFilename; }
	}
	
	public EncodingDescription ChosenEncoding {
		get { return chosenEncoding; }
	}
	
	public bool HasChosenEncoding {
		get { return hasChosenEncoding; }
	}
	
	/* Protected members */

	protected void SetEncodingComboBox (ComboBox comboBox) {
		comboBox.RowSeparatorFunc = SeparatorFunc;
		FillEncodingComboBox(comboBox, Global.Config.PrefsEncodingsShownInMenu);
	}
		
	protected bool SeparatorFunc (TreeModel model, TreeIter iter) {
		string text = (string)model.GetValue(iter, 0);
		return ((text != null) && (text.CompareTo("-") == 0));
	}
	
	/* Private members */
	
	private void LoadEncodings (string[] names) {
		ArrayList encodings = new ArrayList();
		
		foreach (string name in names) {
			EncodingDescription description = new EncodingDescription();
			if (Encodings.Find(name, ref description))
				encodings.Add(description);
		}
		
		encodings.Sort();
		encodings.Insert(0, Encodings.SystemDefault);
		
		this.encodings = (EncodingDescription[])encodings.ToArray(typeof(EncodingDescription));	
	}
	
	private void FillEncodingComboBox (ComboBox comboBox, string[] names) {
		LoadEncodings(names);
		
		foreach (EncodingDescription encoding in encodings)
			comboBox.AppendText(encoding.Description + " (" + encoding.Name + ")");
	
		comboBox.AppendText("-");
		comboBox.AppendText("Add or Remove...");
		
		comboBox.Active = 0;
	}
	
	private void UpdateEncodingComboBox (ComboBox comboBox, string[] names) {
	
		/* Get the first elements that are not in the model */
		int itemCount = comboBox.Model.IterNChildren();
		int knownCount = encodings.Length + 2; //Add 2 for horizontal bar and item at the end
		int remainingCount = itemCount - knownCount;
		
		string[] remainingItems = new string[remainingCount];
		int rowNumber = 0;
		ListStore store = comboBox.Model as ListStore;
		foreach (object[] row in store) {
			if (rowNumber == remainingCount)
				break;
			
			remainingItems[rowNumber] = row[0] as string;
			rowNumber++;			
		}
		
		store.Clear();
		
		foreach (string item in remainingItems)
			comboBox.AppendText(item);
		
		FillEncodingComboBox(comboBox, names);
	}
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnEncodingComboBoxChanged (object o, EventArgs args) {
		ComboBox comboBox = o as ComboBox;
		int itemCount = comboBox.Model.IterNChildren();
		if (comboBox.Active == (itemCount - 1)) {
			EncodingsDialog dialog = new EncodingsDialog();
			dialog.WaitForResponse();
			UpdateEncodingComboBox(comboBox, dialog.ChosenNames);
		}
		
	}

}

}
