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
using System.Text.RegularExpressions;

namespace GnomeSubtitles {

public class Video {
	private HBox videoArea = null;
	private AspectFrame frame = null;
	
	private Player player = null;
	private VideoPosition position = null;
	private VideoSubtitle subtitle = null;

	public Video () {
		videoArea = Global.GetWidget(WidgetNames.VideoAreaHBox) as HBox;
		subtitle = new VideoSubtitle();
		
		/* Create the video Frame */
		frame = new AspectFrame(null, 0.5f, 0.5f, 1.6f, false);
		EventBox videoFrameEventBox = new EventBox();
		videoFrameEventBox.Add(frame);
		videoFrameEventBox.ModifyBg(StateType.Normal, videoFrameEventBox.Style.Black);
		
		/* Attach the video frame */
		Table videoImageTable = Global.GetWidget("videoImageTable") as Table;
		videoImageTable.Attach(videoFrameEventBox, 0, 1, 0, 1);
		videoImageTable.ShowAll();
		
		player = new Player();
		position = new VideoPosition(player);
	
		LoadVideoWidget(player.Widget);
	}
	
	public void DoSomething () { //TODO delete
		System.Console.WriteLine("Doing something");	
	}
	
	/* Public properties */
	
	public float Position {
		get { return player.Position; }
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
	public void Open (string filename) {
		filename = Regex.Escape(filename);
		player.Open(filename);
		player.SeekStart();

		SetControlsSensitivity(true);
		position.Enable();
		frame.Ratio = player.AspectRatio;
	}
	
	public void Close () {
		player.Close();
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
		player.Rewind(position.StepIncrement);
	}
	
	public void Forward () {
		System.Console.WriteLine("Forwarding...");
		player.Forward(position.StepIncrement);
	}
	
	/* Private methods */

	private void LoadVideoWidget (Widget widget) {
		frame.Child = widget;
		widget.Realize();
		widget.Show();
	}

	private void SetControlsSensitivity (bool sensitivity) {
		Global.GetWidget(WidgetNames.VideoControlsVBox).Sensitive = sensitivity;	
	}

}

}