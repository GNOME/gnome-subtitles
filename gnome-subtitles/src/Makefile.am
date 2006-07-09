gsubtitlesdir=$(pkglibdir)
gsubtitles_SCRIPTS = gnome-subtitles.exe
EXTRA_DIST = $(gsubtitles_sources) $(gsubtitles_sources_in)
CLEANFILES = gnome-subtitles.exe

gsubtitles_builddir = $(srcdir)/../build

gsubtitles_sources = GnomeSubtitles/Application/*.cs GnomeSubtitles/Application/Dialogs/*.cs GnomeSubtitles/Execution/*.cs

gsubtitles_build_sources = $(addprefix $(srcdir)/, $(gsubtitles_sources))
gsubtitles_build_sources += $(gsubtitles_generated_sources)
resources = $(srcdir)/Glade/gnome-subtitles.glade

embedded= $(foreach res,$(resources), $(addprefix -resource:,$(res)),$(notdir $(res)))

gsubtitles_references = -r:$(gsubtitles_builddir)/sublib.dll -r:Mono.Posix.dll

gnome-subtitles.exe: $(gsubtitles_build_sources) $(resources)
	$(MCS) $(GTKSHARP_LIBS) $(gsubtitles_references) $(embedded) -out:$(gsubtitles_builddir)/$@ $(gsubtitles_build_sources) 

