/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008-2020 Pedro Castro
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SubLib.Core.Domain;
using SubLib.Util;

namespace GnomeSubtitles.Core {

/* Delegates */
public delegate void LanguageListHandler (string langTag, string providerName, string providerDesc, string providerFile, IntPtr userdata);

public delegate void ProviderListHandler (string providerName, string providerDesc, string providerFile, IntPtr userdata);

public class SpellLanguages {
	private bool enabled = false;
	private ArrayList providers = null;
	private ArrayList languages = null;
	private int activeTextLanguageIndex = -1;
	private int activeTranslationLanguageIndex = -1;

	private LanguageListHandler languageListHandler = null;
	private ProviderListHandler providerListHandler = null;

	public SpellLanguages () {
		languageListHandler = OnLanguageList;
		providerListHandler = OnProviderList;
		
		Init();
		GetEnabledFromConfig();
	}

	/* Events */
	public event BasicEventHandler ToggleEnabled = null;
	public event BasicEventHandler TextLanguageChanged = null;
	public event BasicEventHandler TranslationLanguageChanged = null;


	/* Public members */

	public ArrayList Providers {
		get { return providers;	}
	}

	public ArrayList Languages {
		get { return languages;	}
	}

	public int ActiveTextLanguageIndex {
		get { return GetActiveLanguageIndex(SubtitleTextType.Text); }
	}

	public int ActiveTranslationLanguageIndex {
		get { return GetActiveLanguageIndex(SubtitleTextType.Translation); }
	}

	public SpellLanguage ActiveTextLanguage {
		get { return GetActiveLanguage(SubtitleTextType.Text); }
	}

	public SpellLanguage ActiveTranslationLanguage {
		get { return GetActiveLanguage(SubtitleTextType.Translation); }
	}

	public bool HasActiveTextLanguage {
		get { return ActiveTextLanguageIndex != -1; }
	}

	public bool HasActiveTranslationLanguage {
		get { return ActiveTranslationLanguageIndex != -1; }
	}

	public bool Enabled {
		set {
			if (value != enabled) {
				enabled = value;
				Base.Config.SpellCheckAuto = value;
				EmitToggleEnabled();
			}
		}
		get { return enabled; }
	}

	public int GetActiveLanguageIndex (SubtitleTextType textType) {
		if (textType == SubtitleTextType.Text)
			return activeTextLanguageIndex;
		else
			return activeTranslationLanguageIndex;
	}

	public SpellLanguage GetActiveLanguage (SubtitleTextType textType) {
		int index = GetActiveLanguageIndex(textType);
		return GetLanguage(index);
	}

	public SpellLanguage GetLanguage (int index) {
		if ((index < 0) || (index >= languages.Count))
			return null;
		else
			return languages[index] as SpellLanguage;
	}

	public void SetActiveLanguage (SubtitleTextType textType, string languageID) {
		int index = GetLanguageIndex(languageID);
		SetActiveLanguageIndex(textType, index);
	}

	public void SetActiveLanguageIndex (SubtitleTextType textType, int index) {
		bool isEmpty = ((index < 0) || (index >= languages.Count));

		SpellLanguage activeLanguage = null;
		if (isEmpty)
			index = -1;
		else
			activeLanguage = languages[index] as SpellLanguage;

		Logger.Info("[Spellcheck] Setting active {0} language: {1}", textType, (activeLanguage == null ? "none." : activeLanguage.ID));

		/* Set index variable */
		if (textType == SubtitleTextType.Text)
			activeTextLanguageIndex = index;
		else
			activeTranslationLanguageIndex = index;

		string activeLanguageID = (isEmpty ? String.Empty : activeLanguage.ID);
		SetActiveLanguageInConfig(textType, activeLanguageID);

		EmitLanguageChanged(textType);
	}

	/* LibEnchant imports */

	[DllImport ("libenchant")]
	static extern IntPtr enchant_broker_init ();

	[DllImport ("libenchant")]
	static extern void enchant_broker_free (IntPtr broker);

	[DllImport ("libenchant")]
	static extern void enchant_broker_list_dicts (IntPtr broker, LanguageListHandler cb, IntPtr userdata);

	[DllImport ("libenchant")]
	static extern void enchant_broker_describe (IntPtr broker, ProviderListHandler cb, IntPtr userdata);


	/* Private members */

	private void Init () {
		/* Providers */
		providers = new ArrayList();
		
		/* Languages */
		languages = new ArrayList();
		activeTextLanguageIndex = -1;
		activeTranslationLanguageIndex = -1;

		GetProvidersAndLanguages();
		GetActiveLanguagesFromConfig();
	}

	private void GetProvidersAndLanguages () {
		IntPtr broker = enchant_broker_init ();
		if (broker == IntPtr.Zero)
			return;

		enchant_broker_describe (broker, providerListHandler, IntPtr.Zero);

		enchant_broker_list_dicts (broker, languageListHandler, IntPtr.Zero);

		enchant_broker_free(broker);
		 
		languages.Sort();
		Logger.Info("[Spellcheck] Found {0} providers: {1}", providers.Count, string.Join(",", providers.ToArray()));
		Logger.Info("[Spellcheck] Found {0} languages: {1}", languages.Count, GetLanguageIDsAsString(languages));
	}

	private string GetLanguageIDsAsString (ArrayList languages) {
		List<string> ids = new List<string>();
		foreach (SpellLanguage language in languages) {
			ids.Add(language.ID);
		}

		return String.Join(",", ids);
	}

	private void GetActiveLanguagesFromConfig () {
		string activeTextLanguage = Base.Config.SpellCheckTextLanguage;
		this.activeTextLanguageIndex = GetLanguageIndex(activeTextLanguage);

		string activeTranslationLanguage = Base.Config.SpellCheckTranslationLanguage;
		this.activeTranslationLanguageIndex = GetLanguageIndex(activeTranslationLanguage);
	}

	private void GetEnabledFromConfig () {
		this.enabled = Base.Config.SpellCheckAuto;
	}

	private void SetActiveLanguageInConfig (SubtitleTextType textType, string activeLanguage) {
		if (textType == SubtitleTextType.Text)
			Base.Config.SpellCheckTextLanguage = activeLanguage;
		else
			Base.Config.SpellCheckTranslationLanguage = activeLanguage;
	}

	private int GetLanguageIndex (string languageID) {
		for (int index = 0 ; index < languages.Count ; index++) {
			SpellLanguage language = languages[index] as SpellLanguage;
			if (language.ID == languageID)
				return index;
		}
		return -1;
	}


	/* Event members */

	private void OnLanguageList (string langTag, string providerName, string providerDesc, string providerFile, IntPtr userdata) {
		SpellLanguage language = new SpellLanguage(langTag);
		if (!languages.Contains(language))
			languages.Add(language);
	}
	
	private void OnProviderList (string providerName, string providerDesc, string providerFile, IntPtr userdata) {
		providers.Add(providerName);
	}

	private void EmitToggleEnabled () {
    	if (this.ToggleEnabled != null)
    		this.ToggleEnabled();
    }

    private void EmitLanguageChanged (SubtitleTextType textType) {
    	if (textType == SubtitleTextType.Text)
    		EmitTextLanguageChanged();
    	else
    		EmitTranslationLanguageChanged();
    }

    private void EmitTextLanguageChanged () {
    	if (this.TextLanguageChanged != null)
    		this.TextLanguageChanged();
    }

    private void EmitTranslationLanguageChanged () {
    	if (this.TranslationLanguageChanged != null)
    		this.TranslationLanguageChanged();
    }

}

}
