// Commenting this class. Not used as the Google Translate API no longer allows for free access.
///*
// * This file is part of SubLib.
// * Copyright (C) 2011 Pedro Castro
// *
// * SubLib is free software; you can redistribute it and/or modify
// * it under the terms of the GNU General Public License as published by
// * the Free Software Foundation; either version 2 of the License, or
// * (at your option) any later version.
// *
// * SubLib is distributed in the hope that it will be useful,
// * but WITHOUT ANY WARRANTY; without even the implied warranty of
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// * GNU General Public License for more details.
// *
// * You should have received a copy of the GNU General Public License
// * along with this program; if not, write to the Free Software
// * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
// */

//using SubLib.Core.Domain;
//using SubLib.Exceptions;
//using System;
//using System.Net;
//using System.IO;
//using System.Globalization;
//using System.Text.RegularExpressions;
//using System.Text;

//namespace SubLib.Core {

//	/// <summary>Allows translating subtitles from one language to another using Google Translate service.</summary>
//	public class Translator	{
//		private const string REQUEST_URL_FORMAT = "http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&{0}langpair={1}|{2}";
//		private const string REQUEST_LINE_FORMAT = "q={0}&";

//		private const char SPLIT_CHAR = '\n';
//		private const char CULTURE_REPLACEE = '_';
//		private const char CULTURE_REPLACER = '-';

//		private const string JSON_RESPONSE_SUCCESS = "200";

//		private const string REGEX_ERROR_MATCH = @"^.*\""responseDetails\"": (.+),.*\""responseStatus\"":.*([0-9][0-9][0-9]).*$";
//		private const string REGEX_RESPONSE_MATCH = @"\""translatedText\"":\""([^\""]*)";
//		private const int REGEX_MATCH_INDEX_ONE_VALUE = 1;
//		private const int REGEX_MATCH_INDEX_TWO_VALUE = 2;

//		public const int TIMEOUT = 3000;


//		/* Public methods */

//		/// <summary>Translates given text between two given languages.</summary>
//		public static string TranslateText (string inputText, string fromLanguage, string toLanguage, int timeoutMillis) {
//			// testing emptiness of the string
//			if (inputText.Trim().Length == 0)
//				return String.Empty;

//			// splitting input text into single lines; line-breaks cannot be transferred to Google Translate
//			StringBuilder stringBuilder = new StringBuilder();
//			foreach (String split in inputText.Split(SPLIT_CHAR)) {
//				stringBuilder.Append(String.Format(REQUEST_LINE_FORMAT, System.Web.HttpUtility.UrlEncode(split)));
//			}

//			try {
//	            //TODO 'key' parameter could be added
//				// building request URL
//				string requestUrl = String.Format(REQUEST_URL_FORMAT, stringBuilder.ToString(),
//	                CultureInfo.CreateSpecificCulture(fromLanguage.Replace(CULTURE_REPLACEE, CULTURE_REPLACER)).TwoLetterISOLanguageName.ToLowerInvariant(),
//				    CultureInfo.CreateSpecificCulture(toLanguage.Replace(CULTURE_REPLACEE, CULTURE_REPLACER)).TwoLetterISOLanguageName.ToLowerInvariant()
//	            );

//				// translating
//	            HttpWebRequest req = HttpWebRequest.Create(requestUrl) as HttpWebRequest;
//				req.Timeout = timeoutMillis;
//				req.Method = WebRequestMethods.Http.Get;
//                HttpWebResponse res = req.GetResponse() as HttpWebResponse;

//				// processing response
//                string responseJson = new StreamReader(res.GetResponseStream()).ReadToEnd();

//                return ProcessJson(responseJson);
//            }
//			catch (Exception e) {
//				throw new TranslatorException(e.Message);
//            }
//        }


//		/* Private methods */

//		/// <summary>Processes JSON response from Google Translate service.</summary>
//		private static string ProcessJson (string responseJson) {
//			// fixing english-related quote problem
//			responseJson = responseJson.Replace("\\\"", "\\u0026quot;");

//			// matching responseStatus
//			StringBuilder stringBuilder = new StringBuilder();
//			Match match = Regex.Match(responseJson, REGEX_ERROR_MATCH, RegexOptions.IgnoreCase | RegexOptions.Compiled);
//			if (match.Success && match.Groups.Count > REGEX_MATCH_INDEX_TWO_VALUE && match.Groups[REGEX_MATCH_INDEX_TWO_VALUE].Value == JSON_RESPONSE_SUCCESS) {

//				// matching multi-line translated text and building output
//				bool first = true;
//	            MatchCollection matchCollection = Regex.Matches(responseJson, REGEX_RESPONSE_MATCH, RegexOptions.IgnoreCase | RegexOptions.Compiled);
//				foreach (Match textMatch in matchCollection) {
//					if (textMatch.Success && textMatch.Groups.Count > REGEX_MATCH_INDEX_ONE_VALUE) {
//						if (!first)
//							stringBuilder.Append(SPLIT_CHAR);

//						stringBuilder.Append(textMatch.Groups[REGEX_MATCH_INDEX_ONE_VALUE].Value);
//						first = false;
//					}
//				}

//				// replacing escaped characters that are not decoded automatically
//				stringBuilder = stringBuilder.Replace("\\u0026#39", "'");
//				stringBuilder = stringBuilder.Replace("\\u0026quot;", "\"");
//				stringBuilder = stringBuilder.Replace("\\u0026amp;", "&");
//				stringBuilder = stringBuilder.Replace("\\u0026lt;", "<");
//				stringBuilder = stringBuilder.Replace("\\u0026qt;", ">");
//				stringBuilder = stringBuilder.Replace("\\u0026amp;", "&");
//				stringBuilder = stringBuilder.Replace("\\u0026", "&");
//				stringBuilder = stringBuilder.Replace("\\u003c", "<");
//				stringBuilder = stringBuilder.Replace("\\u003e", ">");
//				stringBuilder = stringBuilder.Replace("\\\\", "\\");

//				return System.Web.HttpUtility.UrlDecode(stringBuilder.ToString());
//	        }
//			else {
//				throw new Exception((match.Groups.Count > REGEX_MATCH_INDEX_TWO_VALUE ? match.Groups[REGEX_MATCH_INDEX_TWO_VALUE].Value + ": " : String.Empty) + (match.Groups.Count > REGEX_MATCH_INDEX_ONE_VALUE ? match.Groups[REGEX_MATCH_INDEX_ONE_VALUE].Value : String.Empty));
//			}
//		}

//    }
//}