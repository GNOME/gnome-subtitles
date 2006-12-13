SUBDIRS = $(SUBLIB_DIR) . src data

sublibdir = $(pkglibdir)
sublib_DATA = build/sublib.dll

$(sublib_DATA):
	cp $(SUBLIB_DIR)/build/sublib.dll build

EXTRA_DIST = 				\
	CREDITS				\
	build.properties		\
	gnome-subtitles.mdp		\
	gnome-subtitles.mds		\
	nant.build
