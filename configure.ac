AC_PREREQ(2.58)
AC_INIT(gnome-subtitles, 0.6, http://bugzilla.gnome.org/enter_bug.cgi?product=gnome-subtitles)

GNOME_COMMON_INIT

GNOME_DOC_INIT
GTK_DOC_CHECK([1.0])


AC_CONFIG_MACRO_DIR([m4])
AC_CONFIG_SRCDIR([configure.ac])

AM_INIT_AUTOMAKE(1.9)
AM_MAINTAINER_MODE

IT_PROG_INTLTOOL([0.35.0])

AC_PROG_INSTALL
AC_PREFIX_PROGRAM(mono)

dnl Gettext stuff.

GETTEXT_PACKAGE=gnome-subtitles
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
AC_PATH_PROG(MCS, gmcs, no)
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

echo -n "checking for sublib package dir... "
for i in sublib*
do
	SUBLIB_DIR=$i
done
if test -d $SUBLIB_DIR
then
	echo $SUBLIB_DIR
	AC_CONFIG_SUBDIRS($SUBLIB_DIR)
else
	echo "no"
	SUBLIB_DIR=""
	echo -n "checking for build/sublib.dll... "
	if test -f "build/sublib.dll"
	then
		echo "yes"
	else
		echo "no"
		AC_MSG_ERROR([SubLib not found. Place a compiled sublib.dll into the `build' dir OR extract a sublib source dir into gnome-subtitles base dir.])
	fi
fi

AC_SUBST(SUBLIB_DIR)

dnl pkg-config
MONO_REQUIRED_VERSION=1.1
GTKSHARP_REQUIRED_VERSION=2.8
GTK_REQUIRED_VERSION=2.8

PKG_CHECK_MODULES(MONO, mono >= $MONO_REQUIRED_VERSION)
PKG_CHECK_MODULES(GTKSHARP, gtk-sharp-2.0 glade-sharp-2.0 gnome-sharp-2.0 gconf-sharp-2.0 >= $GTKSHARP_REQUIRED_VERSION)
PKG_CHECK_MODULES(GTK, gtk+-2.0 >= $GTK_REQUIRED_VERSION)

AC_SUBST(MCS)

dnl Handle GConf
AC_PATH_PROG(GCONFTOOL, gconftool-2, no)
AM_GCONF_SOURCE_2

AC_OUTPUT([
Makefile
data/Makefile
data/gnome-subtitles.desktop
src/Makefile
src/GnomeSubtitles/Execution/AssemblyInfo.cs
src/GnomeSubtitles/Execution/gnome-subtitles
help/Makefile
po/Makefile.in])
