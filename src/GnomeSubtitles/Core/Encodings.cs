/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2019 Pedro Castro
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

using Mono.Unix;
using System;
using System.Text;

namespace GnomeSubtitles.Core {

public class Encodings {

	/* UTF8 encoding is constructed in a different manner in order to disable the BOM. */
	private static int CODEPAGE_UTF8 = 65001;
	private static int CODEPAGE_UTF8_WITHOUT_BOM = 650010; //Unofficial codepage, for internal use only
	private static Encoding encodingUTF8 = null;
	private static Encoding encodingUTF8WithoutBOM = null;

	/* The original versions of the following tables were taken from gedit
	 * which on the other hand took them from profterm
	 * Copyright (C) 2002 Red Hat, Inc.
 	 */
	private static EncodingDescription[] descriptions = {
		new EncodingDescription(28591, "ISO-8859-1", Catalog.GetString("Western")),
		new EncodingDescription(28592, "ISO-8859-2", Catalog.GetString("Central European")),
		new EncodingDescription(28593, "ISO-8859-3", Catalog.GetString("South European")),
		new EncodingDescription(28594, "ISO-8859-4", Catalog.GetString("Baltic")),
		new EncodingDescription(28595, "ISO-8859-5", Catalog.GetString("Cyrillic")),
		new EncodingDescription(28596, "ISO-8859-6", Catalog.GetString("Arabic")),
		new EncodingDescription(28597, "ISO-8859-7", Catalog.GetString("Greek")),
		new EncodingDescription(28598, "ISO-8859-8", Catalog.GetString("Hebrew")),
		new EncodingDescription(28599, "ISO-8859-9", Catalog.GetString("Turkish")),
		new EncodingDescription(28600, "ISO-8859-10", Catalog.GetString("Nordic")),
		new EncodingDescription(28603, "ISO-8859-13", Catalog.GetString("Baltic")),
		new EncodingDescription(28604, "ISO-8859-14", Catalog.GetString("Celtic")),
		new EncodingDescription(28605, "ISO-8859-15", Catalog.GetString("Western")),
		new EncodingDescription(28606, "ISO-8859-16", Catalog.GetString("Romanian")),
		/* ISO-8859-8-I not used */

		new EncodingDescription(CODEPAGE_UTF8, "UTF-8", Catalog.GetString("Unicode")), /* Added */
		
		new EncodingDescription(CODEPAGE_UTF8_WITHOUT_BOM, "UTF-8-NO-BOM", Catalog.GetString("Unicode"), Catalog.GetString("UTF-8 without BOM")), /* Added */
		new EncodingDescription(65000, "UTF-7", Catalog.GetString("Unicode")),
		new EncodingDescription(1200, "UTF-16", Catalog.GetString("Unicode")), //Little endian
		new EncodingDescription(1201, "UTF-16BE", Catalog.GetString("Unicode")),
		new EncodingDescription(1200, "UTF-16LE", Catalog.GetString("Unicode")),
		new EncodingDescription(12000, "UTF-32", Catalog.GetString("Unicode")), //Little endian
		new EncodingDescription(12001, "UTF-32BE", Catalog.GetString("Unicode")), /* Added */
		new EncodingDescription(12000, "UTF-32LE", Catalog.GetString("Unicode")), /* Added */
		/* UCS-2 and UCS-4 not used */

		new EncodingDescription(950, "BIG5", Catalog.GetString("Chinese Traditional")),
		new EncodingDescription(951, "BIG5-HKSCS", Catalog.GetString("Chinese Traditional")),
		new EncodingDescription(866, "CP866", Catalog.GetString("Cyrillic/Russian")),
		/* ARMSCII-8 not used */

		new EncodingDescription(51932, "EUC-JP", Catalog.GetString("Japanese")),
		new EncodingDescription(932, "CP932", Catalog.GetString("Japanese")),
		/* EUC-JP-MS not used */

		new EncodingDescription(51949, "EUC-KR", Catalog.GetString("Korean")),
		/* EUC-TW not used */

		new EncodingDescription(54936, "GB18030", Catalog.GetString("Chinese Simplified")),
		new EncodingDescription(936, "GB2312", Catalog.GetString("Chinese Simplified")),
		new EncodingDescription(936, "GBK", Catalog.GetString("Chinese Simplified")),
		/* GEOSTD8, HZ not used */

		new EncodingDescription(850, "IBM850", Catalog.GetString("Western")),
		new EncodingDescription(852, "IBM852", Catalog.GetString("Central European")),
		new EncodingDescription(855, "IBM855", Catalog.GetString("Cyrillic")),
		new EncodingDescription(857, "IBM857", Catalog.GetString("Turkish")),
		new EncodingDescription(860, "IBM860", Catalog.GetString("Portuguese")), /* Added */
		new EncodingDescription(861, "IBM861", Catalog.GetString("Icelandic")), /* Added */
		new EncodingDescription(862, "IBM862", Catalog.GetString("Hebrew")),
		new EncodingDescription(863, "IBM863", Catalog.GetString("French Canadian")), /* Added */
		new EncodingDescription(864, "IBM864", Catalog.GetString("Arabic")),
		new EncodingDescription(865, "IBM865", Catalog.GetString("Nordic")), /* Added */
		new EncodingDescription(866, "IBM866", Catalog.GetString("Cyrillic")), /* Added */
		new EncodingDescription(869, "IBM869", Catalog.GetString("Greek")), /* Added */

		new EncodingDescription(50221, "ISO-2022-JP", Catalog.GetString("Japanese")),
		new EncodingDescription(50225, "ISO-2022-KR", Catalog.GetString("Korean")),
		new EncodingDescription(1361, "JOHAB", Catalog.GetString("Korean")),
		new EncodingDescription(20866, "KOI8-R", Catalog.GetString("Cyrillic")),
		new EncodingDescription(21866, "KOI8-U", Catalog.GetString("Cyrillic/Ukrainian")), /* Added */
		/* ISO-IR-111, KOI8R, KOI8U not used */

		new EncodingDescription(932, "SHIFT_JIS", Catalog.GetString("Japanese")),
		/* TCVN, TIS-620, UHC, VISCII not used */

		new EncodingDescription(1250, "WINDOWS-1250", Catalog.GetString("Central")),
		new EncodingDescription(1251, "WINDOWS-1251", Catalog.GetString("Cyrillic")),
		new EncodingDescription(1252, "WINDOWS-1252", Catalog.GetString("Western")),
		new EncodingDescription(1253, "WINDOWS-1253", Catalog.GetString("Greek")),
		new EncodingDescription(1254, "WINDOWS-1254", Catalog.GetString("Turkish")),
		new EncodingDescription(1255, "WINDOWS-1255", Catalog.GetString("Hebrew")),
		new EncodingDescription(1256, "WINDOWS-1256", Catalog.GetString("Arabic")),
		new EncodingDescription(1257, "WINDOWS-1257", Catalog.GetString("Baltic")),
		new EncodingDescription(1258, "WINDOWS-1258", Catalog.GetString("Vietnamese")),
	};

	/* Public properties */

	public static EncodingDescription[] All {
		get { return descriptions; }
	}

	public static EncodingDescription SystemDefault {
		get {
			string region = Catalog.GetString("Current Locale");

			Encoding defaultEncoding = Encoding.Default;
			int codePage = defaultEncoding.CodePage;

			string code = String.Empty;
			string name = String.Empty;
			EncodingDescription tempDesc = EncodingDescription.Empty;
			if (Find(codePage, ref tempDesc)) {
				code = tempDesc.Code;
				name = tempDesc.Name;
			} else {
				name = defaultEncoding.WebName.ToUpper();
				code = name;
			}

			return new EncodingDescription(codePage, code, region, name);
		}
	}

	/* Public methods */

	/// <summary>Finds the description for the encoding with the specified name.</summary>
	/// <param name="code">The encoding's code.</param>
	/// <param name="description">The encoding's description that was found.</param>
	/// <returns>Whether the description was found.</returns>
	public static bool Find (string code, ref EncodingDescription description) {
		foreach (EncodingDescription desc in descriptions) {
			if (desc.Code == code) {
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
		EncodingDescription desc = EncodingDescription.Empty;
		if (Find(codePage, ref desc))
			return desc.Name;

		try {
			Encoding encoding = GetEncoding(codePage);
			if (encoding != null)
				return encoding.WebName;
		}
		catch (ArgumentException) {
			//Don't do anything, this will be handled next
		}
		return null;
	}
	
	public static Encoding GetEncoding (int codePage) {

		//We output UTF-8 with BOM by default		
		if (codePage == CODEPAGE_UTF8) {
			if (encodingUTF8 == null) {
				encodingUTF8 = new UTF8Encoding(true);
			}
			return encodingUTF8;
		}
		
		//UTF-8 without BOM
		if (codePage == CODEPAGE_UTF8_WITHOUT_BOM) {
			if (encodingUTF8WithoutBOM == null) {
				encodingUTF8WithoutBOM = new UTF8Encoding(false);
			}
			return encodingUTF8WithoutBOM;
		}

		return Encoding.GetEncoding(codePage);
	}

}

}
