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

require Gforth-Objects/stringobj.fs
require unix/socket.fs
require sandmotorapi.fs
require forth-packages/multi-tasking/0.4.0/multi-tasking.fs

task servertask \ task for sand server running in
servertask construct
task sandtable
sandtable construct

10000 value stream-timeout
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
variable buffer2$
variable recieve-buffer$
variable command$
variable thecommand$
variable User-Agent$
variable GET$
variable lastresult$
0 value curlagent \ true means it is a curl agent false means it is a browser based or other agent
false value sandserverloop \ flag to turn off sand server loop false is run true is stop
false value sandtabletask \ flag false when no task running true when sandtable task is active
strings heap-new constant submessages$

: parse-command-submessages ( -- ) \ take command$ and parse command and submessages out of it
  submessages$ destruct
  submessages$ construct
  s" &" command$ $@ submessages$ split$>$s

;

: udto$ ( ud -- caddr u )  \ convert double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;
: lineending ( -- caddr u ) \ return a string to produce a line end in html
  s\" <br>\n" ;

require sandcommands.fs

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
  s\" Server: Sandserver0.1\r\n" buffer$ $+!
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

: parsehttp ( -- ) \ get the command, user-agent
  recieve-buffer$ $@ s" GET " s"  " parse$to$ GET$ $!
  GET$ $@ s" /?command=" search true = if
    10 - swap 10 + swap command$ $!
  else
    2drop 0 0 command$ $!
  then
  recieve-buffer$ $@ s" User-Agent: " s\" \r\n" parse$to$ User-Agent$ $!
  User-Agent$ $@ s" curl/" search to curlagent 2drop
;

: html-header ( -- caddr u )
  s\" <!DOCTYTPE html>" buffer$ $!
  s\" <html>" buffer$ $+!
  s\" <head><title>Sandtable Message return</title><link rel=\"icon\" href=\"data:;base64,iVBORw0KGgo=\"></head>" buffer$ $+!
  s\" <body>" buffer$ $+!
  buffer$ $@ ;
: html-footer ( -- caddr u )
  s\" </body></html>" ;

: parse-command ( -- )
  command$ $@ s" &" search if
    swap drop
    command$ $@ rot -
  then
  thecommand$ $!
  thecommand$ $@ swap drop 0 > if
    thecommand$ $@ commands-instant search-wordlist 0 <> if
      execute
      lastresult$ $@ buffer1$ $+! lineending buffer1$ $+!
    then
    thecommand$ $@ commands-slow search-wordlist 0 <> if
      sandtabletask false = if
        true to sandtabletask
        sandtable start-task
        thecommand$ $@ buffer1$ $+! s"  command has started!" buffer1$ $+! lineending buffer1$ $+!
      else
        drop \ remove command xt
        s" Sandtable is currently busy with another task!"  buffer1$ $+! lineending buffer1$ $+!
      then
    then
    thecommand$ $@ commands-instant search-wordlist 0 = if
      thecommand$ $@ commands-slow search-wordlist 0 = if
        thecommand$ $@ buffer1$ $+! s"  command not found!" buffer1$ $+! lineending buffer1$ $+!
      else drop
      then
    else drop
    then
  else
    s" No command issued!" buffer1$ $+! lineending buffer1$ $+!
  then ;

: process-recieve ( caddr u -- caddr u )
  recieve-buffer$ $!
  recieve-buffer$ $@ addtolog
  recieve-buffer$ $@ dump ." ^ message ^" cr
  hostname dump ." ^ hostname ^" cr
  usockfd . ." < socket fd" cr
  parsehttp
  s" Message $ is > " buffer1$ $!
  GET$ $@ buffer1$ $+! lineending buffer1$ $+!
  s" Command $ is > " buffer1$ $+!
  command$ $@ buffer1$ $+! lineending buffer1$ $+!
  s" From User-Agent> " buffer1$ $+!
  User-Agent$ $@ buffer1$ $+! lineending buffer1$ $+!
  parse-command  \ find and execute commands
  curlagent if
    buffer1$ $@ http-response
  else
    html-header buffer2$ $!
    buffer1$ $@ buffer2$ $+!
    html-footer buffer2$ $+!
    buffer2$ $@ http-response
  then
;
: socketloop ( -- )
  stream-timeout set-socket-timeout
  sandtable-port# create-server to userver
  userver 2 listen
  userver . ." < server id " cr
  begin
    userver accept-socket to usockfd
    usockfd message-buffer @ mb-maxsize read-socket
    process-recieve
    usockfd write-socket
    usockfd close-socket
    \ keyboardstop
    sandserverloop
  until
  userver close-server
  s" sand server shutting down now!" type cr ;

: startsandserver ( -- ) \ start the socket sand server ... note this will work but can not be used at command line with gforth still responding to user terminal
  false to sandserverloop
  ['] socketloop servertask start-task
  begin 1000 ms again
  ;

\ startsandserver
\ socketloop
\ bye
