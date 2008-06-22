/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2008 Pedro Castro
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
using Glade;
using Gtk;
using Mono.Unix;
using SubLib;
using System;
using System.Text;

namespace GnomeSubtitles.Dialog {

public class FilePropertiesDialog : GladeDialog {

	/* Constant strings */
	private const string gladeFilename = "FilePropertiesDialog.glade";

	/* Widgets */

	[WidgetAttribute] private Label nameValueLabel = null;
	[WidgetAttribute] private Label pathValueLabel = null;
	[WidgetAttribute] private Label encodingValueLabel = null;
	[WidgetAttribute] private Label subtitleFormatValueLabel = null;
	[WidgetAttribute] private Label timingModeValueLabel = null;
	
	public FilePropertiesDialog () : base(gladeFilename) {
		FillLabelValues();
	}
	
	/* Private methods */
	
	private void FillLabelValues () {
		FileProperties properties = Base.Document.TextFile;
		
		FillName(properties.Filename);
		FillPath(properties.Directory);
		FillEncoding(properties.Encoding);
		FillSubtitleFormat(properties.SubtitleType);
		FillTimingMode(properties.TimingMode);
	}
	
	private void FillName (string name) {
		if (name == String.Empty)
			name = Catalog.GetString("Unknown");

		nameValueLabel.Text = name;
	}
	
	private void FillPath (string path) {
		if (path == String.Empty)
			path = Catalog.GetString("Unknown");

		pathValueLabel.Text = path;
	}
	
	private void FillEncoding (Encoding encoding) {
		string encodingName = String.Empty;
		if (encoding == null)
			encodingName = Catalog.GetString("Unknown");
		else {
			encodingName = Encodings.GetEncodingName(encoding.CodePage);
			if ((encodingName == null) || (encodingName == String.Empty))
				encodingName = Catalog.GetString("Unknown");
		}
		
		encodingValueLabel.Text = encodingName;
	}
	
	private void FillSubtitleFormat (SubtitleType type) {
		string format = String.Empty;
		if (type == SubtitleType.Unknown)
			format = Catalog.GetString("Unknown");
		else {
			SubtitleTypeInfo typeInfo = Subtitles.GetAvailableType(type);
			format = typeInfo.Name;
		}
		
		subtitleFormatValueLabel.Text = format;
	}
	
	private void FillTimingMode (TimingMode mode) {
		timingModeValueLabel.Text = mode.ToString();
	}


	#pragma warning disable 169		//Disables warning about handlers not being used
	
	private void OnResponse (object o, ResponseArgs args) {
		Close();
	}
	
}

}