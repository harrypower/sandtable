#! /usr/local/bin/gforth-arm

warnings off
:noname ; is bootmessage

require Gforth-Objects/objects.fs
require Gforth-Objects/stringobj.fs
require BBB_Gforth_gpio/syscalls386.fs
require tmc2130.fs

string heap-new constant mytemppad$

: #to$ ( n -- c-addr u1 ) \ convert n to string
    s>d swap over dabs <<# #s rot sign #> #>> mytemppad$ !$ mytemppad$ @$ ;

0 value xmotor
0 value ymotor
true value configured
: configure-stuff ( -- nflag ) \ nflag is false if configuration happened other value if some problems
  s" /home/debian/sandtable/config-pins.fs" system $? to configured
  configured 0 = if
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
  configured ;

0 value fid

: running? ( -- nflag ) \ nflag is false if this process is already running any other value if it is not running.
  s" /run/hometest.pid" file-status swap drop ;

: setrunning ( -- )
   s" /run/hometest.pid" r/w create-file throw to fid
   false fid = if
      getpid #to$
      fid write-file throw
      fid flush-file throw
      fid close-file throw
   then ;

0 constant xm
1 constant ym
100 constant xylimit

: xyget-sg_result ( uxm -- usgr )
  case
  xm of
    DRV_STATUS xmotor getreg throw swap drop
  endof
  ym of
    DRV_STATUS ymotor getreg throw swap drop
  endof
  endcase
  %1111111111 and ;

: xysteps ( utime usteps uxy -- )
 case
   xm of
     xmotor timedsteps
   endof
   ym of
     ymotor timedsteps
   endof
   endcase ;

: xybase ( uxy -- uavg nflag ) \ uxy is motor to test with .. uavg is base average to use to find end with nflag is true for good test false for bad test
 0 0 { uf ub }
 case
 xm of
   1 xmotor usequickreg
   1 xmotor setdirection
   950 1000 xm xysteps
   xm xyget-sg_result to uf
   0 xmotor setdirection
   950 1000 xm xysteps
   xm xyget-sg_result to ub
   uf ub - xylimit >  \ forward end ?
   ub uf - xylimit >  \ backward end ?
   or if \ repeat in other order
       950 1000 xm xysteps
       xm xyget-sg_result to ub
       1 xmotor setdirection
       950 1000 xm xysteps
       xm xyget-sg_result to uf
       uf ub - xylimit > \ bad testing results possible
       ub uf - xylimit >
       or if 0 false \ return bad test results
       else uf ub + 2 / true then
   else \ good results return
     uf ub + 2 / true \ return address with good test results
   then
 endof
 ym of
   1 ymotor usequickreg
   1 ymotor setdirection
   950 1000 ym xysteps
   ym xyget-sg_result to uf
   0 ymotor setdirection
   950 1000 ym xysteps
   ym xyget-sg_result to ub
   uf ub - xylimit >  \ forward end ?
   ub uf - xylimit >  \ backward end ?
   or if \ repeat in other order
       950 1000 ym xysteps
       ym xyget-sg_result to ub
       1 ymotor setdirection
       950 1000 ym xysteps
       ym xyget-sg_result to uf
       uf ub - xylimit > \ bad testing results possible
       ub uf - xylimit >
       or if 0 false \ return bad test results
       else  uf ub + 2 / true then
   else \ good results return
     uf ub + 2 / true \ return address with good test results
   then
 endof
 endcase ;

: xhome ( -- nflag )
 xmotor enable-motor
 xm xybase swap xylimit + { ubase }
 if \ now find home
   0 xmotor setdirection
   begin
     950 1000 xm xysteps
     \ xm xyget-sg_result dup . ." x reading " ubase dup . ." x ubase " cr >
     xm xyget-sg_result ubase >
   until
   true \ now at start edge
 else
   false \ test not stable
 then
 xmotor disable-motor ;

: yhome ( -- nflag )
 ymotor enable-motor
 ym xybase swap xylimit + { ubase }
 if \ now find home
   0 ymotor setdirection
   begin
     950 1000 ym xysteps
     \ ym xyget-sg_result dup . ." y reading " ubase dup . ." y ubase " cr >
     ym xyget-sg_result ubase >
   until
   true \ now at start edge
 else
   false \ test not stable
 then
 ymotor disable-motor ;

: xyhome ( -- nflag )
 xhome
 if true else xmotor enable-motor 1 xmotor setdirection 950 10000 xm xysteps xmotor disable-motor xhome then
 yhome
 if true else ymotor enable-motor 1 xmotor setdirection 950 10000 xm xysteps ymotor disable-motor yhome then
 and ;

: startup ( -- )
  running? false <> if
    setrunning
    configure-stuff
    false = if xyhome drop ( . ." < xyhome message " cr ) then
  then  ;

: closedown ( -- )
  mytemppad$ [bind] string destruct
  xmotor [bind] tmc2130 destruct
  ymotor [bind] tmc2130 destruct
  s" /run/hometest.pid" delete-file throw
 ;

: doall ( -- )
  try
    startup
    closedown
    false
  endtry drop ( . ." < this is the error " cr ) 
  restore ;

doall
bye
