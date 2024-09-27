install_man('gnome-subtitles.1')

appdata_file = i18n.merge_file(
	input: 'org.gnome.GnomeSubtitles.appdata.xml.in',
	output: 'org.gnome.GnomeSubtitles.appdata.xml',
	type: 'xml',
	po_dir: meson.source_root() / 'po',
	install: true,
	install_dir: get_option('datadir') / 'metainfo'
)

appstreamcli = find_program('appstreamcli', required: get_option('require_all_tests'))
if appstreamcli.found()
	test('validate-appdata', appstreamcli, depends: appdata_file, args: ['validate', '--no-net', '--explain', appdata_file])
endif


desktop_file = i18n.merge_file(
	input: 'org.gnome.GnomeSubtitles.desktop.in',
	output: 'org.gnome.GnomeSubtitles.desktop',
	type: 'desktop',
	po_dir: meson.source_root() / 'po',
	install: true,
	install_dir: get_option('datadir') / 'applications'
)

desktop_file_validate = find_program('desktop-file-validate', required: get_option('require_all_tests'))
if desktop_file_validate.found()
	test('validate-desktop', desktop_file_validate, depends: desktop_file, args: [desktop_file])
endif

install_data('org.gnome.GnomeSubtitles.gschema.xml', install_dir: get_option('datadir') / 'glib-2.0' / 'schemas')
#Note: schema installed in /build-aux/meson_post_install.py

install_data('gnome-subtitles.svg', install_dir: get_option('datadir') / 'icons/hicolor/scalable/apps')

