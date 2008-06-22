/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2008 Pedro Castro
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
using System.Runtime.InteropServices;
using SubLib;

namespace GnomeSubtitles.Core {

/* Delegates */
public delegate void LanguageListHandler (string langTag, string providerName, string providerDesc, string providerFile, IntPtr userdata);

public class SpellLanguages {
	private bool enabled = false;
	private ArrayList languages = null;
	private int activeTextLanguageIndex = -1;
	private int activeTranslationLanguageIndex = -1;

	private LanguageListHandler languageListHandler = null;

	public SpellLanguages () {
		languageListHandler = OnLanguageList;
		GetAvailableLanguages();
		GetEnabledFromConfig();
	}
	
	/* Events */
	public event EventHandler ToggleEnabled = null;
	public event EventHandler TextLanguageChanged = null;
	public event EventHandler TranslationLanguageChanged = null;
	
	
	/* Public members */

	public ArrayList Languages {
		get {
			if (languages == null)
				GetAvailableLanguages();
			
			return languages;		
		}
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
				Base.Config.PrefsSpellCheckAutocheck = value;
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
	
		System.Console.Error.WriteLine("Setting active language: " + (activeLanguage == null ? "none." : activeLanguage.ID));
	
		/* Set index variable */
		if (textType == SubtitleTextType.Text)
			activeTextLanguageIndex = index;
		else
			activeTranslationLanguageIndex = index;

		String activeLanguageID = (isEmpty ? String.Empty : activeLanguage.ID);
		SetActiveLanguageInConfig(textType, activeLanguageID);

		EmitLanguageChanged(textType);
		if (!isEmpty)
			Base.Ui.Menus.SetToolsAutocheckSpellingSensitivity(true);
	}
	
	/* LibEnchant imports */
	
	[DllImport ("libenchant")]
	static extern IntPtr enchant_broker_init ();
	
	[DllImport ("libenchant")]
	static extern void enchant_broker_free (IntPtr broker);
	
	[DllImport ("libenchant")]
	static extern void enchant_broker_list_dicts (IntPtr broker, LanguageListHandler cb, IntPtr userdata);

	
	/* Private members */
	
	private void GetAvailableLanguages () {
		if (languages == null)
			Init();
		
		FillLanguages();
		GetActiveLanguagesFromConfig();
	}
	
	private void FillLanguages () {
		IntPtr broker = enchant_broker_init ();
		if (broker == IntPtr.Zero)
			return;
		
		enchant_broker_list_dicts (broker, languageListHandler, IntPtr.Zero);
			
		enchant_broker_free(broker);
		
		languages.Sort();
		System.Console.Error.WriteLine("Got " + languages.Count + " languages.");
	}
	
	private void GetActiveLanguagesFromConfig () {
		string activeTextLanguage = Base.Config.PrefsSpellCheckActiveTextLanguage;
		this.activeTextLanguageIndex = GetLanguageIndex(activeTextLanguage);
		
		string activeTranslationLanguage = Base.Config.PrefsSpellCheckActiveTranslationLanguage;
		this.activeTranslationLanguageIndex = GetLanguageIndex(activeTranslationLanguage);
	}
	
	private void GetEnabledFromConfig () {
		this.enabled = Base.Config.PrefsSpellCheckAutocheck;
	}
	
	private void SetActiveLanguageInConfig (SubtitleTextType textType, string activeLanguage) {
		if (textType == SubtitleTextType.Text)
			Base.Config.PrefsSpellCheckActiveTextLanguage = activeLanguage;
		else
			Base.Config.PrefsSpellCheckActiveTranslationLanguage = activeLanguage;
	}
	
	private int GetLanguageIndex (string languageID) {
		for (int index = 0 ; index < languages.Count ; index++) {
			SpellLanguage language = languages[index] as SpellLanguage;
			if (language.ID == languageID)
				return index;
		}
		return -1;
	}
	
	private void Init () {
		languages = new ArrayList();
		activeTextLanguageIndex = -1;
		activeTranslationLanguageIndex = -1;
	}
	
	/* Event members */
	
	private void OnLanguageList (string langTag, string providerName, string providerDesc, string providerFile, IntPtr userdata) {
		SpellLanguage language = new SpellLanguage(langTag);
		if (!languages.Contains(language))
			languages.Add(language);
	}
	
	private void EmitToggleEnabled () {
    	if (this.ToggleEnabled != null)
    		this.ToggleEnabled(this, EventArgs.Empty);
    }
    
    private void EmitLanguageChanged (SubtitleTextType textType) {
    	if (textType == SubtitleTextType.Text)
    		EmitTextLanguageChanged();
    	else
    		EmitTranslationLanguageChanged();
    }
    
    private void EmitTextLanguageChanged () {
    	if (this.TextLanguageChanged != null)
    		this.TextLanguageChanged(this, EventArgs.Empty);
    }
    
    private void EmitTranslationLanguageChanged () {
    	if (this.TranslationLanguageChanged != null)
    		this.TranslationLanguageChanged(this, EventArgs.Empty);
    }

}

}
