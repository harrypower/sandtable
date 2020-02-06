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

get-order get-current

variable junk$

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
      0 0 i 1+ get-variable-pairs$  [bind] strings []@$
      false = if >number swap drop 0 = if d>s to nvalue true to nflag else 2drop 0 to nvalue false to nflag then else 2drop false to nflag then
      leave
    then
  2 +loop \ note variable value pairs are put into get-variable-pairs$ by (get-pairs$) word so they should be in groups of two
  nvalue nflag ;

wordlist constant commands-slow
wordlist constant commands-instant

commands-instant set-current
\ place instant commands there
: xmin ( -- )
  s" xmin value is " junk$ $!
  xm-min 0 udto$ junk$ $+!
  junk$ $@ lastresult$ $! ;

: ymin ( -- )
  s" ymin value is " junk$ $!
  ym-min 0 udto$ junk$ $+!
  junk$ $@  lastresult$ $! ;

: xmax ( -- )
  s" xmax value is " junk$ $!
  xm-max 0 udto$ junk$ $+!
  junk$ $@  lastresult$ $! ;

: ymax ( -- )
  s" ymax value is " junk$ $!
  ym-max 0 udto$ junk$ $+!
  junk$ $@  lastresult$ $! ;

: xnow ( -- )
  s" Current x value is " junk$ $!
  xposition 0 udto$ junk$ $+!
  junk$ $@  lastresult$ $! ;

: ynow ( -- )
  s" Current y value is " junk$ $!
  yposition 0 udto$ junk$ $+!
  junk$ $@  lastresult$ $! ;

: status ( -- )
  configured? false = if  s" Sandtable is configured now!" junk$ $! then
  configured? true = if s" Sandtable is not configured now!" junk$ $! then
  lineending junk$ $+!
  homedone? true = if s" Sandtable has been sent to home succesfully" junk$ $+! then
  homedone? false = if s" Sandtable has not been sent to home succesfully" junk$ $+! then
  lineending junk$ $+!
  junk$ $@  lastresult$ $! ;

: stopsandserver ( -- ) \ stop the sand server loop
  true to sandserverloop
  s" Sandserver loop shutting done now!" junk$ $!
  lineending junk$ $+!
  junk$ $@  lastresult$ $!
  \ closedown
  ;

: lastresult ( -- )  \ this does nothing and does not change the lastresult$
  ;

: fastcalibration ( -- )
  0 0 false { nx ny nflag }
  \ get x and y from submessage if present
  s" " junk$ $!
  (get-pairs$)
  s" x" (variable-pair-value) = if
    to nx
    s" y" (variable-pair-value) = if
      to ny
      nx xm-min >= nx xm-max <= ny ym-min >= ny ym-max <= and and and to nflag
    else ~~
      drop
      s" y variable missing or bad!" junk$ $+! lineending junk$ $+!
    then
  else ~~
    drop
    s" x variable missing or bad!" junk$ $+! lineending junk$ $+!
  then ~~
  s" Following was recievd:" junk$ $! lineending junk$ $+!
  submessages$ [bind] strings $qty 0 ?do
    i submessages$ [bind] strings []@$ drop junk$ $+! lineending junk$ $+!
  loop
  \ place x and y on stack
  \ quickstart false = if
  \   s" Fast calibration done!" junk$ $! lineending junk$ $+!
  \ else
  \   s" Fast calibration failed!" junk$ $! lineending junk$ $+!
  \ then
  junk$ $@ lastresult$ $! ;

commands-slow set-current
\ place slow sandtable commands here

: configuresandtable ( -- )
  20000 ms
  \ just a test for sandtable task for now
  \ configuration commands put here
  \ configure-stuff false = if
  \  s" Sandtable software configured!" junk$ $! lineending junk$ $+!
  \ else
  \  s" Sandtable software not configured!" junk$ $! lineending junk$ $+!
  \ then
  \ dohome true = if
  \  s" Sandtable motors calibrated!" junk$ $! lineending junk$ $+!
  \ else
  \  s" Sandtable motors not calibrated!" junk$ $! lineending junk$ $+!
  \ junk$ $@  lastresult$ $!
  s" Sandtable configuration was completed properly!" lastresult$ $!
  false to sandtabletask \ to allow other sandtable tasks to perform
;

set-current set-order
