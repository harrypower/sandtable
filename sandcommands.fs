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
\ will be included by socksandserver.fs

\ Revisions:
\ 01/29/2020 started coding

get-order get-current

variable junk$

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

: lastresult ( -- )
  s" The last sandtable command result is:> " junk$ $!
  lastresult$ $@ s" is:>" search true = if 4 - swap 4 + swap else lastresult$ $@ then
  junk$ $+! junk$ $@ lastresult$ $! ;

: fastcalibration ( -- )
  \ get x and y from submessage if present
  \ place x and y on stack
  \ quickstart false = if
  \   s" Fast calibration done!" junk$ $! lineending junk$ $+!
  \ else
  \   s" Fast calibration failed!" junk$ $! lineending junk$ $+!
  \ then
  \ junk$ $@ lastresult$ $!
  s" not done!" junk$ $! lineending junk$ $+!
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
