#Option needed because main.c is in a subdir
AUTOMAKE_OPTIONS = subdir-objects

gnomesubtitlesdir = $(pkglibdir)
gnomesubtitles_SCRIPTS = $(ASSEMBLY)
gnomesubtitles_DATA = $(ASSEMBLY_CONFIG)
GS_BUILDDIR = $(srcdir)/../build
GS_DATADIR = $(srcdir)/../data

ASSEMBLY = $(GS_BUILDDIR)/gnome-subtitles.exe
ASSEMBLY_CONFIG = $(ASSEMBLY).config
ASSEMBLY_CONFIG_SRC = $(srcdir)/GnomeSubtitles/Execution/gnome-subtitles.exe.config

AM_CFLAGS = $(gstreamer_CFLAGS) -Wall -g -fPIC
gnomesubtitles_LTLIBRARIES = libgst_backend.la
libgst_backend_la_SOURCES = External/GstBackend/gst-backend.c
libgst_backend_la_LIBADD = $(gstreamer_LIBS)
libgst_backend_la_LDFLAGS = -module -avoid-version
libgst_backend_la_LIBTOOLFLAGS = --tag=disable-static 

GSSOURCES = \
	$(srcdir)/External/Enchant/*.cs \
	$(srcdir)/External/GtkSpell/*.cs \
	$(srcdir)/External/GstBackend/*.cs \
	$(srcdir)/External/Interop/*.cs \
	$(srcdir)/External/NCharDet/*.cs \
	$(srcdir)/GnomeSubtitles/Core/*.cs \
	$(srcdir)/GnomeSubtitles/Core/Command/*.cs \
	$(srcdir)/GnomeSubtitles/Dialog/*.cs \
	$(srcdir)/GnomeSubtitles/Dialog/Message/*.cs \
	$(srcdir)/GnomeSubtitles/Execution/*.cs \
	$(srcdir)/GnomeSubtitles/Ui/*.cs \
	$(srcdir)/GnomeSubtitles/Ui/Component/*.cs \
	$(srcdir)/GnomeSubtitles/Ui/Edit/*.cs \
	$(srcdir)/GnomeSubtitles/Ui/VideoPreview/*.cs \
	$(srcdir)/GnomeSubtitles/Ui/VideoPreview/Exceptions/*.cs \
	$(srcdir)/GnomeSubtitles/Ui/View/*.cs \
	$(srcdir)/SubLib/Core/*.cs \
	$(srcdir)/SubLib/Core/Domain/*.cs \
	$(srcdir)/SubLib/Core/Search/*.cs \
	$(srcdir)/SubLib/Core/Timing/*.cs \
	$(srcdir)/SubLib/Exceptions/*.cs \
	$(srcdir)/SubLib/IO/*.cs \
	$(srcdir)/SubLib/IO/Input/*.cs \
	$(srcdir)/SubLib/IO/Output/*.cs \
	$(srcdir)/SubLib/IO/SubtitleFormats/*.cs \
	$(srcdir)/SubLib/Util/*.cs

GS_RESOURCES = \
	$(srcdir)/Glade/MainWindow.ui \
	$(GS_DATADIR)/gnome-subtitles-logo.png

GS_SYSTEM_REFERENCES = \
	Mono.Posix.dll \
	System.Web.dll

$(ASSEMBLY): $(GSSOURCES) $(GS_RESOURCES)
	$(MCS) $(MCS_FLAGS) -target:winexe -out:$(ASSEMBLY) $(GS_SYSTEM_REFERENCES:%=-r:%) $(GTKSHARP_LIBS) $(SUBLIB_LIBS) $(GS_RESOURCES:%=-resource:%) $(GSSOURCES)

$(ASSEMBLY_CONFIG): $(gnomesubtitles_LTLIBRARIES)
	cp -f $(ASSEMBLY_CONFIG_SRC) $(GS_BUILDDIR)
	cp -f $(srcdir)/.libs/libgst_backend.so $(GS_BUILDDIR)

bin_SCRIPTS = $(srcdir)/GnomeSubtitles/Execution/gnome-subtitles

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb
EXTRA_DIST = $(gnomesubtitles_SOURCES) $(GSSOURCES) $(GS_RESOURCES) $(ASSEMBLY_CONFIG_SRC)

