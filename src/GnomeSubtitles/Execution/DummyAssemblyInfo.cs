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

using System.Reflection;

/* Dummy class for building without Meson (ex: in Monodevelop), as Meson doesn't allow
 * creating source files. AssemblyInfo.cs.in used instead when building with Meson.
 */
[assembly: AssemblyVersion("0.0.0")]
[assembly: AssemblyTitle ("Gnome Subtitles")]
[assembly: AssemblyDescription ("Video subtitling for the GNOME desktop")]
[assembly: AssemblyCopyright ("Copyright 0000-0000 Pedro Castro")]
