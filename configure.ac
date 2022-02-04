dnl Process this file with autoconf to produce a configure script.

AC_PREREQ(2.63)
AC_INIT([gnome-subtitles],
	[1.7.2],
	[https://gitlab.gnome.org/GNOME/gnome-subtitles/issues/new],
	[gnome-subtitles],
	[https://www.gnomesubtitles.org])

AC_CONFIG_MACRO_DIR([m4])
AC_SUBST([ACLOCAL_AMFLAGS], ["-I $ac_macro_dir \${ACLOCAL_FLAGS}"])
AC_CONFIG_SRCDIR([configure.ac])

AM_INIT_AUTOMAKE([1.9 tar-ustar])
AM_MAINTAINER_MODE

LT_PREREQ([2.2.6])
LT_INIT

AC_PROG_INSTALL
AC_PREFIX_PROGRAM(mono)

IT_PROG_INTLTOOL([0.50])

dnl Gettext stuff.

GETTEXT_PACKAGE=org.gnome.GnomeSubtitles
AC_SUBST(GETTEXT_PACKAGE)
AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE,"$GETTEXT_PACKAGE", [Gettext package])

AM_GLIB_GNU_GETTEXT


dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
	AC_MSG_ERROR([You need to install pkg-config])
fi

dnl C# compiler
AC_PATH_PROG(CSC, csc, no)
AC_PATH_PROG(MCS, mcs, no)
AC_PATH_PROG(MONO, mono, no)

CS="C#"
if test "x$CSC" = "xno" -a "x$MCS" = "xno"  ; then
	dnl AC_MSG_ERROR([You need to install a C# compiler])
	AC_MSG_ERROR([No $CS compiler found])
fi

if test "x$MCS" = "xno" ; then
	MCS=$CSC
fi

if test "x$MONO" = "xno"; then
	AC_MSG_ERROR([No mono runtime found])
fi

dnl create build dir if it does not exist
if test ! -d "build"
then
	mkdir build
fi

#Dependencies
MONO_REQUIRED_VERSION=4.0
#GTKSHARP_REQUIRED_VERSION=2.99.3
GTK_REQUIRED_VERSION=3.12
GSTREAMER_REQUIRED_VERSION=1.0

PKG_CHECK_MODULES(MONO, [mono >= $MONO_REQUIRED_VERSION])
#PKG_CHECK_MODULES(GTKSHARP, [gtk-sharp-3.0 >= $GTKSHARP_REQUIRED_VERSION])
PKG_CHECK_MODULES(GTK, [gtk+-3.0 >= $GTK_REQUIRED_VERSION])
PKG_CHECK_MODULES(gstreamer, [
	gstreamer-1.0 >= $GSTREAMER_REQUIRED_VERSION
	gstreamer-video-1.0 >= $GSTREAMER_REQUIRED_VERSION
	gstreamer-plugins-base-1.0 >= $GSTREAMER_REQUIRED_VERSION
	gstreamer-pbutils-1.0 >= $GSTREAMER_REQUIRED_VERSION
])
AC_SUBST(gstreamer_CFLAGS)
AC_SUBST(gstreamer_LIBS)


dnl Check gtk-sharp version
#PKG_CHECK_MODULES(GTKSHARP_INVALID, gtk-sharp-2.0 >= 2.12.2 gtk-sharp-2.0 <= 2.12.6, gtksharp_invalid=true, gtksharp_invalid=false)

#if test "x$gtksharp_invalid" = "xtrue"; then
# 	AC_MSG_ERROR([gtk-sharp versions between 2.12.2 and 2.12.6 (inclusive) are not supported.])
#fi

#PKG_PROG_PKG_CONFIG()

AC_SUBST(MCS)

dnl Handle GSettings
GLIB_GSETTINGS

AC_CONFIG_FILES([
Makefile
data/Makefile
data/org.gnome.GnomeSubtitles.desktop.in
src/Makefile
src/GnomeSubtitles/Execution/AssemblyInfo.cs
src/GnomeSubtitles/Execution/gnome-subtitles
po/Makefile.in
])

AC_OUTPUT

