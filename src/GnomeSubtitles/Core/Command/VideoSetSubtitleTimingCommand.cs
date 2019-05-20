/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2011 Pedro Castro
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

namespace GnomeSubtitles.Core.Command {

public class VideoSetSubtitleStartCommand : ChangeStartCommand {
	private static string description = Catalog.GetString("Setting Subtitle Start");

	public VideoSetSubtitleStartCommand (int frames) : base(frames, false) {
		SetCommandProperties();
	}

	public VideoSetSubtitleStartCommand (TimeSpan time) : base(time, false) {
		SetCommandProperties();
	}

	/* Private methods */

	private void SetCommandProperties () {
		SetDescription(description);
		SetCanGroup(false);
	}
}

public class VideoSetSubtitleEndCommand : ChangeEndCommand {
	private static string description = Catalog.GetString("Setting Subtitle End");

	public VideoSetSubtitleEndCommand (int frames) : base(frames, false) {
		SetCommandProperties();
	}

	public VideoSetSubtitleEndCommand (TimeSpan time) : base(time, false) {
		SetCommandProperties();
	}

	/* Private methods */

	private void SetCommandProperties () {
		SetDescription(description);
		SetCanGroup(false);
	}
}

}
