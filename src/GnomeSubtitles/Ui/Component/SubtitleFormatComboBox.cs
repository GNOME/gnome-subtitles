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
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Ui.Component {

public class SubtitleFormatComboBox {

	private ComboBoxText comboBox = null;
	private SubtitleTypeInfo[] subtitleTypes = null;
	private string[] additionalActions = null; //
	private SubtitleType fixedSubtitleType = SubtitleType.Unknown; //A subtitle type that must be selected

	public SubtitleFormatComboBox (SubtitleType fixedSubtitleType, string[] additionalActions) {
		this.comboBox = new ComboBoxText();
		this.fixedSubtitleType = fixedSubtitleType;
		this.additionalActions = additionalActions;

		this.subtitleTypes = Subtitles.AvailableTypesSorted;
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

	public SubtitleType ChosenSubtitleType {
		get {
			int active = comboBox.Active;
			int actionCount = GetActionCount();
			if (active < actionCount) //An action is active
				return SubtitleType.Unknown;
			else
				return subtitleTypes[active - (actionCount > 0 ? actionCount + 1 : 0)].Type; //1 for break line
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

		int currentItem = 0;
		int activeItem = 0;

		bool hasAdditionalActions = (additionalActions != null) && (additionalActions.Length > 0);

		/* Add additional actions */
		if (hasAdditionalActions) {
			foreach (string additionalAction in additionalActions) {
				comboBox.AppendText(additionalAction);
			}
			comboBox.AppendText("-");
			currentItem += additionalActions.Length + 1;
		}

		/* Add subtitle formats */
		foreach (SubtitleTypeInfo typeInfo in subtitleTypes) {
			comboBox.AppendText(typeInfo.Name + " (" + ConcatenateExtensions(typeInfo.Extensions) + ")");
			if (typeInfo.Type == fixedSubtitleType) {
				activeItem = currentItem;
			}
			currentItem++;
		}

		SetActiveItem(activeItem, false); //Don't use silent change because the signal is already disabled

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

	private string ConcatenateExtensions (string[] extensions) {
		string result = "";
		if (extensions == null) {
			return result;
		}

		foreach (string extension in extensions) {
			result += (String.IsNullOrEmpty(result) ? "" : ", ") + "." + extension;
		}

		return result;
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
