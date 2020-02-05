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

: get-pairs ( -- ) \ extract variable pairs from submessages$ strings
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
  \ 0 0 { nx ny }
  \ get x and y from submessage if present
  0 submessages$ [bind] strings []@$ drop
  s" fastcalibration" compare false = if
\    get-pairs
\    get-variable-pairs$ [bind] strings $qty 2 /mod drop 0 = if \ at least there are pairs
\      s" The following data found!" junk$ $! lineending junk$ $+!
\      get-variable-pairs$ [bind] strings $qty 0 do  \ find x variable
\        i get-variable-pairs$ [bind] strings []@$ drop s" x" compare false =
\        if s" x is " junk$ $+! i 1+ get-variable-pairs$ [bind] strings []@$ drop junk$ $+! lineending junk$ $+! then
\      loop
\      get-variable-pairs$ [bind] strings $qty 0 do  \ find x variable
\        i get-variable-pairs$ [bind] strings []@$ drop s" y" compare false =
\        if s" y is " junk$ $+! i 1+ get-variable-pairs$ [bind] strings []@$ drop junk$ $+! lineending junk$ $+! then
\      loop
\    else  \ not all pairs so data bad
\      s" some varible data bad or missing ... following is what was recievd!" junk$ $! lineending junk$ $+!
\      submessages$ [bind] strings $qty 1 do
\        i submessages$ [bind] strings []@$ drop junk$ $+! lineending junk$ $+!
\      loop
\    then
  else
    s" needed fast calibration data missing!" junk$ $! lineending junk$ $+!
  then
  \ place x and y on stack
  \ quickstart false = if
  \   s" Fast calibration done!" junk$ $! lineending junk$ $+!
  \ else
  \   s" Fast calibration failed!" junk$ $! lineending junk$ $+!
  \ then
  \ junk$ $@ lastresult$ $!
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
