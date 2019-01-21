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
require realtimeMSD.fs
require Gforth-Objects/double-linked-list.fs

double-linked-list dict-new constant x-array-data
double-linked-list dict-new constant y-array-data
realtimeMSD dict-new constant xdata
realtimeMSD dict-new constant ydata
0 value xmotor
0 value ymotor
true value configured?  \ true means not configured false means configured
false value homedone?   \ false means table has not been homed true means table was homed succesfully
0 constant xm
1 constant ym
1600 value stopbuffer
0 constant xm-min
0 constant ym-min
275500 constant xm-max
275500 constant ym-max
1 constant forward
0 constant backward
true value xposition  \ is the real location of x motor .. note if value is true then home position not know so x is not know yet
true value yposition  \ is the real location of y motor .. note if value is true then home position not know so y is not know yet
1200 value silentspeed  \ loop wait amount for normal silent operation .... 500 to 3000 is operating range
20000 value xcalspeed
1 value xcalsteps
20000 value ycalspeed
1 value ycalsteps
75 value calstep-amounts
1.6e fvariable xcal-threshold-a xcal-threshold-a f!
2.1e fvariable xcal-threshold-b xcal-threshold-b f!
1.6e fvariable ycal-threshold-a ycal-threshold-a f!
2.1e fvariable ycal-threshold-b ycal-threshold-b f!
10 value steps
1 value xcalreg
1 value ycalreg
500 value calwait
21 value max-cal-test

: configure-stuff ( -- nflag ) \ nflag is false if configuration happened other value if some problems
  s" /home/debian/sandtable/config-pins.fs" system $? to configured?
  configured? 0 = if
    1 %10000000000000000 1 %10000000000000 1 %1000000000000 1
    tmc2130 heap-new to xmotor abort" xmotor did not construct"
    xmotor disable-motor
    1 %100000000000000000 1 %1000000000000000 1 %100000000000000 0
    tmc2130 heap-new to ymotor abort" ymotor did not construct"
    ymotor disable-motor

    \ GCONF uIHOLD_IRUN uTPOWERDOWN uTPWMTHRS uTCOOLTHRS uTHIGH uCHOPCONF   uCOOLCONF                  uPWMCONF
    %100 %01110001111100000000 1 0    0 0 %00110000000000101000000010010011 0                          %0111000000101011111111
    0 xmotor quickreg!
    %100 %00000000001100000011 1 1000 0 0 %00011000000000001000000010010011 %1000000000000000000000000 %0001000000111111111111
    1 xmotor quickreg!
    1 xmotor usequickreg
    forward xmotor setdirection
    %100 %00000000001100000011 1 1000 0 0 %00011000000000001000000010010011 %0000000000000000000000000 %0111100000101011111111
    2 xmotor quickreg!
    %100 %00000000001100000011 1 1000 0 0 %00011000000000001000000010010011 %1000111100000000000000000 %0001000000111111111111
    3 xmotor quickreg!
    %100 %00000000001100000011 1 1000 0 0 %00011000000000001000000010010011 %1000101000000000000000000 %0101000000111111111111
    4 xmotor quickreg!
    %100 %00000000001100000011 1 1000 0 0 %00011000000000001000000010010011 %1000010100000000000000000 %0001000000111111111111
    5 xmotor quickreg!
    %100 %00000000001100000011 1 1000 0 0 %00011000000000001000000010010011 %1000000000000000000000000 %0001000000111111111111
    6 xmotor quickreg!

    %100 %01110001111100000000 1 0    0 0 %00110000000000101000000010010011 0                          %0111000000101011111111
    0 ymotor quickreg!
    %100 %00000000001100000011 1 1000 0 0 %00011000000000001000000010010011 %1000000000000000000000000 %0001000000111111111111
    1 ymotor quickreg!
    1 ymotor usequickreg
    forward ymotor setdirection
    %100 %00000000001100000011 1 1000 0 0 %00011000000000001000000010010011 %0000000000000000000000000 %0111100000101011111111
    2 ymotor quickreg!
    %100 %00000000001100000011 1 1000 0 0 %00011000000000001000000010010011 %1000111100000000000000000 %0001000000111111111111
    3 ymotor quickreg!
    %100 %00000000001100000011 1 1000 0 0 %00011000000000001000000010010011 %1000101000000000000000000 %0101000000111111111111
    4 ymotor quickreg!
    %100 %00000000001100000011 1 1000 0 0 %00011000000000001000000010010011 %1000010100000000000000000 %0001000000111111111111
    5 ymotor quickreg!
    %100 %00000000001100000011 1 1000 0 0 %00011000000000001000000010010011 %1000000000000000000000000 %0001000000111111111111
    6 ymotor quickreg!
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
        backward xmotor setdirection
        silentspeed xposition ux - xmotor timedsteps
      else
        forward xmotor setdirection
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
        backward ymotor setdirection
        silentspeed yposition uy - ymotor timedsteps
      else
        forward ymotor setdirection
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
\ also note that these home position words do not check if sandtable is configured so only use the main word to calibrate the sandtable

: xyget-sg_result ( uxym -- usgr )  \ get result of stall guard readings
  case
  xm of
    DRV_STATUS xmotor getreg abort" xmotor DRV_STATUS failed" swap drop
  endof
  ym of
    DRV_STATUS ymotor getreg abort" ymotor DRV_STATUS failed" swap drop
  endof
  endcase
  %1111111111 and ;

: xyget-MSCURACT ( uxym -- nCUR_A nCUR_B ) \ uxym is a motor ym or xm.  this will return the microstep for motor for each phase
  case
  xm of
    MSCURACT xmotor getreg abort" xmotor MSCURACT failed" swap drop
  endof
  ym of
    MSCURACT xmotor getreg abort" ymotor MSCURACT failed" swap drop
  endof
  endcase
  dup %111111111 and swap
  %1111111110000000000000000 and 16 rshift
;
: xyget-LOST_STEPS ( uxym -- ulost ) \ uxym is a motor ym or xm.  ulost is the value returned indicating lost steps
  case
  xm of
    LOST_STEPS xmotor getreg abort" xmotor LOST_STEPS failed" swap drop
  endof
  ym of
    LOST_STEPS ymotor getreg abort" ymotor LOST_STEPS failed" swap drop
  endof
  endcase ;
: xyget-MSCNT ( uxym -- uMSCNT )  \ uxym is motor ym or xm... MSCNT is the register in tmc2130 that says the actual phase of motor
  case
  xm of
    MSCNT xmotor getreg abort" xmotor MSCNT read failed" swap drop
  endof
  ym of
    MSCNT ymotor getreg abort" ymotor MSCNT read failed" swap drop
  endof
  endcase ;

: xysteps { uquickreg udirection ucalspeed ucalsteps uxy -- uresult } \ simply step motor based on this info then return stall guard result
  configured? false = if
    uxy case
      xm of
        uquickreg xmotor usequickreg
        udirection xmotor setdirection
        ucalspeed ucalsteps xmotor timedsteps
        xm xyget-sg_result
      endof
      ym of
        uquickreg ymotor usequickreg
        udirection ymotor setdirection
        ucalspeed ucalsteps ymotor timedsteps
        ym xyget-sg_result
      endof
    endcase
  else 0 then ;

: ndosteps { uquickreg udirection ucalspeed ucalsteps uloop uxy -- umean usd }
  configured? false = if
    uxy case xm of xmotor enable-motor endof ym of ymotor enable-motor endof endcase
    uxy case xm of xdata endof ym of ydata endof endcase
    [bind] realtimeMSD construct
    uloop 0 do
      uquickreg udirection ucalspeed ucalsteps uxy xysteps
      uxy case xm of xdata endof ym of ydata endof endcase n>data
    loop
    uxy case xm of xmotor disable-motor endof ym of ymotor disable-motor endof endcase
    uxy case xm of xdata endof ym of ydata endof endcase
    dup nmean@ swap nsdp@
  else 0 0
  then
;


: nstep-list { uquickreg udirection ucalspeed ucalsteps uloop uxy -- uxydll }
  configured? false = if
    uxy case xm of xmotor enable-motor endof ym of ymotor enable-motor endof endcase
    uxy case xm of x-array-data endof ym of y-array-data endof endcase
    [bind] double-linked-list construct
    uloop 0 do
      uquickreg udirection ucalspeed ucalsteps uxy xysteps
      uxy case xm of x-array-data endof ym of y-array-data endof endcase ll-cell!
    loop
    uxy case xm of xmotor disable-motor endof ym of ymotor disable-motor endof endcase
    uxy case xm of x-array-data endof ym of y-array-data endof endcase
  else 0
  then ;

: listmotordata { udatalist -- }
  udatalist ll-set-start
  begin udatalist ll-cell@ . udatalist ll> until ;

: xedgedetect ( usd umean utestsd utestmean -- nflag ) \ looks for edge conditions ... nflag is true if edge found or false if not found
  { usd umean utestsd utestmean }
  utestmean usd s>f xcal-threshold-a f@ f* f>s umean + >
  umean usd s>f xcal-threshold-a f@ f* f>s - 0 < if utestmean 0 = else utestmean umean usd s>f xcal-threshold-a f@ f* f>s - < then or
  utestsd usd s>f xcal-threshold-b f@ f* f>s > and ;
: yedgedetect ( usd umean utestsd utestmean -- nflag ) \ looks for edge conditions ... nflag is true if edge found or false if not found
  { usd umean utestsd utestmean }
  utestmean usd s>f ycal-threshold-a f@ f* f>s umean + >
  umean usd s>f ycal-threshold-a f@ f* f>s - 0 < if utestmean 0 = else utestmean umean usd s>f ycal-threshold-a f@ f* f>s - < then or
  utestsd usd s>f ycal-threshold-b f@ f* f>s > and ;

: doxycalibrate ( uxy -- nflag ) \ uxy is ym or xm ... nflag is false for calibration failed and true for calibration passed
  0 0 0 { uxy umean usd maxloops }
  configured? false = if
    uxy case
      xm of
        begin
          xcalreg forward xcalspeed xcalsteps calstep-amounts 2 * xm ndosteps 2drop
          calwait ms
          xcalreg backward xcalspeed xcalsteps calstep-amounts 2 * xm ndosteps to usd to umean
          usd . ." x usd " umean . ." x umean  #1" cr
          calwait ms
          usd umean xcalreg backward xcalspeed xcalsteps calstep-amounts xm ndosteps swap
          .s ." x usd umean testsd testmean " maxloops . ." maxloops" cr
          xedgedetect if
            calwait ms
            xcalreg forward xcalspeed xcalsteps calstep-amounts 2 * xm ndosteps 2drop
            calwait ms
            xcalreg backward xcalspeed xcalsteps calstep-amounts 2 * xm ndosteps to usd to umean
            usd . ." x usd " umean . ." x umean  #2" cr
            calwait ms
            usd umean xcalreg backward xcalspeed xcalsteps calstep-amounts xm ndosteps swap
            .s ." x usd umean testsd testmean " cr
            xedgedetect if
              calwait ms
              xcalreg forward xcalspeed xcalsteps calstep-amounts 2 * xm ndosteps 2drop
              calwait ms
              xcalreg backward xcalspeed xcalsteps calstep-amounts 2 * xm ndosteps to usd to umean
              usd . ." x usd " umean . ." x umean  #3" cr
              calwait ms
              usd umean xcalreg backward xcalspeed xcalsteps calstep-amounts xm ndosteps swap
              .s ." x usd umean testsd testmean final" cr
              xedgedetect
            else
              false
            then          else
            false
          then
          maxloops 1 + dup to maxloops max-cal-test >= or
        until
        maxloops max-cal-test >= if 10 throw else true then \ edge not detected for x axis calibration failed!
        \ note at this point if the edge is not found here redo the above loop but the maxloops needs to be preserved and continued
      endof
      ym of
        begin
          ycalreg forward ycalspeed ycalsteps calstep-amounts 2 * ym ndosteps 2drop
          calwait ms
          ycalreg backward ycalspeed ycalsteps calstep-amounts 2 * ym ndosteps to usd to umean
          usd . ." y usd " umean . ." y umean #1" cr
          calwait ms
          usd umean ycalreg backward ycalspeed ycalsteps calstep-amounts ym ndosteps swap
          .s ." y usd umean testsd testmean " maxloops . ." maxloops" cr
          yedgedetect if
            calwait ms
            ycalreg forward ycalspeed ycalsteps calstep-amounts 2 * ym ndosteps 2drop
            calwait ms
            ycalreg backward ycalspeed ycalsteps calstep-amounts 2 * ym ndosteps to usd to umean
            usd . ." y usd " umean . ." y umean #2" cr
            calwait ms
            usd umean ycalreg backward ycalspeed ycalsteps calstep-amounts ym ndosteps swap
            .s ." y usd umean testsd testmean " cr
            yedgedetect if
              calwait ms
              ycalreg forward ycalspeed ycalsteps calstep-amounts 2 * ym ndosteps 2drop
              calwait ms
              ycalreg backward ycalspeed ycalsteps calstep-amounts 2 * ym ndosteps to usd to umean
              usd . ." y usd " umean . ." y umean #3" cr
              calwait ms
              usd umean ycalreg backward ycalspeed ycalsteps calstep-amounts ym ndosteps swap
              .s ." y usd umean testsd testmean final" cr
              yedgedetect
            else
              false
            then
          else
            false
          then
          maxloops 1 + dup to maxloops max-cal-test >= or
        until
        maxloops max-cal-test >= if 11 throw else true then \ edge not detected for y axis calibration failed!
        \ note at this point if the edge is not found here redo the above loop but the maxloops needs to be preserved and continued
      endof
    endcase
  else false then ;

: xwarmup ( -- )
  xmotor enable-motor
  xcalreg forward xcalspeed xcalsteps xm xysteps drop
  30000 ms
  xmotor disable-motor ;
: ywarmup ( -- )
  ymotor enable-motor
  ycalreg forward ycalspeed ycalsteps ym xysteps drop
  30000 ms
  ymotor disable-motor ;

: dohome ( -- nflag ) \ find x and y home position ... nflag is true if calibration is done.   nflag is false for or other value for a calibration failure
  try
    configured? false = if
    \  xwarmup
      xm doxycalibrate if
        xmotor enable-motor
        0 forward silentspeed stopbuffer xm xysteps drop \ moves a small distance from home stop position
        xmotor disable-motor
        xm-min to xposition
        true
      else false
      then
    \  ywarmup
      ym doxycalibrate if
        ymotor enable-motor
        0 forward silentspeed stopbuffer ym xysteps drop \ moves a small distance from home stop position
        ymotor disable-motor
        ym-min to  yposition
        true
      else false
      then
      and
    else
        false
    then
  restore dup true = if true to homedone? else false to homedone? true to xposition true to yposition then
  endtry ;

: closedown ( -- )
  true to configured?  \ true means not configured false means configured
  false to homedone?   \ false means table has not been homed true means table was homed succesfully
  true to xposition  \ is the real location of x motor .. note if value is true then home position not know so x is not know yet
  true to yposition  \ is the real location of y motor .. note if value is true then home position not know so y is not know yet
  xmotor disable-motor
  ymotor disable-motor
  xmotor [bind] tmc2130 destruct
  ymotor [bind] tmc2130 destruct ;

: testcal 0 do cr dohome . 275000 dup movetoxy . loop ;
