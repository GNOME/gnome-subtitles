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

using GnomeSubtitles.Core;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using SubLib.Core.Timing;
using System;

namespace GnomeSubtitles.Ui.VideoPreview {

public class VideoPosition {
	private Player player = null;
	private Label positionLabel = null;
	private Label positionValueLabel = null;
	private Label lengthValueLabel = null;
	private TimeSpan position = TimeSpan.Zero;

	/* Slider related */
	private Scale slider = null;
	private uint userUpdateTimeoutId = 0; //the ID of the timeout after which the value will be updated by the user selection
	private bool isPlayerUpdate = false;

	/* Constants */
	private const int userUpdateTimeout = 100; //ms
	private long seekIncrement = 500; //ms

	/* Delegates */
	public delegate void VideoPositionPulseHandler (TimeSpan position);

	/* Events */

	public event VideoPositionPulseHandler PositionPulse;

	public VideoPosition (Player player) {
		slider = Base.GetWidget(WidgetNames.VideoSlider) as Scale;
		positionLabel = Base.GetWidget(WidgetNames.VideoPositionLabel) as Label;
		positionValueLabel = Base.GetWidget(WidgetNames.VideoPositionValueLabel) as Label;
		lengthValueLabel = Base.GetWidget(WidgetNames.VideoLengthValueLabel) as Label;

		this.player = player;
		Base.InitFinished += OnBaseInitFinished;
	}


	/* Public properties */

	public long SeekIncrement {
		get { return seekIncrement; }
	}

	public TimeSpan CurrentTime {
		get { return position; }
	}

	public int CurrentFrames {
		get { return Convert.ToInt32(TimingUtil.TimeToFrames(position, player.FrameRate)); }
	}

	public TimeSpan Duration {
		get { return TimeSpan.FromMilliseconds(player.Duration); }
	}

	public int DurationInFrames {
		get { return Convert.ToInt32(TimingUtil.TimeToFrames(player.Duration, player.FrameRate)); }
	}

	/* Public methods */

	public void Disable () {
		DisconnectSliderSignals();
		RemoveUserUpdateTimeout();

		position = TimeSpan.Zero;
		UpdatePositionValues(TimeSpan.Zero);
		SetLength(TimeSpan.Zero);

		slider.Sensitive = false;
	}


	/* Event members */

	private void OnSliderValueChanged (object o, EventArgs args) {
		if (isPlayerUpdate) {
			isPlayerUpdate = false;
			return;
		}
		RemoveUserUpdateTimeout();
		AddUserUpdateTimeout();
	}

	/// <summary>Handles changes in the player position.</summary>
	private void OnPlayerPositionPulse (long newPosition) {
		position = TimeSpan.FromMilliseconds(newPosition);

		if (userUpdateTimeoutId == 0)  //There is not a manual positioning going on
			UpdateSlider(position);

		UpdatePositionValueLabel(position);
		EmitVideoPositionPulse(position);
	}

	private void OnBaseVideoLoaded (Uri videoUri) {
		SetLength(Base.Ui.Video.Duration);
		slider.Sensitive = true;
		ConnectSliderSignals();
	}

	private bool UpdatePlayerPosition () {
		userUpdateTimeoutId = 0;
		player.Seek((long)slider.Value);
		return false;
	}

	private void RemoveUserUpdateTimeout () {
		if (userUpdateTimeoutId != 0) {
			GLib.Source.Remove(userUpdateTimeoutId);
			userUpdateTimeoutId = 0;
		}
	}

	private void AddUserUpdateTimeout () {
		userUpdateTimeoutId = GLib.Timeout.Add(userUpdateTimeout, UpdatePlayerPosition);
	}

	private void EmitVideoPositionPulse (TimeSpan newPosition) {
		if (PositionPulse != null)
			PositionPulse(newPosition);
	}

	private void ConnectSliderSignals () {
		slider.ValueChanged += OnSliderValueChanged;
	}

	private void DisconnectSliderSignals () {
		slider.ValueChanged -= OnSliderValueChanged;
	}

	private void OnBaseInitFinished () {
		Base.TimingModeChanged += OnBaseTimingModeChanged;
		Base.VideoLoaded += OnBaseVideoLoaded;

		player.PositionPulse += OnPlayerPositionPulse;
	}

	private void OnBaseTimingModeChanged (TimingMode timingMode) {
		UpdatePositionLabel(timingMode);
		UpdatePositionValueLabel(position);
		TimeSpan length = TimeSpan.FromMilliseconds(player.Duration);
		UpdateLengthLabel(timingMode, length);
	}

	/* Private members */

	private void UpdatePositionValues (TimeSpan newPosition) {
		UpdateSlider(newPosition);
		UpdatePositionValueLabel(newPosition);
	}

	private void UpdateSlider (TimeSpan newPosition) {
		isPlayerUpdate = true;
		slider.Value = newPosition.TotalMilliseconds;
	}

	private void UpdatePositionValueLabel (TimeSpan newPosition) {
		if (Base.TimingMode == TimingMode.Times)
			positionValueLabel.Text = Util.TimeSpanToText(newPosition);
		else {
			double frames = (newPosition == TimeSpan.Zero ? 0 : TimingUtil.TimeToFrames(newPosition, player.FrameRate));
			positionValueLabel.Text = Convert.ToInt32(frames).ToString();
		}
	}

	private void UpdatePositionLabel (TimingMode timingMode) {
		string mode = (timingMode == TimingMode.Times ? Catalog.GetString("Time") : Catalog.GetString("Frame"));
		positionLabel.Markup = "<b>" + mode + "</b>";
	}

	private void SetLength (TimeSpan length) {
		SetSliderLength(length);
		UpdateLengthLabel(Base.TimingMode, length);
	}

	private void UpdateLengthLabel (TimingMode timingMode, TimeSpan length) {
		if (timingMode == TimingMode.Times)
			lengthValueLabel.Text = Util.TimeSpanToText(length);
		else {
			double frames = (length == TimeSpan.Zero ? 0 : TimingUtil.TimeToFrames(length, player.FrameRate));
			lengthValueLabel.Text = Convert.ToInt32(frames).ToString();
		}
	}

	private void SetSliderLength (TimeSpan length) {
		slider.Adjustment.Upper = length.TotalMilliseconds;
	}

}

}
