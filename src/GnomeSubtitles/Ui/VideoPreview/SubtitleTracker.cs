using GnomeSubtitles.Core;
using Gtk;
using SubLib.Core.Domain;
using SubLib.Core.Search;
using System;

namespace GnomeSubtitles.Ui.VideoPreview
{


public class SubtitleTracker{
		
		
	private SearchOperator searchOp = null;
	private TimeSpan subtitleStart = TimeSpan.Zero;
	private TimeSpan subtitleEnd = TimeSpan.Zero;	
	private int currentSubtitleIndex = 0;
	private Subtitle subtitle = null;
		
	/* Delegates */
	public delegate void VideoCurrentSubtitleChangedHandler (int indexSubtitle);
		
	/* Events */
		
	public event VideoCurrentSubtitleChangedHandler SubtitleChanged;

	public SubtitleTracker ()
	{
		Base.InitFinished += OnBaseInitFinished;
	}
		
	public int FindSubtitleNearPosition(TimeSpan position){
		if (IsTimeInCurrentSubtitle(position)) 
				return currentSubtitleIndex;
		else {
				int foundSubtitle = searchOp.FindNearTime((float)position.TotalSeconds);				
				return foundSubtitle;
		}

 	}
		
	public void Close(){
		if( IsSubtitleLoaded )
			UnSetCurrentSubtitle();
	}
	
	/* Private properties */
	
	private bool IsSubtitleLoaded {
		get { return subtitle != null; }
	}
		
	/* Private methods */
	
	private bool IsTimeInCurrentSubtitle (TimeSpan time) {
		return IsSubtitleLoaded && (time >= subtitleStart) && (time <= subtitleEnd);	
	}
	
		
	private void SetCurrentSubtitle (int index) {
		
		if( index != currentSubtitleIndex ){
			subtitle = Base.Document.Subtitles[index];
			subtitleStart = subtitle.Times.Start;
			subtitleEnd = subtitle.Times.End;			
			currentSubtitleIndex = index;			
			}
	}
	
	private void UnSetCurrentSubtitle () {
		if( currentSubtitleIndex != -1 ){			
			currentSubtitleIndex = -1;				
			subtitle = null;
			subtitleStart = TimeSpan.Zero;
			subtitleEnd = TimeSpan.Zero;			
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
		searchOp = new SearchOperator(document.Subtitles);
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
