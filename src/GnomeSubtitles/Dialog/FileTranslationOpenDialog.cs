/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2010 Pedro Castro
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
using GnomeSubtitles.Core;
using GnomeSubtitles.Ui.Component;

namespace GnomeSubtitles.Dialog
{
	public class TranslationFileOpenDialog : FileOpenDialog
	{
		public TranslationFileOpenDialog () : base(!Base.IsVideoLoaded, false, "Open Translation")
		{
			autoChooseTranslationFile = false;
			
			if (Base.IsDocumentLoaded)	{
				AutoChooseTranslationFile(Base.Document.TextFile.Filename);
				
				if (!Base.IsVideoLoaded)
					AutoChooseVideoFile(Base.Document.TextFile.Filename);
			}
		}
		
		/* Protected members */
		
		protected override string GetStartFolder () {
		if (Base.IsDocumentLoaded && Base.Document.IsTranslationLoaded && Base.Document.HasTranslationFileProperties && Base.Document.TranslationFile.IsPathRooted)
			return Base.Document.TranslationFile.Directory;
		else
			return base.GetStartFolder();
		}

		/* Private members */
		
		protected override void InitSelectedSubtitleCombo () {
			selectedTranslation = new FilexEncodingCombo(translationFileLabel ,translationFileComboBox, translationEncodingComboBox);
			ActiveSelectionCombos.Add(selectedTranslation);
		}
	}
}
