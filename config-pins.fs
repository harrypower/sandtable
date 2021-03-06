#! /usr/local/bin/gforth-arm
\ config-pins.fs

\    Copyright (C) 2018  Philip King Smith

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
\    beaglebone black uart pin configuration via cli for tmc2130.fs driver communication and motor use 

warnings off
:noname ; is bootmessage
\ *****************************************************************
\ this is for uart use on tmc2208 devices
\ s\" config-pin p9.24 uart\n" system
\ TXD
\ s\" config-pin p9.26 uart\n" system
\ RXD
\ this is /dev/ttyO1
\ used for x motor

\ s\" config-pin p9.21 uart\n" system
\ TXD
\ s\" config-pin p9.22 uart\n" system
\ RXD
\ this is /dev/ttyO2
\ used for y motor
\ ." /dev/ttyO1 on P9 pin 24 and 26 now configured for uart! " cr
\ ." /dev/ttyO2 on P9 pin 21 and 22 now configured for uart! " cr
\ *****************************************************************
\ this is for spi use on tmc2130 devices
s\" config-pin p9.28 spi_cs\n"  system \ ( cs )
s\" config-pin p9.29 spi\n"     system \ ( D0 )
s\" config-pin p9.30 spi\n"     system \ ( D1 )
s\" config-pin p9.31 spi_sclk\n" system \ ( clock )
\ this is spi1 used for x motor
s\" config-pin p9.17 spi_cs\n"  system \ ( cs )
s\" config-pin p9.18 spi\n"     system \ ( D1 )
s\" config-pin p9.21 spi\n"     system \ ( D0 )
s\" config-pin p9.22 spi_sclk\n" system \ ( clock )
\ this is spi0 used for y motor
\ ." x and y motor spi data pins configured!" cr

s\" config-pin p8.11 output\n" system
\ x stepper dir  ( gpio1_13)
s\" config-pin p8.12 output\n" system
\ x stepper step ( gpio1_12)
s\" config-pin p9.15 output\n" system
\ x stepper enable ( gpio1_16)

s\" config-pin p8.15 output\n" system
\ y stepper dir  ( gpio1_15)
s\" config-pin p8.16 output\n" system
\ y stepper step ( gpio1_14)
s\" config-pin p9.23 output\n" system
\ y stepper enable ( gpio1_17)
\ *****************************************************************
\ ." x and y stepper motor dir,step and enable pins configured!" cr

bye
