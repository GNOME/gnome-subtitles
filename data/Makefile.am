desktopdir=$(datadir)/applications
desktop_in_files = org.gnome.GnomeSubtitles.desktop.in
desktop_DATA = $(desktop_in_files:.desktop.in=.desktop)
@INTLTOOL_DESKTOP_RULE@

appdatadir = $(datadir)/metainfo
appdata_in_files = org.gnome.GnomeSubtitles.appdata.xml.in
appdata_DATA = $(appdata_in_files:.xml.in=.xml)
@INTLTOOL_XML_RULE@

gnomesubtitlesicondir = $(datadir)/icons/hicolor/scalable/apps
gnomesubtitlesicon_DATA = gnome-subtitles.svg

gsettings_SCHEMAS=org.gnome.GnomeSubtitles.gschema.xml
@GSETTINGS_RULES@	

man_MANS = gnome-subtitles.1

CLEANFILES =			\
	$(appdata_DATA)		\
	$(desktop_DATA)

EXTRA_DIST =				\
	$(appdata_in_files)		\
	$(desktop_in_files)		\
	$(gnomesubtitlesicon_DATA)	\
	$(man_MANS)			\
	$(gsettings_SCHEMAS)

