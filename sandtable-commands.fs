#! /usr/local/bin/gforth
\ sandtable-commands.fs
\    Copyright (C) 2020  Philip King Smith

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

\ Command processor for sandtable.

\ Requires:
\ sandmotorapi.fs
\ Gforth-Objects/stringobj.fs
\ curl to be installed on OS

\ Revisions:
\ 02/17/2020 started coding
\ 02/17/2020 first test of idea working
\ 02/18/2020 adding more stuff to continue test will real Beaglebone Black hardwear

require sandmotorapi.fs
require Gforth-Objects/stringobj.fs
cr \ to make debugging look better

variable argcommand$
argcommand$ $! \ at this point the string is on the stack so put it here for now!
variable curl$
variable buffer$
variable port#$
variable server_addres$
variable junk$
variable convert$

strings heap-new constant submessages$
strings heap-new constant get-variable-pairs$

: udto$ ( ud -- caddr u )  \ convert unsigned double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;
: lineending ( -- caddr u ) \ return a string to produce a line end in html
  s\" <br>\n" ;

: (parse-command&submessages) ( -- ) \ take command$ and parse command and submessages out of it
  submessages$ [bind] strings destruct
  submessages$ [bind] strings construct
  s" &" argcommand$ $@ submessages$ [bind] strings split$>$s
;
: (get-pairs$) ( -- ) \ extract variable pairs from submessages$ strings
  0 { nqty }
  submessages$ [bind] strings $qty to nqty
  get-variable-pairs$ [bind] strings destruct
  get-variable-pairs$ [bind] strings construct
  nqty 1 > if \ there are some variable pairs to parse
    nqty 1 do
      s" =" i submessages$ [bind] strings []@$ drop
      get-variable-pairs$ [bind] strings split$>$s
    loop
  then ;
: (variable-pair-value) ( caddr u - nvalue nflag ) \ look for string caddr u in get-variable-pairs$ and return its value if it is valid ... nflag is true if valid value ... nflag is false if not found or invalid
  0 false { caddr u nvalue nflag }
  get-variable-pairs$ [bind] strings $qty 0 ?do \ find x variable
    i get-variable-pairs$ [bind] strings []@$ drop caddr u compare false = \ caddr u string is the same as found in get-variable-pairs$ string at index i
    if
      i 1+ get-variable-pairs$  [bind] strings []@$ ( n n caddr u nflag )
      false = if ( n n caddr u )
        s>number? true = if ( n n d )
          d>s to nvalue true to nflag
        else ( n n d )
          2drop false to nvalue false to nflag
        then
      else ( n n caddr u )
        2drop false to nflag
      then ( n n )
      leave
    then
  2 +loop \ note variable value pairs are put into get-variable-pairs$ by (get-pairs$) word so they should be in groups of two
  nvalue nflag ;

s" http://192.168.0.59" server_addres$ $!
\ s" http://localhost" server_addres$ $!
s" :52222" port#$ $!

: sendcurlmessage ( ucaddr u -- ucaddr1 u1 )
  s\" curl --get --data-binary \"" curl$ $! curl$ $+! s\" \"  " curl$ $+! server_addres$ $@ curl$ $+! port#$ $@ curl$ $+! curl$ $@ sh-get
;

: returnmessage ( -- caddr u )
  \ ." started return message " .s cr
  15000 ms  \ time to test if key idea works
  s" command=testshget" buffer$ $!
  (parse-command&submessages)
  \ ." after parse-command stuff " .s cr
  (get-pairs$)
  \ ." after (get-pairs$) stuff" .s cr
  s" key" (variable-pair-value) true = if \ key present so this came from sandsocketserver so return message to them saying done and received
    s" &" buffer$ $+! \ add this to allow key to be added
    s" key=" buffer$ $+!
    s>d dto$ buffer$ $+! \ turn key# into a string again
    ." the message sent:" cr buffer$ $@ type cr
    buffer$ $@ sendcurlmessage
    ." The sandsocketserver output after sent message:" cr
    type cr \ this will go to stdout and the log file
  else \  no key present so this is from command line so return info at stdout
    drop \ key# remove from stack
    ." This was received with no key: " cr
    argcommand$ $@ type cr
  then
  ." the arguments recieved: " argcommand$ $@ type cr
  \ ." stack at end of returnmessage " .s cr
;

returnmessage

get-order get-current \ store order and current on stack
wordlist constant sandtable

sandtable set-current
\ put all outside executable sandtable commands here
\ ***** note non of the following commands are done yet in any way *****

: fastcalibration ( -- ) \ perform the quickstart function from sandtableapi.fs
 0 0 false { nx ny nflag }
 \ get x and y from submessage if present
 s" " junk$ $!
 (parse-command&submessages)
 (get-pairs$)
 s" x" (variable-pair-value) if
   to nx
   s" y" (variable-pair-value) if
     to ny
     nx xm-min >= nx xm-max <= ny ym-min >= ny ym-max <= and and and to nflag
   else
     drop
     s" y variable missing or not valid!" junk$ $+! lineending junk$ $+!
   then
 else
   drop
   s" x variable missing or not valid!" junk$ $+! lineending junk$ $+!
 then
 s" Following was recievd:" junk$ $+! lineending junk$ $+!
 submessages$ [bind] strings $qty 0 ?do
   i submessages$ [bind] strings []@$ drop junk$ $+! lineending junk$ $+!
 loop
 nflag if
   nx ny \ place x and y on stack
   quickstart false = if
     s" Fast calibration done!"
   else
     s" Fast calibration failed!"
   then
   junk$ $+! lineending junk$ $+!
 else
   s" Fast calibration was not performed!" junk$ $+! lineending junk$ $+!
 then
 \ junk$ $@ lastresult$ $!
  ;

: configuresandtable ( -- ) \ perform the configure-stuff and dohome words from sandtableapi.fs
 configure-stuff false = if
   s" Sandtable software configured!"
 else
   s" Sandtable software not configured!"
 then
 junk$ $! lineending junk$ $+!
\  dohome true = if
\    s" Sandtable motors calibrated!"
\  else
\    s" Sandtable motors not calibrated!"
\  then
\  junk$ $+! lineending junk$ $+!
 \ junk$ $@  lastresult$ $!
 \ need to add fork stuff here
;

: gotoxy ( -- ) \ perform the movetoxy word from
0 0 false { nx ny nflag }
\ get x and y from submessage if present
s" " junk$ $!
(parse-command&submessages)
(get-pairs$)
s" x" (variable-pair-value) if
  to nx
  s" y" (variable-pair-value) if
    to ny
    nx xm-min >= nx xm-max <= ny ym-min >= ny ym-max <= and and and to nflag
  else
    drop
    s" y variable missing or not valid!" junk$ $+! lineending junk$ $+!
  then
else
  drop
  s" x variable missing or not valid!" junk$ $+! lineending junk$ $+!
then
s" Following was recieved:" junk$ $+! lineending junk$ $+!
submessages$ [bind] strings $qty 0 ?do
  i submessages$ [bind] strings []@$ drop junk$ $+! lineending junk$ $+!
loop
nflag if
  nx ny \ place x and y on stack
  movetoxy case
    200 of s" move done!" junk$ $+! lineending junk$ $+! endof
    201 of s" x and or y not on table so movement not performed!" junk$ $+! lineending junk$ $+! endof
    202 of s" Sandtable not configured or calibrated yet!" junk$ $+! lineending junk$ $+! endof
  endcase
else
  s" Gotoxy was not performed!" junk$ $+! lineending junk$ $+!
then
\ junk$ $@ lastresult$ $!
\ need to add fork stuff here
;

set-current set-order \ restore order and current from stack

bye
