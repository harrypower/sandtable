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

\ require config-pins.fs
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
." x and y motor spi data pins configured!" cr

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
   ( %00000000000000010000000011000011)
\ 0x10 0x00061f0a mymotorX putreg . .
\ 0x11 0x0000000a mymotorX putreg . .
\ 0x00 0x00000004 mymotorX putreg . .
\ 0x13 0x000001f4 mymotorX putreg . .
\ 0x70 0x000401c8 mymotorX putreg . .

\ my attempt at settings i need for x axis motor
0x6c %00010000000011001000000000000010  mymotorx putreg . .
( diss2g 0, dedge 0, intpol 1, mres %0000, sync 0, vhighchm 1, vhighfs 1, vsense 0, TBL %01, chm 0, rndtf 0, disfdcc 0, TFD 0, HEND 0, HSTRT 0,TOFF %10)
0x10 %00010001000000000000              mymotorx putreg . .
( IHOLD 0, IRUN %10000, IHOLDELAY 1)
0x11 %00000100                          mymotorx putreg . .
( TPOWER DOWN )
0x00 %100                               mymotorx putreg . .
( GCONF with en_pwm-mode 1 )
0x13 %100                               mymotorx putreg . .
( TPWMTHRS )
0x70 %0101100000010010000000            mymotorx putreg . .
( freewheel %01 ,pwm_symmetric %0 ,pwm_autoscale %1 ,PWM freq %10 ,PWM_GRAD %00000100 ,PWM_AMPL %10000000 )
mymotorX disable-motor
1 mymotorx setdirection

: test ( -- )
  mymotorx enable-motor
  1 mymotorX setdirection
  50 mymotorX faststeps
  mymotorX disable-motor ;

: xsteps ( usteps -- )
  mymotorX enable-motor
  mymotorX faststeps
  mymotorX disable-motor ;

: dataset { nreg nnewdata nstartbit nwipebits -- uspi_status nflag } \ note this only works on r/w registers... aka GCONF and CHOPCONF
  1 nwipebits 0 ?do 1 i lshift or loop
  nstartbit lshift invert to nwipebits
  nnewdata nstartbit lshift to nnewdata
  nreg mymotorX getreg throw swap drop
  nwipebits and nnewdata or
  nreg swap mymotorX putreg ;

: varxsteps ( utime usteps -- )
  mymotorx enable-motor
  mymotorX timedsteps
  mymotorX disable-motor ;
