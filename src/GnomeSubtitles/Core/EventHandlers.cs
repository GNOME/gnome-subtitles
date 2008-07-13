/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2006-2008 Pedro Castro
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
using Gnome;
using Gtk;
using Mono.Unix;
using SubLib;
using System;

namespace GnomeSubtitles.Core {

public class EventHandlers {

	public EventHandlers () {
		ConnectSignals();
    }


	/* File Menu */
	
	public void OnFileNew (object o, EventArgs args) {
		Base.Ui.New(String.Empty);
	}
	
	public void OnFileOpen (object o, EventArgs args) {
		Base.Ui.Open();
	}
	
	public void OnFileSave (object o, EventArgs args) {
		Base.Ui.Save();
	}
	
	public void OnFileSaveAs (object o, EventArgs args) {
		Base.Ui.SaveAs();
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
		new HeadersDialog().Show();
	}
	
	public void OnFileProperties (object o, EventArgs args) {
		new FilePropertiesDialog().Show();
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
		new PreferencesDialog().Show();
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
			Base.Ui.Video.Subtitle.ToShowText = true;
	}
	
	public void OnViewVideoSubtitlesTranslation (object o, EventArgs args) {
		if ((o as RadioMenuItem).Active)
			Base.Ui.Video.Subtitle.ToShowText = false;
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
		new TimingsAdjustDialog().Show();	
	}
	
	public void OnTimingsShift (object o, EventArgs args) {
		Base.Dialogs.TimingsShiftDialog.Show();
	}
	
	
	/* Video Menu */
		
	public void OnVideoOpen (object o, EventArgs args) {
		Base.Ui.OpenVideo();
	}
	
	public void OnVideoClose (object o, EventArgs args) {
		Base.Ui.CloseVideo();
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
	
	public void OnVideoSeekToSelection (object o, EventArgs args) {
		Base.Ui.Video.SeekToSelection();	
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
	
	/* Tools Menu */
		
	public void OnToolsAutocheckSpelling (object o, EventArgs args) {
		bool enabled = (o as CheckMenuItem).Active;
		Base.SpellLanguages.Enabled = enabled;
	}
	
	public void OnToolsSetTextLanguage (object o, EventArgs args) {
		new SetLanguageDialog(SubtitleTextType.Text).Show();
	}
	
	public void OnToolsSetTranslationLanguage (object o, EventArgs args) {
		new SetLanguageDialog(SubtitleTextType.Translation).Show();
	}
	
	
	/*	Help Menu */
	
	public void OnHelpContents (object o, EventArgs args) {
		try {
			const string filename = "gnome-subtitles.xml";
			Gnome.Help.DisplayDesktopOnScreen(Gnome.Program.Get(), Base.ExecutionContext.SystemHelpDir, filename, null, Base.Ui.Window.Screen);
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
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
		new Dialog.AboutDialog().Show();
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

    
    /*	Subtitle View	*/
    
    public void OnRowActivated (object o, RowActivatedArgs args) {
    	Base.Ui.Video.SeekToSelection();
    }
    
    public void OnSubtitleViewKeyPressed (object o, KeyPressEventArgs args) {
    	Gdk.Key key = args.Event.Key;
    	if (key == Gdk.Key.Delete)
    		OnEditDeleteSubtitles(o, EventArgs.Empty);
		else if (key == Gdk.Key.Insert)
			OnEditInsertSubtitleAfter(o, EventArgs.Empty);
    }
    
    
    /*	Command Manager */

    private void OnUndoToggled (object o, EventArgs args) {
    	Base.Ui.Menus.UpdateFromUndoToggled();
    }
    
    private void OnRedoToggled (object o, EventArgs args) {
    	Base.Ui.Menus.UpdateFromRedoToggled();
    }
    
    private void OnCommandActivated (object o, CommandActivatedArgs args) {
    	Base.Ui.Menus.UpdateFromCommandActivated();
    	Base.Document.UpdateFromCommandActivated(args.Target);
    }
    
    /* Private members */
    
    private void ConnectSignals () {
    	Base.CommandManager.UndoToggled += OnUndoToggled;
    	Base.CommandManager.RedoToggled += OnRedoToggled;
    	Base.CommandManager.CommandActivated += OnCommandActivated;
    }

}

}
