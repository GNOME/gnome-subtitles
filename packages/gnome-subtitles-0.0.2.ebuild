# Copyright 1999-2006 Gentoo Foundation
# Distributed under the terms of the GNU General Public License v2
# $Header: $

inherit mono

DESCRIPTION="A subtitle editor for the GNOME Desktop"
HOMEPAGE="http://gsubtitles.sourceforge.net/"
SRC_URI="mirror://sourceforge/gsubtitles/${P}.tar.gz"

LICENSE="GPL"
SLOT="0"
KEYWORDS="~x86 ~amd64"
RESTRICT="nomirror"

DEPEND=">=dev-lang/mono-1.1.8
	>=dev-dotnet/gtk-sharp-2.8
	>=dev-dotnet/gnome-sharp-2.8
	>=dev-dotnet/glade-sharp-2.8
	>=x11-libs/gtk+-2.8"

src_compile() {
	cd ${S} || die "Could not cd to gnome-subtitles directory."
	econf || die "Configure failed"
	emake -j1 || die "Make failed"
}

src_install() {
	einstall || die "Install failed"
}
