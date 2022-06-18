meson.add_install_script('meson_post_install.py', get_option('libdir'), get_option('install_gtk_sharp').to_string(), get_option('install_gst_sharp').to_string())

configure_file(
	input: 'gnome-subtitles.in',
	output: 'gnome-subtitles',
	configuration: {
		'mono': find_program('mono', required: true).path(),
		'libdir': join_paths(get_option('prefix'), get_option('libdir'))
	},
	install_dir: get_option('bindir')
)

install_data('gnome-subtitles.exe.config', install_dir: get_option('libdir') / 'gnome-subtitles')

