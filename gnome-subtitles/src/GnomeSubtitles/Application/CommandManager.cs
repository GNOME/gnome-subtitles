/*
 * This file is part of Gnome Subtitles, a subtitle editor for Gnome.
 * Copyright (C) 2006 Pedro Castro
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

using Gtk;
using System;
using System.Collections;
using SubLib;

namespace GnomeSubtitles {

public class CommandManager {
	private int limit = 25;
	private Command[] commands = null;
	private bool wasModified = false;

	
	private int undoCount = 0;
	private int redoCount = 0;
	private int iterator = 0;
	
	public event EventHandler UndoToggled;
	public event EventHandler RedoToggled;
	public event EventHandler CommandActivated;
	public event EventHandler Modified;

	public CommandManager (int undoLimit, EventHandler onUndoToggled, EventHandler onRedoToggled,
			EventHandler onCommandActivated, EventHandler onModified) {
		limit = undoLimit;
		commands = new Command[undoLimit];
		UndoToggled += onUndoToggled;
		RedoToggled += onRedoToggled;
		CommandActivated += onCommandActivated;
		Modified += onModified;
	}
	
	public bool CanUndo {
		get { return undoCount > 0; }
	}
	
	public bool CanRedo {
		get { return redoCount > 0; }
	}
	
	public string UndoDescription {
		get {
			if (CanUndo)
				return "Undo " + PreviousCommand().Description;
			else
				return String.Empty;
		}
	}
	
	public string RedoDescription {
		get {
			if (CanRedo)
				return "Redo " + NextCommand().Description;
			else
				return String.Empty;
		}
	}
	
	public bool WasModified {
		get { return wasModified; }
		set { wasModified = value; }	
	}
	
	public void Execute (Command command) {
		command.Execute();
		ProcessExecute(command);
		SetModified();
	}
	
	public void Undo () {
		if (CanUndo) {
			PreviousCommand().Undo();
			ProcessUndo();	
			SetModified();
		}
	}
	
	public void Redo () {
		if (CanRedo) {
			NextCommand().Redo();
			ProcessRedo();
			SetModified();
		}
	}


	private void ProcessExecute (Command command) {
		bool couldUndoBefore = CanUndo;
		bool couldRedoBefore = CanRedo;
		
		ClearRedo();
		
		bool canGroup = false;
		if (CanUndo && command.CanGroup) {
			Command lastCommand = PreviousCommand();
			if ((lastCommand.GetType() == command.GetType()) && (lastCommand.CanGroupWith(command)))
				canGroup = true;
		}
		
		if (!canGroup) {
			commands[iterator] = command;
			Next();
			undoCount = IncrementCount(undoCount);
			EmitCommandActivated();
		}

		if (!couldUndoBefore)
			EmitUndoToggled();
			
		if (couldRedoBefore)
			EmitRedoToggled();

	}
	
	private void ProcessUndo () {
		bool couldRedoBefore = CanRedo;
	
		Previous();
		undoCount = DecrementCount(undoCount);
		redoCount = IncrementCount(redoCount);
		
		if (!CanUndo)
			EmitUndoToggled();
		
		if (!couldRedoBefore)
			EmitRedoToggled();
			
		EmitCommandActivated();
	}
	
	private void ProcessRedo () {
		bool couldUndoBefore = CanUndo;
	
		Next();
		undoCount = IncrementCount(undoCount);
		redoCount = DecrementCount(redoCount);	
		
		if (!CanRedo)
			EmitRedoToggled();
			
		if (!couldUndoBefore)
			EmitUndoToggled();
		
		EmitCommandActivated();
	}
	
	private void EmitUndoToggled () {
		if (UndoToggled != null)
			UndoToggled(this, EventArgs.Empty);
	}
	
	private void EmitRedoToggled () {
		if (RedoToggled != null)
			RedoToggled(this, EventArgs.Empty);
	}
	
	private void EmitCommandActivated () {
		CommandActivated(this, EventArgs.Empty);
	}
	
	private void SetModified () {
		if (!wasModified) {
			wasModified = true;
			Modified(this, EventArgs.Empty);		
		}
	}
	
	private Command NextCommand () {
		return commands[iterator];
	}
	
	private Command PreviousCommand () {
		if (iterator == 0)
			return commands[limit - 1];
		else
			return commands[iterator - 1];
	}
	
	private void Next () {
		if (iterator == (limit - 1))
			iterator = 0;
		else
			iterator++;
	}
	
	private void Previous () {
		if (iterator == 0)
			iterator = (limit - 1);
		else
			iterator--;	
	}
	
	private int IncrementCount (int count) {
		if (count < limit)
			return count + 1;
		else
			return count;
	}
	
	private int DecrementCount (int count) {
		return count - 1;
	}
	
	private void ClearRedo () {
		redoCount = 0;
	}

}

public abstract class Command {
	private GUI gui;
	private string description;
	private bool canGroup;

	public Command (GUI gui, string description, bool canGroup) {
		this.gui = gui;
		this.description = description;
		this.canGroup = canGroup;
	}
	
	public string Description {
		get { return description; }
	}
	
	public bool CanGroup {
		get { return canGroup; }
	}

	protected GUI GUI {
		get { return gui; }
	}

	public abstract void Execute ();

	public virtual void Undo () {
		Execute();
	}
	
	public virtual void Redo () {
		Undo();
	}
	
	public virtual bool CanGroupWith (Command command) {
		return false;
	}
}

public abstract class MultipleSelectionCommand : Command {
	private TreePath[] paths = null;
	
	public MultipleSelectionCommand (GUI gui, string description, bool canGroup) : base(gui, description, canGroup) {
		this.paths = gui.SubtitleView.SelectedPaths;
	}
	
	protected TreePath[] Paths {
		get { return paths; }
	}
	
	public override void Execute () {
		ChangeValues();
		GUI.RefreshAndReselect();
		AfterSelected();
	}
	
	public override void Undo () {
		ChangeValues();
		SelectPaths();
		ScrollToSelection();
		AfterSelected();
	}
	
	/* Methods to be extended */
	
	protected virtual void AfterSelected () {
		return;
	}
	
	protected virtual void ChangeValues () {
		return;
	}
	
	/* Private methods */
	
	protected void SelectPaths () {
		if (paths.Length == 0)
			return;
			
		SubtitleView subtitleView = GUI.SubtitleView;
		subtitleView.DisconnectSelectionChangedSignals();

		subtitleView.UnselectAll();
		TreeSelection selection = subtitleView.Widget.Selection;
		foreach (TreePath path in paths)
			selection.SelectPath(path);

		subtitleView.ConnectSelectionChangedSignals();
		subtitleView.Reselect();
	 }
	 
	protected void ScrollToSelection () {
		if (paths.Length == 0)
			return;

		TreePath startPath, endPath;
		GUI.SubtitleView.Widget.GetVisibleRange(out startPath, out endPath);
		int startIndice = startPath.Indices[0];
		int endIndice = endPath.Indices[0];
		
		//Check if there is a subtitle currently visible
		foreach (TreePath path in paths) {
			int pathIndice = path.Indices[0];
			if ((pathIndice >= startIndice) && (pathIndice <= endIndice))
				return;
		}
		GUI.SubtitleView.ScrollToPath(paths[0], true);
	}

}

public abstract class SingleSelectionCommand : Command {
	private TreePath path = null;
	
	public SingleSelectionCommand (GUI gui, string description, bool canGroup) : base(gui, description, canGroup) {
		this.path = gui.SubtitleView.SelectedPath;
	}
	
	protected TreePath Path {
		get { return path; }
		set { path = value; }
	}
	
	protected int PathIndex {
		get { return path.Indices[0]; }
	}
	
	public override void Execute () {
		ChangeValues();
		GUI.SubtitleView.Refresh();
		AfterSelected();
	}
	
	public override void Undo () {
		ChangeValues();
		
		if (PathMatchesCurrentSelection)
			GUI.SubtitleView.Reselect();
		else
			SelectPath();
			
		ScrollToSelection();
		AfterSelected();
	}
	
	/* Methods to be extended */
	
	protected virtual void ChangeValues () {
		return;
	}
	
	protected virtual void AfterSelected () {
		return;
	}
	
	/* Protected methods */
	
	protected void ScrollToSelection () {
		GUI.SubtitleView.ScrollToPath(path);
	}
	
	protected void SelectPath () {
		GUI.SubtitleView.UnselectAll();
		GUI.SubtitleView.Widget.Selection.SelectPath(Path);
	}
	

	/* Private methods */
	
	private bool PathMatchesCurrentSelection {
		get {
			TreeSelection selection = GUI.SubtitleView.Widget.Selection;
			return (selection.CountSelectedRows() == 1)
				&& (selection.PathIsSelected(path));
		}
	}

}

}