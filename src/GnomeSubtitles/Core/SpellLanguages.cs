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
	private ArrayList languages = null;
	private int activeLanguageIndex = -1;
	private bool enabled = false;
	
	private LanguageListHandler languageListHandler = null;
	

	public SpellLanguages () {
		languageListHandler = OnLanguageList;
		GetAvailableLanguages();
		GetEnabledFromConfig();
	}
	
	/* Events */
	public event EventHandler LanguageChanged = null;
	public event EventHandler ToggleEnabled = null;
	
	
	/* Public members */

	public ArrayList Languages {
		get {
			if (languages == null)
				GetAvailableLanguages();
			
			return languages;		
		}
	}
	
	public int ActiveLanguageIndex {
		get { return activeLanguageIndex; }
	}
	
	public string ActiveLanguage {
		get {
			if (activeLanguageIndex == -1)
				return String.Empty;
			else
				return languages[activeLanguageIndex] as string;
		}
		set { 
			int index = GetActiveLanguageIndex(value);
			activeLanguageIndex = index;
			
			bool isEmpty = ((index == -1) || (value == null) || (value == String.Empty));
			string activeLanguage = (isEmpty ? String.Empty : value);
			SetActiveLanguageInConfig(activeLanguage);

			EmitLanguageChanged();
			if (!isEmpty)
				Global.GUI.Menus.SetToolsAutocheckSpellingSensitivity(true);
		}
	}
	
	public bool HasActiveLanguage {
		get { return activeLanguageIndex != -1; }
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
	
	
	/* LibEnchant imports */
	
	[DllImport ("libenchant.so.1")]
	static extern IntPtr enchant_broker_init ();
	
	[DllImport ("libenchant.so.1")]
	static extern void enchant_broker_free (IntPtr broker);
	
	[DllImport ("libenchant.so.1")]
	static extern void enchant_broker_list_dicts (IntPtr broker, LanguageListHandler cb, IntPtr userdata);

	
	/* Private members */
	
	private void GetAvailableLanguages () {
		if (languages == null)
			Init();
		
		FillLanguages();
		GetActiveLanguageFromConfig();
	}
	
	private void FillLanguages () {
		IntPtr broker = enchant_broker_init ();
		if (broker == IntPtr.Zero)
			return;
		
		enchant_broker_list_dicts (broker, languageListHandler, IntPtr.Zero);
			
		enchant_broker_free(broker);
		
		languages.Sort();
	}
	
	private void GetActiveLanguageFromConfig () {
		string activeLanguage = Global.Config.PrefsSpellCheckActiveLanguage;
		this.activeLanguageIndex = GetActiveLanguageIndex(activeLanguage);
	}
	
	private void GetEnabledFromConfig () {
		this.enabled = Global.Config.PrefsSpellCheckAutocheck && this.HasActiveLanguage;
		
		/* Check for inconsistency */
		if (Global.Config.PrefsSpellCheckAutocheck && (!this.HasActiveLanguage))
			Global.Config.PrefsSpellCheckAutocheck = false;
	}
	
	private void SetActiveLanguageInConfig (string activeLanguage) {
		Global.Config.PrefsSpellCheckActiveLanguage = activeLanguage;
	}
	
	private int GetActiveLanguageIndex (String activeLanguage) {
		return languages.IndexOf(activeLanguage);
	}
	
	private void Init () {
		languages = new ArrayList();
		activeLanguageIndex = -1;
	}
	
	/* Event members */
	
	private void OnLanguageList (string langTag, string providerName, string providerDesc, string providerFile, IntPtr userdata) {
		languages.Add(langTag);
	}
	
	private void EmitToggleEnabled () {
    	if (this.ToggleEnabled != null)
    		this.ToggleEnabled(this, EventArgs.Empty);
    }
    
    private void EmitLanguageChanged () {
    	if (this.LanguageChanged != null)
    		this.LanguageChanged(this, EventArgs.Empty);
    }

}

}
