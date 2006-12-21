/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
 * Copyright (C) 2006 Pedro Castro
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
using System.Text.RegularExpressions;

namespace GnomeSubtitles {

public delegate void PlayerPositionHandler (float position);

public class Video {
	private VBox vBox = null;
	private AspectFrame frame = null;
	private HScale slider = null;
	
	private Player player = null;
	private uint setPlayerPositionTimeoutId = 0;
	private bool sliderUpdateByPlayer = true;

	/* Constants */
	private const int setPlayerPositionTimeout = 100; //Milliseconds

	public Video () {
		player = new Player(UpdateSlider);
		
		vBox = Global.GetWidget(WidgetNames.VideoVBox) as VBox;
		frame = Global.GetWidget(WidgetNames.VideoFrame) as AspectFrame;
		slider = Global.GetWidget(WidgetNames.VideoSlider) as HScale;

		LoadVideoWidget();
	}
	
	private void SetControlsSensitivity (bool sensitivity) {
		Global.GetWidget(WidgetNames.VideoControlsHBox).Sensitive = sensitivity;	
	}
	
	public void Show () {
		System.Console.WriteLine("SHOWING VBOX");
		vBox.Show();
		
		/* Required for the vBox and children to be redrawn before launching the video player */
		Paned paned = Global.GetWidget(WidgetNames.MainPaned) as Paned;
		paned.ResizeChildren();
		Gdk.Window.ProcessAllUpdates();
	}

	public void Hide () {
		System.Console.WriteLine("HIDING VBOX");
		vBox.Hide();
	}
	
	public void Open (string filename) {
		SetControlsSensitivity(true);

		filename = Regex.Escape(filename);
		player.Open(filename);
		frame.Ratio = player.Ratio;
		slider.Adjustment.Upper = player.TimeLength;
		player.SeekStart();
	}
	
	public void Close () {
		player.Close();
		
		/* Update the frame */
		frame.Child.Hide();
		frame.Child.Show();
		frame.Ratio = 1.67f;
		
		/* Update the PlayPause button */
		ToggleButton button = Global.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		button.Active = false;
		
		/* Update the slider */
		if (setPlayerPositionTimeoutId != 0)
			GLib.Source.Remove(setPlayerPositionTimeoutId);
		
		sliderUpdateByPlayer = true;
		slider.Value = 0;
		
		SetControlsSensitivity(false);
	}
	
	public void Quit () {
		player.Close();
	}
	
	public void TogglePlayPause () {
		ToggleButton button = Global.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		button.Active = !button.Active; //Toggle() only emits the Toggled event
	}
	
	public void Play () {
		System.Console.WriteLine("PLAY pressed");
		player.Play();
	}
	
	public void Pause () {
		System.Console.WriteLine("PAUSE pressed");
		player.Pause();
	}
	
	public void Rewind () {
		System.Console.WriteLine("Rewinding...");
		float position = player.Rewind((float)slider.Adjustment.StepIncrement);
		if (position != -1)
			UpdateSlider(position);

		System.Console.WriteLine("New position is now " + position);
		System.Console.WriteLine("Slider value is now " + slider.Value);
	}
	
	public void Forward () {
		System.Console.WriteLine("Forwarding...");
		float position = player.Forward((float)slider.Adjustment.StepIncrement);
		if (position != -1)
			UpdateSlider(position);

		System.Console.WriteLine("New position is now " + position);
		System.Console.WriteLine("Slider value is now " + slider.Value);
	}
	
	public void UpdateFromSliderValueChanged () {
		if (sliderUpdateByPlayer) {
			sliderUpdateByPlayer = false;
			return;
		}			
	
		if (setPlayerPositionTimeoutId != 0)
			GLib.Source.Remove(setPlayerPositionTimeoutId);
		
		setPlayerPositionTimeoutId = GLib.Timeout.Add(setPlayerPositionTimeout, SetPlayerPosition);
	}
	
	/* Private methods */

	private void LoadVideoWidget () {
		Widget child = player.Widget;
		frame.Child = child;
		child.Realize();
		child.Show();
	}
	
	/* Event members */
	
	private void UpdateSlider (float position) {
		if (setPlayerPositionTimeoutId != 0) //There is a manual positioning going on 
			return;
		
		sliderUpdateByPlayer = true;
		slider.Value = position;
	}
	
	private bool SetPlayerPosition () {
		setPlayerPositionTimeoutId = 0;
		player.Seek((float)slider.Value);
		return false;
	}

}

}