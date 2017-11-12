gnomesubtitlesdir = $(pkglibdir)
gnomesubtitles_SCRIPTS = $(ASSEMBLY)
gnomesubtitles_DATA = $(ASSEMBLY_CONFIG)
GS_BUILDDIR = $(srcdir)/../build
GS_DATADIR = $(srcdir)/../data

ASSEMBLY = $(GS_BUILDDIR)/gnome-subtitles.exe
ASSEMBLY_CONFIG = $(ASSEMBLY).config
ASSEMBLY_CONFIG_SRC = $(srcdir)/GnomeSubtitles/Execution/gnome-subtitles.exe.config

AM_CFLAGS = $(gstreamer_CFLAGS) -Wall -g -fPIC
gnomesubtitles_LTLIBRARIES = libgstreamer_playbin.la
libgstreamer_playbin_la_SOURCES = $(srcdir)/External/GStreamerPlaybin/main.c
libgstreamer_playbin_la_LIBADD = $(gstreamer_LIBS)
libgstreamer_playbin_la_LDFLAGS = -module -avoid-version
libgstreamer_playbin_la_LIBTOOLFLAGS = --tag=disable-static 

GS_SOURCES = \
	$(srcdir)/External/GStreamerPlaybin/*.cs \
	$(srcdir)/External/NCharDet/*.cs \
	$(srcdir)/GnomeSubtitles/Core/*.cs \
	$(srcdir)/GnomeSubtitles/Core/Command/*.cs \
	$(srcdir)/GnomeSubtitles/Dialog/*.cs \
	$(srcdir)/GnomeSubtitles/Dialog/Unmanaged/*.cs \
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
	$(srcdir)/SubLib/IO/SubtitleFormats/*.cs

GS_RESOURCES = \
	$(srcdir)/Glade/AboutDialog.ui \
	$(srcdir)/Glade/EncodingsDialog.ui \
	$(srcdir)/Glade/FileOpenDialog.ui \
	$(srcdir)/Glade/FilePropertiesDialog.ui \
	$(srcdir)/Glade/FileSaveAsDialog.ui \
	$(srcdir)/Glade/HeadersDialog.ui \
	$(srcdir)/Glade/MainWindow.ui \
	$(srcdir)/Glade/PreferencesDialog.ui \
	$(srcdir)/Glade/SearchDialog.ui \
	$(srcdir)/Glade/SetLanguageDialog.ui \
	$(srcdir)/Glade/TimingsAdjustDialog.ui \
	$(srcdir)/Glade/TimingsShiftDialog.ui \
	$(srcdir)/Glade/TimingsSynchronizeDialog.ui \
	$(srcdir)/Glade/VideoOpenDialog.ui \
	$(srcdir)/Glade/VideoSeekToDialog.ui \
	$(GS_DATADIR)/gnome-subtitles.svg \
	$(GS_DATADIR)/gnome-subtitles-logo.png \
	$(GS_DATADIR)/video-set-subtitle-end-16x.png \
	$(GS_DATADIR)/video-set-subtitle-start-16x.png\
	$(GS_DATADIR)/video-set-subtitle-start-end-30x.png

GS_SYSTEM_REFERENCES = \
	Mono.Posix.dll \
	System.Web.dll

$(ASSEMBLY): $(GS_SOURCES) $(GS_RESOURCES)
	$(MCS) $(MCS_FLAGS) -target:winexe -out:$(ASSEMBLY) $(GS_SYSTEM_REFERENCES:%=-r:%) $(GTKSHARP_LIBS) $(SUBLIB_LIBS) $(GS_RESOURCES:%=-resource:%) $(GS_SOURCES)

$(ASSEMBLY_CONFIG): $(gnomesubtitles_LTLIBRARIES)
	cp -f $(ASSEMBLY_CONFIG_SRC) $(GS_BUILDDIR)
	cp -f $(srcdir)/.libs/libgstreamer_playbin.so $(GS_BUILDDIR)

bin_SCRIPTS = $(srcdir)/GnomeSubtitles/Execution/gnome-subtitles

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb
EXTRA_DIST = $(gnomesubtitles_SOURCES) $(GS_SOURCES) $(GS_RESOURCES) $(ASSEMBLY_CONFIG_SRC)

