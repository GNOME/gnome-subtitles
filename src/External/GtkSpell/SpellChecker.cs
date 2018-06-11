/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2018 Pedro Castro
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
using SubLib.Util;
using System;
using System.Runtime.InteropServices;

namespace External.GtkSpell {

public class SpellChecker {

	private TextView textView = null;
	private IntPtr checker = IntPtr.Zero;
	
	public SpellChecker (TextView textView) {
		this.textView = textView;
	}
	
	/// <summary>
	/// Enables spellchecking for the specified language. Also used to set a new language.
	/// </summary>
	public void Enable (string language) {
		//Attach if necessary
		if (checker == IntPtr.Zero) {
			checker = gtk_spell_checker_new();
			gtk_spell_checker_attach(checker, textView.Handle);
		}
		
		bool success = gtk_spell_checker_set_language(checker, language, IntPtr.Zero);
		if (!success) {
			Logger.Error("[GtkSpell] Unable to set language " + language);
			Disable();
		}
	}

	/// <summary>
	/// Disables spellchecking entirely.
	/// </summary>
	public void Disable () {
		if (checker != IntPtr.Zero) {
			gtk_spell_checker_detach(checker);
			checker = IntPtr.Zero;
		}
	}
	
	


	/* GtkSpell imports */
	
	[DllImport ("libgtkspell")]
	private static extern IntPtr gtk_spell_checker_new ();
	
	[DllImport ("libgtkspell")]
	private static extern IntPtr gtk_spell_checker_attach (IntPtr spellChecker, IntPtr textView);

	[DllImport ("libgtkspell")]
	private static extern void gtk_spell_checker_detach (IntPtr spellChecker);

	[DllImport ("libgtkspell")]
	private static extern bool gtk_spell_checker_set_language (IntPtr textView, string lang, IntPtr error);

}

}
