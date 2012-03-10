using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using GnomeSubtitles.Core;
using SubLib.Core.Domain;


namespace GnomeSubtitles.Core
{
	/// <summary>
	/// !ThreadSafe in any way
	/// </summary>
	public class FileTools
	{
		
		private static Regex languageIdRegex = new Regex(@"[a-z][a-z]_[a-z][a-z]");
		private static Regex subtitleFileExtentionsRegex  = new Regex(BuildExtentionsPattern());
		private static Regex videoFilesRegex = new Regex(@"^.*\.((3g2)|(3gp)|(asf)|(avi)|(bdm)|(cpi)|(divx)|(flv)|(m4v)|(mkv)|(mod)|(mov)|(mp3)|(mp4)|(mpeg)|(mpg)|(mts)|(ogg)|(ogm)|(rm)|(rmvb)|(spx)|(vob)|(wav)|(wma)|(wmv)|(xvid))$");
			
		/*Public Funtions */
		
		/// <summary>
		/// Returns an collection of strings containing any files matching the type specified, Returns null if path is incorrect or empty list if no files of type found  
		/// </summary>
		/// <param name="path">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="type">
		/// A <see cref="ValidFileTypes"/>
		/// </param>
		/// <returns>
		/// A <see cref="IEnumerable<System.String>"/>
		/// </returns>
		public static IEnumerable<string> GetFilesOfType (string path, ValidFileTypes type) {
			if ((path == null) || (path == String.Empty) || (!Directory.Exists(path)) )
				return null;

			string[] allFiles = Directory.GetFiles(path, "*.*");
			return GetFilesOfType(allFiles, type);
		}
		
		/// <summary>
		/// Returns a collection of strings containing all string paths or filenames from supplied collection that match specified type
		/// </summary>
		/// <param name="filepaths">
		/// A <see cref="System.String[]"/>
		/// </param>
		/// <param name="type">
		/// A <see cref="ValidFileTypes"/>
		/// </param>
		/// <returns>
		/// A <see cref="IEnumerable<System.String>"/>
		/// </returns>
		public static IEnumerable<string> GetFilesOfType (IEnumerable<string> filepaths, ValidFileTypes type) {
			List<string> returnfiles = new List<string>();
			foreach (string filepath in filepaths) {
				if (GetFileType(filepath) == type) {
					returnfiles.Add(filepath);
				}
			}
			returnfiles.Sort();
			return returnfiles;
		}
		
		/// <summary>
		/// Finds closest match available in given files parent folder that is of the specified type, Returns null if no match is found
		/// </summary>
		/// <param name="path">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="type">
		/// A <see cref="ValidFileTypes"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string FromFolderFindMatchOfType (string filepath, ValidFileTypes type) {			
			return FindMatchingFile(filepath, GetFilesOfType(filepath, type));
		}
		
		/// <summary>
		/// Finds nearest match to supplied filename in given collection of files, Returns null if no match is found
		/// </summary>
		/// <param name="filetomatch">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="filestosearch">
		/// A <see cref="IEnumerable<System.String>"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string FindMatchingFile (string filetomatch, IEnumerable<string> filestosearch) {
			string filenametomatch = GetCleanFilenamePattern(filetomatch);
			string currentcultureid = FilenameContainsLanguageId(filetomatch) ? GetFilenameLanguageId(filetomatch) : CultureInfo.CurrentUICulture.Name; 
			
			List<string> matchingfiles = new List<string>();
			List<string> matchingfileswithid = new List<string>();
			List<string> matchingforeignfileswithid = new List<string>();
			
			foreach(string file in filestosearch) {
				string filename = Path.GetFileName(file);
				if (Regex.IsMatch(filename, filenametomatch) && file != filetomatch) 
					matchingfiles.Add(file);
			}
			if (matchingfiles.Count > 0) {
				foreach(string matchingfile in matchingfiles) {
					if (FilenameContainsLanguageId(matchingfile)) 
						matchingfileswithid.Add(matchingfile);
				}
				if (matchingfileswithid.Count > 0) {
					foreach (string matchingfilewithid in matchingfileswithid) {
						if (GetFilenameLanguageId(matchingfilewithid) != currentcultureid)
							matchingforeignfileswithid.Add(matchingfilewithid);
					}
					if (matchingforeignfileswithid.Count > 0){
						return matchingforeignfileswithid[0];
					} else {
						return matchingfileswithid[0];
					}
				} else {
					return matchingfiles[0];
				}
			} return null;
		}
		
		/// <summary>
		/// Simply bolts on "file://" to given string and returns as uri
		/// </summary>
		/// <param name="filepath">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="Uri"/>
		/// </returns>
		public static Uri GetUriFromFilePath (string filepath) {
			return new Uri ("file://" + filepath);
		}
		
		/*Private Functions */
		
		private static string BuildExtentionsPattern () {
			SubtitleTypeInfo[] types = Subtitles.AvailableTypesSorted;
			StringBuilder pattern = new StringBuilder(@"^.*\.(");
			foreach(SubtitleTypeInfo type in types) {
				foreach (string extention in type.Extensions)
					pattern.Append("(").Append(extention).Append(")|");
			}
			pattern.Remove(pattern.Length -1, 1);	//remove last "|"
			pattern.Append(")$");
			return pattern.ToString();
		}
		
		private static bool FileIsSubtitle (string file) {
			return (subtitleFileExtentionsRegex.IsMatch(Path.GetExtension(file))); 
		}
		
		private static bool FileIsVideo (string file) {
			return videoFilesRegex.IsMatch(file);		
		}
				
		private static bool FilenameContainsLanguageId (string file) {
			return (languageIdRegex.IsMatch(file));
			
		}		
		
		/// <summary>
		/// Returns a filetype based on a simple file extention regex test
		/// </summary>
		/// <param name="file">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="ValidFileTypes"/>
		/// </returns>
		private static ValidFileTypes GetFileType (string file) {
			if (FileTools.FileIsSubtitle(file)) {
				return ValidFileTypes.Subtitle;	
			} else if (FileTools.FileIsVideo(file)) { 
				return ValidFileTypes.Video;
			} else {
				return ValidFileTypes.None;
			}
		}
	
		private static string GetFilenameLanguageId (string candidate) {
			if (FilenameContainsLanguageId(candidate)){
				foreach (SpellLanguage lang in Base.SpellLanguages.Languages) {
					if(lang.StringMatchesId(candidate))
						return lang.ID;
				}
			}
			return String.Empty;
		}
	
		/// <summary>
		/// Attempts to remove any tags added by Gnome-Subtitles and escape any regex wildcards from a string filepath or filename  
		/// </summary>
		/// <param name="filename">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		private static string GetCleanFilenamePattern (string filename) {
			filename = Path.GetFileNameWithoutExtension(filename);
			if (GetFilenameLanguageId(filename) != String.Empty)
				filename.Replace(GetFilenameLanguageId(filename), "");
			filename = Regex.Escape(filename);
			filename.Trim();
			return filename;
		}
	}
}

