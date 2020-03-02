\ sandcommands.fs

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

\ commands used by socket server to control sandtable

\ Requires:
\ will be included by socksandserver.fs and not to be used on its own

\ Revisions:
\ 01/29/2020 started coding
\ 02/19/2020 added key stuff
\ 02/19/2020 using nohup and command line to send message to sandtable command processor

variable oneuse$

variable shjunk$
: sh-sandtable-command ( caddr u -- caddr1 u1)
  { caddr u }
  s\" nohup gforth -e \"s\\\" " shjunk$ $!
  caddr u shjunk$ $+!
  s\" \\\"\" sandtable-commands.fs > sandtable-command.data 2>&1 &" shjunk$ $+! \ note the last & here is to disconnect this new process from the socketserver process
  shjunk$ $@ sh-get ;

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

get-order get-current

wordlist constant commands-slow
wordlist constant commands-instant

commands-instant set-current
\ place instant commands there
: xmin ( -- )
  s" xmin value is " oneuse$ $!
  xm-min 0 udto$ oneuse$ $+!
  oneuse$ $@ lastresult$ $! ;

: ymin ( -- )
  s" ymin value is " oneuse$ $!
  ym-min 0 udto$ oneuse$ $+!
  oneuse$ $@  lastresult$ $! ;

: xmax ( -- )
  s" xmax value is " oneuse$ $!
  xm-max 0 udto$ oneuse$ $+!
  oneuse$ $@  lastresult$ $! ;

: ymax ( -- )
  s" ymax value is " oneuse$ $!
  ym-max 0 udto$ oneuse$ $+!
  oneuse$ $@  lastresult$ $! ;

: xnow ( -- )
  s" Current x value is " oneuse$ $!
  xposition 0 udto$ oneuse$ $+!
  oneuse$ $@  lastresult$ $! ;

: ynow ( -- )
  s" Current y value is " oneuse$ $!
  yposition 0 udto$ oneuse$ $+!
  oneuse$ $@  lastresult$ $! ;

: status ( -- )
  homedone? true = if s" Sandtable has been sent to home succesfully!" oneuse$ $! then
  homedone? false = if s" Sandtable has not been sent to home succesfully yet!" oneuse$ $+! then
  lineending oneuse$ $+!
  oneuse$ $@  lastresult$ $! ;

: stopsandserver ( -- ) \ stop the sand server loop
  true to stopserverflag
  s" Sandserver shutting done now!" oneuse$ $!
  lineending oneuse$ $+!
  oneuse$ $@  lastresult$ $!
  \ ." stack at end of stopsandserver" .s cr
  ;

: lastresult ( -- )  \ this does nothing and does not change the lastresult$
  ;

: sandtable-message ( -- ) \ this will be the only used by sandtable-commands.fs to return messages
;

commands-spawned set-current
\ place slower commands-spawned sandtable commands here

: fastcalibration ( -- ) \ perform the quickstart function from sandtableapi.fs
;

: configuresandtable ( -- ) \ perform the configure-stuff and dohome words from sandtableapi.fs
;

: gotoxy ( -- ) \ perform the movetoxy word from
;


set-current set-order
