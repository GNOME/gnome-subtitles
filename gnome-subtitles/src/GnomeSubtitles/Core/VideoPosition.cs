/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
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
using System;

namespace GnomeSubtitles {

public class VideoPosition {
	private Player player = null;
	//private Label positionLabel = null;
	private Label positionValueLabel = null;
	private Label lengthValueLabel = null;	
	private float position = 0;
	
	/* Slider related */
	private HScale slider = null;
	private uint userUpdateTimeoutId = 0; //the ID of the timeout after which the value will be updated by the user selection
	private bool isPlayerUpdate = false; //TODO should this be TRUE?

	/* Constants */
	private const int userUpdateTimeout = 100; //Milliseconds

	/* Handlers */
	public delegate void VideoPositionChangedHandler (float position);

	/* Events */
	public event VideoPositionChangedHandler Changed;

	public VideoPosition (Player player) {
		slider = Global.GetWidget(WidgetNames.VideoSlider) as HScale;
		//positionLabel = Global.GetWidget(WidgetNames.VideoPositionLabel) as Label;
		positionValueLabel = Global.GetWidget(WidgetNames.VideoPositionValueLabel) as Label;
		lengthValueLabel = Global.GetWidget(WidgetNames.VideoLengthValueLabel) as Label;

		this.player = player;
		player.OnPositionChanged = OnPlayerPositionChanged;
	}

	/* Public properties */

	public float StepIncrement {
		get { return (float)slider.Adjustment.StepIncrement; }
	}
	
	/// <summary>The current position, in seconds.</summary>
	public float Current {
		get { return position; }
	}
	
	/* Public methods */
	
	public void Disable () {
		DisconnectSliderSignals();
		RemoveUserUpdateTimeout();
		UpdatePositionValues(0);
		SetLength(0);
		slider.Sensitive = false;	
	}
	
	public void Enable () {
		SetLength(player.Length);
		slider.Sensitive = true;
		ConnectSliderSignals();
	}

	/* Event members */
	
	private void OnSliderValueChanged (object o, EventArgs args) {
		System.Console.WriteLine("Slider Value has changed! " + isPlayerUpdate);
	
		if (isPlayerUpdate) {
			isPlayerUpdate = false;
			return;
		}
		RemoveUserUpdateTimeout();
		AddUserUpdateTimeout();
	}
	
	/// <summary>Handles changes in the player position.</summary>
	/// <param name="newPosition">The new position, in seconds, or -1 if the end was reached.</param>
	private void OnPlayerPositionChanged (float newPosition) {
		if (userUpdateTimeoutId == 0)  //There is not a manual positioning going on
			UpdateSlider(newPosition);

		UpdatePositionLabel(newPosition);
		EmitVideoPositionChanged(newPosition);
	}
	
	private bool UpdatePlayerPosition () {
		userUpdateTimeoutId = 0;
		player.Seek((float)slider.Value);
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
	
	private void EmitVideoPositionChanged (float newPosition) {
		if (Changed != null)
			Changed(newPosition);
	}
	
	private void ConnectSliderSignals () {
		slider.ValueChanged += OnSliderValueChanged;
	}
	
	private void DisconnectSliderSignals () {
		slider.ValueChanged -= OnSliderValueChanged;
	}

	/* Private members */
	
	private void UpdatePositionValues (float newPosition) {
		UpdateSlider(newPosition);
		UpdatePositionLabel(newPosition);
	}
	
	private void UpdateSlider (float newPosition) {
		isPlayerUpdate = true;
		slider.Value = newPosition;
	}

	private void UpdatePositionLabel (float newPosition) {
		positionValueLabel.Text = Util.SecondsToTimeText(newPosition);
	}
	
	private void SetLength (float length) {
		slider.Adjustment.Upper = length;
		lengthValueLabel.Text = Util.SecondsToTimeText(length);
	}

}

}