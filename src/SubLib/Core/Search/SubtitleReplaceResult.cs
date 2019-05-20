/*
 * This file is part of SubLib.
 * Copyright (C) 2009-2019 Pedro Castro
 *
 * SubLib is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * SubLib is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;

namespace SubLib.Core.Search {

public class SubtitleReplaceResult {
	private int number = -1;
	private string text = String.Empty;
	private string translation = String.Empty;

	public SubtitleReplaceResult (int number, string text, string translation) {
		this.number = number;
		this.text = text;
		this.translation = translation;
	}

	/* Public properties */

	public int Number {
		get { return number; }
	}

	public string Text {
		get { return text; }
	}

	public string Translation {
		get { return translation; }
	}

}

}
