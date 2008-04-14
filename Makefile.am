SUBDIRS = $(GSTPB_DIR) . src data po help

ACLOCAL_AMFLAGS = -I m4

gstpbdir = $(pkglibdir)
gstpb_SCRIPTS = build/GStreamerPlaybin.dll build/libgstreamer_playbin.so

$(gstpb_SCRIPTS):
	cp -f $(GSTPB_DIR)/src/GStreamerPlaybin.dll build && cp -f $(GSTPB_DIR)/src/libgstreamer_playbin.so build

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
