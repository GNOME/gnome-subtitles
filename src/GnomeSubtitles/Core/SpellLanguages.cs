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

namespace GnomeSubtitles {

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
	
	public string ActiveTextLanguage {
		get { return GetActiveLanguage(SubtitleTextType.Text); }
		set { SetActiveLanguage(SubtitleTextType.Text, value); }
	}
	
	public string ActiveTranslationLanguage {
		get { return GetActiveLanguage(SubtitleTextType.Translation); }
		set { SetActiveLanguage(SubtitleTextType.Translation, value); }
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
				Global.Config.PrefsSpellCheckAutocheck = value;
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
	
	public string GetActiveLanguage (SubtitleTextType textType) {
		int index = GetActiveLanguageIndex(textType);
		if (index == -1)
			return String.Empty;
		else
			return languages[index] as string;
	}
	
	public void SetActiveLanguage (SubtitleTextType textType, string language) {
		int index = GetLanguageIndex(language);
		SetActiveLanguageIndex(textType, index);
			
		bool isEmpty = ((index == -1) || (language == null) || (language == String.Empty));
		string activeLanguage = (isEmpty ? String.Empty : language);
		SetActiveLanguageInConfig(textType, activeLanguage);

		EmitLanguageChanged(textType);
		if (!isEmpty)
			Global.GUI.Menus.SetToolsAutocheckSpellingSensitivity(true);
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
	}
	
	private void SetActiveLanguageIndex (SubtitleTextType textType, int index) {
		if (textType == SubtitleTextType.Text)
			activeTextLanguageIndex = index;
		else
			activeTranslationLanguageIndex = index;
	}
	
	private void GetActiveLanguagesFromConfig () {
		string activeTextLanguage = Global.Config.PrefsSpellCheckActiveTextLanguage;
		this.activeTextLanguageIndex = GetLanguageIndex(activeTextLanguage);
		
		string activeTranslationLanguage = Global.Config.PrefsSpellCheckActiveTranslationLanguage;
		this.activeTranslationLanguageIndex = GetLanguageIndex(activeTranslationLanguage);
	}
	
	private void GetEnabledFromConfig () {
		this.enabled = Global.Config.PrefsSpellCheckAutocheck;
	}
	
	private void SetActiveLanguageInConfig (SubtitleTextType textType, string activeLanguage) {
		if (textType == SubtitleTextType.Text)
			Global.Config.PrefsSpellCheckActiveTextLanguage = activeLanguage;
		else
			Global.Config.PrefsSpellCheckActiveTranslationLanguage = activeLanguage;
	}
	
	private int GetLanguageIndex (String language) {
		return languages.IndexOf(language);
	}
	
	private void Init () {
		languages = new ArrayList();
		activeTextLanguageIndex = -1;
		activeTranslationLanguageIndex = -1;
	}
	
	/* Event members */
	
	private void OnLanguageList (string langTag, string providerName, string providerDesc, string providerFile, IntPtr userdata) {
		languages.Add(langTag);
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
