##
# spec file for package gnome-subtitles
#
# Copyright (c) Damien Carbery <daymobrew users.sourceforge.net>
#               Henrique Malheiro <henrique.malheiro gmail.com>
#               Pedro Castro <noup users.sourceforge.net>
#
#
%define dist	%(test -f /etc/redhat-release && echo .fc`rpm -qf --qf='%{VERSION}' /etc/redhat-release`)
%define OnSuSE   %(test -f /etc/SuSE-release && echo 1 || echo 0)

Name:           gnome-subtitles
Summary:        Video subtitling for the GNOME desktop
Version:        0.3
Release:        1%dist
%if %OnSuSE
Distribution:   SuSE 10.1
%endif
Group:          Applications/Multimedia
License:        GPL
URL:            http://gnome-subtitles.sf.net/
Source:         http://downloads.sourceforge.net/%{name}/%{name}-%{version}.tar.gz
BuildRoot:      %{_tmppath}/%{name}-%{version}-build
BuildArch:      noarch

Prereq:         /sbin/ldconfig

Requires:       mono-core >= 1.0
Requires:       gtk2 >= 2.10
Requires:       gtk-sharp2 >= 2.8
Requires:       gconf-sharp2 >= 2.8
Requires:       gconf2

%if %OnSuSE
Requires:       glade-sharp2 >= 2.8
Requires:       gnome-sharp2 >= 2.8
%endif

BuildRequires:  mono-devel >= 1.0
BuildRequires:  gtk2-devel >= 2.8
BuildRequires:  gtk-sharp2 >= 2.8
BuildRequires:  gconf-sharp2 >= 2.8

%if %OnSuSE
BuildRequires:  glade-sharp2 >= 2.8
BuildRequires:  gnome-sharp2 >= 2.8
%endif

%if "%fedora" >= "6"
BuildRequires:	gtk-sharp2-devel >= 2.10
BuildRequires:	gnome-sharp-devel >= 2.16
%endif

%description
Gnome Subtitles is a subtitle editor for the GNOME desktop. It supports the most
common subtitle formats and allows for subtitle editing, conversion and
synchronization.

%prep
%setup -q

%build
# Set _libdir otherwise it becomes /usr/lib64 on SuSE x64.
%define _libdir %{_prefix}/lib
%if %OnSuSE
./configure --prefix=%{_prefix} --bindir=%{_bindir} --libdir=%{_libdir} --datadir=%{_datadir} --sysconfdir=%{_sysconfdir}
%else
./configure --prefix=/usr --bindir=/usr/bin --libdir=/usr/lib --datadir=/usr/share --sysconfdir=%{_sysconfdir}
%endif
make

%install
export GCONF_DISABLE_MAKEFILE_SCHEMA_INSTALL=1
make DESTDIR=$RPM_BUILD_ROOT install
unset GCONF_DISABLE_MAKEFILE_SCHEMA_INSTALL

%clean
rm -rf $RPM_BUILD_ROOT

%post
%if %OnSuSE
export GC2=/opt/gnome/bin/gconftool-2
%else
export GC2=/usr/bin/gconftool-2
%endif
/sbin/ldconfig
export GCONF_CONFIG_SOURCE=`$GC2 --get-default-source`
$GC2 --makefile-install-rule %{_sysconfdir}/gconf/schemas/gnome-subtitles.schemas > /dev/null

%postun
/sbin/ldconfig

%files
%defattr(-,root,root)
%{_bindir}/%{name}
%{_libdir}/%{name}
%{_datadir}/applications/%{name}.desktop
%{_datadir}/pixmaps/%{name}.png
%{_sysconfdir}/gconf/schemas/gnome-subtitles.schemas

%doc README NEWS AUTHORS COPYING CREDITS TODO


%changelog
* Tue Mar 27 2007 - Damien Carbery <daymobrew users.sourceforge.net>
- Bump to 0.3 and add GConf code to %install and %post.
* Thu Dec 14 2006 - Henrique Malheiro <henrique.malheiro@gmail.com>
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
