# Gnome Subtitles

https://gnomesubtitles.org

Gnome Subtitles is a subtitle editor for the GNOME desktop. It supports the most
common text-based subtitle formats and allows for subtitle editing, translation
and synchronization.


## Dependencies

Runtime dependencies:
- mono-runtime			>= 4.0
- libmono-i18n4.0-all		>= 4.0
- libmono-posix4.0-cil		>= 4.0
- libgtk3.0			>= 3.12
- libenchant			>= 1.6
- libgtkspell3			>= 3.0
- libgstreamer1.0		>= 1.18
- gstreamer1.0-gtk3		>= 1.18
- gstreamer1.0-plugins-good	>= 1.18
- gstreamer1.0-libav		>= 1.18 (optional, for media codecs)

Build dependencies:
- git
- xsltproc
- gettext
- gcc
- meson			>= 0.53
- mono-devel		>= 4.0
- libgtk-3-dev		>= 3.12
- libgstreamer1.0-dev	>= 1.18
- libges-1.0-dev	>= 1.18
- libgstreamer-plugins-base1.0-dev	>= 1.18
- libgstreamer-plugins-bad1.0-dev	>= 1.18


## Building

	meson build && meson compile -C build


## Installing

	meson install -C build


## Hacking

Build the solution with Meson (see Building above) to compile all dependencies
and make them available in Monodevelop. This only needs to be done once.
Fire up Monodevelop and open the solution file 'gnome-subtitles.sln'.


## License

Gnome Subtitles is released under the GNU General Public License (GPL) version 2 or
later, see the file [COPYING](COPYING) for more information.


## Need help?

Create an issue on gitlab:

	https://gitlab.gnome.org/GNOME/gnome-subtitles/-/issues/

or contact the maintainer:

	pedro at gnomesubtitles.org

