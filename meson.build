project('gnome-subtitles', 'cs',
	version : '1.8',
	meson_version : '>= 0.53',
	license: 'GPL-2.0-or-later')

sources_copyright_last_year = '2022'

i18n = import('i18n')

dependency('mono', version : '>= 4.0')
dependency('gtk+-3.0', version : '>= 3.12')
dependency('gstreamer-1.0', version : '>= 1.18')
dependency('gstreamer-video-1.0', version : '>= 1.18')
dependency('gstreamer-plugins-base-1.0', version : '>= 1.18')
dependency('gstreamer-pbutils-1.0', version : '>= 1.18')

gst_sharp = subproject('gstreamer-sharp')
gst_sharp_dep = gst_sharp.get_variable('gst_sharp_dep')
gtk_sharp_dep = gst_sharp.get_variable('gtk_sharp_dep')

subdir('data')
subdir('po')
subdir('src')
subdir('build-aux')

summary('Install gtk-sharp', get_option('install_gtk_sharp'))
summary('Install gstreamer-sharp', get_option('install_gst_sharp'))
summary('Require all tests', get_option('require_all_tests'))

