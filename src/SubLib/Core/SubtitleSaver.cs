/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2017 Pedro Castro
 *
 * SubLib is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * SubLib is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using SubLib.Core.Domain;
using SubLib.IO;
using SubLib.IO.Output;
using SubLib.IO.SubtitleFormats;
using SubLib.Util;

namespace SubLib.Core {

/// <summary>Represents the main mechanism for saving <see cref="Subtitles" />.</summary>
public class SubtitleSaver {
	private FileProperties fileProperties = null;


	/* Public properties */

	/// <summary>The new properties of the file after saving.</summary>
	/// <remarks>This includes the updated <see cref="TimingMode" /> for the subtitles after saving. When saving to
	/// a subtitle format that supports only time or frame timing modes, this is updated to reflect that.</remarks>
	public FileProperties FileProperties {
		get { return fileProperties; }
	}

	/* Public methods */

	/// <summary>Saves subtitles to the file with the specified properties.</summary>
	/// <param name="subtitles">The subtitles to save.</param>
	/// <param name="properties">The properties of the file to save the subtitles to. Its <see cref="TimingMode" /> property is used to
	/// choose the timing mode for subtitle formats that support both time and frame modes.</param>
	/// <param name="textType">The type of text content to save.</param>
	/// <remarks>An updated <see cref="SubLib.FileProperties" /> object can be accessed with <see cref="FileProperties" /> after saving.</remarks>
	public void Save (Subtitles subtitles, FileProperties properties, SubtitleTextType textType) {
		SubtitleFormat format = BuiltInSubtitleFormats.GetFormat(properties.SubtitleType);
		SubtitleOutput output = new SubtitleOutput(format, textType);

		string text = output.Build(subtitles.Collection, subtitles.Properties, properties);
		FileInputOutput.WriteFile(properties.Path, text, properties.Encoding);

		fileProperties = GetUpdatedFileProperties(properties);
		Logger.Info("[SubtitleSaver] Saved {0} \"{1}\" with encoding \"{2}\", format \"{3}\" and frame rate \"{4}\"",
			textType, properties.Path, properties.Encoding, format.Name, subtitles.Properties.CurrentFrameRate);
	}

	/* Private methods */

	private FileProperties GetUpdatedFileProperties (FileProperties properties) {
		SubtitleFormat format = BuiltInSubtitleFormats.GetFormat(properties.SubtitleType);
		TimingMode newTimingMode = (format.Mode == SubtitleMode.Both) ? properties.TimingMode : format.ModeAsTimingMode;

		return new FileProperties(properties.Path, properties.Encoding, properties.SubtitleType, newTimingMode, properties.NewlineType);
	}
}

}