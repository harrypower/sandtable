#! /usr/local/bin/gforth-arm
\ config-uart.fs

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
\
\    beaglebone black uart pin configuration via cli

warnings off
:noname ; is bootmessage

s\" config-pin p9.24 uart\n" system
s\" config-pin p9.26 uart\n" system
\ this is /dev/ttyO1

s\" config-pin p9.21 uart\n" system
s\" config-pin p9.22 uart\n" system
\ this is /dev/ttyO2

." /dev/ttyO1 on P9 pin 24 and 26 now configured for uart! " cr
." /dev/ttyO2 on P9 pin 21 and 22 now configured for uart! " cr

bye
