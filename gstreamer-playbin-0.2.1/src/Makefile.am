## Process this file with automake to produce Makefile.in


AM_CFLAGS = $(gstreamer_CFLAGS) -Wall -g

lib_LTLIBRARIES = libgstreamer_playbin.la
libgstreamer_playbin_la_SOURCES = main.c
libgstreamer_playbin_la_LDFLAGS = $(gstreamer_LIBS) -lgstinterfaces-0.10


pkglib_SCRIPTS = GStreamerPlaybin.dll
linuxpkgconfigdir = @prefix@/lib/pkgconfig
linuxpkgconfig_DATA = gstreamerplaybinwrapper.pc



noinst_SCRIPTS = GStreamerPlaybin.dll



FILES = Engine.cs AssemblyInfo.cs


GStreamerPlaybin.dll: 
	$(MCS) -target:library -out:$@ $(FILES)



EXTRA_DIST = $(FILES) main.c gstreamerplaybinwrapper.pc.in ../README
CLEANFILES = GStreamerPlaybin.dll
