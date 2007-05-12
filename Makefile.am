SUBDIRS = $(SUBLIB_DIR) . src data po help

ACLOCAL_AMFLAGS = -I m4

sublibdir = $(pkglibdir)
sublib_DATA = build/sublib.dll

$(sublib_DATA):
	cp $(SUBLIB_DIR)/build/sublib.dll build

EXTRA_DIST = 			\
	autogen.sh			\
	CREDITS				\
	gnome-doc-utils.make	\
	gnome-subtitles.mdp \
	gnome-subtitles.mds \
	intltool-extract.in \
	intltool-merge.in 	\
	intltool-update.in

DISTCLEANFILES = 		\
	intltool-extract 	\
	intltool-merge 		\
	intltool-update		\
	gnome-doc-utils.make
	
	
