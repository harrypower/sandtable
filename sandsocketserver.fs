#! /usr/local/bin/gforth-arm
\ sandsocketserver.fs

\    Copyright (C) 2019  Philip King Smith

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

\ socket server for sandtable
\ the sandtable cgi will talk to this server and other code that wishes to make the sand table do things

\ Requires:
\ unix/socket.fs
\ sandmotorapi.fs
\ config-pins.fs

\ Revisions:
\ 1/26/2019 started coding

require unix/socket.fs

5354 value sandtable-port#
1024 value mb-maxsize
variable message-buffer
mb-maxsize allocate throw message-buffer !
0 value userver
0 value usockfd
0 value logfid
variable junkbuffer$
variable buffer$

: udto$ ( ud -- caddr u )  \ convert double to a string
    swap over dabs <<# #s rot #> #>> buffer$ $! buffer$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> buffer$ $! buffer$ $@ ;

: openlog ( -- )
  s" /run/sandtablelog" file-status swap drop false = if
    s" /run/sandtablelog" r/w open-file throw
    to logfid
  else
    s" /run/sandtablelog" r/w create-file throw
    to logfid
  then ;

: addtolog ( caddr u -- )
  openlog
  logfid file-size throw
  logfid reposition-file throw
  utime udto$ logfid write-line throw
  logfid write-line throw
  utime udto$ logfid write-line throw
  logfid flush-file throw
  logfid close-file throw ;


: readthesocket ( -- caddr u )
  sandtable-port# create-udp-server to userver
  userver 1 listen
  userver accept-socket to usockfd
  usockfd message-buffer @ mb-maxsize read-socket
  usockfd . ." < socket id " cr
  userver . ." < server id " cr
;

: socketloop ( -- )
  begin
    readthesocket
    2dup addtolog
    type cr ." ^ message ^" cr
    s" data recieved" usockfd write-socket
    usockfd close-socket
    userver close-server
  again ;

: repeatmain ( -- )
  begin
    try
      socketloop
      false
    restore dto$ buffer$ $! s"  <-error" buffer$ $+! addtolog
    endtry
  again ;

\ repeatmain
