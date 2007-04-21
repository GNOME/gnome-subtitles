/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007 Pedro Castro
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
using System.Text;

namespace GnomeSubtitles {

public class Encodings {

	/* The original versions of the following tables were taken from gedit
	 * which on the other hand took them from profterm
	 * Copyright (C) 2002 Red Hat, Inc.
 	 */
	private static EncodingDescription[] descriptions = {
		new EncodingDescription(28591, "ISO-8859-1", Cat.Get("Western")),
		new EncodingDescription(28592, "ISO-8859-2", Cat.Get("Central European")),
		new EncodingDescription(28593, "ISO-8859-3", Cat.Get("South European")),
		new EncodingDescription(28594, "ISO-8859-4", Cat.Get("Baltic")),
		new EncodingDescription(28595, "ISO-8859-5", Cat.Get("Cyrillic")),
		new EncodingDescription(28596, "ISO-8859-6", Cat.Get("Arabic")),
		new EncodingDescription(28597, "ISO-8859-7", Cat.Get("Greek")),
		new EncodingDescription(28598, "ISO-8859-8", Cat.Get("Hebrew")),
		new EncodingDescription(28599, "ISO-8859-9", Cat.Get("Turkish")),
		new EncodingDescription(28600, "ISO-8859-10", Cat.Get("Nordic")),
		new EncodingDescription(28603, "ISO-8859-13", Cat.Get("Baltic")),
		new EncodingDescription(28604, "ISO-8859-14", Cat.Get("Celtic")),
		new EncodingDescription(28605, "ISO-8859-15", Cat.Get("Western")),
		new EncodingDescription(28606, "ISO-8859-16", Cat.Get("Romanian")),
		/* ISO-8859-8-I not used */
		
		new EncodingDescription(65001, "UTF-8", Cat.Get("Unicode")), /* Added */
		new EncodingDescription(65000, "UTF-7", Cat.Get("Unicode")),
		new EncodingDescription(1200, "UTF-16", Cat.Get("Unicode")),
		new EncodingDescription(1201, "UTF-16BE", Cat.Get("Unicode")),
		new EncodingDescription(1200, "UTF-16LE", Cat.Get("Unicode")),
		new EncodingDescription(12000, "UTF-32", Cat.Get("Unicode")),
		new EncodingDescription(12001, "UTF-32BE", Cat.Get("Unicode")), /* Added */
		new EncodingDescription(12000, "UTF-32LE", Cat.Get("Unicode")), /* Added */
		/* UCS-2 and UCS-4 not used */

		new EncodingDescription(950, "BIG5", Cat.Get("Chinese Traditional")),
		new EncodingDescription(951, "BIG5-HKSCS", Cat.Get("Chinese Traditional")),
		new EncodingDescription(866, "CP866", Cat.Get("Cyrillic/Russian")),
		/* ARMSCII-8 not used */
		
		new EncodingDescription(51932, "EUC-JP", Cat.Get("Japanese")),
		new EncodingDescription(932, "CP932", Cat.Get("Japanese")),
		/* EUC-JP-MS not used */
		
		new EncodingDescription(51949, "EUC-KR", Cat.Get("Korean")),
		/* EUC-TW not used */
		
		new EncodingDescription(54936, "GB18030", Cat.Get("Chinese Simplified")),
		new EncodingDescription(936, "GB2312", Cat.Get("Chinese Simplified")),
		new EncodingDescription(936, "GBK", Cat.Get("Chinese Simplified")),
		/* GEOSTD8, HZ not used */
		
		new EncodingDescription(850, "IBM850", Cat.Get("Western")),
		new EncodingDescription(852, "IBM852", Cat.Get("Central European")),
		new EncodingDescription(855, "IBM855", Cat.Get("Cyrillic")),
		new EncodingDescription(857, "IBM857", Cat.Get("Turkish")),
		new EncodingDescription(860, "IBM860", Cat.Get("Portuguese")), /* Added */
		new EncodingDescription(861, "IBM861", Cat.Get("Icelandic")), /* Added */
		new EncodingDescription(862, "IBM862", Cat.Get("Hebrew")),
		new EncodingDescription(863, "IBM863", Cat.Get("French Canadian")), /* Added */
		new EncodingDescription(864, "IBM864", Cat.Get("Arabic")),
		new EncodingDescription(865, "IBM865", Cat.Get("Nordic")), /* Added */
		new EncodingDescription(866, "IBM866", Cat.Get("Cyrillic")), /* Added */
		new EncodingDescription(869, "IBM869", Cat.Get("Greek")), /* Added */
		
		new EncodingDescription(50221, "ISO-2022-JP", Cat.Get("Japanese")),
		new EncodingDescription(50225, "ISO-2022-KR", Cat.Get("Korean")),
		new EncodingDescription(1361, "JOHAB", Cat.Get("Korean")),
		new EncodingDescription(20866, "KOI8-R", Cat.Get("Cyrillic")),
		new EncodingDescription(21866, "KOI8-U", Cat.Get("Cyrillic/Ukrainian")), /* Added */
		/* ISO-IR-111, KOI8R, KOI8U not used */
		
		new EncodingDescription(932, "SHIFT_JIS", Cat.Get("Japanese")),
		/* TCVN, TIS-620, UHC, VISCII not used */
		
		new EncodingDescription(1250, "WINDOWS-1250", Cat.Get("Central")),
		new EncodingDescription(1251, "WINDOWS-1251", Cat.Get("Cyrillic")),
		new EncodingDescription(1252, "WINDOWS-1252", Cat.Get("Western")),
		new EncodingDescription(1253, "WINDOWS-1253", Cat.Get("Greek")),
		new EncodingDescription(1254, "WINDOWS-1254", Cat.Get("Turkish")),
		new EncodingDescription(1255, "WINDOWS-1255", Cat.Get("Hebrew")),
		new EncodingDescription(1256, "WINDOWS-1256", Cat.Get("Arabic")),
		new EncodingDescription(1257, "WINDOWS-1257", Cat.Get("Baltic")),
		new EncodingDescription(1258, "WINDOWS-1258", Cat.Get("Vietnamese")),
	};
	
	/* Public properties */
	
	public static EncodingDescription[] All {
		get { return descriptions; }	
	}

	public static EncodingDescription SystemDefault {
		get {
			string description = Cat.Get("Current Locale");
			
			Encoding defaultEncoding = Encoding.Default;
			int codePage = defaultEncoding.CodePage;
		
			string name = String.Empty;
			EncodingDescription tempDesc = new EncodingDescription();
			if (Find(codePage, ref tempDesc))
				name = tempDesc.Name;
			else
				name = defaultEncoding.WebName.ToUpper();

			return new EncodingDescription(codePage, name, description);
		}
	}
	
	/* Public methods */
	
	/// <summary>Finds the description for the encoding with the specified name.</summary>
	/// <param name="name">The encoding's name.</param>
	/// <param name="description">The encoding's description that was found.</param>
	/// <returns>Whether the description was found.</returns>
	public static bool Find (string name, ref EncodingDescription description) {
		foreach (EncodingDescription desc in descriptions) {
			if (desc.Name == name) {
				description = desc;
				return true;
			}
		}
		return false;
	}
	
	public static bool Find (int codePage, ref EncodingDescription description) {
		foreach (EncodingDescription desc in descriptions) {
			if (desc.CodePage == codePage) {
				description = desc;
				return true;
			}
		}
		return false;
	}
	
	public static string GetEncodingName (int codePage) {
		EncodingDescription desc = new EncodingDescription();
		if (Find(codePage, ref desc))
			return desc.Name;

		try {
			Encoding encoding = Encoding.GetEncoding(codePage);
			if (encoding != null)
				return encoding.WebName;
		}
		catch (ArgumentException) {
			//Don't do anything, this will be handled next
		}
		return null;
	}

}

}
