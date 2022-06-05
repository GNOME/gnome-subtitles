#!/usr/bin/env python3

import os
import shutil
import subprocess
import sys

libdir = sys.argv[1]
install_gtk_sharp = (sys.argv[2] == 'true')
install_gst_sharp = (sys.argv[3] == 'true')
destdir = os.environ.get('DESTDIR')

#Install gsettings schemas (TODO use meson's gnome module instead for installing schemas (since 0.57))
if not destdir:
	print('Compiling gsettings schemas...')
	subprocess.call(['glib-compile-schemas', os.path.join(os.environ['MESON_INSTALL_PREFIX'], 'share', 'glib-2.0', 'schemas')])

#Install gtk-sharp and gst-sharp libs
build_path_subprojects = os.path.join(os.environ['MESON_BUILD_ROOT'], 'subprojects/')
cs_libs = []

if install_gtk_sharp:
	print('Installing gtk-sharp libs')
	cs_libs += [
		os.path.join(os.environ['MESON_SOURCE_ROOT'], 'build-aux/') + 'cairo-sharp.dll.config',
		build_path_subprojects + 'gtk-sharp/Source/atk/atk-sharp.dll',
		build_path_subprojects + 'gtk-sharp/Source/atk/atk-sharp.dll.config',
		build_path_subprojects + 'gtk-sharp/Source/cairo/cairo-sharp.dll',
		build_path_subprojects + 'gtk-sharp/Source/gdk/gdk-sharp.dll',
		build_path_subprojects + 'gtk-sharp/Source/gdk/gdk-sharp.dll.config',
		build_path_subprojects + 'gtk-sharp/Source/gio/gio-sharp.dll',
		build_path_subprojects + 'gtk-sharp/Source/gio/gio-sharp.dll.config',
		build_path_subprojects + 'gtk-sharp/Source/glib/glib-sharp.dll',
		build_path_subprojects + 'gtk-sharp/Source/glib/glib-sharp.dll.config',
		build_path_subprojects + 'gtk-sharp/Source/gtk/gtk-sharp.dll',
		build_path_subprojects + 'gtk-sharp/Source/gtk/gtk-sharp.dll.config',
		build_path_subprojects + 'gtk-sharp/Source/pango/pango-sharp.dll',
		build_path_subprojects + 'gtk-sharp/Source/pango/pango-sharp.dll.config',
	]

if install_gst_sharp:
	print('Installing gstreamer-sharp libs')
	cs_libs += [
		build_path_subprojects + 'gstreamer-sharp/sources/gstreamer-sharp.dll',
		build_path_subprojects + 'gstreamer-sharp/sources/gstreamer-sharp.dll.config'
	]

dst = os.path.join((destdir if destdir else '') + os.environ['MESON_INSTALL_PREFIX'], libdir, 'gnome-subtitles')
for lib in cs_libs:
	shutil.copy2(lib, dst)
