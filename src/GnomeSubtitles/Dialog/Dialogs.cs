/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2008 Pedro Castro
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

using SubLib.Core.Domain;

namespace GnomeSubtitles.Dialog {

public class Dialogs {
	private FileSaveAsDialog fileSaveAsDialog = null;
	private FileSaveAsDialog translationSaveAsDialog = null;
	private SearchDialog searchDialog = null;
	private TimingsShiftDialog timingsShiftDialog = null;
	private TimingsSynchronizeDialog timingsSynchronizeDialog = null;
	
	public FileSaveAsDialog FileSaveAsDialog {
		get {
			if (fileSaveAsDialog == null)
				fileSaveAsDialog = new FileSaveAsDialog(SubtitleTextType.Text);
			
			return fileSaveAsDialog;		
		}
	}
	
	public FileSaveAsDialog TranslationSaveAsDialog {
		get {
			if (translationSaveAsDialog == null)
				translationSaveAsDialog = new FileSaveAsDialog(SubtitleTextType.Translation);
			
			return translationSaveAsDialog;		
		}
	}
	
	public SearchDialog SearchDialog {
		get {
			if (searchDialog == null)
				searchDialog = new SearchDialog();
			
			return searchDialog;		
		}
	}
	
	public TimingsShiftDialog TimingsShiftDialog {
		get {
			if (timingsShiftDialog == null)
				timingsShiftDialog = new TimingsShiftDialog();
			
			return timingsShiftDialog;		
		}
	}

	public TimingsSynchronizeDialog TimingsSynchronizeDialog {
		get {
			if (timingsSynchronizeDialog == null)
				timingsSynchronizeDialog = new TimingsSynchronizeDialog();
			
			return timingsSynchronizeDialog;		
		}
	}



}

}