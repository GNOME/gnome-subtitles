/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2008 Pedro Castro
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
using GnomeSubtitles.Dialog;
using Gtk;
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
	private VideoSubtitle subtitle = null;
	
	private bool isLoaded = false;
	private bool playPauseToggleIsSilent = false; //Used to indicate whether toggling the button should not issue the toggled signal

	/* Constant strings */
	private const string videoSetSubtitleStartIconFilename = "video-set-subtitle-start-16x.png";
	private const string videoSetSubtitleEndIconFilename = "video-set-subtitle-end-16x.png";

	public Video () {
		videoArea = Base.GetWidget(WidgetNames.VideoAreaHBox) as HBox;
		
		/* Create the video Frame */
		frame = new AspectFrame(null, 0.5f, 0.5f, 1.6f, false);
		frame.Shadow = ShadowType.None;
		EventBox videoFrameEventBox = new EventBox();
		videoFrameEventBox.Add(frame);
		videoFrameEventBox.ModifyBg(StateType.Normal, videoFrameEventBox.Style.Black);

		/* Attach the video frame */
		Table videoImageTable = Base.GetWidget("videoImageTable") as Table;
		videoImageTable.Attach(videoFrameEventBox, 0, 1, 0, 1);
		videoImageTable.ShowAll();
		
		/* Set player */
		player = new Player();
		player.OnEndReached = OnPlayerEndReached;
		player.OnErrorCaught = OnPlayerErrorCaught;
		
		position = new VideoPosition(player);
		subtitle = new VideoSubtitle(position);
	
		LoadVideoWidget(player.Widget);
		
		/* Set the custom icons */
		SetCustomIcons();
		
		/* Connect signals */
		ConnectPlayPauseButtonSignals();
	}
	
	/* Public properties */
	
	public VideoPosition Position {
		get { return position; }
	}
	
	public VideoSubtitle Subtitle {
		get { return subtitle; }
	}
	
	public bool IsLoaded {
		get { return isLoaded; }
	}
	
	public float FrameRate {
		get { return player.FrameRate; }
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

		SetControlsSensitivity(true);
		position.Enable();
		frame.Ratio = player.AspectRatio;
		
		Core.Base.Ui.Menus.AddFrameRateVideoTag(player.FrameRate);
		
		isLoaded = true;
	}
	
	public void Close () {
		if (!isLoaded)
			return;
	
		isLoaded = false;

		float oldFrameRate = player.FrameRate; //Need to store this before closing the player
	
		player.Close();
		subtitle.Close();
		position.Disable();
		
		/* Update the frame */
		frame.Child.Hide();
		frame.Child.Show();
		frame.Ratio = 1.67f;
		
		SilentDisablePlayPauseButton();		
		SetControlsSensitivity(false);

		Core.Base.Ui.Menus.RemoveFrameRateVideoTag(oldFrameRate);
	}

	public void UpdateFromTimingMode (TimingMode newMode) {
		position.ToggleTimingMode(newMode);
	}
	
	public void UpdateFromNewDocument (bool wasLoaded) {
    	subtitle.UpdateFromNewDocument(wasLoaded);
    }

	/// <summary>Updates the controls for a subtitle selection change.</summary>
	/// <param name="isSingle">Whether there is only 1 selected subtitle.</param>
	public void UpdateFromSelection (bool isSingle) {
		if (isSingle && IsLoaded)
			SetSelectionDependentControlsSensitivity(true);
		else
			SetSelectionDependentControlsSensitivity(false);
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

		TimeSpan time = SyncUtil.FramesToTime(frames, this.FrameRate);
		Seek(time);
	}
	
	public void SeekToSelection () { //TODO check out
		Subtitle subtitle = Core.Base.Ui.View.Selection.Subtitle;
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

	private void LoadVideoWidget (Widget widget) {
		frame.Child = widget;
		widget.Realize();
		widget.Show();
	}

	private void SetCustomIcons () {
		/* Set the icon for the SetSubtitleStart button */
		Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(null, videoSetSubtitleStartIconFilename);
		Image image = Base.GetWidget(WidgetNames.VideoSetSubtitleStartButtonImage) as Image;
		image.FromPixbuf = pixbuf;

		/* Set the icon for the SetSubtitleEnd button */
		pixbuf = new Gdk.Pixbuf(null, videoSetSubtitleEndIconFilename);
		image = Base.GetWidget(WidgetNames.VideoSetSubtitleEndButtonImage) as Image;
		image.FromPixbuf = pixbuf;
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
	
	/* Event members */
	
	private void ConnectPlayPauseButtonSignals () {
		ToggleButton button = Base.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		button.Toggled += OnPlayPauseButtonToggled;
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
	
	private void OnPlayerEndReached (object o, EventArgs args) {
		ToggleButton playPauseButton = Base.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		playPauseButton.Active = false;
	}
	
	private void OnPlayerErrorCaught (string message) {
		Console.Error.WriteLine("Caught player error: " + message);
		Close();
		VideoErrorDialog dialog = new VideoErrorDialog(message);
		dialog.WaitForResponse();
	}

}

}
