desktopdir = $(datadir)/applications
desktop_DATA = gnome-subtitles.desktop

gnomesubtitlesicondir = $(datadir)/pixmaps
gnomesubtitlesicon_DATA = gnome-subtitles.svg

schemadir = $(GCONF_SCHEMA_FILE_DIR)
schema_DATA = gnome-subtitles.schemas

man_MANS = gnome-subtitles.1

EXTRA_DIST =				\
	$(desktop_DATA)			\
	$(gnomesubtitlesicon_DATA)	\
	$(man_MANS)			\
	$(schema_DATA)

if GCONF_SCHEMAS_INSTALL
install-data-local:
	GCONF_CONFIG_SOURCE=$(GCONF_SCHEMA_CONFIG_SOURCE) $(GCONFTOOL) --makefile-install-rule $(schema_DATA)
endif
