/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2007-2020 Pedro Castro
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
using GnomeSubtitles.Ui.Component;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;

namespace GnomeSubtitles.Dialog {

public class PreferencesDialog : BaseDialog {

	/* Components */
	private EncodingComboBox fileOpenEncoding = null;
	private EncodingComboBox fileOpenFallbackEncoding = null;
	private EncodingComboBox fileSaveEncoding = null;
	private SubtitleFormatComboBox fileSaveFormat = null;
	private NewlineTypeComboBox fileSaveNewline = null;

	/* Widgets */
	private SpinButton videoSeekRewindSpinButton = null;
	private SpinButton reactionDelaySpinButton = null;
	private SpinButton autoBackupSpinButton = null;
	
	public PreferencesDialog () : base() {
		Init(BuildDialog());
	}

	/* Private members */

	private Gtk.Dialog BuildDialog () {
		Gtk.Dialog dialog = new Gtk.Dialog(Catalog.GetString("Preferences"), Base.Ui.Window, DialogFlags.Modal | DialogFlagsUseHeaderBar);

		Notebook notebook = new Notebook();
		notebook.Expand = true;
		notebook.TabPos = PositionType.Top;
		notebook.ShowBorder = false;
		
		notebook.AppendPage(BuildFilesPage(), new Label(Catalog.GetString("Files")));
		notebook.AppendPage(BuildEditingPage(), new Label(Catalog.GetString("Editing")));
		
		dialog.ContentArea.Add(notebook);
		dialog.ContentArea.ShowAll();

		return dialog;
	}
	
	private Widget BuildFilesPage () {
		Box box = new Box(Orientation.Vertical, WidgetStyles.BoxSpacingLarge);
		box.BorderWidth = WidgetStyles.BorderWidthLarge;
		
		//Translation File Saving
		
		CheckButton translationFileSaving = new CheckButton(Catalog.GetString("When saving subtitle files, automatically save their _translation"));
		translationFileSaving.Active = Base.Config.FileTranslationSaveAll;
		translationFileSaving.Toggled += OnTranslationSaveAllToggled;
		box.Add(Util.CreateFrameWithContent(Catalog.GetString("Translation File Saving"), translationFileSaving));


		//File Open Dialog
		
		Grid fileOpenDialogGrid = new Grid();
		fileOpenDialogGrid.RowSpacing = WidgetStyles.RowSpacingMedium;
		fileOpenDialogGrid.ColumnSpacing = WidgetStyles.ColumnSpacingMedium;
		
		Label fileOpenEncodingLabel = Util.CreateLabel(Catalog.GetString("Character _encoding to use:"), 0, 0.5f);
		fileOpenEncoding = BuildFileOpenEncodingComboBox();
		fileOpenEncodingLabel.MnemonicWidget = fileOpenEncoding.Widget;
		fileOpenDialogGrid.Attach(fileOpenEncodingLabel, 0, 0, 1, 1);
		fileOpenDialogGrid.Attach(fileOpenEncoding.Widget, 1, 0, 1, 1);
		
		Label fileOpenFallbackEncodingLabel = Util.CreateLabel(Catalog.GetString("If auto detection _fails, use:"), 0, 0.5f);
		fileOpenFallbackEncoding = BuildFileOpenFallbackEncodingComboBox();
		fileOpenFallbackEncodingLabel.MnemonicWidget = fileOpenFallbackEncoding.Widget;
		fileOpenDialogGrid.Attach(fileOpenFallbackEncodingLabel, 0, 1, 1, 1);
		fileOpenDialogGrid.Attach(fileOpenFallbackEncoding.Widget, 1, 1, 1, 1);
		
		CheckButton videoAutoChoose = new CheckButton(Catalog.GetString("Automatically choose the _video file to open"));
		videoAutoChoose.Active = Base.Config.VideoAutoChooseFile;
		videoAutoChoose.Toggled += OnVideoAutoChooseFileToggled;
		fileOpenDialogGrid.Attach(videoAutoChoose, 0, 2, 2, 1);
		
		box.Add(Util.CreateFrameWithContent(Catalog.GetString("File Open Dialog"), fileOpenDialogGrid));
		
		
		//File Save As Dialog
		
		Grid fileSaveAsDialogGrid = new Grid();
		fileSaveAsDialogGrid.RowSpacing = WidgetStyles.RowSpacingMedium;
		fileSaveAsDialogGrid.ColumnSpacing = WidgetStyles.ColumnSpacingMedium;
		
		Label fileSaveFormatLabel = Util.CreateLabel(Catalog.GetString("_Subtitle format to use:"), 0, 0.5f);
		fileSaveFormat = BuildFileSaveFormatComboBox();
		fileSaveFormatLabel.MnemonicWidget = fileSaveFormat.Widget;
		fileSaveAsDialogGrid.Attach(fileSaveFormatLabel, 0, 0, 1, 1);
		fileSaveAsDialogGrid.Attach(fileSaveFormat.Widget, 1, 0, 1, 1);
		
		Label fileSaveEncodingLabel = Util.CreateLabel(Catalog.GetString("Ch_aracter encoding to use:"), 0, 0.5f);
		fileSaveEncoding = BuildFileSaveEncodingComboBox();
		fileSaveEncodingLabel.MnemonicWidget = fileSaveEncoding.Widget;
		fileSaveAsDialogGrid.Attach(fileSaveEncodingLabel, 0, 1, 1, 1);
		fileSaveAsDialogGrid.Attach(fileSaveEncoding.Widget, 1, 1, 1, 1);
		
		Label fileSaveNewlineLabel = Util.CreateLabel(Catalog.GetString("_Newline type to use:"), 0, 0.5f);
		fileSaveNewline = BuildFileSaveNewlineComboBox();
		fileSaveNewlineLabel.MnemonicWidget = fileSaveNewline.Widget;
		fileSaveAsDialogGrid.Attach(fileSaveNewlineLabel, 0, 2, 1, 1);
		fileSaveAsDialogGrid.Attach(fileSaveNewline.Widget, 1, 2, 1, 1);
		
		box.Add(Util.CreateFrameWithContent(Catalog.GetString("File Save As Dialog"), fileSaveAsDialogGrid));
		
		
		//Backup
		
		Box backupBox = new Box(Orientation.Horizontal, 3);
		bool autoBackupEnabled = Base.Config.BackupAuto;
		
		CheckButton backupCheckButton = new CheckButton(Catalog.GetString("Create a _backup copy of files every"));
		backupCheckButton.Active = autoBackupEnabled;
		backupCheckButton.Toggled += OnAutoBackupToggled;
		backupBox.Add(backupCheckButton);
		
		autoBackupSpinButton = new SpinButton(1, 90, 1);
		autoBackupSpinButton.Numeric = true;
		autoBackupSpinButton.WidthChars = 2;
		autoBackupSpinButton.Sensitive = autoBackupEnabled;
		autoBackupSpinButton.Value = Base.Config.BackupTime / 60; //Minutes
		autoBackupSpinButton.ValueChanged += OnAutoBackupTimeSpinButtonValueChanged;
		backupBox.Add(autoBackupSpinButton);
		
		backupBox.Add(new Label(Catalog.GetString("minutes")));
		
		box.Add(Util.CreateFrameWithContent(Catalog.GetString("Backup"), backupBox));
		
		return box;
	}
	
	private Widget BuildEditingPage () {
		Box box = new Box(Orientation.Vertical, WidgetStyles.BoxSpacingLarge);
		box.BorderWidth = WidgetStyles.BorderWidthLarge;
		
		//Video Seeking
		
		Box videoSeekVBox = new Box(Orientation.Vertical, WidgetStyles.BoxSpacingMedium);
		
		CheckButton videoSeekCheckButton = new CheckButton(Catalog.GetString("_Seek the video to the selected subtitle when changing timings"));
		videoSeekCheckButton.Active = Base.Config.VideoSeekOnChange;
		videoSeekCheckButton.Toggled += OnVideoSeekToggled;
		videoSeekVBox.Add(videoSeekCheckButton);
		
		Box videoSeekRewindHBox = new Box(Orientation.Horizontal, 3);
		videoSeekRewindHBox.Add(new Label(Catalog.GetString("Play")));
		videoSeekRewindSpinButton = new SpinButton(0, 2000, 50);
		videoSeekRewindSpinButton.WidthChars = 4;
		videoSeekRewindSpinButton.Value = Base.Config.VideoSeekOnChangeRewind;
		videoSeekRewindSpinButton.Sensitive = videoSeekCheckButton.Active;
		videoSeekRewindSpinButton.ValueChanged += OnVideoSeekRewindSpinButtonValueChanged;
		videoSeekRewindHBox.Add(videoSeekRewindSpinButton);
		videoSeekRewindHBox.Add(new Label(Catalog.GetString("ms before the actual start to help review new timings")));
		
		videoSeekVBox.Add(videoSeekRewindHBox);
		box.Add(Util.CreateFrameWithContent(Catalog.GetString("Video Seeking"), videoSeekVBox));


		//Gap Between Subtitles
		
		Box splitHBox = new Box(Orientation.Horizontal, 3);
		splitHBox.Add(new Label(Catalog.GetString("Leave")));
		SpinButton subtitleSplitSpinButton = new SpinButton(0, 2000, 50);
		subtitleSplitSpinButton.WidthChars = 4;
		subtitleSplitSpinButton.Value = Base.Config.TimingsTimeBetweenSubtitles;
		subtitleSplitSpinButton.ValueChanged += OnSubtitleSplitSpinButtonValueChanged;
		splitHBox.Add(subtitleSplitSpinButton);
		splitHBox.Add(new Label(Catalog.GetString("ms between subtitles when inserting or splitting")));
		
		box.Add(Util.CreateFrameWithContent(Catalog.GetString("Gap Between Subtitles"), splitHBox));


		//Reaction Delay

		Box reactionDelayHBox = new Box(Orientation.Horizontal, 3);
		bool reactionDelayEnabled = Base.Config.VideoApplyReactionDelay;
		
		CheckButton reactionDelayCheckButton = new CheckButton(Catalog.GetString("Subtr_act"));
		reactionDelayCheckButton.Active = reactionDelayEnabled;
		reactionDelayCheckButton.Toggled += OnReactionDelayToggled;
		reactionDelayHBox.Add(reactionDelayCheckButton);
		
		reactionDelaySpinButton = new SpinButton(0, 2000, 50);
		reactionDelaySpinButton.WidthChars = 4;
		reactionDelaySpinButton.Sensitive = reactionDelayEnabled;
		reactionDelaySpinButton.Value = Base.Config.VideoReactionDelay;
		reactionDelaySpinButton.ValueChanged += OnReactionDelaySpinButtonValueChanged;
		reactionDelayHBox.Add(reactionDelaySpinButton);

		reactionDelayHBox.Add(new Label(Catalog.GetString("ms when setting subtitle start/end while playing the video")));
		box.Add(Util.CreateFrameWithContent(Catalog.GetString("Reaction Delay"), reactionDelayHBox));

		return box;
	}

	private EncodingComboBox BuildFileOpenEncodingComboBox () {
		string[] additionalActions = { Catalog.GetString("Remember the last used encoding") };
		int fixedEncoding = -1;
		ConfigFileOpenEncodingOption fileOpenEncodingOption = Base.Config.FileOpenEncodingOption;
		if (fileOpenEncodingOption == ConfigFileOpenEncodingOption.Specific) {
			string encodingCode = Base.Config.FileOpenEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingCode, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		EncodingComboBox comboBox = new EncodingComboBox(true, additionalActions, fixedEncoding);
		if (fileOpenEncodingOption != ConfigFileOpenEncodingOption.Specific) {
			comboBox.ActiveSelection = (int)fileOpenEncodingOption;
		}
		comboBox.SelectionChanged += OnDefaultsFileOpenEncodingChanged;
		
		return comboBox;
	}

	private EncodingComboBox BuildFileOpenFallbackEncodingComboBox () {
		int fixedEncoding = -1;
		ConfigFileOpenFallbackEncoding fileOpenFallbackEncodingConfig = Base.Config.FileOpenFallbackEncoding;
		if (fileOpenFallbackEncodingConfig == ConfigFileOpenFallbackEncoding.Fixed) {
			string encodingCode = Base.Config.FileOpenFallbackEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingCode, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		EncodingComboBox comboBox = new EncodingComboBox(false, null, fixedEncoding);
		comboBox.SelectionChanged += OnDefaultsFileOpenFallbackEncodingChanged;
		return comboBox;
	}

	private EncodingComboBox BuildFileSaveEncodingComboBox () {
		string[] additionalActions = { Catalog.GetString("Keep the encoding used on file open"), Catalog.GetString("Remember the last used encoding") };
		int fixedEncoding = -1;
		ConfigFileSaveEncodingOption fileSaveEncodingOption = Base.Config.FileSaveEncodingOption;
		if (fileSaveEncodingOption == ConfigFileSaveEncodingOption.Specific) {
			string encodingCode = Base.Config.FileSaveEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingCode, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}

		EncodingComboBox comboBox = new EncodingComboBox(false, additionalActions, fixedEncoding);
		if (fileSaveEncodingOption != ConfigFileSaveEncodingOption.Specific) {
			comboBox.ActiveSelection = (int)fileSaveEncodingOption;
		}
		comboBox.SelectionChanged += OnDefaultsFileSaveEncodingChanged;
		return comboBox;
	}

	private SubtitleFormatComboBox BuildFileSaveFormatComboBox () {
		string[] additionalActions = { Catalog.GetString("Keep the format used on file open"), Catalog.GetString("Remember the last used format") };
		SubtitleType fixedFormat = SubtitleType.Unknown;
		ConfigFileSaveFormatOption fileSaveFormatOption = Base.Config.FileSaveFormatOption;
		if (fileSaveFormatOption == ConfigFileSaveFormatOption.Specific) {
			fixedFormat = Base.Config.FileSaveFormatFixed;
		}

		SubtitleFormatComboBox comboBox = new SubtitleFormatComboBox(fixedFormat, additionalActions);
		if (fileSaveFormatOption != ConfigFileSaveFormatOption.Specific) {
			comboBox.ActiveSelection = (int)fileSaveFormatOption;
		}
		comboBox.SelectionChanged += OnDefaultsFileSaveFormatChanged;
		return comboBox;
	}

	private NewlineTypeComboBox BuildFileSaveNewlineComboBox () {
		string[] additionalActions = { Catalog.GetString("Remember the last used type") };
		NewlineType newlineTypeToSelect = NewlineType.Unknown;
		ConfigFileSaveNewlineOption fileSaveNewlineOption = Base.Config.FileSaveNewlineOption;
		if (fileSaveNewlineOption == ConfigFileSaveNewlineOption.Specific) {
			newlineTypeToSelect = Base.Config.FileSaveNewline;
		}

		NewlineTypeComboBox comboBox = new NewlineTypeComboBox(newlineTypeToSelect, additionalActions);
		if (fileSaveNewlineOption != ConfigFileSaveNewlineOption.Specific) {
			comboBox.ActiveSelection = (int)fileSaveNewlineOption;
		}
		comboBox.SelectionChanged += OnDefaultsFileSaveNewlineChanged;
		return comboBox;
	}


	/* Event members */

	private void OnDefaultsFileOpenEncodingChanged (object o, EventArgs args) {
		int active = fileOpenEncoding.ActiveSelection;
		ConfigFileOpenEncodingOption activeOption = (ConfigFileOpenEncodingOption)Enum.ToObject(typeof(ConfigFileOpenEncodingOption), active);
		if (((int)activeOption) > ((int)ConfigFileOpenEncodingOption.Specific)) //Positions higher than specific are always specific too
			activeOption = ConfigFileOpenEncodingOption.Specific;

		Base.Config.FileOpenEncodingOption = activeOption;
		/* If encoding option is specific, encodingOption=Specific and encoding holds the encoding name */
		if (activeOption == ConfigFileOpenEncodingOption.Specific) {
			EncodingDescription chosenEncoding = fileOpenEncoding.ChosenEncoding;
			if (!chosenEncoding.Equals(EncodingDescription.Empty)) {
				Base.Config.FileOpenEncodingFixed = chosenEncoding.Code;
			}
		}
		else {
			/* If encoding is current locale, encoding holds current locale too, otherwise it just holds auto detect */
			ConfigFileOpenEncoding encodingToStore = ConfigFileOpenEncoding.AutoDetect;
			if (activeOption == ConfigFileOpenEncodingOption.CurrentLocale) {
				encodingToStore = ConfigFileOpenEncoding.CurrentLocale;
			}
			Base.Config.FileOpenEncoding = encodingToStore;
		}
	}

	private void OnDefaultsFileOpenFallbackEncodingChanged (object o, EventArgs args) {
		if (fileOpenFallbackEncoding.IsChosenCurrentLocale)
			Base.Config.FileOpenFallbackEncoding = ConfigFileOpenFallbackEncoding.CurrentLocale;
		else {
			EncodingDescription chosenEncoding = fileOpenFallbackEncoding.ChosenEncoding;
			if (!chosenEncoding.Equals(EncodingDescription.Empty)) {
				Base.Config.FileOpenFallbackEncodingFixed = chosenEncoding.Code;
			}
		}
	}

	private void OnDefaultsFileSaveEncodingChanged (object o, EventArgs args) {
		int active = fileSaveEncoding.ActiveSelection;
		ConfigFileSaveEncodingOption activeOption = (ConfigFileSaveEncodingOption)Enum.ToObject(typeof(ConfigFileSaveEncodingOption), active);
		if (((int)activeOption) > ((int)ConfigFileSaveEncodingOption.Specific)) //Positions higher than specific are always specific too
			activeOption = ConfigFileSaveEncodingOption.Specific;

		Base.Config.FileSaveEncodingOption = activeOption;
		/* If encoding is specific, encodingOption=Specific and encoding holds the encoding name */
		if (activeOption == ConfigFileSaveEncodingOption.Specific) {
			EncodingDescription chosenEncoding = fileSaveEncoding.ChosenEncoding;
			if (!chosenEncoding.Equals(EncodingDescription.Empty)) {
				Base.Config.FileSaveEncodingFixed = chosenEncoding.Code;
			}
		}
		else {
			/* If encoding option is current locale, encoding holds current locale too, otherwise it just holds keep existing */
			ConfigFileSaveEncoding encodingToStore = ConfigFileSaveEncoding.KeepExisting;
			if (activeOption == ConfigFileSaveEncodingOption.CurrentLocale) {
				encodingToStore = ConfigFileSaveEncoding.CurrentLocale;
			}
			Base.Config.FileSaveEncoding = encodingToStore;
		}
	}

	private void OnDefaultsFileSaveFormatChanged (object o, EventArgs args) {
		int active = fileSaveFormat.ActiveSelection;
		ConfigFileSaveFormatOption activeOption = (ConfigFileSaveFormatOption)Enum.ToObject(typeof(ConfigFileSaveFormatOption), active);
		if (((int)activeOption) > ((int)ConfigFileSaveFormatOption.Specific)) //Positions higher than specific are always specific too
			activeOption = ConfigFileSaveFormatOption.Specific;

		Base.Config.FileSaveFormatOption = activeOption;
		/* If format is specific, formatOption=Specific and format holds the format name */
		if (activeOption == ConfigFileSaveFormatOption.Specific) {
			SubtitleType chosenFormat = fileSaveFormat.ChosenSubtitleType;
			if (!chosenFormat.Equals(SubtitleType.Unknown)) {
				Base.Config.FileSaveFormatFixed = chosenFormat;
			}
		}
		else {
			/* If format option is keep existing or remember last, use keep existing */
			Base.Config.FileSaveFormat = ConfigFileSaveFormat.KeepExisting;
		}
	}

	private void OnDefaultsFileSaveNewlineChanged (object o, EventArgs args) {
		int active = fileSaveNewline.ActiveSelection;
		ConfigFileSaveNewlineOption activeOption = (ConfigFileSaveNewlineOption)Enum.ToObject(typeof(ConfigFileSaveNewlineOption), active);
		if (((int)activeOption) > ((int)ConfigFileSaveNewlineOption.Specific)) //Positions higher than specific are always specific too
			activeOption = ConfigFileSaveNewlineOption.Specific;

		Base.Config.FileSaveNewlineOption = activeOption;
		/* If newline is specific, newlineOption=Specific and newline holds the newline type name */
		if (activeOption == ConfigFileSaveNewlineOption.Specific) {
			NewlineType chosenNewlineType = fileSaveNewline.ChosenNewlineType;
			if (!chosenNewlineType.Equals(NewlineType.Unknown)) {
				Base.Config.FileSaveNewline = chosenNewlineType;
			}
		}
		else {
			/* If newline option is remember last, use the system default */
			Base.Config.FileSaveNewline = Core.Util.GetSystemNewlineType();
		}
	}

	private void OnVideoAutoChooseFileToggled (object o, EventArgs args) {
		Base.Config.VideoAutoChooseFile = (o as CheckButton).Active;
	}

	private void OnTranslationSaveAllToggled (object o, EventArgs args) {
		Base.Config.FileTranslationSaveAll = (o as CheckButton).Active;
	}

	private void OnAutoBackupToggled (object o, EventArgs args) {
		bool isActive = (o as CheckButton).Active;

		Base.Config.BackupAuto = isActive;
		autoBackupSpinButton.Sensitive = isActive;

		Base.Backup.ReCheck();
	}

	private void OnAutoBackupTimeSpinButtonValueChanged (object o, EventArgs args) {
		Base.Config.BackupTime = (o as SpinButton).ValueAsInt * 60; //seconds
		Base.Backup.ReCheck();
	}

	private void OnReactionDelayToggled (object o, EventArgs args) {
		bool isActive = (o as CheckButton).Active;

		Base.Config.VideoApplyReactionDelay = isActive;
		reactionDelaySpinButton.Sensitive = isActive;
	}

	private void OnReactionDelaySpinButtonValueChanged (object o, EventArgs args) {
		Base.Config.VideoReactionDelay = (o as SpinButton).ValueAsInt;
	}

	private void OnVideoSeekToggled (object o, EventArgs args) {
		bool isActive = (o as CheckButton).Active;

		Base.Config.VideoSeekOnChange = isActive;
		videoSeekRewindSpinButton.Sensitive = isActive;
	}

	private void OnVideoSeekRewindSpinButtonValueChanged (object o, EventArgs args) {
		Base.Config.VideoSeekOnChangeRewind = (o as SpinButton).ValueAsInt;
	}

	private void OnSubtitleSplitSpinButtonValueChanged (object o, EventArgs args) {
		Base.Config.TimingsTimeBetweenSubtitles = (o as SpinButton).ValueAsInt;
	}
	
}

}