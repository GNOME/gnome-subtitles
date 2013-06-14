/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2011 Pedro Castro
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

using Gdk;
using Gtk;
using System;

namespace GnomeSubtitles.Core {

public class GlobalAccelerators {
	private AccelGroup accelGroup = null;
	private Menu hiddenMenu = null; //Needed to assign actions, not visible

	public GlobalAccelerators () {
		Base.InitFinished += OnBaseInitFinished;
	}

	/* Private members */

	private void Init () {
		this.accelGroup = new AccelGroup();
		this.hiddenMenu = new Menu();

		Base.Ui.Window.AddAccelGroup(this.accelGroup);

		AddAccelerator((uint)Gdk.Key.KP_Add, Gdk.ModifierType.ControlMask, AccelFlags.Visible, Base.Handlers.OnGlobalSubtitleStartIncrease);
		AddAccelerator((uint)Gdk.Key.KP_Subtract, Gdk.ModifierType.ControlMask, AccelFlags.Visible, Base.Handlers.OnGlobalSubtitleStartDecrease);

		AddAccelerator((uint)Gdk.Key.KP_Add, Gdk.ModifierType.Mod1Mask, AccelFlags.Visible, Base.Handlers.OnGlobalSubtitleEndIncrease);
		AddAccelerator((uint)Gdk.Key.KP_Subtract, Gdk.ModifierType.Mod1Mask, AccelFlags.Visible, Base.Handlers.OnGlobalSubtitleEndDecrease);

		AddAccelerator((uint)Gdk.Key.KP_Add, Gdk.ModifierType.ShiftMask, AccelFlags.Visible, Base.Handlers.OnGlobalSelectionShiftIncrease);
		AddAccelerator((uint)Gdk.Key.KP_Subtract, Gdk.ModifierType.ShiftMask, AccelFlags.Visible, Base.Handlers.OnGlobalSelectionShiftDecrease);
	}

	private void AddAccelerator (uint key, ModifierType modifiers, AccelFlags accelFlags, EventHandler handler) {
		MenuItem menuItem = new MenuItem();
		menuItem.AddAccelerator("activate", this.accelGroup, key, modifiers, accelFlags);
		menuItem.Activated += handler;
		menuItem.Show();

		this.hiddenMenu.Append(menuItem);
	}

	/* Event members */

	private void OnBaseInitFinished () {
		Init();
    }

}

}