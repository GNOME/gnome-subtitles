/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2013 Pedro Castro
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
using System.IO;
using System.Text.RegularExpressions;

namespace GnomeSubtitles.Ui.VideoPreview {

public class VideoFiles {
	private static Regex videoFilesRegex = new Regex(@"\.(?:(?:3g2)|(?:3gp)|(?:asf)|(?:avi)|(?:bdm)|(?:cpi)|(?:divx)|(?:flv)|(?:m4v)|(?:mkv)|(?:mod)|(?:mov)|(?:mp3)|(?:mp4)|(?:mpeg)|(?:mpg)|(?:mts)|(?:ogg)|(?:ogm)|(?:ogv)|(?:rm)|(?:rmvb)|(?:spx)|(?:vob)|(?:wav)|(?:webm)|(?:wma)|(?:wmv)|(?:xvid))$", RegexOptions.IgnoreCase);

	/* Public methods */

	public static ArrayList GetVideoFilesAtPath (string path) {
		ArrayList videoFiles = new ArrayList();

		if ((path == null) || (path == String.Empty))
			return videoFiles;

		string[] allFiles = Directory.GetFiles(path, "*.*");
		foreach (string file in allFiles) {
			if (videoFilesRegex.IsMatch(file))
				videoFiles.Add(file);
		}
		return videoFiles;
	}

	public static Uri FindMatchingVideo (string file) {
		string fileDir = Path.GetDirectoryName(file);
		ArrayList videoFiles = GetVideoFilesAtPath(fileDir);
		string filename = Path.GetFileNameWithoutExtension(file);

		foreach (string videoFile in videoFiles) {
			string video = Path.GetFileNameWithoutExtension(videoFile);
			if (video == filename) {
				string videoFilename = Path.GetFileName(videoFile);
				return new Uri(Path.Combine(fileDir, videoFilename));
			}
		}

		return null;
	}


}

}