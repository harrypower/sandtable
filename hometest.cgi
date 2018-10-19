#! /usr/local/bin/gforth-arm

warnings off
:noname ; is bootmessage

require BBB_Gforth_gpio/syscalls386.fs
require tmc2130.fs
require Gforth-Objects/objects.fs
require Gforth-Objects/stringobj.fs

string heap-new constant mytemppad$

: #to$ ( n -- c-addr u1 ) \ convert n to string
    s>d swap over dabs <<# #s rot sign #> #>> mytemppad$ !$ mytemppad$ @$ ;

0 value xmotor
0 value ymotor
true value configured
: configure-stuff ( -- nflag ) \ nflag is false if configuration happened true if some problems
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
  0 xmotor usequickreg

  %100 %00000000011000000000 1 1000 0 0 %00110001000000101000000010010011 %1000000000000000000000000 %0111100000101011111111
  1 xmotor quickreg!

  1 xmotor setdirection

  %100 %01110001111100000000 1 0    0 0 %00110000000000101000000010010011 0                          %0111000000101011111111
  0 ymotor quickreg!
  0 ymotor usequickreg

  %100 %00000000011000000000 1 1000 0 0 %00110001000000101000000010010011 %1000000000000000000000000 %0111100000101011111111
  1 ymotor quickreg!

  1 ymotor setdirection
  then
  configured ;

variable query$
variable apache$s
variable output$
0 value fid

: lineending ( -- caddr u )
  s\" <br>\n\n" ;

: return-message ( -- )
  s\" Content-type: text/html; charset=utf-8\n\n" type
  s\" All Ok\n\n" type ;

: get-message@ ( -- )
  s" QUERY_STRING" getenv query$ $! ;

: running? ( -- nflag ) \ nflag is true if this process is already running but false if it is not running.
  s" /run/hometest.pid" r/o open-file swap to fid
  false = if
    fid close-file drop
    true
  else
    false
  then ;
