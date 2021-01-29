/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008-2021 Pedro Castro
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
using External.Enchant;
using SubLib.Core.Domain;
using SubLib.Util;

namespace GnomeSubtitles.Core {

public class SpellLanguages {
	private bool enabled = false;
	private string[] providers = null;
	private SpellLanguage[] languages = null;
	private int activeTextLanguageIndex = -1;
	private int activeTranslationLanguageIndex = -1;


	public SpellLanguages () {
		Init();
		GetEnabledFromConfig();
	}

	/* Events */
	public event BasicEventHandler EnabledToggled = null;
	public event BasicEventHandler TextLanguageChanged = null;
	public event BasicEventHandler TranslationLanguageChanged = null;


	/* Public members */

	public string[] Providers {
		get { return providers;	}
	}

	public SpellLanguage[] Languages {
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
				EmitEnabledToggled();
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
		if ((index < 0) || (index >= languages.Length))
			return null;
		else
			return languages[index] as SpellLanguage;
	}

	public void SetActiveLanguage (SubtitleTextType textType, string languageID) {
		int index = GetLanguageIndex(languageID);
		SetActiveLanguageIndex(textType, index);
	}

	public void SetActiveLanguageIndex (SubtitleTextType textType, int index) {
		bool isEmpty = ((index < 0) || (index >= languages.Length));

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


	/* Private members */

	private void Init () {
		GetProvidersAndLanguages();
	
		activeTextLanguageIndex = -1;
		activeTranslationLanguageIndex = -1;
		
		GetActiveLanguagesFromConfig();
	}

	private void GetProvidersAndLanguages () {
		Enchant enchant = new Enchant();

		enchant.Open();
		Logger.Info("[Spellcheck] Found Enchant version: {0}", enchant.Version);
		
		providers = enchant.GetProviders();
		Logger.Info("[Spellcheck] Found {0} providers: {1}", providers.Length, string.Join(",", providers));
		
		string[] langTags = enchant.GetLanguages();
		ArrayList langs = new ArrayList();
		foreach (string langTag in langTags) {
			SpellLanguage language = new SpellLanguage(langTag);
			if (!langs.Contains(language)) {
				langs.Add(language);
			}
		}
		langs.Sort();
		languages = (SpellLanguage[])langs.ToArray(typeof(SpellLanguage));
		Logger.Info("[Spellcheck] Found {0} languages: {1}", languages.Length, GetLanguageIDsAsString(languages));
		
		enchant.Close();		
	}

	private string GetLanguageIDsAsString (SpellLanguage[] languages) {
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
		for (int index = 0 ; index < languages.Length ; index++) {
			SpellLanguage language = languages[index] as SpellLanguage;
			if (language.ID == languageID)
				return index;
		}
		return -1;
	}


	/* Event members */

	private void EmitEnabledToggled () {
    	if (this.EnabledToggled != null)
    		this.EnabledToggled();
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