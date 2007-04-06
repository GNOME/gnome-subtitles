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

namespace GnomeSubtitles {

public class Dialogs {
	private SearchDialog searchDialog = null;
	private TimingsShiftDialog timingsShiftDialog = null;
	
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




}

}