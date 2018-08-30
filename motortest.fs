\ #! /usr/local/bin/gforth-arm
\ motortest.fs

\    Copyright (C) 2018  Philip King. Smith

\    This program is free software: you can redistribute it and/or modify
\    it under the terms of the GNU General Public License as published by
\    the Free Software Foundation, either version 3 of the License, or
\    (at your option) any later version.

\    This program is distributed in the hope that it will be useful,
\    but WITHOUT ANY WARRANTY; without even the implied warranty of
\    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\    GNU General Public License for more details.

\    You should have received a copy of the GNU General Public License
\    along with this program.  If not, see <http://www.gnu.org/licenses/>.

\ working on getting motors configured and working

\ Requires:
\ config-pins.fs
\ tmc2130.fs
\ objects.fs

\ Revisions:
\ 8/30/2018 started coding

require config-pins.fs
require tmc2130.fs
require Gforth-Objects/objects.fs


1 %10000000000000000 1 %10000000000000 1 %1000000000000 1
tmc2130 heap-new constant mymotorX \ throw
mymotorX disable-motor

1 %100000000000000000 1 %1000000000000000 1 %100000000000000 0
tmc2130 heap-new constant mymotorY \ throw
mymotorY disable-motor
