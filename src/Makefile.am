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

GSSOURCES = \
	$(srcdir)/External/Enchant/*.cs \
	$(srcdir)/External/GtkSpell/*.cs \
	$(srcdir)/External/GStreamer/*.cs \
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

GS_LOCAL_REFERENCES = \
	$(GS_BUILDDIR)/atk-sharp.dll \
	$(GS_BUILDDIR)/cairo-sharp.dll \
	$(GS_BUILDDIR)/gdk-sharp.dll \
	$(GS_BUILDDIR)/gio-sharp.dll \
	$(GS_BUILDDIR)/glib-sharp.dll \
	$(GS_BUILDDIR)/gtk-sharp.dll \
	$(GS_BUILDDIR)/pango-sharp.dll \
	$(GS_BUILDDIR)/gstreamer-sharp.dll

$(ASSEMBLY): $(GSSOURCES) $(GS_RESOURCES)
	$(MCS) $(MCS_FLAGS) -target:winexe -out:$(ASSEMBLY) $(GS_SYSTEM_REFERENCES:%=-r:%) $(GS_LOCAL_REFERENCES:%=-r:%) $(GS_RESOURCES:%=-resource:%) $(GSSOURCES)

$(ASSEMBLY_CONFIG): $(gnomesubtitles_LTLIBRARIES)
	cp -f $(ASSEMBLY_CONFIG_SRC) $(GS_BUILDDIR)

bin_SCRIPTS = $(srcdir)/GnomeSubtitles/Execution/gnome-subtitles

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb
EXTRA_DIST = $(gnomesubtitles_SOURCES) $(GSSOURCES) $(GS_RESOURCES) $(ASSEMBLY_CONFIG_SRC)

