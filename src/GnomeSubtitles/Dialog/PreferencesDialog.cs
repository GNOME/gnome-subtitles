/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2010 Pedro Castro
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
using GnomeSubtitles.Core;
using GnomeSubtitles.Ui.Component;
using Gtk;
using Mono.Unix;
using System;

namespace GnomeSubtitles.Dialog {

public class PreferencesDialog : GladeDialog {

	/* Constant strings */
	private const string gladeFilename = "PreferencesDialog.glade";

	/* Components */
	private EncodingComboBox fileOpenEncoding = null;

	/* Widgets */
	[WidgetAttribute] private CheckButton videoAutoChooseFileCheckButton = null;
	[WidgetAttribute] private ComboBox fileOpenEncodingComboBox = null;

	public PreferencesDialog () : base(gladeFilename, false) {
		LoadValues();
		Autoconnect();
	}

	/* Private members */
	
	private void LoadValues () {
		SetDefaultsFileOpenEncoding();


		/* Video Auto choose file */
		videoAutoChooseFileCheckButton.Active = Base.Config.PrefsVideoAutoChooseFile;

		
		
	}

	private void SetDefaultsFileOpenEncoding () {
		string[] fileOpenAdditionalActions = { Catalog.GetString("Remember Last Used") };
		int fixedEncoding = -1;
		ConfigFileOpenEncodingOption fileOpenEncodingOption = Base.Config.PrefsDefaultsFileOpenEncodingOption;
		if (fileOpenEncodingOption == ConfigFileOpenEncodingOption.Specific) {
			string encodingName = Base.Config.PrefsDefaultsFileOpenEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingName, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		fileOpenEncoding = new EncodingComboBox(fileOpenEncodingComboBox, true, fileOpenAdditionalActions, fixedEncoding);
		if (fileOpenEncodingOption != ConfigFileOpenEncodingOption.Specific) {
			fileOpenEncoding.ActiveSelection = (int)fileOpenEncodingOption;
		}
		fileOpenEncoding.SelectionChanged += OnDefaultsFileOpenEncodingChanged;
	}

	
	/* Event members */

	#pragma warning disable 169		//Disables warning about handlers not being used

	private void OnDefaultsFileOpenEncodingChanged (object o, EventArgs args) {
		int active = fileOpenEncoding.ActiveSelection;
		ConfigFileOpenEncodingOption activeOption = (ConfigFileOpenEncodingOption)Enum.ToObject(typeof(ConfigFileOpenEncodingOption), active);
		if (((int)activeOption) > ((int)ConfigFileOpenEncodingOption.Specific))
			activeOption = ConfigFileOpenEncodingOption.Specific;

		Base.Config.PrefsDefaultsFileOpenEncodingOption = activeOption;
		/* If encoding option is specific, encodingOption=Specific and encoding holds the encoding name */
		if (activeOption == ConfigFileOpenEncodingOption.Specific) {
			EncodingDescription chosenEncoding = fileOpenEncoding.ChosenEncoding;
			if (!chosenEncoding.Equals(EncodingDescription.Empty)) {
				Base.Config.PrefsDefaultsFileOpenEncodingFixed = chosenEncoding.Name;
			}
		}
		else {
			/* If encoding option is current locale, encoding holds current locale too, otherwise it just holds auto detect */
			ConfigFileOpenEncoding encodingToStore = ConfigFileOpenEncoding.AutoDetect;
			if (activeOption == ConfigFileOpenEncodingOption.CurrentLocale) {
				encodingToStore = ConfigFileOpenEncoding.CurrentLocale;
			}
			Base.Config.PrefsDefaultsFileOpenEncoding = encodingToStore;
		}
	}

	private void OnVideoAutoChooseFileToggled (object o, EventArgs args) {
		Base.Config.PrefsVideoAutoChooseFile = videoAutoChooseFileCheckButton.Active;
	}

}

}
