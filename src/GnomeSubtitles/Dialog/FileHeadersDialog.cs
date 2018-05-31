/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2018 Pedro Castro
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
using GnomeSubtitles.Ui;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using SubLib.IO.SubtitleFormats;
using System;
using System.Reflection;

namespace GnomeSubtitles.Dialog {

public class FileHeadersDialog : BaseDialog {

	private Headers headers;

	//Property Changed event
	private delegate void PropertyChangedHandler (object sender, string property, object newValue);
	private event PropertyChangedHandler PropertyChanged;

	public FileHeadersDialog () : base() {
		headers = (Headers)Base.Document.Subtitles.Properties.Headers.Clone();

		base.Init(BuildDialog());
	}


	/* Private members */

	private Gtk.Dialog BuildDialog() {
		Gtk.Dialog dialog = new Gtk.Dialog(Catalog.GetString("Headers"), Base.Ui.Window, DialogFlags.Modal | DialogFlagsUseHeaderBar,
			Util.GetStockLabel("gtk-cancel"), ResponseType.Cancel, Util.GetStockLabel("gtk-apply"), ResponseType.Ok);

		dialog.DefaultResponse = ResponseType.Ok;
		dialog.DefaultWidth = WidgetStyles.DialogWidthMedium;
		dialog.DefaultHeight = WidgetStyles.DialogHeightLarge;

		Notebook notebook = new Notebook();
		notebook.Expand = true;
		notebook.TabPos = PositionType.Left;
		notebook.BorderWidth = WidgetStyles.BorderWidthMedium;

		Grid grid;

		//Karaoke Lyrics LRC
		grid = CreatePageWithGrid(notebook, "Karaoke Lyrics LRC");
		grid.Attach(CreateLabel(Catalog.GetString("Title:")), 0, 0, 1, 1);
		grid.Attach(CreateEntry("Title"), 1, 0, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Author:")), 0, 1, 1, 1);
		grid.Attach(CreateEntry("Author"), 1, 1, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Artist:")), 0, 2, 1, 1);
		grid.Attach(CreateEntry("Artist"), 1, 2, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Album:")), 0, 3, 1, 1);
		grid.Attach(CreateEntry("Album"), 1, 3, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("By:")), 0, 4, 1, 1);
		grid.Attach(CreateEntry("FileCreator"), 1, 4, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Version:")), 0, 5, 1, 1);
		grid.Attach(CreateEntry("Version"), 1, 5, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Program:")), 0, 6, 1, 1);
		grid.Attach(CreateEntry("Program"), 1, 6, 1, 1);

		//Karaoke Lyrics VKT
		grid = CreatePageWithGrid(notebook, "Karaoke Lyrics VKT");
		grid.Attach(CreateLabel(Catalog.GetString("Author:")), 0, 0, 1, 1);
		grid.Attach(CreateEntry("Author"), 1, 0, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Source:")), 0, 2, 1, 1);
		grid.Attach(CreateEntry("Source"), 1, 2, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Date:")), 0, 3, 1, 1);
		grid.Attach(CreateEntry("Date"), 1, 3, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Frame Rate:")), 0, 4, 1, 1);
		grid.Attach(CreateEntry("FrameRate"), 1, 4, 1, 1);

		//MPSub
		grid = CreatePageWithGrid(notebook, "MPSub");
		grid.Attach(CreateLabel(Catalog.GetString("Title:")), 0, 0, 1, 1);
		grid.Attach(CreateEntry("Title"), 1, 0, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("File:")), 0, 2, 1, 1);
		grid.Attach(CreateEntry("MPSubFileProperties"), 1, 2, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Author:")), 0, 3, 1, 1);
		grid.Attach(CreateEntry("Author"), 1, 3, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Note:")), 0, 4, 1, 1);
		grid.Attach(CreateEntry("Comment"), 1, 4, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Media Type:")), 0, 5, 1, 1);
		ComboBoxText comboBoxMPSubMediaType = CreateComboBoxText("MPSubMediaType",
	          SubtitleFormatMPSub.HeaderMediaTypeAudio, Catalog.GetString("Audio"),
	          SubtitleFormatMPSub.HeaderMediaTypeVideo, Catalog.GetString("Video"));
		grid.Attach(comboBoxMPSubMediaType, 1, 5, 1, 1);

		//Sub Station Alpha / ASS
		grid = CreatePageWithGrid(notebook, "Sub Station Alpha / ASS");
		grid.Attach(CreateLabel(Catalog.GetString("Title:")), 0, 0, 1, 1);
		grid.Attach(CreateEntry("Title"), 1, 0, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Original Script:")), 0, 1, 1, 1);
		grid.Attach(CreateEntry("SubStationAlphaOriginalScript"), 1, 1, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Original Translation:")), 0, 2, 1, 1);
		grid.Attach(CreateEntry("SubStationAlphaOriginalTranslation"), 1, 2, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Original Editing:")), 0, 3, 1, 1);
		grid.Attach(CreateEntry("SubStationAlphaOriginalEditing"), 1, 3, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Original Timing:")), 0, 4, 1, 1);
		grid.Attach(CreateEntry("SubStationAlphaOriginalTiming"), 1, 4, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Original Script Checking:")), 0, 5, 1, 1);
		grid.Attach(CreateEntry("SubStationAlphaOriginalScriptChecking"), 1, 5, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Script Updated By:")), 0, 6, 1, 1);
		grid.Attach(CreateEntry("SubStationAlphaScriptUpdatedBy"), 1, 6, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Collisions:")), 0, 7, 1, 1);
		grid.Attach(CreateEntry("SubStationAlphaCollisions"), 1, 7, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Timer:")), 0, 8, 1, 1);
		grid.Attach(CreateEntry("SubStationAlphaTimer"), 1, 8, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Play Res X:")), 0, 9, 1, 1);
		grid.Attach(CreateSpinButton("SubStationAlphaPlayResX", 0, 10000, 1), 1, 9, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Play Res Y:")), 0, 10, 1, 1);
		grid.Attach(CreateSpinButton("SubStationAlphaPlayResY", 0, 10000, 1), 1, 10, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Play Depth:")), 0, 11, 1, 1);
		grid.Attach(CreateSpinButton("SubStationAlphaPlayDepth", 0, 10000, 1), 1, 11, 1, 1);

		//SubViewer 1
		grid = CreatePageWithGrid(notebook, "SubViewer 1");
		grid.Attach(CreateLabel(Catalog.GetString("Title:")), 0, 0, 1, 1);
		grid.Attach(CreateEntry("Title"), 1, 0, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Author:")), 0, 1, 1, 1);
		grid.Attach(CreateEntry("Author"), 1, 1, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Source:")), 0, 2, 1, 1);
		grid.Attach(CreateEntry("Source"), 1, 2, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Program:")), 0, 3, 1, 1);
		grid.Attach(CreateEntry("Program"), 1, 3, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("File Path:")), 0, 4, 1, 1);
		grid.Attach(CreateEntry("FilePath"), 1, 4, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Delay:")), 0, 5, 1, 1);
		grid.Attach(CreateSpinButton("Delay", 0, 1000000, 1), 1, 5, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("CD Track:")), 0, 6, 1, 1);
		grid.Attach(CreateSpinButton("CDTrack", 1, 1000, 1), 1, 6, 1, 1);

		//SubViewer 2
		grid = CreatePageWithGrid(notebook, "SubViewer 2");
		grid.Attach(CreateLabel(Catalog.GetString("Title:")), 0, 0, 1, 1);
		grid.Attach(CreateEntry("Title"), 1, 0, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Author:")), 0, 1, 1, 1);
		grid.Attach(CreateEntry("Author"), 1, 1, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Source:")), 0, 2, 1, 1);
		grid.Attach(CreateEntry("Source"), 1, 2, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Program:")), 0, 3, 1, 1);
		grid.Attach(CreateEntry("Program"), 1, 3, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("File Path:")), 0, 4, 1, 1);
		grid.Attach(CreateEntry("FilePath"), 1, 4, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Font Name:")), 0, 5, 1, 1);
		grid.Attach(CreateEntry("SubViewer2FontName"), 1, 5, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Font Color:")), 0, 6, 1, 1);
		grid.Attach(CreateEntry("SubViewer2FontColor"), 1, 6, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Font Style:")), 0, 7, 1, 1);
		grid.Attach(CreateEntry("SubViewer2FontStyle"), 1, 7, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Font Size:")), 0, 8, 1, 1);
		grid.Attach(CreateSpinButton("SubViewer2FontSize", 1, 1000, 1), 1, 8, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("Delay:")), 0, 9, 1, 1);
		grid.Attach(CreateSpinButton("Delay", 0, 1000000, 1), 1, 9, 1, 1);
		grid.Attach(CreateLabel(Catalog.GetString("CD Track:")), 0, 10, 1, 1);
		grid.Attach(CreateSpinButton("CDTrack", 1, 1000, 1), 1, 10, 1, 1);

		//Finalize
		dialog.ContentArea.Add(notebook);
		dialog.ContentArea.ShowAll();

		return dialog;
	}

	private Grid CreatePageWithGrid(Notebook notebook, string tabLabel) {
		ScrolledWindow window = new ScrolledWindow();

		Grid grid = new Grid();
		grid.BorderWidth = WidgetStyles.BorderWidthLarge;
		grid.ColumnSpacing = WidgetStyles.ColumnSpacingMedium;
		grid.RowSpacing = WidgetStyles.RowSpacingLarge;
		window.Add(grid);

		notebook.AppendPage(window, new Label(tabLabel));
		return grid;
	}

	private Label CreateLabel(string text) {
		return Util.CreateLabel(text, 0, 0.5f);
	}

	private Entry CreateEntry(string propertyName) {
		PropertyInfo property = typeof(Headers).GetProperty(propertyName);
		string value = (string)property.GetValue(headers) ?? ""; //Make sure we don't pass null to the entry to avoid gtk warnings

		Entry entry = new Entry(value);
		entry.Hexpand = true;
		entry.Changed += (object sender, EventArgs e) => SetProperty(sender, property, ((Entry)sender).Text);
		PropertyChanged += (object sender, string prop, object newValue) => {
			if ((sender != entry) && (prop == propertyName)) {
				entry.Text = (string)newValue;
			}
		};

		return entry;
	}

	//Values: (id, text) pairs
	private ComboBoxText CreateComboBoxText(string propertyName, params string[] values) {
		PropertyInfo property = typeof(Headers).GetProperty(propertyName);

		ComboBoxText comboBox = new ComboBoxText();
		for (int i = 0; i < values.Length; i += 2) {
			comboBox.Append(values[i], values[i + 1]);
		}

		comboBox.ActiveId = (string)property.GetValue(headers);
		comboBox.Changed += (object sender, EventArgs e) => SetProperty(sender, property, ((ComboBox)sender).ActiveId);
		PropertyChanged += (object sender, string prop, object newValue) => {
			if ((sender != comboBox) && (prop == propertyName)) {
				comboBox.ActiveId = (string)newValue;
			}
		};

		return comboBox;
	}

	private SpinButton CreateSpinButton(string propertyName, int min, int max, int step) {
		PropertyInfo property = typeof(Headers).GetProperty(propertyName);
		object value = property.GetValue(headers);

		SpinButton spinButton = new SpinButton(min, max, step);
		spinButton.Numeric = true;
		spinButton.Value = (int)value;

		spinButton.ValueChanged += (object sender, EventArgs e) => SetProperty(sender, property, ((SpinButton)sender).ValueAsInt);

		PropertyChanged += (object sender, string prop, object newValue) => {
			if ((sender != spinButton) && (prop == propertyName)) {
				spinButton.Value = (int)newValue;
			}
		};

		return spinButton;
	}

	private void SetProperty (object sender, PropertyInfo property, object value) {
		/* Prevent unnecessary event fires. Ex: when an entry is changed, other entries for the same property are also changed.
		 * Changing each of them would trigger a PropertyChanged event, which would then be handled again by all the others (in a loop).
		 */
		object currentValue = property.GetValue(headers);
		if (Object.Equals(currentValue, value)) { //Comparison with == wasn't working here, even though both values are value types
			return;
		}

		property.SetValue(headers, value);
		PropertyChanged(sender, property.Name, value);
	}

	private void SaveHeaders () {
		Headers targetHeaders = Base.Document.Subtitles.Properties.Headers;

		foreach (PropertyInfo property in typeof(Headers).GetProperties()) {
			property.SetValue(targetHeaders, property.GetValue(headers));
		}
	}

	/* Event members */

	protected override bool ProcessResponse (ResponseType response) {
		if (response == ResponseType.Ok) {
			SaveHeaders();
		}
		return false;
	}

}

}