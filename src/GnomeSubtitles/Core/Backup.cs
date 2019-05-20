/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2010-2019 Pedro Castro
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

using System;
using System.Threading;
using SubLib.Core;
using SubLib.Core.Domain;
using SubLib.Util;

namespace GnomeSubtitles.Core {

public class Backup {
	private uint backupTimeoutId = 0; //Active timeout ID, used to remove timeout if needed

	public Backup () {
		Base.InitFinished += OnBaseInitFinished;
	}


	/* Public methods */

	/* ReCheck forces restart if we're enabling backup and it's already running */
	public void ReCheck () {
		if (IsBackupRunning()) {
			Disable();
		}

		if (Base.Config.BackupAuto) {
			Enable();
		}
	}


	/* Private members */

	private bool IsBackupRunning () {
		return backupTimeoutId != 0;
	}

	private void Enable() {
		if (backupTimeoutId == 0)
			backupTimeoutId = GLib.Timeout.Add((uint)Base.Config.BackupTime * 1000, DoBackup); //milliseconds
	}

	private void Disable() {
		if (backupTimeoutId != 0) {
			GLib.Source.Remove(backupTimeoutId);
			backupTimeoutId = 0;
		}
	}

	private bool DoBackup() {
		if (!Base.IsDocumentLoaded) {
			return true;
		}

		BackupThreadArgs backupThreadArgs = new BackupThreadArgs();
		if (Base.Document.CanTextBeSaved) {
			backupThreadArgs.TextFilePropertiesClone = Base.Document.TextFile.Clone() as SubLib.Core.Domain.FileProperties;
		}

		if (Base.Document.CanTranslationBeSaved) {
			backupThreadArgs.TranslationFilePropertiesClone = Base.Document.TranslationFile.Clone() as SubLib.Core.Domain.FileProperties;
		}

		if ((backupThreadArgs.TextFilePropertiesClone != null) || (backupThreadArgs.TranslationFilePropertiesClone != null)) {
			backupThreadArgs.SubtitlesClone = Base.Document.Subtitles.Clone() as SubLib.Core.Domain.Subtitles;
			Thread backupThread = new Thread(SaveSubtitleFiles);
			backupThread.Start(backupThreadArgs);
		}

		return true;
	}

	private void SaveSubtitleFiles (object backupThreadArgs) {
		try {
			BackupThreadArgs args = backupThreadArgs as BackupThreadArgs;

			/* Save subtitle file */
			if ((args.TextFilePropertiesClone != null) && (args.TextFilePropertiesClone.SubtitleType != SubtitleType.Unknown) && (args.TextFilePropertiesClone.IsPathRooted)) {
				args.TextFilePropertiesClone.Path += "~";
				SaveFile(args.SubtitlesClone, args.TextFilePropertiesClone, SubtitleTextType.Text);
			}

			/* Save translation file */
			if ((args.TranslationFilePropertiesClone != null) && (args.TranslationFilePropertiesClone.SubtitleType != SubtitleType.Unknown) && (args.TranslationFilePropertiesClone.IsPathRooted)) {
				args.TranslationFilePropertiesClone.Path += "~";
				SaveFile(args.SubtitlesClone, args.TranslationFilePropertiesClone, SubtitleTextType.Translation);
			}
		}
		catch (Exception e) {
			Logger.Error(e, "Caught an exception while creating backup files");
		}
	}

	private bool SaveFile (SubLib.Core.Domain.Subtitles subtitles, FileProperties properties, SubtitleTextType textType) {
		try {
			SubtitleSaver saver = new SubtitleSaver();
			saver.Save(subtitles, properties, textType);
			return true;
		}
		catch (Exception e) {
			Logger.Error(e, "Caught an exception while saving backup file");
			return false;
		}
	}

	/* Event members */

	private void OnBaseInitFinished () {
		if (Base.Config.BackupAuto) {
			Enable();
		}
    }


    /* Inner classes */

    private class BackupThreadArgs {
    	private SubLib.Core.Domain.Subtitles subtitlesClone;
    	private SubLib.Core.Domain.FileProperties textFilePropertiesClone;
    	private SubLib.Core.Domain.FileProperties translationFilePropertiesClone;

		/* Properties */

    	public SubLib.Core.Domain.Subtitles SubtitlesClone {
    		get { return subtitlesClone; }
    		set { this.subtitlesClone = value; }
    	}

    	public SubLib.Core.Domain.FileProperties TextFilePropertiesClone {
    		get { return textFilePropertiesClone; }
    		set { this.textFilePropertiesClone = value; }
    	}

    	public SubLib.Core.Domain.FileProperties TranslationFilePropertiesClone {
    		get { return translationFilePropertiesClone; }
    		set { this.translationFilePropertiesClone = value; }
    	}
    }

}
}
