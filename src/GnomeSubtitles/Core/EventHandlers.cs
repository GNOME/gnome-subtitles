/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2017 Pedro Castro
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
using GnomeSubtitles.Dialog.Unmanaged;
using GnomeSubtitles.Ui;
using GnomeSubtitles.Ui.View;
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using SubLib.Core.Timing;
using System;
using System.Text;

namespace GnomeSubtitles.Core {

public class EventHandlers {
	private bool buttonStartEndKeyPressed = false; //Used to match grab focus and key release events


	/* File Menu */

	public void OnFileNew (object o, EventArgs args) {
		Base.Ui.New();
	}

	public void OnFileOpen (object o, EventArgs args) {
		Base.Ui.Open();
	}

	public void OnFileSave (object o, EventArgs args) {
		Base.Ui.Save();

		if (Base.Document.IsTranslationLoaded && Base.Config.FileTranslationSaveAll) {
			OnFileTranslationSave(o, args);
		}
	}

	public void OnFileSaveAs (object o, EventArgs args) {
		Base.Ui.SaveAs();

		if (Base.Document.IsTranslationLoaded && Base.Config.FileTranslationSaveAll) {
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
		Base.Ui.Quit();
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
		if (Base.TimingMode == TimingMode.Times) {
			TimeSpan time = Base.Ui.Video.Position.CurrentTime;
			if (Base.Ui.Video.IsStatePlaying && Base.Config.VideoApplyReactionDelay) {
				time -= TimeSpan.FromMilliseconds(Base.Config.VideoReactionDelay);
			}
			Base.CommandManager.Execute(new VideoSetSubtitleStartCommand(time));
		}
		else {
			int frames = Base.Ui.Video.Position.CurrentFrames;
			if (Base.Ui.Video.IsStatePlaying && Base.Config.VideoApplyReactionDelay) {
				frames -= (int)TimingUtil.TimeMillisecondsToFrames(Base.Config.VideoReactionDelay, Base.Ui.Video.FrameRate);
			}
			Base.CommandManager.Execute(new VideoSetSubtitleStartCommand(frames));
		}
	}

	public void OnVideoSetSubtitleEnd (object o, EventArgs args) {
		if (Base.TimingMode == TimingMode.Times) {
			TimeSpan time = Base.Ui.Video.Position.CurrentTime;
			if (Base.Ui.Video.IsStatePlaying && Base.Config.VideoApplyReactionDelay) {
				time -= TimeSpan.FromMilliseconds(Base.Config.VideoReactionDelay);
			}
			Base.CommandManager.Execute(new VideoSetSubtitleEndCommand(time));
		}
		else {
			int frames = Base.Ui.Video.Position.CurrentFrames;
			if (Base.Ui.Video.IsStatePlaying && Base.Config.VideoApplyReactionDelay) {
				frames -= (int)TimingUtil.TimeMillisecondsToFrames(Base.Config.VideoReactionDelay, Base.Ui.Video.FrameRate);
			}
			Base.CommandManager.Execute(new VideoSetSubtitleEndCommand(frames));
		}
	}

	public void OnVideoSetSubtitleStartEndButtonPress (object o, ButtonPressEventArgs args) {
		OnVideoSetSubtitleStart(o, args);
	}

	public void OnVideoSetSubtitleStartEndButtonRelease (object o, ButtonReleaseEventArgs args) {
		if (Base.TimingMode == TimingMode.Times) {
			TimeSpan time = Base.Ui.Video.Position.CurrentTime;
			if (Base.Ui.Video.IsStatePlaying && Base.Config.VideoApplyReactionDelay) {
				time -= TimeSpan.FromMilliseconds(Base.Config.VideoReactionDelay);
			}
			Base.CommandManager.Execute(new VideoSetSubtitleEndCommand(time));
			Base.Ui.View.SelectNextSubtitle();
		}
		else {
			int frames = Base.Ui.Video.Position.CurrentFrames;
			if (Base.Ui.Video.IsStatePlaying && Base.Config.VideoApplyReactionDelay) {
				frames -= (int)TimingUtil.TimeMillisecondsToFrames(Base.Config.VideoReactionDelay, Base.Ui.Video.FrameRate);
			}
			Base.CommandManager.Execute(new VideoSetSubtitleEndCommand(frames));
			Base.Ui.View.SelectNextSubtitle();
		}
	}

	public void OnVideoSetSubtitleStartEndGrabFocus (object o, EventArgs args) {
		if (!buttonStartEndKeyPressed) {
			OnVideoSetSubtitleStart(o, args);
			buttonStartEndKeyPressed = true;
		}
	}

	public void OnVideoSetSubtitleStartEndKeyRelease (object o, KeyReleaseEventArgs args) {
		if (buttonStartEndKeyPressed){
			OnVideoSetSubtitleStartEndButtonRelease(o, null);
			buttonStartEndKeyPressed = false;
		}
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

	public void OnHelpContents (object o, EventArgs args) {
		Util.OpenUrl("ghelp:gnome-subtitles");
	}

	public void OnHelpKeyboardShortcuts (object o, EventArgs args) {
		Util.OpenUrl("http://www.gnomesubtitles.org/shortcuts");
	}

	public void OnHelpRequestFeature (object o, EventArgs args) {
		Util.OpenBugReport();
	}

	public void OnHelpReportBug (object o, EventArgs args) {
		Util.OpenBugReport();
	}

	public void OnHelpAbout (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(Dialog.AboutDialog)).Show();
	}


	/*	Window	*/

	public void OnWindowDelete (object o, DeleteEventArgs args) {
    	Base.Ui.Quit();
    	args.RetVal = true;
    }

    public void OnSizeAllocated (object o, SizeAllocatedArgs args) {
    	Base.Config.ViewWindowWidth = args.Allocation.Width;
    	Base.Config.ViewWindowHeight = args.Allocation.Height;
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

}

}
