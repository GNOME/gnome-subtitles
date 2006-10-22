##
# spec file for package gnome-subtitles
#
# Copyright (c) Pedro Castro <noup@users.sourceforge.net>
#		Henrique Malheiro <henrique.malheiro@gmail.com>
#
Name:         gnome-subtitles
Summary:      Subtitle editor for the GNOME Desktop
Version:      0.0.2
Release:      1
Distribution: Fedora Core 5
Group:        Applications/Multimedia
License:      GPL
URL:          http://gsubtitles.sourceforge.net/
Source:	      %{name}-%{version}.tar.gz
BuildRoot:    %{_tmppath}/%{name}-%{version}-build
Prereq:       /sbin/ldconfig


Requires: mono-core >= 1.0
Requires: gtk2 >= 2.8
BuildRequires: mono-devel >= 1.0
BuildRequires: gtk2-devel >= 2.8


%description
Gnome Subtitles is a subtitle editor for the GNOME Desktop. It supports the most common
subtitle formats and allows for subtitle editing, conversion and synchronization.


%prep
%setup -q


%build
%configure
make


%install
make DESTDIR=$RPM_BUILD_ROOT install


%clean
rm -rf $RPM_BUILD_ROOT


%post
/sbin/ldconfig

%postun
/sbin/ldconfig


%files
%defattr(-,root,root)
%{_bindir}
%{_libdir}/%{name}
%{_datadir}/applications/%{name}.desktop
%{_datadir}/pixmaps/%{name}.svg
%doc README NEWS


%changelog
* Sun Oct 22 2006 - Henrique Malheiro <henrique.malheiro@gmail.com>
- First version.
