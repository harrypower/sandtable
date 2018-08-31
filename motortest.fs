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
tmc2130 heap-new constant mymotorX throw
mymotorX disable-motor

1 %100000000000000000 1 %1000000000000000 1 %100000000000000 0
tmc2130 heap-new constant mymotorY throw
mymotorY disable-motor

\ this is from tmc2130 datasheet pdf.  page 84 section 23.1 initialization example
\ 0x6c 0x000100c3 mymotorx putreg . .
\ 0x10 0x00061f0a mymotorX putreg . .
\ 0x11 0x0000000a mymotorX putreg . .
\ 0x00 0x00000004 mymotorX putreg . .
\ 0x13 0x000001f4 mymotorX putreg . .
\ 0x70 0x000401c8 mymotorX putreg . .

\ my attempt at settings i need for x axis motor
0x6c %00000001000000010110000010010000  mymotorx putreg . .
0x10 %00100001000000000000              mymotorx putreg . .
0x10 %00001000                          mymotorx putreg . .
0x00 0x00000004                         mymotorx putreg . .
0x12 0x0                                mymotorx putreg . .
0x13 0x000001f4                         mymotorx putreg . .
0x70 %0101000000000100000010            mymotorx putreg . .

mymotorX enable-motor

\ 1000 mymotorX faststeps
