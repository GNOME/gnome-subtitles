/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2009-2019 Pedro Castro
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

using GnomeSubtitles.Core;
using Mono.Unix;

namespace GnomeSubtitles.Dialog {

public class FileTranslationSaveDialog : FileSaveDialog {

	public FileTranslationSaveDialog () : base(Catalog.GetString("Save Translation As")) {
	}

	protected override string GetStartFolder () {
		if (Base.Document.HasTranslationFileProperties && Base.Document.TranslationFile.IsPathRooted) {
			return Base.Document.TranslationFile.Directory;
		}

		return base.GetStartFolder();
	}

	protected override string GetStartFilename () {
		if (Base.Document.HasTranslationFileProperties && !string.IsNullOrEmpty(Base.Document.TranslationFile.Filename)) {
			return Base.Document.TranslationFile.Filename;
		}

		return Base.Document.UnsavedTranslationFilename;
	}

}

}
