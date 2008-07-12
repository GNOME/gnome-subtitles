gnomesubtitlesdir = $(pkglibdir)
gnomesubtitles_SCRIPTS = $(ASSEMBLY)
gnomesubtitles_DATA = $(ASSEMBLY_CONFIG)
GS_BUILDDIR = $(srcdir)/../build
GS_DATADIR = $(srcdir)/../data

ASSEMBLY = $(GS_BUILDDIR)/gnome-subtitles.exe
ASSEMBLY_CONFIG = $(ASSEMBLY).config
GSTPB = $(GS_BUILDDIR)/GStreamerPlaybin.dll

GS_SOURCES = \
	$(srcdir)/GnomeSubtitles/Core/*.cs \
	$(srcdir)/GnomeSubtitles/Core/Command/*.cs \
	$(srcdir)/GnomeSubtitles/Dialog/*.cs \
	$(srcdir)/GnomeSubtitles/Execution/*.cs \
	$(srcdir)/GnomeSubtitles/Ui/*.cs \
	$(srcdir)/GnomeSubtitles/Ui/Edit/*.cs \
	$(srcdir)/GnomeSubtitles/Ui/VideoPreview/*.cs \
	$(srcdir)/GnomeSubtitles/Ui/VideoPreview/Exception/*.cs \
	$(srcdir)/GnomeSubtitles/Ui/View/*.cs

GS_RESOURCES = \
	$(srcdir)/Glade/AboutDialog.glade \
	$(srcdir)/Glade/EncodingsDialog.glade \
	$(srcdir)/Glade/FileOpenDialog.glade \
	$(srcdir)/Glade/FilePropertiesDialog.glade \
	$(srcdir)/Glade/FileSaveAsDialog.glade \
	$(srcdir)/Glade/HeadersDialog.glade \
	$(srcdir)/Glade/MainWindow.glade \
	$(srcdir)/Glade/PreferencesDialog.glade \
	$(srcdir)/Glade/ReportBugWindow.glade \
	$(srcdir)/Glade/SearchDialog.glade \
	$(srcdir)/Glade/SetLanguageDialog.glade \
	$(srcdir)/Glade/TimingsAdjustDialog.glade \
	$(srcdir)/Glade/TimingsShiftDialog.glade \
	$(srcdir)/Glade/VideoOpenDialog.glade \
	$(GS_DATADIR)/gnome-subtitles.png \
	$(GS_DATADIR)/gnome-subtitles-logo.png \
	$(GS_DATADIR)/video-set-subtitle-end-16x.png \
	$(GS_DATADIR)/video-set-subtitle-start-16x.png

GS_SYSTEM_REFERENCES = \
	$(GSTPB) \
	Mono.Posix.dll

$(ASSEMBLY): $(GS_SOURCES) $(GS_RESOURCES) $(GSTPB)
	$(MCS) $(MCS_FLAGS) -target:winexe -out:$(ASSEMBLY) $(GS_SYSTEM_REFERENCES:%=-r:%) $(GTKSHARP_LIBS) $(SUBLIB_LIBS) $(GS_RESOURCES:%=-resource:%) $(GS_SOURCES)

$(ASSEMBLY_CONFIG):
	cp -f $(srcdir)/GnomeSubtitles/Execution/gnome-subtitles.exe.config $(GS_BUILDDIR)

bin_SCRIPTS = $(srcdir)/GnomeSubtitles/Execution/gnome-subtitles

CLEANFILES = $(bin_SCRIPTS) $(ASSEMBLY) $(ASSEMBLY).mdb
EXTRA_DIST = $(gnomesubtitles_SOURCES) $(GS_SOURCES) $(GS_RESOURCES) $(ASSEMBLY_CONFIG)

