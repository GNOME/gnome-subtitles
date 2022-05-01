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
using GnomeSubtitles.Ui;
using GnomeSubtitles.Ui.View;
using Gtk;
using SubLib.Core.Domain;
using SubLib.Core.Timing;
using System;
using System.Text;

namespace GnomeSubtitles.Core {

public class EventHandlers {
	private bool videoStartEndKeyPressed = false; //Used to match key press and release events


	/* Window */
	
	public void OnWindowKeyPress (object o, KeyPressEventArgs args) {
		if (!videoStartEndKeyPressed
				&& (args.Event.Key == Gdk.Key.j)
				&& args.Event.State.HasFlag(Gdk.ModifierType.ControlMask)
				&& Base.Ui.Video.IsLoaded
				&& Base.GetWidget(WidgetNames.VideoSetSubtitleStartEndButton).Sensitive) {
				
			videoStartEndKeyPressed = true;
			VideoSetSubtitleStartEndBegin();
		}
	}
	
	public void OnWindowKeyRelease (object o, KeyReleaseEventArgs args) {
		if (videoStartEndKeyPressed
				&& (args.Event.Key == Gdk.Key.j)
				&& Base.Ui.Video.IsLoaded
				&& Base.GetWidget(WidgetNames.VideoSetSubtitleStartEndButton).Sensitive) {
				
			videoStartEndKeyPressed = false;
			VideoSetSubtitleStartEndFinish();
		}
	}


	/* File Menu */

	public void OnFileNew (object o, EventArgs args) {
		Base.Ui.New();
	}

	public void OnFileOpen (object o, EventArgs args) {
		Base.Ui.Open();
	}

	public void OnFileSave (object o, EventArgs args) {
		//Save the subtitles
		Boolean saved = Base.Ui.Save();

		//Save the translation
		if (saved && Base.Document.IsTranslationLoaded && Base.Config.FileTranslationSaveAll) {
			OnFileTranslationSave(o, args);
		}
	}

	public void OnFileSaveAs (object o, EventArgs args) {
		//Save the subtitles
		Boolean saved = Base.Ui.SaveAs();

		//Save the translation
		if (saved && Base.Document.IsTranslationLoaded && Base.Config.FileTranslationSaveAll) {
			OnFileTranslationSave(o, args);
		}
	}

	public void OnFileTranslationNew (object o, EventArgs args) {
		Base.Ui.TranslationNew();
	}

	public void OnFileTranslationOpen (object o, EventArgs args) {
		Base.Ui.TranslationOpen();
	}

	public void OnFileTranslationSave (object o, EventArgs args) {
		Base.Ui.TranslationSave();
	}
	
	public void OnFileTranslationReload (object o, EventArgs args) {
		Base.Ui.TranslationReload();
	}

	public void OnFileTranslationSaveAs (object o, EventArgs args) {
		Base.Ui.TranslationSaveAs();
	}

	public void OnFileTranslationClose (object o, EventArgs args) {
		Base.Ui.TranslationClose();
	}

	public void OnFileHeaders (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(FileHeadersDialog)).Show();
	}

	public void OnFileProperties (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(FilePropertiesDialog)).Show();
	}

	public void OnFileClose (object o, EventArgs args) {
		Base.Ui.Close();
	}

    public void OnFileQuit (object o, EventArgs args) {
		Base.Quit();
	}


	/* Edit Menu */

	public void OnEditUndo (object o, EventArgs args) {
		Base.CommandManager.Undo();
	}

	public void OnEditRedo (object o, EventArgs args) {
		Base.CommandManager.Redo();
	}

	public void OnEditCopy (object o, EventArgs args) {
		Base.Clipboards.Copy();
	}

	public void OnEditCut (object o, EventArgs args) {
		Base.Clipboards.Cut();
	}

	public void OnEditPaste (object o, EventArgs args) {
		Base.Clipboards.Paste();
	}

	public void OnEditFormatBold (object o, EventArgs args) {
		bool newBoldValue = ((o is CheckMenuItem) ? (o as CheckMenuItem).Active : (o as ToggleToolButton).Active);
		Base.CommandManager.Execute(new ChangeBoldStyleCommand(newBoldValue));
	}

	public void OnEditFormatItalic (object o, EventArgs args) {
		bool newItalicValue = ((o is CheckMenuItem) ? (o as CheckMenuItem).Active : (o as ToggleToolButton).Active);
		Base.CommandManager.Execute(new ChangeItalicStyleCommand(newItalicValue));
	}

	public void OnEditFormatUnderline (object o, EventArgs args) {
		bool newUnderlineValue = ((o is CheckMenuItem) ? (o as CheckMenuItem).Active : (o as ToggleToolButton).Active);
		Base.CommandManager.Execute(new ChangeUnderlineStyleCommand(newUnderlineValue));
	}

	public void OnEditSplit (object o, EventArgs args) {
		Base.CommandManager.Execute(new SplitSubtitlesCommand());
	}

	public void OnEditMerge (object o, EventArgs args) {
		Base.CommandManager.Execute(new MergeSubtitlesCommand());
	}

	public void OnEditInsertSubtitleBefore (object o, EventArgs args) {
		if ((Base.Document.Subtitles.Count == 0) || (Base.Ui.View.Selection.Count == 0))
			Base.CommandManager.Execute(new InsertFirstSubtitleCommand());
		else
			Base.CommandManager.Execute(new InsertSubtitleBeforeCommand());
	}

	public void OnEditInsertSubtitleAfter (object o, EventArgs args) {
		if (Base.Document.Subtitles.Count == 0)
			Base.CommandManager.Execute(new InsertFirstSubtitleCommand());
		else if (Base.Ui.View.Selection.Count > 0)
			Base.CommandManager.Execute(new InsertSubtitleAfterCommand());
		else
			Base.CommandManager.Execute(new InsertLastSubtitleCommand());
	}

	public void OnEditInsertSubtitleAtVideoPosition (object o, EventArgs args) {
		Base.CommandManager.Execute(new InsertSubtitleAtVideoPositionCommand());
	}

	public void OnEditDeleteSubtitles (object o, EventArgs args) {
		if (Base.Ui.View.Selection.Count > 0)
			Base.CommandManager.Execute(new DeleteSubtitlesCommand());
	}

	public void OnEditPreferences (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(PreferencesDialog)).Show();
	}


	/* View Menu */

	public void OnViewTimes (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			Base.TimingMode = TimingMode.Times;
	}

	public void OnViewFrames (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			Base.TimingMode = TimingMode.Frames;
	}

	public void OnViewLineLengthsToggled (object o, EventArgs args) {
		CheckMenuItem menuItem = o as CheckMenuItem;
		Base.Config.ViewLineLengths = menuItem.Active;
	}

	public void OnViewVideo (object o, EventArgs args) {
		if ((o as CheckMenuItem).Active)
			Base.Ui.Video.Show();
		else
			Base.Ui.Video.Hide();
	}

	public void OnViewVideoSubtitlesText (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			Base.Ui.Video.Overlay.ToShowText = true;
	}

	public void OnViewVideoSubtitlesTranslation (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			Base.Ui.Video.Overlay.ToShowText = false;
	}


	/* Search Menu */

	public void OnSearchFind (object o, EventArgs args) {
		Base.Ui.View.Search.ShowFind();
	}

	public void OnSearchFindNext (object o, EventArgs args) {
		Base.Ui.View.Search.FindNext();
	}

	public void OnSearchFindPrevious (object o, EventArgs args) {
		Base.Ui.View.Search.FindPrevious();
	}

	public void OnSearchReplace (object o, EventArgs args) {
		Base.Ui.View.Search.ShowReplace();
	}


	/*	Timings Menu */

	public void OnTimingsAdjust (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(TimingsAdjustDialog)).Show();
	}

	public void OnTimingsShift (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(TimingsShiftDialog)).Show();
	}

	public void OnTimingsSynchronize (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(TimingsSynchronizeDialog)).Show();
	}


	/* Video Menu */

	public void OnVideoOpen (object o, EventArgs args) {
		Base.Ui.OpenVideo();
	}

	public void OnVideoClose (object o, EventArgs args) {
		Base.CloseVideo();
	}

	public void OnVideoPlayPause (object o, EventArgs args) {
		ToggleButton button = Base.GetWidget(WidgetNames.VideoPlayPauseButton) as ToggleButton;
		button.Active = !button.Active; //Toggle() only emits the Toggled event
	}

	public void OnVideoRewind (object o, EventArgs args) {
		Base.Ui.Video.Rewind();
	}

	public void OnVideoForward (object o, EventArgs args) {
		Base.Ui.Video.Forward();
	}

	public void OnVideoSpeedUp (object o, EventArgs args) {
	    Base.Ui.Video.SpeedUp();
	}

	public void OnVideoSpeedDown (object o, EventArgs args) {
	    Base.Ui.Video.SpeedDown();
	}

	public void OnVideoSpeed (object o, EventArgs args) {
	    Base.Ui.Video.SpeedReset();
	}

	public void OnVideoSeekTo (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(VideoSeekToDialog)).Show();
	}

	//TODO allow to seek when multiple subtitles are selected (use first)
	public void OnVideoSeekToSelection (object o, EventArgs args) {
		Base.Ui.Video.SeekToSelection();
	}

	public void OnVideoSelectNearestSubtitle (object o, EventArgs args) {
		Base.Ui.Video.SelectNearestSubtitle();
	}

	public void OnVideoAutoSelectSubtitles (object o, EventArgs args) {
		Base.Ui.View.SetAutoSelectSubtitles((o as CheckMenuItem).Active);
 	}

	public void OnVideoLoopSelectionPlayback (object o, EventArgs args) {
		Base.Ui.Video.SetLoopSelectionPlayback((o as CheckMenuItem).Active);
 	}

	public void OnVideoSetSubtitleStart (object o, EventArgs args) {
		SetSubtitleStartFromVideo();
	}

	public void OnVideoSetSubtitleEnd (object o, EventArgs args) {
		SetSubtitleEndFromVideo();
	}
	
	

	/* The following 4 event handlers work with the Set Subtitle Start+End button as follows:
		 * 
		 * 1) OnVideoSetSubtitleStartEndEvent + OnVideoSetSubtitleStartEndButtonRelease:
		 *    - Capture mouse click on the button. The mouse click is kept pressed for the duration of the subtitle.
		 *    - Previously we used the GtkButton "pressed" event, however it's now deprecated. It was supposed to be
		 *      replaced by the GtkWidget "button-press-event" however it doesn't behave the same way. The former
		 *      is triggered by a left mouse click on a button, however the new one is not. Apparently, buttons
		 *      are supposed to only trigger the clicked/activated signal when the mouse click is released and not
		 *      when pressed. Therefore, we now use GtkWidget "event" event to capture a mouse button press and
		 *      "button-release-event" for the button release.
		 *    - According to the GtkWidget docs, the GDK_BUTTON_RELEASE_MASK mask is required but all seems to work without it.
		 * 
		 * 2) OnWindowKeyPress + OnWindowKeyRelease:
		 *    - Capture the keyboard shortcut key to perform the button actions (without actually pressing it).
		 */

	public void OnVideoSetSubtitleStartEndEvent (object o, WidgetEventArgs args) {
		if (args.Event is Gdk.EventButton) {
			Gdk.EventButton eventButton = args.Event as Gdk.EventButton;
			if ((eventButton.Type == Gdk.EventType.ButtonPress) && (eventButton.Button == 1)) {
				VideoSetSubtitleStartEndBegin();
			}
		}
	}
	
	public void OnVideoSetSubtitleStartEndButtonRelease (object o, ButtonReleaseEventArgs args) {
		if (args.Event.Button != 1) {
			return;
		}
	
		VideoSetSubtitleStartEndFinish();
	}
	

	/* Tools Menu */

	public void OnToolsAutocheckSpelling (object o, EventArgs args) {
		bool enabled = (o as CheckMenuItem).Active;
		Base.SpellLanguages.Enabled = enabled;
	}

	public void OnToolsSetLanguages (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(SetLanguagesDialog)).Show();
	}

	//public void OnToolsTranslateText (object o, EventArgs args) {
	//	Base.CommandManager.Execute(new TranslatorCommand(true));
	//}

	//public void OnToolsTranslateTranslation (object o, EventArgs args) {
	//	Base.CommandManager.Execute(new TranslatorCommand(false));
	//}


	/*	Help Menu */

	public void OnHelpKeyboardShortcuts (object o, EventArgs args) {
		Util.OpenUrl("https://gnomesubtitles.org/shortcuts");
	}
	
	public void OnHelpDonate (object o, EventArgs args) {
		Util.OpenUrl("https://gnomesubtitles.org/donate");
	}

	public void OnHelpRequestFeature (object o, EventArgs args) {
		Util.OpenUrl("https://gitlab.gnome.org/GNOME/gnome-subtitles/issues/");
	}

	public void OnHelpReportBug (object o, EventArgs args) {
		Util.OpenUrl("https://gitlab.gnome.org/GNOME/gnome-subtitles/issues/");
	}

	public void OnHelpAbout (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(Dialog.AboutDialog)).Show();
	}


	/*	Window	*/

	public void OnWindowDelete (object o, DeleteEventArgs args) {
		bool quit = Base.Quit();
		args.RetVal = !quit; //True to keep the window open
    }
    
    public void OnWindowDestroy (object o, EventArgs args) {
		Base.Config.ViewWindowWidth = Base.Ui.WindowState.Width;
		Base.Config.ViewWindowHeight = Base.Ui.WindowState.Height;
	}

	public void OnSizeAllocated (object o, SizeAllocatedArgs args) {
		Window window = o as Window;

    	int width, height;
    	window.GetSize(out width, out height);

    	Base.Ui.WindowState.Width = width;
    	Base.Ui.WindowState.Height = height;
    }


	/* Subtitle Area */

	public void OnSubtitleAreaDragDataReceived (object o, DragDataReceivedArgs args) {
		string uriString = Encoding.UTF8.GetString(args.SelectionData.Data);
		bool success = false;
		Uri fileUri;

		if (Uri.TryCreate(uriString, UriKind.Absolute, out fileUri) && (args.Info == DragDrop.DragDropTargetUriList)) {
			Base.Ui.Open(fileUri.LocalPath);
			success = true;
		}

		Gtk.Drag.Finish(args.Context, success, false, args.Time);
	}


	/*	Subtitle View	*/

    public void OnRowActivated (object o, RowActivatedArgs args) {
    	Base.Ui.Video.SeekToPath(args.Path);
    }

    public void OnSubtitleViewKeyPressed (object o, KeyPressEventArgs args) {
    	Gdk.Key key = args.Event.Key;
    	if (key == Gdk.Key.Delete)
    		OnEditDeleteSubtitles(o, EventArgs.Empty);
		else if (key == Gdk.Key.Insert)
			OnEditInsertSubtitleAfter(o, EventArgs.Empty);
    }


	/* Video Area */

	public void OnVideoAreaDragDataReceived (object o, DragDataReceivedArgs args) {
		string uriString = Encoding.UTF8.GetString(args.SelectionData.Data);
		bool success = false;
		Uri fileUri;

		if (Uri.TryCreate(uriString, UriKind.Absolute, out fileUri) && (args.Info == DragDrop.DragDropTargetUriList)) {
			Base.OpenVideo(fileUri);
			success = true;
		}

		Gtk.Drag.Finish(args.Context, success, false, args.Time);
	}


	/* Global Accelerators */

	public void OnGlobalSubtitleStartIncrease (object o, EventArgs args) {
		if (Base.Ui.View.Selection.Count == 1) {
			Base.Ui.Edit.SpinButtons.StartSpinButtonIncreaseStep();
		}
	}

	public void OnGlobalSubtitleStartDecrease (object o, EventArgs args) {
		/* Do nothing if there isn't only 1 subtitle selected */
		if (Base.Ui.View.Selection.Count != 1)
			return;

		Subtitle subtitle = Base.Ui.View.Selection.Subtitle;
		if ((Base.TimingModeIsTimes && (subtitle.Times.Start >= TimeSpan.FromMilliseconds(Base.Config.TimingsTimeStep)))
				|| (!Base.TimingModeIsTimes) && (subtitle.Frames.Start >= Base.Config.TimingsFramesStep)){

			Base.Ui.Edit.SpinButtons.StartSpinButtonDecreaseStep();
		}
	}

	public void OnGlobalSubtitleEndIncrease (object o, EventArgs args) {
		if (Base.Ui.View.Selection.Count == 1) {
			Base.Ui.Edit.SpinButtons.EndSpinButtonIncreaseStep();
		}
	}

	public void OnGlobalSubtitleEndDecrease (object o, EventArgs args) {
		/* Do nothing if there isn't only 1 subtitle selected */
		if (Base.Ui.View.Selection.Count != 1)
			return;

		Subtitle subtitle = Base.Ui.View.Selection.Subtitle;
		if ((Base.TimingModeIsTimes && (subtitle.Times.End >= TimeSpan.FromMilliseconds(Base.Config.TimingsTimeStep)))
				|| (!Base.TimingModeIsTimes) && (subtitle.Frames.End >= Base.Config.TimingsFramesStep)){

			Base.Ui.Edit.SpinButtons.EndSpinButtonDecreaseStep();
		}
	}

	public void OnGlobalSelectionShiftIncrease (object o, EventArgs args) {
		/* Do nothing if no subtitles are selected */
		if (Base.Ui.View.Selection.Count == 0)
			return;

		if (Base.TimingModeIsTimes) {
			Base.CommandManager.Execute(new ShiftTimingsCommand(TimeSpan.FromMilliseconds(Base.Config.TimingsTimeStep), SelectionIntended.Simple));
		}
		else {
			Base.CommandManager.Execute(new ShiftTimingsCommand(Base.Config.TimingsFramesStep, SelectionIntended.Simple));
		}
	}

	public void OnGlobalSelectionShiftDecrease (object o, EventArgs args) {
		Subtitle firstSelectedSubtitle = Base.Ui.View.Selection.FirstSubtitle;

		/* Do nothing if no subtitles are selected */
		if (firstSelectedSubtitle == null)
			return;

		if (Base.TimingModeIsTimes) {
			TimeSpan timeStep = TimeSpan.FromMilliseconds(Base.Config.TimingsTimeStep);
			if (firstSelectedSubtitle.Times.Start >= timeStep) {
				Base.CommandManager.Execute(new ShiftTimingsCommand(timeStep.Negate(), SelectionIntended.Simple));
			}
		}
		else {
			int framesStep = Base.Config.TimingsFramesStep;
			if (firstSelectedSubtitle.Frames.Start >= framesStep) {
				Base.CommandManager.Execute(new ShiftTimingsCommand(-framesStep, SelectionIntended.Simple));
			}
		}
	}
	
	
	/* Private methods */

	private void SetSubtitleStartFromVideo () {
		ExecuteVideoSetSubtitleTimingCommand(typeof(VideoSetSubtitleStartCommand));
	}
	
	private void SetSubtitleEndFromVideo () {
		ExecuteVideoSetSubtitleTimingCommand(typeof(VideoSetSubtitleEndCommand));
	}
	
	private void ExecuteVideoSetSubtitleTimingCommand (Type commandClass) {
		object position = null;
		if (Base.TimingMode == TimingMode.Times) {
			TimeSpan time = Base.Ui.Video.Position.CurrentTime;
			if (Base.Ui.Video.IsStatusPlaying && Base.Config.VideoApplyReactionDelay) {
				time -= TimeSpan.FromMilliseconds(Base.Config.VideoReactionDelay);
			}
			position = time;
		}
		else {
			int frames = Base.Ui.Video.Position.CurrentFrames;
			if (Base.Ui.Video.IsStatusPlaying && Base.Config.VideoApplyReactionDelay) {
				frames -= (int)TimingUtil.TimeMillisecondsToFrames(Base.Config.VideoReactionDelay, Base.Ui.Video.FrameRate);
			}
			position = frames;
		}
		
		Base.CommandManager.Execute((ChangeTimingCommand)Activator.CreateInstance(commandClass, position));
	}
	
	private void VideoSetSubtitleStartEndBegin () {
		SetSubtitleStartFromVideo();
	}
	
	private void VideoSetSubtitleStartEndFinish () {
		SetSubtitleEndFromVideo();
		Base.Ui.View.SelectNextSubtitle();
	}


}

}
