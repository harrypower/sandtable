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
\ objects.fs

\ Revisions:
\ 1/26/2019 started coding
\ 1/28/2019 added sandmotorapi.fs require for later motor control threads
\ *** note this means bind is redefined in objects.fs from its first use in unix/socket.fs so be aware of this
\ *** bind can be used for an object and is not needed as a socket item so it is an ok tradeoff

require unix/socket.fs
require sandmotorapi.fs
require forth-packages/multi-tasking/0.4.0/multi-tasking.fs


40000 value stream-timeout
52222 value sandtable-port#
1024 value mb-maxsize
variable message-buffer
mb-maxsize allocate throw message-buffer !
0 value userver
0 value usockfd
0 value logfid
variable buffer$
variable convert$
variable buffer1$
variable message-buffer$
variable command$

: udto$ ( ud -- caddr u )  \ convert double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;

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

: http-header ( -- caddr u )
  s\" HTTP/1.1 200 OK\r\n" buffer$ $!
  s\" Connection: close\r\n" buffer$ $+!
  s\" Server: Gforth0.79\r\n" buffer$ $+!
  s\" Accept-Ranges: bytes\r\n" buffer$ $+!
  s\" Content-type: text/html; charset=utf-8\r\n" buffer$ $+!
  buffer$ $@ ;

: http-response ( caddr u -- caddr' u' ) \ caddr u is the message string to send
  { caddr u }
  http-header buffer$ $!
  s\" Content-Length: " buffer$ $+!
  u s>d udto$ buffer$ $+!
  s\" \r\n" buffer$ $+!
  s\" \r\n" buffer$ $+!

  caddr u buffer$ $+!
  s\" \r\n" buffer$ $+!
  s\" \r\n\r\n" buffer$ $+!
  buffer$ $@ ;

: parse$to$ ( caddr u start$addr ustart end$addr uend -- caddr1 u1 )
\ find start$ in caddr string then look for end$ .. if found return the string between start$ and end$ only or return 0 0 if start$ and end$ not found
  0 0 { caddr u start$addr ustart end$addr uend addr-a addr-b }
  caddr u start$addr ustart search true = if
    ustart - swap ustart + dup to addr-a swap
    end$addr uend search true = if
      drop to addr-b
      addr-a addr-b addr-a -
      else 2drop 0 0
      then
  else
    2drop 0 0
  then ;

: keyboardstop ( -- nflag ) \ nflag is true if 's' is pressed on keyboard false otherwise
  ekey? if ekey 115 = if true else false then else false then ;

: socketloop ( -- )
  stream-timeout set-socket-timeout
  sandtable-port# create-server to userver
  userver 4 listen
  userver . ." < server id " cr
  begin
    userver accept-socket to usockfd
    usockfd message-buffer @ mb-maxsize read-socket
    message-buffer$ $!
    message-buffer$ $@
    2dup addtolog
    2dup dump ." ^ message ^" cr
    hostname dump ." ^ hostname ^" cr
    usockfd . ." < socket fd" cr
    s" Got this message > " buffer1$ $!
    s" GET " s"  " parse$to$ buffer1$ $+!
    buffer1$ $@ http-response usockfd write-socket
    usockfd close-socket
    keyboardstop
  until
  userver close-server
  ;
