/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2009 Pedro Castro
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
using GnomeSubtitles.Dialog.Unmanaged;
using Gtk;
using GStreamer;
using SubLib.Core;
using SubLib.Core.Domain;
using SubLib.Core.Timing;
using System;
using System.Text.RegularExpressions;

namespace GnomeSubtitles.Ui.VideoPreview {

public class Video {
	private HBox videoArea = null;
	private AspectFrame frame = null;
	
	private Player player = null;
	private VideoPosition position = null;
	private SubtitleOverlay overlay = null;
	
	private bool isLoaded = false;
	private bool playPauseToggleIsSilent = false; //Used to indicate whether toggling the button should not issue the toggled signal

	/* Constant strings */
	private const string videoSetSubtitleStartIconFilename = "video-set-subtitle-start-16x.png";
	private const string videoSetSubtitleEndIconFilename = "video-set-subtitle-end-16x.png";

	public Video () {
		videoArea = Base.GetWidget(WidgetNames.VideoAreaHBox) as HBox;
		
		InitializeVideoFrame();
		InitializePlayer();
		
		position = new VideoPosition(player);
		overlay = new SubtitleOverlay();

		SetCustomIcons();
		Base.InitFinished += OnBaseInitFinished;
	}
	
	/* Public properties */
	
	public VideoPosition Position {
		get { return position; }
	}
	
	public SubtitleOverlay Overlay {
		get { return overlay; }
	}
	
	public bool IsLoaded {
		get { return isLoaded; }
	}
	
	public float FrameRate {
		get { return player.FrameRate; }
	}

	public TimeSpan Duration {
		get { return player.Duration; }
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
	/// <exception cref="PlayerCouldNotOpenVideoException">Thrown if the player could not open the video.</exception>
	public void Open (Uri videoUri) {
		Close();

		player.Open(videoUri);
	}
	
	public void Close () {
		if (!isLoaded)
			return;
	
		isLoaded = false;

		player.Close();
		overlay.Close();
		position.Disable();
		
		/* Update the frame */
		frame.Child.Hide();
		frame.Child.Show();
		frame.Ratio = Player.DefaultAspectRatio;
		
		SilentDisablePlayPauseButton();		
		SetControlsSensitivity(false);

		Core.Base.Ui.Menus.RemoveFrameRateVideoTag();
	}
	
	public void Quit () {
		player.Close();
	}
	
	public void Rewind () {
		player.Rewind(position.SeekIncrement);
	}
	
	public void Forward () {
		player.Forward(position.SeekIncrement);
	}
	
	/// <summary>Seeks to the specified time.</summary>
	/// <param name="time">The time position to seek to, in seconds.</param>
	public void Seek (TimeSpan time) {
		if (!isLoaded)
			return;

		player.Seek(time);
	}
	
	public void Seek (int frames) {
		if (!isLoaded)
			return;

		TimeSpan time = TimingUtil.FramesToTime(frames, this.FrameRate);
		Seek(time);
	}
	
	public void SeekToSelection () { //TODO check out
		Subtitle subtitle = Core.Base.Ui.View.Selection.FirstSubtitle;
    	TimeSpan time = subtitle.Times.Start;
    	Seek(time);
	}
	
	/* Private methods */

	private void Play () {
		player.Play();
	}
	
	private void Pause () {
		player.Pause();
	}

	private void SetCustomIcons () {
		/* Set the icon for the SetSubtitleStart button */
		Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(null, videoSetSubtitleStartIconFilename);
		Image image = Base.GetWidget(WidgetNames.VideoSetSubtitleStartButtonImage) as Image;	
		image.Pixbuf = pixbuf;

		/* Set the icon for the SetSubtitleEnd button */
		pixbuf = new Gdk.Pixbuf(null, videoSetSubtitleEndIconFilename);
		image = Base.GetWidget(WidgetNames.VideoSetSubtitleEndButtonImage) as Image;
		image.Pixbuf = pixbuf;
	}
	
	private void InitializeVideoFrame () {
		/* Create frame */
		frame = new AspectFrame(null, 0.5f, 0.5f, 1.6f, false);
		frame.Shadow = ShadowType.None;
		
		/* Create event box */
		EventBox videoFrameEventBox = new EventBox();
		videoFrameEventBox.Add(frame);
		videoFrameEventBox.ModifyBg(StateType.Normal, videoFrameEventBox.Style.Black);
	
		/* Attach event box */
		Table videoImageTable = Base.GetWidget("videoImageTable") as Table;
		videoImageTable.Attach(videoFrameEventBox, 0, 1, 0, 1);
		videoImageTable.ShowAll();
	}
	
	private void InitializePlayer () {
		player = new Player(frame);
		
		player.FoundVideoInfo += OnPlayerFoundVideoInfo;
		player.StateChanged += OnPlayerStateChanged;
		player.FoundDuration += OnPlayerFoundDuration;
		player.EndOfStream += OnPlayerEndOfStream;
		player.Error += OnPlayerError;
	}

	private void SetControlsSensitivity (bool sensitivity) {
		Base.GetWidget(WidgetNames.VideoTimingsVBox).Sensitive = sensitivity;
		Base.GetWidget(WidgetNames.VideoPlaybackHBox).Sensitive = sensitivity;
		
		if ((Core.Base.Ui.View.Selection.Count == 1) && sensitivity)
			SetSelectionDependentControlsSensitivity(true);
		else
			SetSelectionDependentControlsSensitivity(false);
	}
	
	private void SetSelectionDependentControlsSensitivity (bool sensitivity) {
		Base.GetWidget(WidgetNames.VideoSetSubtitleStartButton).Sensitive = sensitivity;
		Base.GetWidget(WidgetNames.VideoSetSubtitleEndButton).Sensitive = sensitivity;
	}
	
	private void SilentDisablePlayPauseButton () {
		ToggleButton button = Base.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		if (button.Active) {
			playPauseToggleIsSilent = true;
			button.Active = false;
		}		
	}

	private void handlePlayerLoading () {
		if (isLoaded || (!isPlayerLoadComplete()))
			return;

		isLoaded = true;
		SetControlsSensitivity(true);
		Base.UpdateFromVideoLoaded(player.VideoUri);
	}

	private bool isPlayerLoadComplete () {
		return (player != null) && (player.State != MediaStatus.Unloaded) && (player.HasVideoInfo) && (player.HasDuration);
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
	
	private void OnPlayerFoundVideoInfo (VideoInfoEventArgs args) {
		handlePlayerLoading();
	}
	
	private void OnPlayerStateChanged (StateEventArgs args) {
		if (args.State == MediaStatus.Loaded) {
			handlePlayerLoading();
		}
	}

	private void OnPlayerFoundDuration (TimeSpan duration) {
		handlePlayerLoading();
	}
	
	private void OnPlayerEndOfStream () {
		ToggleButton playPauseButton = Base.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		playPauseButton.Active = false;
	}
	
	private void OnPlayerError (Uri videoUri, Exception e) {
		Close();
		VideoErrorDialog dialog = new VideoErrorDialog(videoUri, e);
		bool toOpenAnother = dialog.WaitForResponse();
		if (toOpenAnother)
			Base.Ui.OpenVideo();
	}

	private void OnSubtitleSelectionChanged (TreePath[] paths, Subtitle subtitle) {
		if ((subtitle != null) && IsLoaded)
			SetSelectionDependentControlsSensitivity(true);
		else
			SetSelectionDependentControlsSensitivity(false);
	}

}

}
