/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2022 Pedro Castro
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

using GnomeSubtitles.Core.Command;
using GnomeSubtitles.Dialog;
using GnomeSubtitles.Execution;
using GnomeSubtitles.Ui;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;
using System.Text;

namespace GnomeSubtitles.Core {

/* Delegates */
public delegate void DocumentHandler (Document document);
public delegate void VideoLoadedHandler (Uri videoUri);
public delegate void TimingModeChangedHandler (TimingMode timingMode);
public delegate void BasicEventHandler ();

public static class Base {
	private static MainUi ui = null;
	private static ExecutionContext executionContext = null;
	private static EventHandlers handlers = null;
	private static CommandManager commandManager = null;
	private static Clipboards clipboards = null;
	private static GlobalAccelerators globalAccelerators = null;
	private static DragDrop dragDrop = null;
	private static Config config = null;
	private static Dialogs dialogs = null;
	private static SpellLanguages spellLanguages = null;
	private static Backup backup = null;

	private static Document document = null;
	private static Uri videoUri = null;
	private static TimingMode timingMode = TimingMode.Times;

	/* Events */
	public static event BasicEventHandler InitFinished;
	public static event DocumentHandler DocumentLoaded;
	public static event DocumentHandler DocumentUnloaded;
	public static event BasicEventHandler TranslationLoaded;
	public static event BasicEventHandler TranslationUnloaded;
	public static event VideoLoadedHandler VideoLoaded;
	public static event BasicEventHandler VideoUnloaded;
	public static event TimingModeChangedHandler TimingModeChanged;


	/* Public properties */

	public static MainUi Ui {
		get { return ui; }
	}

	public static ExecutionContext ExecutionContext {
		get { return executionContext; }
	}

	public static EventHandlers Handlers {
		get { return handlers; }
	}

	public static CommandManager CommandManager {
		get { return commandManager; }
	}

	public static Clipboards Clipboards {
		get { return clipboards; }
	}

	public static GlobalAccelerators GlobalAccelerators {
		get { return globalAccelerators; }
	}

	public static DragDrop DragDrop {
		get { return dragDrop; }
	}

	public static Config Config {
		get { return config; }
	}

	public static Dialogs Dialogs {
		get { return dialogs; }
	}

	public static SpellLanguages SpellLanguages {
		get { return spellLanguages; }
	}

	public static Backup Backup {
		get { return backup; }
	}

	public static Document Document {
		get { return document; }
	}

	public static Uri VideoUri {
		get { return videoUri; }
	}

	public static bool IsDocumentLoaded {
		get { return document != null; }
	}

	public static bool IsVideoLoaded {
		get { return videoUri != null; }
	}

	public static TimingMode TimingMode {
		get { return timingMode; }
		set {
			if (timingMode != value) {
				timingMode = value;
				EmitTimingModeChangedEvent();
			}
		}
	}

	public static bool TimingModeIsFrames {
		get { return timingMode == TimingMode.Frames; }
	}

	public static bool TimingModeIsTimes {
		get { return timingMode == TimingMode.Times; }
	}


	/* Public methods */

	/// <summary>Runs the main GUI, after initialization.</summary>
	public static void Execute (ExecutionContext context) {
		executionContext = context;
	
		executionContext.Execute(() => {
			Init();
			ui.Start();
		});
	}

	/// <summary>Quits the program.</summary>
	public static bool Quit () {
		return ui.Quit();
	}

	public static void NewDocument () {
		if (IsDocumentLoaded)
			CloseDocument();

		document = new Document();
		EmitDocumentLoadedEvent();

		/* Create first subtitle. This happens after EmitDocumentLoadedEvent for all widgets to be ready */
		if (document.Subtitles.Count == 0)
			commandManager.Execute(new InsertFirstSubtitleCommand());
	}

	public static void OpenDocument (string path, Encoding encoding) {
		if (IsDocumentLoaded)
			CloseDocument();

		document = new Document(path, encoding);
		TimingMode = document.TextFile.TimingMode;
		EmitDocumentLoadedEvent();

		/* Select first subtitle. This happens after EmitDocumentLoadedEvent for all widgets to be ready */
		Ui.View.Selection.SelectFirst();
	}

	public static void CloseDocument () {
		if (!IsDocumentLoaded)
			return;

		if (document.IsTranslationLoaded)
			CloseTranslation();

		document.Close();
		CommandManager.Clear();
		EmitDocumentUnloadedEvent();

		document = null;
	}

	public static void OpenVideo (Uri uri) {
		if (uri == null)
			return;

		if (IsVideoLoaded)
			CloseVideo();

		ui.Video.Open(uri);
	}

	public static void UpdateFromVideoLoaded (Uri uri) {
		videoUri = uri;

		EmitVideoLoadedEvent();
	}

	public static void CloseVideo () {
		ui.Video.Close();
		videoUri = null;

		EmitVideoUnloadedEvent();
	}

	public static void Open (string path, Encoding encoding, Uri videoUri) {
		OpenDocument(path, encoding);
		OpenVideo(videoUri);
	}
	
	public static void OpenTranslation (string path, Encoding encoding) {
		if (document.IsTranslationLoaded)
			CloseTranslation();

		document.OpenTranslation(path, encoding);
		EmitTranslationLoadedEvent();

		/* Reselect, for the widgets to update accordingly */
		Ui.View.Selection.Reselect();
	}

	public static void NewTranslation () {
		if (document.IsTranslationLoaded)
			CloseTranslation();

		document.NewTranslation();
		EmitTranslationLoadedEvent();

		/* Reselect, for the widgets to update accordingly */
		Ui.View.Selection.Reselect();
	}

	public static void CloseTranslation () {
		document.CloseTranslation();
		EmitTranslationUnloadedEvent();
	}

	public static Widget GetWidget (string name) {
		return MainUi.GetWidget(name);
	}

	/* Private members */

	/// <summary>Initializes the base program structure.</summary>
	private static void Init () {
		Catalog.Init(ExecutionContext.TranslationDomain, ExecutionContext.LocaleDir);

		/* Initialize Command manager */
		commandManager = new CommandManager();

		/* Initialize handlers */
		handlers = new EventHandlers();

		/* Initialize misc */
		clipboards = new Clipboards();
		globalAccelerators = new GlobalAccelerators();
		dragDrop = new DragDrop();
		config = new Config();
		dialogs = new Dialogs();
		spellLanguages = new SpellLanguages();
		backup = new Backup();

		/* Initialize the GUI */
		ui = new MainUi(handlers);
		
		//The window must be made visible here because classes such as EventHandlers.OnSizeAllocated) may depend on
		//'ui' being set when the window is made visible (so it can't be made visible inside MainUi's constructor).
		ui.Show();

		clipboards.WatchPrimaryChanges = true;

		EmitInitFinishedEvent();
	}

	/* Event members */

	private static void EmitInitFinishedEvent () {
		if (InitFinished != null)
			InitFinished();
	}

	private static void EmitDocumentLoadedEvent () {
		if (DocumentLoaded != null)
			DocumentLoaded(document);
	}

	private static void EmitDocumentUnloadedEvent () {
		if (DocumentUnloaded != null)
			DocumentUnloaded(document);
	}

	private static void EmitTranslationLoadedEvent () {
		if (TranslationLoaded != null)
			TranslationLoaded();
	}

	private static void EmitTranslationUnloadedEvent () {
		if (TranslationUnloaded != null)
			TranslationUnloaded();
	}

	private static void EmitVideoLoadedEvent () {
		if (VideoLoaded != null)
			VideoLoaded(videoUri);
	}

	private static void EmitVideoUnloadedEvent () {
		if (VideoUnloaded != null)
			VideoUnloaded();
	}

	private static void EmitTimingModeChangedEvent () {
		if (TimingModeChanged != null)
			TimingModeChanged(timingMode);
	}

}

}
