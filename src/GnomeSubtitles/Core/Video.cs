/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2007 Pedro Castro
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

using Gtk;
using SubLib;
using System;
using System.Text.RegularExpressions;

namespace GnomeSubtitles {

public class Video {
	private HBox videoArea = null;
	private AspectFrame frame = null;
	
	private Player player = null;
	private VideoPosition position = null;
	private VideoSubtitle subtitle = null;
	
	private bool isLoaded = false;

	/* Constant strings */
	private const string videoSetSubtitleStartIconFilename = "video-set-subtitle-start-16x.png";
	private const string videoSetSubtitleEndIconFilename = "video-set-subtitle-end-16x.png";

	public Video () {
		videoArea = Global.GetWidget(WidgetNames.VideoAreaHBox) as HBox;
		
		/* Create the video Frame */
		frame = new AspectFrame(null, 0.5f, 0.5f, 1.6f, false);
		frame.Shadow = ShadowType.None;
		EventBox videoFrameEventBox = new EventBox();
		videoFrameEventBox.Add(frame);
		videoFrameEventBox.ModifyBg(StateType.Normal, videoFrameEventBox.Style.Black);

		/* Attach the video frame */
		Table videoImageTable = Global.GetWidget("videoImageTable") as Table;
		videoImageTable.Attach(videoFrameEventBox, 0, 1, 0, 1);
		videoImageTable.ShowAll();
		
		player = new Player();
		player.EndReached += OnPlayerEndReached;
		
		position = new VideoPosition(player);
		subtitle = new VideoSubtitle(position);
	
		LoadVideoWidget(player.Widget);
		
		/* Set the custom icons */
		SetCustomIcons();
	}
	
	/* Public properties */
	
	public VideoPosition Position {
		get { return position; }
	}
	
	public bool IsLoaded {
		get { return isLoaded; }
	}
	
	/* Public methods */
	
	public void Show () {
		videoArea.Show();
		
		/* Required for the vBox and children to be redrawn before launching the video player */
		Paned paned = Global.GetWidget(WidgetNames.MainPaned) as Paned;
		paned.ResizeChildren();
		Gdk.Window.ProcessAllUpdates();
	}

	public void Hide () {
		videoArea.Hide();
	}
	
	/// <summary>Opens a video file.</summary>
	/// <exception cref="PlayerNotFoundException">Thrown if the player executable was not found.</exception>
	/// <exception cref="PlayerCouldNotOpenVideoException">Thrown if the player could not open the video.</exception>
	public void Open (string filename) {
		Close();

		filename = Util.QuoteFilename(filename);
		
		player.Open(filename);
		player.SeekStart();

		SetControlsSensitivity(true);
		position.Enable();
		frame.Ratio = player.AspectRatio;
		
		isLoaded = true;
	}
	
	public void Close () {
		isLoaded = false;
	
		player.Close();
		subtitle.Close();
		position.Disable();
		
		/* Update the frame */
		frame.Child.Hide();
		frame.Child.Show();
		frame.Ratio = 1.67f;
		
		/* Update the PlayPause button */
		ToggleButton button = Global.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		button.Active = false;
		
		SetControlsSensitivity(false);
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
	
	public void ToggleTimingMode (TimingMode newMode) {
		position.ToggleTimingMode(newMode);
	}
	
	public void TogglePlayPause () {
		ToggleButton button = Global.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		button.Active = !button.Active; //Toggle() only emits the Toggled event
	}
	
	public void Play () {
		if (player.Paused)
			player.Play();
	}
	
	public void Pause () {
		if (!player.Paused)
			player.Pause();
	}
	
	public void Rewind () {
		player.Rewind(position.StepIncrement);
	}
	
	public void Forward () {
		player.Forward(position.StepIncrement);
	}
	
	/// <summary>Seeks to the specified time.</summary>
	/// <param name="time">The time position to seek to, in seconds.</param>
	public void Seek (float time) {
		if (!isLoaded)
			return;

		player.Seek(time);
	}
	
	public void SeekToSelection () {
		Subtitle subtitle = Global.GUI.View.Selection.Subtitle;
    	float time = (float)subtitle.Times.Start.TotalSeconds;
    	Seek(time);
	}
	
	/* Private methods */

	private void LoadVideoWidget (Widget widget) {
		frame.Child = widget;
		widget.Realize();
		widget.Show();
	}

	private void SetCustomIcons () {
		/* Set the icon for the SetSubtitleStart button */
		Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(null, videoSetSubtitleStartIconFilename);
		Image image = Global.GetWidget(WidgetNames.VideoSetSubtitleStartButtonImage) as Image;
		image.FromPixbuf = pixbuf;

		/* Set the icon for the SetSubtitleEnd button */
		pixbuf = new Gdk.Pixbuf(null, videoSetSubtitleEndIconFilename);
		image = Global.GetWidget(WidgetNames.VideoSetSubtitleEndButtonImage) as Image;
		image.FromPixbuf = pixbuf;
	}

	private void SetControlsSensitivity (bool sensitivity) {
		Global.GetWidget(WidgetNames.VideoTimingsVBox).Sensitive = sensitivity;
		Global.GetWidget(WidgetNames.VideoPlaybackHBox).Sensitive = sensitivity;
		
		if ((Global.GUI.View.Selection.Count == 1) && sensitivity)
			SetSelectionDependentControlsSensitivity(true);
		else
			SetSelectionDependentControlsSensitivity(false);
	}
	
	private void SetSelectionDependentControlsSensitivity (bool sensitivity) {
		Global.GetWidget(WidgetNames.VideoSetSubtitleStartButton).Sensitive = sensitivity;
		Global.GetWidget(WidgetNames.VideoSetSubtitleEndButton).Sensitive = sensitivity;
	}
	
	/* Event members */
	
	private void OnPlayerEndReached (object o, EventArgs args) {
		ToggleButton playPauseButton = Global.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		playPauseButton.Active = false;
	}

}

}
