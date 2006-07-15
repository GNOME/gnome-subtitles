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
using SubLib;

namespace GnomeSubtitles {

public class Subtitles : SubLib.Subtitles {
	private ListStore model = new ListStore(typeof(Subtitle));

	
	public Subtitles (SubLib.Subtitles subtitles) : base(subtitles.Collection, subtitles.Properties) {
		ReLoad();
	}
	
	public ListStore Model {
		get { return model; }
	}
	
	public int Count {
		get { return Collection.Count; }
	}
	
	public Subtitle Get (TreeIter iter) {
		return (Subtitle)model.GetValue(iter, 0);
	}

	public Subtitle Get (TreePath path) {
    		return Get(path.Indices[0]);	
	}
	
	public Subtitle Get (int index) {
		return Collection.Get(index);
	}

	
	public void Add (Subtitle subtitle) {
		Collection.Add(subtitle);
		model.AppendValues(subtitle);
	}
	
	public void Add (Subtitle subtitle, int index) {
		Collection.Add(subtitle, index);
		model.SetValue(model.Insert(index), 0, subtitle);
	}
	
	public void AddAfter (int index) {
		Subtitle existing = Get(index);
		TimeSpan subtitleStart = existing.Times.End + TimeSpan.FromSeconds(SubtitleConstants.MinTimeBetweenSubtitles);
		TimeSpan subtitleEnd = subtitleStart + TimeSpan.FromSeconds(SubtitleConstants.MaxSingleLineSubtitleDuration);
		Subtitle subtitle = new Subtitle(Properties, subtitleStart, subtitleEnd);
		Add(subtitle, index + 1);	
	}
	
	public bool Remove (TreePath path) {
		int index = path.Indices[0];
		if ((index < 0) || (index >= Collection.Count))
			return false;
			
		TreeIter iter;
		model.GetIter(out iter, path);
		
		Collection.Remove(index);
		model.Remove(ref iter);
		return true;
	}

	public bool Remove (int index) {
		if ((index < 0) || (index >= Collection.Count))
			return false;
	
		TreeIter iter;
		model.GetIterFromString(out iter, index.ToString());

		Collection.Remove(index);
		model.Remove(ref iter);
		return true;
	}
	
	public void EmitSubtitleChanged (TreePath path) {
		model.EmitRowChanged(path, TreeIter.Zero);	
	}

	
	public void ReLoad () {
		model.Clear();
		foreach (Subtitle subtitle in Collection) {
			model.AppendValues(subtitle);
		}
	}

}

}