meson.add_install_script('meson_post_install.py', get_option('libdir'))

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

