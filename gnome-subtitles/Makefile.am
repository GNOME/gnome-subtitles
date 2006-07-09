SUBDIRS = src
bin_SCRIPTS = gnome-subtitles
EXTRA_DIST = script.in

gsubtitles_builddir = $(srcdir)/build
gsubtitles_etcdir = $(srcdir)/etc

REWRITE = \
        sed -e "s|\@prefix\@|$(prefix)|g"               \
            -e "s|\@pkglibdir\@|$(pkglibdir)|g"         \
            -e "s|\@bindir\@|$(bindir)|g"		\
	    -e "s|\@MONO\@|$(MONO)|g" 

$(bin_SCRIPTS): $(gsubtitles_etcdir)/script.in Makefile
	$(REWRITE) $(gsubtitles_etcdir)/script.in > $(gsubtitles_builddir)/$@.tmp
	mv $(gsubtitles_builddir)/$@.tmp $(gsubtitles_builddir)/$@


