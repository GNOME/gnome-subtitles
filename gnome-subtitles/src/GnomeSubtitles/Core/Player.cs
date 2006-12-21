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
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

namespace GnomeSubtitles {

public class Player {
	private Socket socket = null;
	private Process process = null;
	private PlayerPosition position = null;

	public Player (PlayerPositionHandler handler) {
		CreateSocket();
		position = new PlayerPosition(this, handler);
	}
	
	public Socket Widget {
		get { return socket; }
	}

	
	/* Public properties */
	
	public float Ratio {
		get { return (float)Width / (float)Height; }
	}
	
	/// <summary>The length of the video, in seconds.</summary>
	public float TimeLength {
		get { return GetAsFloat("get_time_length"); }
	}
	
	public float TimePosition {
		get {
			if (position.Paused)
				return GetAsFloat("pausing get_time_pos");
			else
				return GetAsFloat("get_time_pos");
			}
	}


	/* Public methods */

	/// <summary>Opens a video file.</summary>
	public void Open (string filename) {
		position.Stop();
	
		if (process == null)
			StartNewProcess();

		Exec("loadfile " + filename);

		ClearOutput();
		position.Start();
	}


	/// <summary>Closes the video.</summary>
	public void Close () {
		position.Stop();
		TerminateProcess();
	}
	
	public void SeekStart () {
		System.Console.WriteLine("Seeking to start.");
		Exec("pausing seek 0 2");
	}
	
	public void Play () {
		if (position.Paused) {
			System.Console.WriteLine("Playing...");
			Exec("pause");
			position.Paused = false;
		}
	}
	
	public void Pause () {
		if (!position.Paused) {
			System.Console.WriteLine("Pausing...");
			position.Paused = true;
			Exec("pause");
		}
	}
	
	public void Seek (float position) {
		Exec("pausing_keep seek " + position + " 2");	
	}
	
	public float Rewind (float decrement) {
		Exec("pausing_keep seek -" + decrement + " 0");
		if (position.Paused)
			return TimePosition;
		else
			return -1;
	}
	
	public float Forward (float increment) {
		Exec("pausing_keep seek " + increment + " 0");
		if (position.Paused)
			return TimePosition;
		else
			return -1;
	}
	
	/* Private properties */
	
	private int Width {
		get { return GetAsInt32("get_property width"); }
	}
	
	private int Height {
		get { return GetAsInt32("get_property height"); }
	}
	
	/* Private methods */
	
	private void CreateSocket () {
		socket = new Socket();
		socket.ModifyBg(StateType.Normal, socket.Style.Black);	
	}
	
	private void StartNewProcess () {
		/* Configure startup of new process */
		Process newProcess = new Process();
		newProcess.StartInfo.FileName = "mplayer";

		newProcess.StartInfo.Arguments = "-wid " + socket.Id + " -osdlevel 3 -subfont-autoscale 2 -fontconfig -quiet -nomouseinput -slave -idle";
		if (!newProcess.StartInfo.EnvironmentVariables.ContainsKey("TERM")) {
			newProcess.StartInfo.EnvironmentVariables.Add("TERM", "xterm");
		}
		newProcess.StartInfo.UseShellExecute = false;
		newProcess.StartInfo.RedirectStandardInput = true;
		newProcess.StartInfo.RedirectStandardOutput = true;

		System.Console.WriteLine(newProcess.Start());
		process = newProcess;
	}
	
	/// <summary>Terminates the current running process, if it exists.</summary>
	/// <remarks>Waits for the process to end, and kills it if it doesn't.</remarks>
	private void TerminateProcess () {
		if (process != null) {
			Exec("quit");
			bool exited = process.WaitForExit(1000); //Wait 1 second to exit
			if (!exited) {
				System.Console.WriteLine("Process didn't exit, killing it.");
				process.Kill();
			}
			process = null;
		}
	}
	
	private void Exec (string command) {
		System.Console.WriteLine("Executing command: " + command);
		process.StandardInput.WriteLine(command);
	}
	
	private string Get (string command) {
		Exec(command);
		string line = process.StandardOutput.ReadLine();
		System.Console.WriteLine("Response was: " + line);
		int index = line.LastIndexOf("=");
		return (index == -1 ? String.Empty : line.Substring(index + 1));
	}
	
	private int GetAsInt32 (string command) {
		string text = Get(command);
		return Convert.ToInt32(text);
	}
	
	private float GetAsFloat (string command) {
		string text = Get(command);
		return (float)Convert.ToDouble(text);
	}

	/// <summary>Clears the current output.</summary>
	/// <remarks>This uses a hack to detect the end of the output, as the StreamReader could not detect it correctly.
	/// It executes a command and uses the result of that command to detect the end of output.</remarks>
	private void ClearOutput () {
		Exec("get_vo_fullscreen");
		StreamReader reader = process.StandardOutput;
		while (true) {
			string line = reader.ReadLine();
			if (line.StartsWith("ANS_VO_FULLSCREEN"))
				break;
		}
	}
	
	private string FindFontFilename () {
		Font font = SystemFonts.GetFontByName("arial");
		System.Console.WriteLine(font.Name);
		return String.Empty;	
	}
	
	
}

}