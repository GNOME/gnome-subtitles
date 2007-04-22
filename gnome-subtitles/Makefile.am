SUBDIRS = $(SUBLIB_DIR) . src data po

sublibdir = $(pkglibdir)
sublib_DATA = build/sublib.dll

$(sublib_DATA):
	cp $(SUBLIB_DIR)/build/sublib.dll build

EXTRA_DIST = 			\
	CREDITS				\
	gnome-subtitles.mdp \
	gnome-subtitles.mds \
	intltool-extract.in \
	intltool-merge.in \
	intltool-update.in
