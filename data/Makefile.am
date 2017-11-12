desktopdir = $(datadir)/applications
desktop_DATA = gnome-subtitles.desktop

appdatadir = $(datadir)/appdata
appdata_DATA = gnome-subtitles.appdata.xml

gnomesubtitlesicondir = $(datadir)/pixmaps
gnomesubtitlesicon_DATA = gnome-subtitles.svg

gsettings_SCHEMAS=org.gnome.GnomeSubtitles.gschema.xml
@GSETTINGS_RULES@	

#schemadir = $(GCONF_SCHEMA_FILE_DIR)
#schema_DATA = gnome-subtitles.schemas

man_MANS = gnome-subtitles.1

EXTRA_DIST =				\
	$(desktop_DATA)			\
	$(appdata_DATA)			\
	$(gnomesubtitlesicon_DATA)	\
	$(man_MANS)			\
	$(gsettings_SCHEMAS)

#if GCONF_SCHEMAS_INSTALL
#install-data-local:
#	GCONF_CONFIG_SOURCE=$(GCONF_SCHEMA_CONFIG_SOURCE) $(GCONFTOOL) --makefile-install-rule $(schema_DATA)
#endif
