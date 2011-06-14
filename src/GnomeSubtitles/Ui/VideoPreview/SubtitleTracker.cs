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

using GnomeSubtitles.Core;
using Gtk;
using SubLib.Core.Domain;
using SubLib.Core.Search;
using System;

namespace GnomeSubtitles.Ui.VideoPreview {

public class SubtitleTracker {
	private SearchOperator searchOp = null;
	private int currentSubtitleIndex = 0;
	private Subtitle subtitle = null;

	/* Delegates */
	public delegate void VideoCurrentSubtitleChangedHandler (int indexSubtitle);
		
	/* Events */
	public event VideoCurrentSubtitleChangedHandler SubtitleChanged;


	public SubtitleTracker () {
		Base.InitFinished += OnBaseInitFinished;
	}

	/* Public methods */

	public int FindSubtitleNearPosition (TimeSpan position) {
		if (IsTimeInCurrentSubtitle(position))
			return currentSubtitleIndex;
		else
			return searchOp.FindNearTime((float)position.TotalSeconds); //TODO write method in SubLib that accepts TimeSpans
 	}
		
	public void Close(){
		if (IsSubtitleLoaded())
			UnSetCurrentSubtitle();
	}


	/* Private methods */
	
	private bool IsSubtitleLoaded () {
		return this.subtitle != null;
	}
	
	private bool IsTimeInCurrentSubtitle (TimeSpan time) {
		return IsSubtitleLoaded() && (time >= this.subtitle.Times.Start) && (time <= this.subtitle.Times.End);
	}
	
	private void SetCurrentSubtitle (int index) {
		if (index != currentSubtitleIndex) {
			subtitle = Base.Document.Subtitles[index];	
			currentSubtitleIndex = index;			
		}
	}
	
	private void UnSetCurrentSubtitle () {
		if (currentSubtitleIndex != -1) {
			currentSubtitleIndex = -1;				
			subtitle = null;			
		}
	}

	private void EmitCurrentSubtitleChanged(int newIndex) {
		if (SubtitleChanged != null)
			SubtitleChanged(newIndex);
	}


	/* Event members */
	
	private void OnBaseInitFinished () {
		Base.Ui.Video.Position.Changed += OnVideoPositionChanged;		
		Base.DocumentLoaded += OnBaseDocumentLoaded;
	}
	
	private void OnBaseDocumentLoaded (Document document) {
		this.searchOp = new SearchOperator(document.Subtitles);
	}

	private void OnVideoPositionChanged (TimeSpan newPosition) {
		if (!(Base.IsDocumentLoaded))
			return;
	
		if (!(IsTimeInCurrentSubtitle(newPosition))) {
			int foundSubtitle = searchOp.FindWithTime((float)newPosition.TotalSeconds); //TODO write method in SubLib that accepts TimeSpans
			if (foundSubtitle == -1)
				UnSetCurrentSubtitle();			
			else
				SetCurrentSubtitle(foundSubtitle);
			
			EmitCurrentSubtitleChanged(currentSubtitleIndex);
		}
	}		

	}
	
}
