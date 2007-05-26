##
# spec file for package gnome-subtitles
#
# Copyright (c) Damien Carbery <daymobrew users.sourceforge.net>
#               Henrique Malheiro <henrique.malheiro gmail.com>
#               Pedro Castro <noup users.sourceforge.net>
#
#
%define dist	%(test -f /etc/redhat-release && \
		echo .fc`rpm -qf --qf='%{VERSION}' /etc/redhat-release`)
%define fedora  `rpm -qf --qf='%{version}`

Name:           gnome-subtitles
Summary:        Video subtitling for the GNOME desktop
Version:        0.5.1
Release:        1%{?dist}
Group:          Applications/Multimedia
License:        GPL
URL:            http://gnome-subtitles.sourceforge.net/
Source:         http://downloads.sourceforge.net/%{name}/%{name}-%{version}.tar.gz
BuildRoot:      %(mktemp -ud %{_tmppath}/%{name}-%{version}-%{release}-XXXXXX)
BuildArch:      noarch

# Mono only available on these: (s390x disabled for now)
ExclusiveArch: %ix86 x86_64 ppc ia64 armv4l sparc

Prereq:         /sbin/ldconfig

Requires(pre):	GConf2
Requires(post):	GConf2
Requires(preun):GConf2

Requires(post): scrollkeeper
Requires(postrun): scrollkeeper

Requires:       mono-core >= 1.0
Requires:       gtk2 >= 2.8
Requires:       gtk-sharp2 >= 2.8

BuildRequires:  scrollkeeper

BuildRequires:  mono-devel >= 1.0
BuildRequires:  gtk2-devel >= 2.8
BuildRequires:  gtk-sharp2 >= 2.8

%if "%fedora" >= "6"
BuildRequires:	gtk-sharp2-devel >= 2.10
BuildRequires:	gnome-sharp-devel >= 2.16
%endif

%description
Gnome Subtitles is a subtitle editor for the GNOME desktop.
It supports the most common subtitle formats and allows for
subtitle editing, conversion and synchronization.

%prep
%setup -q

%build
./configure --prefix=%{_prefix} --bindir=%{_bindir} --libdir=%{_libdir} \
  --datadir=%{_datadir} --sysconfdir=%{_sysconfdir} --mandir=%{_mandir} \
  --disable-scrollkeeper
make

%install
rm -rf $RPM_BUILD_ROOT
export GCONF_DISABLE_MAKEFILE_SCHEMA_INSTALL=1
make install DESTDIR=$RPM_BUILD_ROOT

%clean
rm -rf $RPM_BUILD_ROOT

%pre
if [ "$1" -gt 1 ]; then
	export GC2=/usr/bin/gconftool-2
	export GCONF_CONFIG_SOURCE=`$GC2 --get-default-source`
	$GC2 --makefile-uninstall-rule \
		%{_sysconfdir}/gconf/schemas/%{name}.schemas > /dev/null
fi

%post
export GC2=/usr/bin/gconftool-2
export GCONF_CONFIG_SOURCE=`$GC2 --get-default-source`
$GC2 --makefile-install-rule %{_sysconfdir}/gconf/schemas/%{name}.schemas > /dev/null
scrollkeeper-update -q -o %{_datadir}/omf/%{name} || :
/sbin/ldconfig

%preun
if [ "$1" -eq 0 ]; then
	export GC2=/usr/bin/gconftool-2
	export GCONF_CONFIG_SOURCE=`$GC2 --get-default-source`
	$GC2 --makefile-uninstall-rule \
		%{_sysconfdir}/gconf/schemas/%{name}.schemas > /dev/null
fi

%postun
scrollkeeper-update -q || :
/sbin/ldconfig

%files
%defattr(0664,root,root,0755)
%attr(0755,root,root) %{_bindir}/%{name}
%attr(0755,root,root) %{_libdir}/%{name}
%{_datadir}/applications/%{name}.desktop
%{_datadir}/pixmaps/%{name}.png
%{_datadir}/gnome/help/%{name}/C/%{name}.xml
%{_datadir}/gnome/help/%{name}/C/legal.xml
%{_datadir}/omf/%{name}/%{name}-C.omf
%{_sysconfdir}/gconf/schemas/%{name}.schemas
%docdir %{_mandir}/man1
%{_mandir}/man1/%{name}.1.gz 
%doc README NEWS AUTHORS COPYING CREDITS TODO



%changelog
* Fri May 25 2007 - Henrique Malheiro <henrique.malheiro gmail.com> - 0.5.1-1
- Removed config directive for gconf schemas files.
- Added Required(post), Required(postrun) and BuildRequires for scrollkeeper.
- Disabled scrollkeeper in the configure command of the build section.
- Added scroolkeeper update commands to post and postrun sections.
- Updated files list to include help related files (xml and omf files).
* Sat Apr 28 2007 - Marcin Zajaczkowski <mszpak ATT wp DOTT pl> - 0.4-2
- Changed defattr and attr
- Added URL for a source tarball
- Changed Arch options (AFAIK mono is not available for all platforms supported by FC7)
* Thu Apr 26 2007 - Henrique Malheiro <henrique.malheiro gmail.com> 0.4-1
- Removed support for the SuSE distribution.
- Corrected buildroot path.
- Added man page in files list.
* Wed Mar 14 2007 - Henrique Malheiro <henrique.malheiro gmail.com>
- Updated application version to 0.2.1.
- Back to dependencies gtk2, gtk-sharp2, gtk2-devel, version 2.8 or above.
* Fri Mar 09 2007 - Henrique Malheiro <henrique.malheiro gmail.com>
- Updated application version to 0.2.
- Updated dependencies to include gtk2, gtk-sharp2, gtk2-devel, version 2.10 or above.
* Thu Dec 14 2006 - Henrique Malheiro <henrique.malheiro gmail.com>
- Updated the application icon extension from svg to png.
- Updated the build requirements for fedora core 6 to include gtk-sharp2-devel
  and gnome-sharp-devel.
- Added the dist tag for using the same spec file for both distributions, fedora
  core 5 and fedora core 6 and removed the distribution tag for fedora. This
  will be useful for Fedora Core Extras.
* Wed Dec 13 2006 - Pedro Castro <noup users.sourceforge.net>
- Updated the website URL.
- Updated for release 0.1.
* Tue Oct 31 2006 - Damien Carbery <daymobrew users.sourceforge.net>
- Merged with the SUSE Linux spec file.
* Tue Oct 31 2006 - Henrique Malheiro <henrique.malheiro gmail.com>
- Adapted spec file for the new 0.0.3 version of gnome-subtitles.
- Modified build architecture to noarch. Configure macro had to be replaced by
  a custom configure.
- Corrected the path to the executable in the files section.
- Added new doc files to the files section.
* Sun Oct 22 2006 - Henrique Malheiro <henrique.malheiro gmail.com>
- Initial version of the spec file, written for the 0.0.2 version of gnome-subtitles.
