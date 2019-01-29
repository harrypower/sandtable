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
require sandmotorapi.fs

2000 value stream-timeout
5354 value sandtable-port#
1024 value mb-maxsize
variable message-buffer
mb-maxsize allocate throw message-buffer !
0 value userver
0 value usockfd
0 value logfid
variable buffer$

: udto$ ( ud -- caddr u )  \ convert double to a string
    <<# #s  #> #>> buffer$ $! buffer$ $@ ;
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

: addtolog ( caddr u -- )  \ *** need to put a trim to this file in here to prevent large log files ***
  openlog
  logfid file-size throw
  logfid reposition-file throw
  utime udto$ logfid write-line throw
  logfid write-line throw
  logfid flush-file throw
  logfid close-file throw ;

: http-response ( -- caddr u )
  s\" HTTP/1.1 200 OK\n" buffer$ $!
  s\" Date: Mon, 28 Jan 2019 11:31:00 GMT\n" buffer$ $+!
  s\" Connection: close\n" buffer$ $+!
  s\" Server: Gforth0.79\n" buffer$ $+!
  s\" Accept-Ranges: bytes\n" buffer$ $+!
  s\" Content-type: text/html; charset=utf-8\n" buffer$ $+!
  s\" Content-Length: 32\n" buffer$ $+!
  s\" Last-Modified: Mon, 28 Jan 2019 10:14:49 GMT\n" buffer$ $+!
  s\" \n" buffer$ $+!
  s\" <html> message recieved </html>\n" buffer$ $+!
  s\" \n\n" buffer$ $+!
  buffer$ $@ ;

: socketloop ( -- )
  stream-timeout set-socket-timeout
  sandtable-port# create-server to userver
  userver 3 listen
  userver . ." < server id " cr
  begin
    userver accept-socket to usockfd
    usockfd message-buffer @ mb-maxsize read-socket
    http-response usockfd write-socket
    \ dup s>d udto$ buffer$ $! s\"  data recieved\n\n" buffer$ $+! buffer$ $@ usockfd write-socket
    2dup addtolog
    type cr ." ^ message ^" cr
    usockfd close-socket
  again
  userver close-server ;

: repeatmain ( -- )
  begin
    try
      socketloop
      false
    restore s>d dto$ buffer$ $! s"  <-error" buffer$ $+! addtolog
    endtry
  again ;

\ repeatmain
