# Copyright 1999-2006 Gentoo Foundation
# Distributed under the terms of the GNU General Public License v2
# $Header: $

inherit mono

DESCRIPTION="Movie subtitling for the Gnome desktop"
HOMEPAGE="http://gsubtitles.sourceforge.net/"
SRC_URI="mirror://sourceforge/gsubtitles/${P}.tar.gz"

LICENSE="GPL-2"
IUSE=""
SLOT="0"
KEYWORDS="~x86 ~amd64"
RESTRICT="nomirror"

DEPEND=">=dev-lang/mono-1.1
	>=dev-dotnet/gtk-sharp-2.8
	>=dev-dotnet/gnome-sharp-2.8
	>=dev-dotnet/glade-sharp-2.8
	>=x11-libs/gtk+-2.8"

DOCS="AUTHORS ChangeLog COPYING CREDITS \
      INSTALL NEWS README TODO"

src_compile() {
	econf || die "Configure failed"
	emake -j1 || die "Make failed"
}

src_install() {
	make install DESTDIR=${D} || die "Install failed"
	dodoc ${DOCS} || die "Docs install failed"
}
