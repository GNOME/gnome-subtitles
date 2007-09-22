AM_CFLAGS = $(gstreamer_CFLAGS) -Wall -g

noinst_PROGRAMS = libgstreamer_playbin.so
libgstreamer_playbin_so_SOURCES = main.c
libgstreamer_playbin_so_LDFLAGS = $(gstreamer_LIBS) -lgstinterfaces-0.10 -shared -Wl,-soname,libgstreamer_playbin.so

noinst_SCRIPTS = GStreamerPlaybin.dll

FILES = Engine.cs AssemblyInfo.cs

GStreamerPlaybin.dll: 
	$(MCS) -target:library -out:$@ $(FILES)

EXTRA_DIST = $(FILES) main.c ../README
CLEANFILES = GStreamerPlaybin.dll

