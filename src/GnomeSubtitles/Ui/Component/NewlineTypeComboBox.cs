/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2017 Pedro Castro
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
using Mono.Unix;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Ui.Component {

public class NewlineTypeComboBox {

	private ComboBoxText comboBox = null;
	private NewlineType newlineTypeToSelect = NewlineType.Unknown;
	private string[] additionalActions = null;

	public NewlineTypeComboBox (NewlineType newlineTypeToSelect, string[] additionalActions) {
		this.comboBox = new ComboBoxText();
		this.newlineTypeToSelect = newlineTypeToSelect;
		this.additionalActions = additionalActions;

		FillComboBox();
		ConnectHandlers();
	}

	/* Events */

	public event EventHandler SelectionChanged;


	/* Public properties */

	public ComboBoxText Widget {
		get { return comboBox; }
	}

	public bool HasChosenAction {
		get { return comboBox.Active < GetActionCount(); }
	}

	public int ChosenAction {
		get { return (HasChosenAction ? comboBox.Active : -1); }
	}

	public NewlineType ChosenNewlineType {
		get {
			int active = comboBox.Active;
			int actionCount = GetActionCount();
			if (active < actionCount) //An action is active
				return NewlineType.Unknown;
			else {
				int newlineTypePosition = active - (actionCount > 0 ? actionCount + 1 : 0) + 1; //plus 1 because NewlineType 0 is unknown
				return (NewlineType)Enum.ToObject(typeof(NewlineType), newlineTypePosition);
			}
		}
		set {
			int actionCount = GetActionCount();
			int position = (int)value - 1 + (actionCount > 0 ? actionCount + 1 : 0);
			this.ActiveSelection = position;
		}
	}

	public int ActiveSelection {
		get { return comboBox.Active; }
		set { SetActiveItem(value, false); }
	}


	/* Private members */

	private void FillComboBox () {
		DisconnectComboBoxChangedSignal();

		(comboBox.Model as ListStore).Clear();

		bool hasAdditionalActions = (additionalActions != null) && (additionalActions.Length > 0);

		/* Add additional actions */
		if (hasAdditionalActions) {
			foreach (string additionalAction in additionalActions) {
				comboBox.AppendText(additionalAction);
			}
			comboBox.AppendText("-");
		}

		/* Prepare newline types to add */
		string mac = "Mac OS Classic";
		string unix = "Unix/Linux";
		string windows = "Windows";
		string systemDefault = " (" + Catalog.GetString("System Default") + ")";
		NewlineType systemNewline = Core.Util.GetSystemNewlineType();
		SetSystemNewlineSuffix(systemNewline, ref mac, ref unix, ref windows, systemDefault);

		/* Add newline types */
		comboBox.AppendText(mac);
		comboBox.AppendText(unix);
		comboBox.AppendText(windows);

		if (newlineTypeToSelect != NewlineType.Unknown) {
			int activeItem = (int)newlineTypeToSelect - 1 + (hasAdditionalActions ? additionalActions.Length + 1 : 0);
			SetActiveItem(activeItem, false); //Don't use silent change because the signal is already disabled
		}

		ConnectComboBoxChangedSignal();
	}

	private void SetActiveItem (int item, bool silent) {
		int itemCount = comboBox.Model.IterNChildren();
		if (itemCount == 0)
			return;

		if (silent)
			DisconnectComboBoxChangedSignal();

		comboBox.Active = item;

		if (silent)
			ConnectComboBoxChangedSignal();
	}

	private int GetActionCount () {
		return (additionalActions != null ? additionalActions.Length : 0);
	}

	private void SetSystemNewlineSuffix (NewlineType newline, ref string mac, ref string unix, ref string windows, string suffix) {
		switch (newline) {
			case NewlineType.Macintosh:
				mac += suffix;
				break;
			case NewlineType.Unix:
				unix += suffix;
				break;
			case NewlineType.Windows:
				windows += suffix;
				break;
		}
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
		if (SelectionChanged != null) {
			SelectionChanged(o, args);
		}
	}

}

}
