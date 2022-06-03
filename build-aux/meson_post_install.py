#!/usr/bin/env python3

import os
import shutil
import subprocess
import sys

libdir = sys.argv[1]

#Install gsettings schemas (TODO use meson's gnome module instead for installing schemas (since 0.57))
if not os.environ.get('DESTDIR'):
	print('Compiling gsettings schemas...')
	subprocess.call(['glib-compile-schemas', os.path.join(os.environ['MESON_INSTALL_PREFIX'], 'share', 'glib-2.0', 'schemas')])

#Copy gtk# and gst# .dll and .config files to gnome subtitles' lib dir
build_path_subprojects = os.path.join(os.environ['MESON_BUILD_ROOT'], 'subprojects/')
cs_libs = [
	os.path.join(os.environ['MESON_SOURCE_ROOT'], 'build-aux/') + 'cairo-sharp.dll.config',
	build_path_subprojects + 'gtk-sharp/Source/cairo/cairo-sharp.dll',
	build_path_subprojects + 'gtk-sharp/Source/gio/gio-sharp.dll',
	build_path_subprojects + 'gtk-sharp/Source/gio/gio-sharp.dll.config',
	build_path_subprojects + 'gtk-sharp/Source/glib/glib-sharp.dll',
	build_path_subprojects + 'gtk-sharp/Source/glib/glib-sharp.dll.config',
	build_path_subprojects + 'gtk-sharp/Source/gtk/gtk-sharp.dll',
	build_path_subprojects + 'gtk-sharp/Source/gtk/gtk-sharp.dll.config',
	build_path_subprojects + 'gstreamer-sharp/sources/gstreamer-sharp.dll',
	build_path_subprojects + 'gstreamer-sharp/sources/gstreamer-sharp.dll.config'
]

dst = os.path.join(os.environ.get('DESTDIR') + os.environ['MESON_INSTALL_PREFIX'], libdir, 'gnome-subtitles')
print('Copying gtk# and gst# .dll and .config files to ' + dst)
for lib in cs_libs:
	shutil.copy2(lib, dst)
