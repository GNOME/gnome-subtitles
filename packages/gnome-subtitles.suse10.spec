##
# spec file for package gnome-subtitles
#
# Copyright Pedro Castro <noup users.sourceforge.net> and
#     Damien Carbery <daymobrew users.sourceforge.net>
#
#
Name:         gnome-subtitles
License:      GPL
Group:        Productivity/Multimedia/Other
Version:      0.0.2
Release:      1
Distribution: SuSE
Summary:      Subtitle editor for the GNOME Desktop
Source:       http://easynews.dl.sourceforge.net/sourceforge/gsubtitles/gnome-subtitles-%{version}.tar.gz
URL:          http://gsubtitles.sourceforge.net/
BuildRoot:    %{_tmppath}/%{name}-%{version}-build
AutoReqProv:  on
Prereq:       /sbin/ldconfig


Requires: mono-core >= 1.0
Requires: gtk2 >= 2.8
Requires: gtk-sharp2 >= 2.8
Requires: glade-sharp2 >= 2.8
Requires: gnome-sharp2 >= 2.8
BuildRequires: mono-devel >= 1.0
BuildRequires: gtk2-devel >= 2.8
BuildRequires: gtk-sharp2 >= 2.8
BuildRequires: glade-sharp2 >= 2.8
BuildRequires: gnome-sharp2 >= 2.8


%description
Gnome Subtitles is a subtitle editor for the GNOME Desktop. It supports the most common
subtitle formats and allows for subtitle editing, conversion and synchronization.


%prep
%setup -q


%build
CFLAGS="$RPM_OPT_FLAGS"			\
./configure --prefix=%{_prefix}
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


%changelog
* Fri Oct 06 2006 - daymobrew users.sourceforge.net
- First version.

