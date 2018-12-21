\ sandmotorapi.fs

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

\ configures x and y motors and can control motors

\ Requires:
\ config-pins.fs
\ tmc2130.fs
\	syscalls386.fs
\ BBB_GPIO_lib.fs
\ objects.fs
\ mdca-obj.fs

\ Revisions:
\ 11/09/2018 started coding
\ 19/12/2018 calibration simplified but needs to have realtime standard deviation impimented to detect bad calibration
\ - movetoxy has issues yet with speed and calculation errors... need to come up with better idea here

require tmc2130.fs

0 value xmotor
0 value ymotor
true value configured?  \ true means not configured false means configured
false value homedone?   \ false means table has not been homed true means table was homed succesfully
0 constant xm
1 constant ym
100 value xylimit \ used to find home
1500 constant stopbuffer
0 constant xm-min
0 constant ym-min
276000 constant xm-max
276000 constant ym-max
true value xposition  \ is the real location of x motor .. note if value is true then home position not know so x is not know yet
true value yposition  \ is the real location of y motor .. note if value is true then home position not know so y is not know yet
1200 value silentspeed  \ loop wait amount for normal silent operation .... 500 to 3000 is operating range
900 value calspeed
2000 value calsteps
10 value steps

: configure-stuff ( -- nflag ) \ nflag is false if configuration happened other value if some problems
  s" /home/debian/sandtable/config-pins.fs" system $? to configured?
  configured? 0 = if
    1 %10000000000000000 1 %10000000000000 1 %1000000000000 1
    tmc2130 heap-new to xmotor throw
    xmotor disable-motor
    1 %100000000000000000 1 %1000000000000000 1 %100000000000000 0
    tmc2130 heap-new to ymotor throw
    ymotor disable-motor

    \ GCONF uIHOLD_IRUN uTPOWERDOWN uTPWMTHRS uTCOOLTHRS uTHIGH uCHOPCONF   uCOOLCONF                  uPWMCONF
    %100 %01110001111100000000 1 0    0 0 %00110000000000101000000010010011 0                          %0111000000101011111111
    0 xmotor quickreg!
    %100 %00000000011000000000 1 1000 0 0 %00110001000000101000000010010011 %1000000000000000000000000 %0111100000101011111111
    1 xmotor quickreg!
    1 xmotor usequickreg
    1 xmotor setdirection

    %100 %01110001111100000000 1 0    0 0 %00110000000000101000000010010011 0                          %0111000000101011111111
    0 ymotor quickreg!
    %100 %00000000011000000000 1 1000 0 0 %00110001000000101000000010010011 %1000000000000000000000000 %0111100000101011111111
    1 ymotor quickreg!
    1 ymotor usequickreg
    1 ymotor setdirection
  then
  configured? ;

\ ************************ These following words are for normal speed movement only and as such are silent
: movetox { ux -- nflag } \ move to x position on table nflag is true if the move is executed and false if the move was not possible for some reason
  configured? false = homedone? true = xposition true <> and and
  if \ only do steps if all configured and home is know
    xm-max ux >= xm-min ux <= and
    if
      xmotor enable-motor
      0 xmotor usequickreg
      xposition ux >
      if
        0 xmotor setdirection
        silentspeed xposition ux - xmotor timedsteps
      else
        1 xmotor setdirection
        silentspeed ux xposition - xmotor timedsteps
      then
      ux to xposition
      xmotor disable-motor
      true
    else
      false
    then
  else
    false
  then ;

: movetoy { uy -- nflag } \ move to y position on table nflag is true if the move is executed and false if the move was not possible for some reason
  configured? false = homedone? true = yposition true <> and and
  if \ only do steps if all configured and home is know
    ym-max uy >= ym-min uy <= and
    if
      ymotor enable-motor
      0 ymotor usequickreg
      yposition uy >
      if
        0 ymotor setdirection
        silentspeed yposition uy - ymotor timedsteps
      else
        1 ymotor setdirection
        silentspeed uy yposition - ymotor timedsteps
      then
      uy to yposition
      ymotor disable-motor
      true
    else
      false
    then
  else
    false
  then ;

: movetoxy ( ux uy -- nflag ) \ move to the x and y location at the same time ... nflag is true if the move is executed and false if the move was not possible
  0e 0e { ux uy F: mslope F: bintercept }
  ux xposition = if uy movetoy exit then
  uy yposition = if ux movetox exit then
  configured? false = homedone? true = yposition true <> xposition true <> and and and
  if \ only do steps if all configured and home is know
    ym-max uy >= ym-min uy <= and xm-max ux >= xm-min ux <= and and
    if
      ymotor enable-motor xmotor enable-motor
      0 ymotor usequickreg 0 xmotor usequickreg
      xposition ux > if 0 else 1 then xmotor setdirection
      yposition uy > if 0 else 1 then ymotor setdirection
      yposition uy - s>f
      xposition ux - s>f
      f/ to mslope
      yposition s>f mslope xposition s>f f* f- to bintercept
      ux xposition >
      if
        ux 1 + xposition do
          silentspeed
          steps xmotor timedsteps i to xposition
          mslope i s>f f* bintercept f+ f>s dup dup yposition <>
          if
            yposition - abs silentspeed
            swap ymotor timedsteps to yposition
          else
            drop drop
          then
        steps +loop
      else
        ux 1 - xposition -do
          silentspeed
          steps xmotor timedsteps i to xposition
          mslope i s>f f* bintercept f+ f>s dup dup yposition <>
          if
            yposition - abs silentspeed
            swap ymotor timedsteps to yposition
          else
            drop drop
          then
        steps -loop
      then
      ymotor disable-motor xmotor disable-motor
      true \ move done
    else false \ not in bounds
    then
  else false \ not configured or home yet
  then ;

\ ************************   these following words are for home position use only not for normal movement use above words for that
: xyget-sg_result ( uxym -- usgr )
  case
  xm of
    DRV_STATUS xmotor getreg throw swap drop
  endof
  ym of
    DRV_STATUS ymotor getreg throw swap drop
  endof
  endcase
  %1111111111 and ;

: calxysteps ( utime usteps uxy -- )  \ this is to be used by home position code below use movetox or movetoy for normal motion
 case
   xm of
     xmotor timedsteps
   endof
   ym of
     ymotor timedsteps
   endof
 endcase ;

: docalxybase { uxy -- uresult } \ uxy is the motor x or y ... uresult is stallguard value after moving motor
  calspeed calsteps uxy calxysteps
  uxy xyget-sg_result ;

: calxybase ( uxy -- uavg nflag ) \ uxy is the motor to get infor from ... uavg is the stallGuard average .. nflag is true if success false is bad base readings
  0 0 { uavg usd } \ uavg is average .. usd is standard deviation
  case
  xm of
    1 xmotor usequickreg
    1 xmotor setdirection
    5 0 do calspeed calsteps xm calxysteps loop
    0 xmotor setdirection
    xm docalxybase to uavg
    uavg . ."  0 base test" cr
    4 0 do
    xm docalxybase dup uavg + 2 / to uavg
      . space i 1 + . ."  base test" cr
    loop
  endof
  ym of
    1 ymotor usequickreg
    1 ymotor setdirection
    5 0 do calspeed calsteps ym calxysteps loop
    0 ymotor setdirection
    ym docalxybase to uavg
    uavg . ."  0 base test" cr
    4 0 do
      ym docalxybase dup uavg + 2 / to uavg
      . space i 1 + . ."  base test" cr
    loop
  endof
  endcase
  uavg true
  uavg . ."  base avg!" cr
  ;

: calxhome ( -- nflag )
  xmotor enable-motor
  xm calxybase drop drop \ warm up motor first
  xm calxybase swap xylimit + { ubase }
  if \ now find home
   0 xmotor setdirection
   begin
     calspeed calsteps  xm calxysteps
     xm xyget-sg_result dup . ." x reading " ubase dup . ." x ubase " cr >
     \ xm xyget-sg_result ubase >
   until
   true \ now at start edge
   0 xmotor usequickreg
   1 xmotor setdirection
   silentspeed stopbuffer xm calxysteps \ moves a small distance from home stop position
   0 to xposition
  else
   false \ test not stable
   true to xposition \ this means xposition is not know because of home failure
  then
  xmotor disable-motor ;

: calyhome ( -- nflag )
  ymotor enable-motor
  ym calxybase drop drop \ warm up motor first
  ym calxybase swap xylimit + { ubase }
  if \ now find home
   0 ymotor setdirection
   begin
     calspeed calsteps  ym calxysteps
     ym xyget-sg_result dup . ." y reading " ubase dup . ." y ubase " cr >
     \ ym xyget-sg_result ubase >
   until
   true \ now at start edge
   0 ymotor usequickreg
   1 ymotor setdirection
   silentspeed stopbuffer ym calxysteps \ moves a small distance from home stop position
   0 to yposition
  else
   false \ test not stable
   true to yposition \ this means yposition is not know because of home failure
  then
  ymotor disable-motor ;


\ : calxhome ( -- nflag )
\ ;
\ : calyhome ( -- nflag )
\ ;

: xyhome ( -- nflag ) \ nflag is true if x and y are at home position and false if there was a falure of some kind
 calxhome
 if true else xmotor enable-motor 1 xmotor setdirection calspeed 10000 xm calxysteps xmotor disable-motor calxhome then
 calyhome
 if true else ymotor enable-motor 1 ymotor setdirection calspeed 10000 ym calxysteps ymotor disable-motor calyhome then
 and dup to homedone? ;

: dogohome ( -- nflag ) \ if nflag is true x and y motors are configured sent home if nflag is false something failed here
    configured? false = if xyhome else false then ;
\ ************************

: closedown ( -- )
  true to configured?  \ true means not configured false means configured
  false to homedone?   \ false means table has not been homed true means table was homed succesfully
  true to xposition  \ is the real location of x motor .. note if value is true then home position not know so x is not know yet
  true to yposition  \ is the real location of y motor .. note if value is true then home position not know so y is not know yet
  xmotor disable-motor
  ymotor disable-motor
  xmotor [bind] tmc2130 destruct
  ymotor [bind] tmc2130 destruct ;
