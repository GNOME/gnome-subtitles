/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2008 Pedro Castro
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
using Gtk;
using Mono.Unix;
using System;
using System.Collections;
using System.Text;

namespace GnomeSubtitles.Dialog {

public abstract class SubtitleFileChooserDialog : GladeDialog {
	private int fixedEncoding = -1;
	private bool isEncodingsChangeSilent = false; //used to indicate whether a change in the encodings list should be taken into account

	/* Protected variables */
	protected new FileChooserDialog dialog = null;
	
	protected string chosenFilename = String.Empty;
	protected EncodingDescription chosenEncoding;
	protected bool hasChosenEncoding = false;
	
	protected EncodingDescription[] encodings = null;

	/* Widgets */
	
	[WidgetAttribute] private ComboBox encodingComboBox = null;

	protected SubtitleFileChooserDialog (string filename, bool persistent) : base(filename, persistent, true) {
		dialog = base.dialog as FileChooserDialog;
		
		fixedEncoding = GetFixedEncoding();
		SetEncodingComboBox();
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

	protected void SetEncodingComboBox () {
		AddInitialEncodingComboBoxItems(encodingComboBox);
		encodingComboBox.RowSeparatorFunc = SeparatorFunc;
		FillEncodingComboBox(Global.Config.PrefsEncodingsShownInMenu);
	}
		
	protected bool SeparatorFunc (TreeModel model, TreeIter iter) {
		string text = (string)model.GetValue(iter, 0);
		return ((text != null) && (text.CompareTo("-") == 0));
	}
	
	protected int GetActiveEncodingComboBoxItem () {
		return encodingComboBox.Active;
	}
	
	/* Virtual members */
	
	protected virtual int GetFixedEncoding () {
		return -1;
	}
	
	protected virtual void AddInitialEncodingComboBoxItems (ComboBox comboBox) {
	}
	
	
	
	/* Private members */
	
	private void LoadEncodings (string[] names) {
		ArrayList encodings = new ArrayList();
		bool fixedCodePageInserted = false;
		
		foreach (string name in names) {
			EncodingDescription description = new EncodingDescription();
			if (Encodings.Find(name, ref description)) {
				encodings.Add(description);
				if (description.CodePage == fixedEncoding)
					fixedCodePageInserted = true;
			}
		}
		
		/* Insert fixed encoding code page if not already inserted */
		if ((fixedEncoding != -1) && (!fixedCodePageInserted)) {
			EncodingDescription description = new EncodingDescription();
			if (Encodings.Find(fixedEncoding, ref description))
				encodings.Add(description);		
		}

		encodings.Sort();
		encodings.Insert(0, Encodings.SystemDefault);
		
		this.encodings = (EncodingDescription[])encodings.ToArray(typeof(EncodingDescription));	
	}
	
	private void FillEncodingComboBox (string[] names) {
		LoadEncodings(names);
		int activeItem = 0;
		
		int currentItem = 0;
		foreach (EncodingDescription encoding in encodings) {
			encodingComboBox.AppendText(encoding.Description + " (" + encoding.Name + ")");
			if (encoding.CodePage == fixedEncoding) {
				activeItem = currentItem;
			}
			currentItem++;
		}

		encodingComboBox.AppendText("-");
		encodingComboBox.AppendText(Catalog.GetString("Add or Remove..."));
		
		encodingComboBox.Active = activeItem;
	}

	private void UpdateEncodingComboBox (ComboBox comboBox, string[] names) {
		isEncodingsChangeSilent = true;
	
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
		
		FillEncodingComboBox(names);
		
		isEncodingsChangeSilent = false;
	}
	
	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnEncodingComboBoxChanged (object o, EventArgs args) {
		if (isEncodingsChangeSilent)
			return;

		ComboBox comboBox = o as ComboBox;
		int itemCount = comboBox.Model.IterNChildren();
		if (comboBox.Active == (itemCount - 1)) {
			EncodingsDialog dialog = new EncodingsDialog();
			dialog.Show();
			dialog.WaitForResponse();
			UpdateEncodingComboBox(comboBox, dialog.ChosenNames);
		}
		
	}

}

}
