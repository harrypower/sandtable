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

variable junk$

: sh-sandtable-command ( caddr u -- caddr1 u1)
  { caddr u }
  s\" nohup gforth -e \"s\\\" " junk$ $!
  caddr u junk$ $+!
  s\" \\\"\" sandtable-commands.fs > sandtable-command.data 2>&1 &" junk$ $+! \ note the last & here is to disconnect this new process from the socketserver process
  junk$ $@ sh-get ;

get-order get-current

wordlist constant commands-spawned
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
  homedone? true = if s" Sandtable has been sent to home succesfully!" junk$ $+! then
  homedone? false = if s" Sandtable has not been sent to home succesfully yet!" junk$ $+! then
  lineending junk$ $+!
  junk$ $@  lastresult$ $! ;

: stopsandserver ( -- ) \ stop the sand server loop
  true to stopserverflag
  s" Sandserver shutting done now!" junk$ $!
  lineending junk$ $+!
  junk$ $@  lastresult$ $! ;

: lastresult ( -- )  \ this does nothing and does not change the lastresult$
  ;

: sandtable-message ( -- ) \ this will be the only used by sandtable-commands.fs to return messages
;

: testshget ( -- ) \ this is called as a command to fininish the commands-forded below
  \ this command should be configured to only responde to the child sending this message back to the parent to allow parent to do this wait and return information
  s" got the message from sandtable-commands.fs" lastresult$ $! lineending lastresult$ $+!
  command$ $@ lastresult$ $+! lineending lastresult$ $+!
;

commands-spawned set-current
\ place slower commands-spawned sandtable commands here

: teststuff ( -- ) \ just a test
  ." got to teststuff before sh-get!" cr
  s" testcommand&xnow=234&ynow=3234&x=5&y=10" sh-sandtable-command
  lastresult$ $!
  ." after teststuff sh-get!" cr
 ;

: fastcalibration ( -- ) \ perform the quickstart function from sandtableapi.fs
;

: configuresandtable ( -- ) \ perform the configure-stuff and dohome words from sandtableapi.fs
;

: gotoxy ( -- ) \ perform the movetoxy word from
;


set-current set-order
