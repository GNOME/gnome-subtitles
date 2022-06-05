# Gnome Subtitles

https://gnomesubtitles.org

Gnome Subtitles is a subtitle editor for the GNOME desktop. It supports the most
common text-based subtitle formats and allows for subtitle editing, translation
and synchronization.


## Dependencies

Run time dependencies:
- mono-runtime		>= 4.0
- libmono-i18n4.0-all	>= 4.0
- libmono-posix4.0-cil	>= 4.0
- libgtk3.0		>= 3.12
- libgtk3.0-cil		>= 2.99.2
- gstreamer1.0		>= 1.0
- gstreamer1.0-x	>= 1.0
- gstreamer1.0-gtk3	>= 1.0
- gstreamer1.0-plugins-good	>= 1.0
- gstreamer1.0-libav	>= 1.0
- enchant		>= 1.6
- gtkspell3		>= 3.0

Build time dependencies:
- meson			>= 0.53
- pkg-config, intltool, autoconf, automake, libtool and yelp-tools
- libgtk-3-dev		>= 3.12
- mono-devel		>= 4.0
- gtk-sharp3		>= 2.99.2
- libgstreamer1.0-dev	>= 1.0
- libgstreamer-plugins-base1.0-dev >= 1.0


## Building

	meson build && meson compile -C build


## Installing

	meson install -C build


## Hacking

Build the solution with Meson (see Building above) to compile all dependencies
and make them available to Monodevelop. This only needs to be done once.
Fire up Monodevelop and open the solution file 'gnome-subtitles.sln'.


## License

Gnome Subtitles is released under the GNU General Public License (GPL) version 2 or
later, see the file [COPYING](COPYING) for more information.


## Need help?

Create an issue on gitlab:

	https://gitlab.gnome.org/GNOME/gnome-subtitles/issues/new

or contact the maintainer:

	pedro at gnomesubtitles.org

