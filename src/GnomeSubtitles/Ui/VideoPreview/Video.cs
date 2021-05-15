/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2021 Pedro Castro
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

using Gdk;
using GnomeSubtitles.Core;
using GnomeSubtitles.Dialog.Message;
using GnomeSubtitles.Ui.VideoPreview.Exceptions;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using SubLib.Core.Timing;
using SubLib.Util;
using System;
using System.IO;

namespace GnomeSubtitles.Ui.VideoPreview {

public class Video {
	private Box videoArea = null;
	private AspectFrame frame = null;

	private Uri mediaUri = null;
	private Player player = null;
	private VideoPosition position = null;
	private SubtitleOverlay overlay = null;
	private SubtitleTracker tracker = null;

	private bool isLoaded = false;
	private bool playPauseToggleIsSilent = false; //Used to indicate whether toggling the button should not issue the toggled signal

	/* Constants */
	private const float DefaultAspectRatio = 1.67f;
	private const int MinSpeed = 10;
	private const int SpeedStep = 10;
	private const int MaxSpeed = 200;

	private readonly string PlayerErrorPrimaryMessage = Catalog.GetString("Media Player Error");
	private readonly string PlayerUnexpectedErrorSecondaryMessage = Catalog.GetString("An unexpected error has occurred: '{0}'. See the log for additional information.");
	

	public Video () {
		videoArea = Base.GetWidget(WidgetNames.VideoAreaHBox) as Box;

		InitializeVideoFrame();
		InitializePlayer();

		position = new VideoPosition(player);
		tracker = new SubtitleTracker();
		overlay = new SubtitleOverlay();

		Base.InitFinished += OnBaseInitFinished;
	}

	/* Public properties */

	public VideoPosition Position {
		get { return position; }
	}

	public SubtitleOverlay Overlay {
		get { return overlay; }
	}

	public SubtitleTracker Tracker {
		get { return tracker; }
	}

	public bool IsLoaded {
		get { return isLoaded; }
	}

	public bool IsStatusPlaying {
		get { return isLoaded && player.Status == MediaStatus.Playing; }
	}

	public float FrameRate {
		get { return player.HasVideo ? player.FrameRate : SubtitleConstants.DefaultFrameRate; }
	}

	public TimeSpan Duration {
		get { return TimeSpan.FromMilliseconds(player.Duration); }
	}

	public bool HasAudio {
		get { return (player != null) && (player.HasAudio); }
	}

	public bool HasVideo {
		get { return (player != null) && (player.HasVideo); }
	}


	/* Public methods */

	public void Show () {
		videoArea.Show();
	}

	public void Hide () {
		videoArea.Hide();
	}

	/// <summary>Opens a video file.</summary>
	public void Open(Uri mediaUri) {
		this.mediaUri = mediaUri;
		
		try {
			player.Open(mediaUri);
		} catch(Exception e) {
			HandlePlayerError(e);
			return;
		}

		frame.Show();
	}
	
	public void Close() {
		if (!isLoaded) {
			return;
		}

		isLoaded = false;

		mediaUri = null;
		player.Close();
		position.Disable();
		tracker.Close();
		overlay.Close();

		/* Update the frame */
		frame.Ratio = DefaultAspectRatio;
		frame.Hide();
		
		SilentDisablePlayPauseButton();
		UpdateSpeedControls(player.Speed);
		SetControlsSensitivity(false);
	}
	

	//Things we need to do if there was an error while opening a file
	private void CloseWhenOpeningHasFailed() {
		mediaUri = null;
		
		try {
			player.Close();
		} catch(Exception e) {
			Logger.Error(e, "Player error while forcing it to close after a fatal error has occurred when opening a file. Should be ok.");
		}
	}

	public void Quit () {
		if (isLoaded) {
			player.Close();
		}
	}

	public void SetLoopSelectionPlayback (bool enabled){
		if (enabled)
			Base.Ui.Video.Position.PositionPulse += OnVideoPositionPulseLoopPlayback;
		else
			Base.Ui.Video.Position.PositionPulse -= OnVideoPositionPulseLoopPlayback;
	}

	public void Rewind () {
		player.Rewind(position.SeekIncrement);
	}

	public void Forward () {
		player.Forward(position.SeekIncrement);
	}

	public void SpeedUp () {
		int newSpeed = player.Speed + SpeedStep;
		if (newSpeed > MaxSpeed) {
			return;
		}

		player.SetSpeed(newSpeed);
	    UpdateSpeedControls(newSpeed);
	}

	public void SpeedDown () {
	    int newSpeed = player.Speed - SpeedStep;
		if (newSpeed < MinSpeed) {
			return;
		}

		player.SetSpeed(newSpeed);
	    UpdateSpeedControls(newSpeed);
	}

	public void SpeedReset () {
	    player.ResetSpeed();
	    UpdateSpeedControls(player.Speed);
	}

	/// <summary>Seeks to the specified time.</summary>
	/// <param name="time">The time position to seek to, in seconds.</param>
	public void Seek (TimeSpan time) {
		if (!isLoaded)
			return;

		player.Seek((long)time.TotalMilliseconds);
	}

	public void Seek (int frames) {
		if (!isLoaded)
			return;

		TimeSpan time = TimingUtil.FramesToTime(frames, this.FrameRate);
		Seek(time);
	}

	public void SeekToPath (TreePath path) {
		Subtitle subtitle = Base.Document.Subtitles[path];
		if (subtitle != null) {
			Seek(subtitle.Times.Start);
		}
	}

	public void SeekToSelection () {
		SeekToSelection(false);
	}

	public void SeekToSelection (bool allowRewind) {
		Subtitle subtitle = Core.Base.Ui.View.Selection.FirstSubtitle;
    	TimeSpan time = subtitle.Times.Start;
    	if (allowRewind && Base.Config.VideoSeekOnChange) {
    		TimeSpan rewind = TimeSpan.FromMilliseconds(Base.Config.VideoSeekOnChangeRewind);
    		time = (time >= rewind ? time - rewind : TimeSpan.Zero);
    	}
    	Seek(time);
	}

	public void SelectNearestSubtitle () {
		int indexToSelect = tracker.FindSubtitleNearPosition(position.CurrentTime);
		Base.Ui.View.Selection.Select(indexToSelect, true, true);
	}

	/* Private methods */

	private void Play () {
		player.Play();
	}

	private void Pause () {
		player.Pause();
	}

	private void UpdateSpeedControls (int speed) {
		float speedFraction = ((float)speed) / 100;
		(Base.GetWidget(WidgetNames.VideoSpeedButton) as Button).Label = String.Format("{0:0.0}x", speedFraction);

		Base.GetWidget(WidgetNames.VideoSpeedDownButton).Sensitive = (speed > MinSpeed);
		Base.GetWidget(WidgetNames.VideoSpeedUpButton).Sensitive = (speed < MaxSpeed);
	}

	private void InitializeVideoFrame () {

		/* Create frame */
		frame = new AspectFrame(null, 0.5f, 0.5f, 1.6f, false);
		frame.ShadowType = ShadowType.None; //Otherwise we have a border around the frame

		/* Create event box */
		EventBox videoFrameEventBox = new EventBox();
		videoFrameEventBox.Add(frame);
		RGBA black = new RGBA();
		black.Red = 0;
		black.Green = 0;
		black.Blue = 0;
		black.Alpha = 1;
		videoFrameEventBox.OverrideBackgroundColor(StateFlags.Normal, black); //So the area outside the video is also black

		Bin bin = Base.GetWidget(WidgetNames.VideoImageOverlay) as Bin;
		bin.Add(videoFrameEventBox);
		bin.ShowAll();
	}
	
	private void HandlePlayerError(Exception e) {
		Logger.Error(e, "Player error (status {0})", player.Status);

		string secondaryMessage = (e is PlayerException ? e.Message : string.Format(PlayerUnexpectedErrorSecondaryMessage, e.Message));

		/* All player errors are fatal, so we need to close it.
		 * If we get a player error and we're not loaded yet, it means we got the error
		 * while loading the file, so we need to do some cleanup.
		 * Otherwise, we just close it in the normal fashion.
		 */
		if (!isLoaded) {
			string filename = Path.GetFileName(mediaUri.LocalPath);
			CloseWhenOpeningHasFailed();
			ShowVideoFileOpenErrorDialog(filename, secondaryMessage);
		} else {
			Base.CloseVideo();
			DialogUtil.ShowError(PlayerErrorPrimaryMessage, secondaryMessage);
		}
	}

	private void InitializePlayer () {
		player = new Player(frame);

		player.StatusChanged += OnPlayerStatusChanged;
		player.EndOfStreamReached += OnPlayerEndOfStreamReached;
		player.ErrorFound += OnPlayerErrorFound;
	}

	private void SetControlsSensitivity (bool sensitivity) {
		Base.GetWidget(WidgetNames.VideoTimingsVBox).Sensitive = sensitivity;
		Base.GetWidget(WidgetNames.VideoPlaybackHBox).Sensitive = sensitivity;

		if ((Base.Ui.View.Selection.Count == 1) && sensitivity)
			SetSelectionDependentControlsSensitivity(true);
		else
			SetSelectionDependentControlsSensitivity(false);
	}

	private void SetSelectionDependentControlsSensitivity (bool sensitivity) {
		Base.GetWidget(WidgetNames.VideoSetSubtitleStartButton).Sensitive = sensitivity;
		Base.GetWidget(WidgetNames.VideoSetSubtitleEndButton).Sensitive = sensitivity;
		Base.GetWidget(WidgetNames.VideoSetSubtitleStartEndButton).Sensitive = sensitivity;
	}

	private void SilentDisablePlayPauseButton () {
		ToggleButton button = Base.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		if (button.Active) {
			playPauseToggleIsSilent = true;
			button.Active = false;
		}
	}


	/* Event members */

	private void OnBaseInitFinished () {
		ToggleButton button = Base.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		button.Toggled += OnPlayPauseButtonToggled;

		Base.Ui.View.Selection.Changed += OnSubtitleSelectionChanged;
	}

	private void OnPlayPauseButtonToggled (object o, EventArgs args) {
		if (playPauseToggleIsSilent) {
			playPauseToggleIsSilent = false;
			return;
		}

    	if ((o as ToggleButton).Active)
			Play();
		else
			Pause();
	}

	private void OnPlayerStatusChanged(MediaStatus newStatus) {
		if (newStatus == MediaStatus.Loaded) {
			isLoaded = true;
			frame.Ratio = player.HasVideo ? player.AspectRatio : DefaultAspectRatio;
			SetControlsSensitivity(true);
			Base.UpdateFromVideoLoaded(mediaUri);
		}
	}

	private void OnPlayerEndOfStreamReached() {
		ToggleButton playPauseButton = Base.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		playPauseButton.Active = false;
	}

	private void OnPlayerErrorFound(string error) {
		HandlePlayerError(new PlayerException(error));
	}
	
	private void ShowVideoFileOpenErrorDialog(string filename, string error) {
		VideoFileOpenErrorDialog dialog = new VideoFileOpenErrorDialog(filename, error);
		bool toOpenAnother = dialog.WaitForResponse();
		if (toOpenAnother) {
			Base.Ui.OpenVideo();
		}
	}

	private void OnSubtitleSelectionChanged (TreePath[] paths, Subtitle subtitle) {
		if ((subtitle != null) && IsLoaded)
			SetSelectionDependentControlsSensitivity(true);
		else
			SetSelectionDependentControlsSensitivity(false);
	}

	/// <summary>Do loop playback when it's enabled, seeking to current selection on video position change.</summary>
	private void OnVideoPositionPulseLoopPlayback (TimeSpan position) {
		if (!(Base.IsDocumentLoaded))
			return;

		Subtitle firstSubtitle = Base.Ui.View.Selection.FirstSubtitle;
		if (firstSubtitle == null)
			return;

		Subtitle lastSubtitle = Base.Ui.View.Selection.LastSubtitle;
		if ((position < firstSubtitle.Times.Start) || (position > lastSubtitle.Times.End))
			SeekToSelection();
	}

}

}
