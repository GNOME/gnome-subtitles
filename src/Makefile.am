gnomesubtitlesdir = $(pkglibdir)
gnomesubtitles_DATA = $(ASSEMBLY)
GS_BUILDDIR = $(srcdir)/../build
GS_DATADIR = $(srcdir)/../data

ASSEMBLY = $(GS_BUILDDIR)/gnome-subtitles.exe
SUBLIB = $(GS_BUILDDIR)/sublib.dll

GS_SOURCES = \
	$(srcdir)/External/Freedesktop.RecentFiles/*.cs \
	$(srcdir)/GnomeSubtitles/Commands/*.cs \
	$(srcdir)/GnomeSubtitles/Core/*.cs \
	$(srcdir)/GnomeSubtitles/Dialogs/*.cs \
	$(srcdir)/GnomeSubtitles/Execution/*.cs

GS_RESOURCES = \
	$(srcdir)/Glade/gnome-subtitles.glade \
	$(GS_DATADIR)/gnome-subtitles.png \
	$(GS_DATADIR)/gnome-subtitles-logo.png

GS_SYSTEM_REFERENCES = \
	$(SUBLIB) \
	Mono.Posix.dll

$(ASSEMBLY): $(GS_SOURCES) $(GS_RESOURCES)
	$(MCS) $(MCS_FLAGS) -target:winexe -out:$(ASSEMBLY) $(GS_SYSTEM_REFERENCES:%=-r:%) $(GTKSHARP_LIBS) $(GS_RESOURCES:%=-resource:%) $(GS_SOURCES)

bin_SCRIPTS = $(srcdir)/GnomeSubtitles/Execution/gnome-subtitles

CLEANFILES = $(bin_SCRIPTS) $(ASSEMBLY) $(ASSEMBLY).mdb
EXTRA_DIST = $(GS_SOURCES) $(GS_RESOURCES)
