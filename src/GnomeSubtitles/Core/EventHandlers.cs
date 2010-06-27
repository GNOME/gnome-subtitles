/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2010 Pedro Castro
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
using Gtk;
using Mono.Unix;
using SubLib.Core.Domain;
using System;
using System.Text;

namespace GnomeSubtitles.Core {

public class EventHandlers {
	private bool buttonStartEndKeyPressed = false;
	
	/* File Menu */
	
	public void OnFileNew (object o, EventArgs args) {
		Base.Ui.New(String.Empty);
	}
	
	public void OnFileOpen (object o, EventArgs args) {
		Base.Ui.Open();
	}
	
	public void OnFileSave (object o, EventArgs args) {
		Base.Ui.Save();

		if (Base.Document.IsTranslationLoaded && Base.Config.PrefsTranslationSaveAll) {
			OnFileTranslationSave(o, args);
		}
	}
	
	public void OnFileSaveAs (object o, EventArgs args) {
		Base.Ui.SaveAs();
		
		if (Base.Document.IsTranslationLoaded && Base.Config.PrefsTranslationSaveAll) {
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
	
	public void OnFileTranslationSaveAs (object o, EventArgs args) {
		Base.Ui.TranslationSaveAs();
	}
	
	public void OnFileTranslationClose (object o, EventArgs args) {
		Base.Ui.TranslationClose();
	}
	
	public void OnFileHeaders (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(HeadersDialog)).Show();
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
	
	public void OnEditInsertSubtitleBefore (object o, EventArgs args) {
		Base.CommandManager.Execute(new InsertSubtitleBeforeCommand());
	}
	
	public void OnEditInsertSubtitleAfter (object o, EventArgs args) {
		if (Base.Document.Subtitles.Count == 0)
			Base.CommandManager.Execute(new InsertFirstSubtitleCommand());
		else
			Base.CommandManager.Execute(new InsertSubtitleAfterCommand());
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
	
	public void OnTimingsInputFrameRate (object o, EventArgs args) {
		RadioMenuItem menuItem = o as RadioMenuItem;
		if (menuItem.Active) {
			float frameRate = Menus.FrameRateFromMenuItem((menuItem.Child as Label).Text);
			Base.CommandManager.Execute(new ChangeInputFrameRateCommand(frameRate));
		}
	}
	
	public void OnTimingsVideoFrameRate (object o, EventArgs args) {
		RadioMenuItem menuItem = o as RadioMenuItem;
		if (menuItem.Active) {
			float frameRate = Menus.FrameRateFromMenuItem((menuItem.Child as Label).Text);
			Base.CommandManager.Execute(new ChangeVideoFrameRateCommand(frameRate));
		}
	}
	
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
		Base.Ui.View.SetAutoSelectSubtitles();
 	}
		
	public void OnVideoSetSubtitleStart (object o, EventArgs args) {
		if (Base.TimingMode == TimingMode.Times) {
			TimeSpan time = Base.Ui.Video.Position.CurrentTime;
			Base.CommandManager.Execute(new VideoSetSubtitleStartCommand(time));
		}
		else {
			int frames = Base.Ui.Video.Position.CurrentFrames;
			Base.CommandManager.Execute(new VideoSetSubtitleStartCommand(frames));
		}			
	}
		
	public void OnVideoSetSubtitleEnd (object o, EventArgs args) {
		if (Base.TimingMode == TimingMode.Times) {
			TimeSpan time = Base.Ui.Video.Position.CurrentTime;
			Base.CommandManager.Execute(new VideoSetSubtitleEndCommand(time));
		}
		else {
			int frames = Base.Ui.Video.Position.CurrentFrames;
			Base.CommandManager.Execute(new VideoSetSubtitleEndCommand(frames));
		}
	}

	public void OnVideoSetSubtitleStartEnd (object o, EventArgs args) {
		if (Base.TimingMode == TimingMode.Times) {
			TimeSpan time = Base.Ui.Video.Position.CurrentTime;
			Base.CommandManager.Execute(new VideoSetSubtitleEndCommand(time));
			Base.Ui.View.SelectNextSubtitle();
		}
		else {
			int frames = Base.Ui.Video.Position.CurrentFrames;
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
			OnVideoSetSubtitleStartEnd(o, args);
			buttonStartEndKeyPressed = false;
		}
	}

	/* Tools Menu */
		
	public void OnToolsAutocheckSpelling (object o, EventArgs args) {
		bool enabled = (o as CheckMenuItem).Active;
		Base.SpellLanguages.Enabled = enabled;
	}
	
	public void OnToolsSetTextLanguage (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(SetTextLanguageDialog)).Show();
	}
	
	public void OnToolsSetTranslationLanguage (object o, EventArgs args) {
		Base.Dialogs.Get(typeof(SetTranslationLanguageDialog)).Show();
	}
	
	
	/*	Help Menu */
	
	public void OnHelpContents (object o, EventArgs args) {
		const string arg = "ghelp:gnome-subtitles";
		if ((!Util.OpenUrl("gnome-help " + arg)) && (!Util.OpenUrl("yelp " + arg))) {
			BasicErrorDialog errorDialog = new BasicErrorDialog(Catalog.GetString("The Gnome Subtitles Manual could not be found."), Catalog.GetString("Please verify that your installation has been completed successfully."));
			errorDialog.Show();
		}
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
    	Base.Config.PrefsWindowWidth = args.Allocation.Width;
    	Base.Config.PrefsWindowHeight = args.Allocation.Height;
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

}

}
