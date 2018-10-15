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
\ 0x00 %100                               mymotorx putreg . .
( GCONF with en_pwm-mode 1 )
\ 0x10 %01110001111100000000              mymotorx putreg . .
( IHOLD 0, IRUN %11110, IHOLDELAY %111)
\ 0x11 0                                  mymotorx putreg . .
( TPOWER DOWN )
\ 0x13 0                                  mymotorx putreg . .
( TPWMTHRS )
\ 0x14 0                                  mymotorX putreg . .
( TCOOLTHRS )
\ 0x15 0                                  mymotorX putreg . .
( THIGH )
\ 0x6c %00010000000000101000000010010011  mymotorx putreg . .
( diss2g 0, dedge 0, intpol 1, mres %0000, sync 0, vhighchm 0, vhighfs 0, vsense 1, TBL %01, chm 0, rndtf 0, disfdcc 0, TFD 0, HEND 1, HSTRT 1,TOFF %11 )
\ 0x6d %1000000000000000000000000         mymotorX putreg . .
( sfilt 0,sgt %0 , seimin 0, sedn 00, semax 0, seup 00, semin 0)
\ 0x70 %0111000000101011111111            mymotorx putreg . .
( freewheel %01 ,pwm_symmetric %1 ,pwm_autoscale %1 ,PWM freq %00 ,PWM_GRAD %1010 ,PWM_AMPL %11111111 )

\ GCONF uIHOLD_IRUN uTPOWERDOWN uTPWMTHRS uTCOOLTHRS uTHIGH uCHOPCONF   uCOOLCONF                  uPWMCONF
%100 %01110001111100000000 1 0    0 0 %00010000000000101000000010010011 0                          %0111000000101011111111
0 mymotorX quickreg!
0 mymotorX usequickreg

%100 %01110001111100000000 1 1000 0 0 %00010001000000101000000010010011 %1000000000000000000000000 %0111100000101011111111
1 mymotorX quickreg!

%100 %01110001000000000000 1 1000 0 0 %00010001000000101000000010010011 %1000000000000000000000000 %0111100000101011111111
2 mymotorX quickreg!

%100 %01110000100000000000 1 1000 0 0 %00010001000000101000000010010011 %1000000000000000000000000 %0111100000101011111111
3 mymotorX quickreg!

%100 %01110000100000000000 1 1000 0 0 %00010001000000101000000010010011 0 %0111100000101011111111
4 mymotorX quickreg!

%100 %01110000010100000000 1 1000 0 0 %00010001000000101000000010010011 %1000000000000000000000000 %0111100000101011111111
5 mymotorX quickreg!

%100 %01110001111100000000 1 0    0 0 %00110000000000101000000010010011 0                          %0111000000101011111111
6 mymotorX quickreg!

%100 %01110001000000000000 1 1000 0 0 %00110001000000101000000010010011 %1000000000000000000000000 %0111100000101011111111
7 mymotorX quickreg!

mymotorX disable-motor
1 mymotorx setdirection

%100 %01110001111100000000 1 0    0 0 %00010000000000101000000010010011 0                          %0111000000101011111111
0 mymotory quickreg!
0 mymotory usequickreg

%100 %01110001111100000000 1 1000 0 0 %00010001000000101000000010010011 %1000000000000000000000000 %0111100000101011111111
1 mymotory quickreg!

%100 %01110001000000000000 1 1000 0 0 %00010001000000101000000010010011 %1000000000000000000000000 %0111100000101011111111
2 mymotory quickreg!

%100 %01110000100000000000 1 1000 0 0 %00010001000000101000000010010011 %1000000000000000000000000 %0111100000101011111111
3 mymotory quickreg!

mymotory disable-motor
1 mymotory setdirection

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

: sgt ( utime usteps -- )
  mymotorX enable-motor
  mymotorX timedsteps
  mymotorX [bind] tmc2130 print
  1000 ms cr
  mymotorX [bind] tmc2130 print
;

0 constant xm
1 constant ym

: varxysteps ( utime usteps uxy -- )
  case
    xm of
      mymotorx enable-motor
      mymotorX timedsteps
      mymotorX disable-motor
    endof
    ym of
      mymotory enable-motor
      mymotory timedsteps
      mymotory disable-motor
    endof
    endcase ;

: xysgt ( utime usteps uxy -- )
  case
    xm of
      mymotorX enable-motor
      mymotorX timedsteps
      mymotorX [bind] tmc2130 print
    endof
    ym of
      mymotory enable-motor
      mymotory timedsteps
      mymotory [bind] tmc2130 print
    endof
  endcase ;
