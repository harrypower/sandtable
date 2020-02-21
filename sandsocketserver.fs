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
\ stringobj.fs from Gforth-Objects git

\ Revisions:
\ 1/26/2019 started coding
\ 1/28/2019 added sandmotorapi.fs require for later motor control threads
\ *** note this means bind is redefined in objects.fs from its first use in unix/socket.fs so be aware of this
\ *** bind can be used for an object and is not needed as a socket item so it is an ok tradeoff
\ 02/19/2020 note sandmotorapi.fs is only used for some variables and not to process directly the sandtable ... that is done externaly
\ 02/19/2020 this socket server simply now gets a message and passes it on to sandtable-commands.fs if it is not running currently to process the sandtable command
\ 02/19/2020 a key# was added to ensure only one sandtable-commands.fs process is running and only that process can return that key# to tell this server it is done!

require unix/socket.fs
require sandmotorapi.fs  \ note this sandmotorapi.fs stuff is not executed in this code but is used to get sandtable data sandtable-commands.fs will execute the sandtable motors
require Gforth-Objects/stringobj.fs
require unix/libc.fs
require random.fs

only forth also definitions

2000 value stream-timeout
52222 value sandtable-port#
1024 value mb-maxsize
variable message-buffer
mb-maxsize allocate throw message-buffer !
0 value userver
0 value usockfd
0 value logfid
variable command$
variable thecommand$
variable User-Agent$
variable GET$
variable lastresult$

seed-init \ start of random stuff

false value stopserverflag \ this is the server loop control itself .. when it is false the loop continues when it is true the loop stops
false value curlagent \ true means it is a curl agent false means it is a browser based or other agent
strings heap-new constant submessages$
strings heap-new constant get-variable-pairs$

: parse-command&submessages ( -- ) \ take command$ and parse command and submessages out of it
  submessages$ [bind] strings destruct
  submessages$ [bind] strings construct
  s" &" command$ $@ submessages$ [bind] strings split$>$s
;

variable convert$
: udto$ ( ud -- caddr u )  \ convert unsigned double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;
: lineending ( -- caddr u ) \ return a string to produce a line end in html
  s\" <br>\n" ;

0 value key# \ 0 means no key# issued so no sandtable code running... any other number means santable code is running or has not returned key# yet
variable key$
: keymake$ ( -- caddr u  ) \ make a new random key# to use for sandtable execution or return existing key# if it has not been returned
  key# 0= if
    rnd  to key#
    key# s>d dto$
    s" key=" key$ $!
    key$ $+!
  then
  key$ $@ ;

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

variable tempheader$
: http-header ( -- caddr u )
  s\" HTTP/1.1 200 OK\r\n" tempheader$ $!
  s\" Connection: close\r\n" tempheader$ $+!
  s\" Server: Sandserver 0.1\r\n" tempheader$ $+!
  s\" Accept-Ranges: bytes\r\n" tempheader$ $+!
  s\" Content-type: text/html; charset=utf-8\r\n" tempheader$ $+!
  tempheader$ $@ ;

variable tempresponse$
: http-response ( caddr u -- caddr' u' ) \ caddr u is the message string to send
  { caddr u }
  http-header tempresponse$ $!
  s\" Content-Length: " tempresponse$ $+!
  u s>d udto$ tempresponse$ $+!
  s\" \r\n" tempresponse$ $+!
\  s\" \r\n" tempresponse$ $+!

  caddr u tempresponse$ $+!
  s\" \r\n" tempresponse$ $+!
  s\" \r\n\r\n" tempresponse$ $+!
  tempresponse$ $@ ;

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

: parsehttp ( cadrr u -- ) \ get the command and user-agent
  { caddr u }
  caddr u s" GET " s"  " parse$to$ GET$ $!
  GET$ $@ s" /?command=" search true = if
    10 - swap 10 + swap command$ $!
  else
    2drop 0 0 command$ $!
  then
  caddr u s" User-Agent: " s\" \r\n" parse$to$ User-Agent$ $!
  User-Agent$ $@ s" curl/" search to curlagent 2drop
;

variable tmphtmlheader$
: html-header ( -- caddr u )
  s\" <!DOCTYTPE html>" tmphtmlheader$ $!
  s\" <html>" tmphtmlheader$ $+!
  s\" <head><title>Sandtable Message return</title></head>" tmphtmlheader$ $+!
  s\" <body>" tmphtmlheader$ $+!
  tmphtmlheader$ $@ ;

: html-footer ( -- caddr u )
  s\" </body></html>" ;

variable junk-buffer$
: parse-command ( -- )  \ parse the command from the command$ and break it up into submessages the execute the command if it exists as a command
  parse-command&submessages
  0 submessages$ [bind] strings []@$ drop \ the first string should be the command
  thecommand$ $!
  \ ." stack in parse-command top " .s cr
  thecommand$ $@ swap drop 0 > if
    thecommand$ $@ commands-instant search-wordlist 0 <> if
    \ ." stack in parse-command before execute of commands-instant " .s cr
      \ note commands-instant are basic data retreval or the command to update the data to this sand server ... the commands are in wordlist commands-instant
      execute
      \ ." stack in parse-command after exectue of commands-instant " .s cr
      lastresult$ $@ junk-buffer$ $+! lineending junk-buffer$ $+!
      \ ." stack in parse-command after string junk-buffer$ stuff of commands-instant " .s cr
    then
    \ ." stack in parse-command after commands-instant " .s cr
    thecommand$ $@ commands-spawned search-wordlist 0 <> if
      \ note commands-spawned are the sandtable process that take some time to complete.  the commads are in wordlist commands-spawned.  the commad here will basically call the sandtable-commands.fs via sh-get shell command with data
      execute
      lastresult$ $@ junk-buffer$ $+! lineending junk-buffer$ $+!
    then
    thecommand$ $@ commands-instant search-wordlist 0 = if
      thecommand$ $@ commands-spawned search-wordlist 0 = if
        thecommand$ $@ junk-buffer$ $+! s"  command not found!" junk-buffer$ $+! lineending junk-buffer$ $+!
      else drop \ not zero so drop xt
      then
    else drop \ not zero so drop xt
    then
    \ ." stack in parse-command execute ifs " .s cr
  else
    s" No command issued!" junk-buffer$ $+! lineending junk-buffer$ $+!
  then ;

variable tmphtmlresponse$
variable receive-buffer$
: process-received ( caddr u -- caddr1 u1 )
  receive-buffer$ $!
  receive-buffer$ $@ addtolog
  receive-buffer$ $@ dump ." ^ message ^" cr
  usockfd . ." < socket fd" cr
  receive-buffer$ $@ parsehttp
  s" Message $ is > " junk-buffer$ $!
  GET$ $@ junk-buffer$ $+! lineending junk-buffer$ $+!
  s" Command $ is > " junk-buffer$ $+!
  command$ $@ junk-buffer$ $+! lineending junk-buffer$ $+!
  s" From User-Agent> " junk-buffer$ $+!
  User-Agent$ $@ junk-buffer$ $+! lineending junk-buffer$ $+!
  \ ." stack before parse-command in process-received " .s cr
  parse-command  \ find and execute commands
  \ ." stack after parse-command in process-received " .s cr
\  curlagent if
\   junk-buffer$ $@ http-response
\  else
    html-header tmphtmlresponse$ $!
    junk-buffer$ $@ tmphtmlresponse$ $+! \ this is message returned in socket call
    html-footer tmphtmlresponse$ $+!
    tmphtmlresponse$ $@ http-response
\  then
;

\ variable wstatus
: socketloop ( -- )
  stream-timeout set-socket-timeout
  sandtable-port# create-server to userver
  userver . ." < server id " cr
  \ ." stack before serverloop " .s cr
  begin
    \ ." stack begining of serverloop " .s cr
    userver 8 listen
    userver accept-socket to usockfd
    usockfd message-buffer @ mb-maxsize read-socket \ recived message from web front end or a cdl curl command
    \ ." stack before process-received in loop " .s cr
    process-received \ ( -- caddr u ) this will be the string to return
    \ ." stack after process-received in loop " .s cr
    usockfd write-socket  \ return the message to calling program
    usockfd close-socket
    \ ." stack after close-socket in loop " .s cr
    stopserverflag
  until
  userver close-server
  s" sand server shutting down now!" type cr
  ;

\ socketloop
\ bye
