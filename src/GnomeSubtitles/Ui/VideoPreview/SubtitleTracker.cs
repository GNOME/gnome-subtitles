/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2017 Pedro Castro
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
using Gtk;
using SubLib.Core.Domain;
using SubLib.Core.Search;
using System;

namespace GnomeSubtitles.Ui.VideoPreview {

public class SubtitleTracker {
	private SearchOperator searchOp = null;

	/* Keep the current subtitle as an optimization. This way, while the video is showing the same subtitle,
	 * we don't need to constantly search for the subtitle corresponding to its position.
	 */
	private int currentSubtitleIndex = -1;
	private Subtitle subtitle = null;

	/* Delegates */
	public delegate void VideoSubtitlePulseHandler(int indexSubtitle);

	/* Events */
	public event VideoSubtitlePulseHandler SubtitlePulse;


	public SubtitleTracker () {
		Base.InitFinished += OnBaseInitFinished;
	}

	/* Public methods */

	public int FindSubtitleNearPosition (TimeSpan position) {
		//We don't optimize this (by looking at the current subtitle) because it's unnecessary. This method isn't called that much.
		return searchOp.FindNearTime(position);
 	}

	public void Close(){
		if (IsSubtitleLoaded()) {
			UnSetCurrentSubtitle();
		}
	}


	/* Private methods */

	private bool IsSubtitleLoaded () {
		return this.subtitle != null;
	}

	private bool IsTimeInCurrentSubtitle (TimeSpan time) {
		return IsSubtitleLoaded() && (time >= this.subtitle.Times.Start) && (time <= this.subtitle.Times.End);
	}

	private void SetCurrentSubtitle (int index) {
		this.subtitle = Base.Document.Subtitles[index];
		this.currentSubtitleIndex = index;
	}

	private void UnSetCurrentSubtitle () {
		this.currentSubtitleIndex = -1;
		this.subtitle = null;
	}

	private void EmitSubtitlePulse(int newIndex) {
		if (SubtitlePulse != null)
			SubtitlePulse(newIndex);
	}


	/* Event members */

	private void OnBaseInitFinished () {
		Base.Ui.Video.Position.PositionPulse += OnVideoPositionPulse;
		Base.DocumentLoaded += OnBaseDocumentLoaded;
	}

	private void OnBaseDocumentLoaded (Document document) {
		this.searchOp = new SearchOperator(document.Subtitles);
	}

	private void OnVideoPositionPulse (TimeSpan newPosition) {
		if (!(Base.IsDocumentLoaded))
			return;

		if (!IsTimeInCurrentSubtitle(newPosition)) {
			int foundSubtitle = searchOp.FindWithTime(newPosition);
			if (foundSubtitle == -1) {
				UnSetCurrentSubtitle();
			} else {
				SetCurrentSubtitle(foundSubtitle);
			}
		}

		EmitSubtitlePulse(this.currentSubtitleIndex);
	}

	}

}
