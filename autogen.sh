#!/bin/sh

srcdir=`dirname $0`
[ -z "$srcdir" ] && srcdir=.

if [ ! -f "$srcdir/configure.ac" ]; then
 echo "$srcdir doesn't look like a source directory" >&2
 exit 1
fi

GNOMEDOC=`which yelp-build`
if test -z $GNOMEDOC; then
        echo "*** The tools to build the documentation are not found,"
        echo "    please install the yelp-tool package ***"
        exit 1
fi

which gnome-autogen.sh || {
	echo "You need to install gnome-common"
	exit 1
}

. gnome-autogen.sh "$@"

