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

using GnomeSubtitles.Core;
using GnomeSubtitles.Dialog;
using Gtk;
using Mono.Unix;
using System;
using System.Collections;

namespace GnomeSubtitles.Ui.Component {

public class EncodingComboBox {

	private ComboBoxText comboBox = null;
	private int comboBoxActiveItem = 0; //Stores the last active item so we can get back to it after using Add/Remove

	private ArrayList encodings = null; //Encodings present in the combo box
	private int actionCount = 0; //Actions correspond to the initial items (including auto detection)
	private bool hasAutoDetect = false; //Whether to add Auto Detect to the top of the item list
	private string[] additionalActions = null; //
	private int fixedEncoding = -1; //Codepage of an encoding that must be present (-1 if not)
	private ArrayList configShownEncodings = null; //Encodings shown in menu from config

	public EncodingComboBox (bool hasAutoDetect, string[] additionalActions, int fixedEncoding) {
		this.comboBox = new ComboBoxText();
		this.additionalActions = additionalActions;
		this.hasAutoDetect = hasAutoDetect;
		this.fixedEncoding = fixedEncoding;

		SetActionCount();
		SetComboBox(Base.Config.FileEncodingsShownInMenu);
		ConnectHandlers();
	}

	public EncodingComboBox (bool hasAutoDetect) : this(hasAutoDetect, null, -1) {
	}


	/* Events */

	public event EventHandler SelectionChanged;


	/* Public properties */

	public ComboBoxText Widget {
		get { return comboBox; }
	}

	public bool HasChosenAction {
		get { return comboBox.Active < actionCount; }
	}

	public int ChosenAction {
		get { return (HasChosenAction ? comboBox.Active : -1); }
	}

	public EncodingDescription ChosenEncoding {
		get {
			int active = comboBox.Active;
			if (active < actionCount) //An action is active
				return EncodingDescription.Empty;
			else
				return (EncodingDescription)encodings[active - (actionCount > 0 ? actionCount + 1 : 0)]; //1 for break line
		}
	}

	public bool IsChosenCurrentLocale {
		get {
			if (actionCount > 0)
				return comboBox.Active == actionCount + 1;
			else
				return comboBox.Active == 0;
		}
	}

	public int ActiveSelection {
		get { return comboBox.Active; }
		set { SetActiveItem(value, false); }
	}


	/* Private members */

	private void SetActionCount () {
		this.actionCount = (hasAutoDetect ? 1 : 0) + (additionalActions != null ? additionalActions.Length : 0);
	}

	private void SetComboBox (string[] codes) {
		configShownEncodings = new ArrayList(codes);
		LoadEncodings();
		FillComboBox();
	}

	private void LoadEncodings () {
		bool toAddFixedEncoding = (fixedEncoding != -1);
		ArrayList encodings = new ArrayList();

		foreach (string code in configShownEncodings) {
			EncodingDescription description = EncodingDescription.Empty;
			if (Encodings.Find(code, ref description)) {
				encodings.Add(description);
				if (toAddFixedEncoding && (description.CodePage == fixedEncoding))
					toAddFixedEncoding = false;
			}
		}

		if (toAddFixedEncoding) {
			EncodingDescription description = EncodingDescription.Empty;
			if (Encodings.Find(fixedEncoding, ref description))
				encodings.Add(description);
		}

		encodings.Sort();
		encodings.Insert(0, Encodings.SystemDefault);

		this.encodings = encodings;
	}

	private void FillComboBox () {
		DisconnectComboBoxChangedSignal();

		(comboBox.Model as ListStore).Clear();

		int activeItem = comboBoxActiveItem;
		int currentItem = 0;

		/* Add auto detect */
		if (hasAutoDetect) {
			AddAutoDetect();
			currentItem ++;
		}

		/* Add additional actions */
		if (additionalActions != null) {
			foreach (string additionalAction in additionalActions) {
				comboBox.AppendText(additionalAction);
				currentItem++;
			}
		}

		if (currentItem != 0) {
			comboBox.AppendText("-");
			currentItem++;
		}

		/* Add encodings */
		foreach (EncodingDescription encoding in encodings) {
			comboBox.AppendText(encoding.Region + " (" + encoding.Name + ")");
			if (encoding.CodePage == fixedEncoding) {
				activeItem = currentItem;
			}
			currentItem++;
		}

		/* Add add/remove action */
		comboBox.AppendText("-");
		comboBox.AppendText(Catalog.GetString("Add or Remove…"));

		SetActiveItem(activeItem, false); //Don't use silent change because the signal is already disabled

		ConnectComboBoxChangedSignal();
	}

	private void AddAutoDetect () {
		comboBox.AppendText(Catalog.GetString("Automatically Detected"));
	}

	private void SetActiveItem (int item, bool silent) {
		int itemCount = comboBox.Model.IterNChildren();
		if (itemCount == 0)
			return;

		if (silent)
			DisconnectComboBoxChangedSignal();

		comboBoxActiveItem = (item < itemCount - 2 ? item : 0);
		comboBox.Active = comboBoxActiveItem;

		if (silent)
			ConnectComboBoxChangedSignal();
	}

	/* Event members */

	private void ConnectHandlers () {
		comboBox.RowSeparatorFunc = ComboBoxUtil.SeparatorFunc;
	}

	private void ConnectComboBoxChangedSignal () {
		comboBox.Changed += OnComboBoxChanged;
	}

	private void DisconnectComboBoxChangedSignal () {
		comboBox.Changed -= OnComboBoxChanged;
	}

	private void OnComboBoxChanged (object o, EventArgs args) {
		ComboBox comboBox = o as ComboBox;
		int itemCount = comboBox.Model.IterNChildren();
		int selectedItem = comboBox.Active;

		if (selectedItem == (itemCount - 1)) {
			EncodingsDialog dialog = Base.Dialogs.Get(typeof(EncodingsDialog), comboBox.Toplevel) as EncodingsDialog;
			dialog.Show();
			dialog.WaitForResponse();
			SetComboBox(dialog.ChosenCodes);
		}
		else {
			comboBoxActiveItem = selectedItem;
			if (SelectionChanged != null)
				SelectionChanged(o, args);
		}
	}

}

}
