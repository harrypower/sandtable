#! /usr/local/bin/gforth
\ getcgi.fs
\ Copyright (C) 2019  Philip King Smith

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
\ the sandtable cgi will talk to this code to leave a command for sandtable to do

\ Requires:
\ stdatafiles.fs

\ Revisions:
\ 03/15/2020 started coding
warnings off

:noname ; is bootmessage

s" no terminator found in stdin!" exception constant noterm

variable convert$
: udto$ ( ud -- caddr u )  \ convert unsigned double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;
: lineending ( -- caddr u ) \ return a string to produce a line end in html
  s\" <br>\n" ;

require stdatafiles.fs

: (getstdin)  ( -- caddr u nflag ) \ will return caddr u containing one charcater if nflag is true and caddr u will be empty if stdin can be read from
  stdin key?-file true = if
    pad 1 stdin read-file throw pad swap true
  else
    pad 0 false
  then ;
variable sdtin$
: getstdin ( -- caddr u ) \ recieve the stdin to this code
\ note this will have a terminator in this returned string at the end of the string ... remove this if not used
  sdtin$ $init
  200 0 do
    1 ms
    begin
      (getstdin) while
      sdtin$ $+!
    repeat
    2drop
  loop
  sdtin$ $@ ;

variable messagebuff$
2variable timeout
: processcmdline  ( "ccc" -- ) \ this is called from inetd and will simply get the stdin message sent from inetd and return a message
  try
    getstdin 2dup + 1- @ 255 and 10 = if 1- else  noterm throw then \ remove terminator or throw noterm error
    messagebuff$ $!
    messagebuff$ $@ type
    s" < this message was received!" type lineending type
    getstatus true = if
      s" ready" search swap drop swap drop true = if
        messagebuff$ $@ cmddatarecieve
        s" The command has been sent to sandtable!" type lineending type
        50 ms
        utime 2000 s>d d+ timeout 2!
        begin
          10 ms 
          cmddatasend true = if
            2drop true
          else
            2drop timeout 2@ utime d>
          then
        until
        cmddatasend true = if
          type s" < sandtable response to the received command!" type lineending type
        else
          2drop s" Sandtable has not responded to command!"  type lineending type
        then
      else
        s" Sandtable is currently busy with other commands!" type lineending type
      then
    else
      2drop
      s" Sandtable is currently not running!"  type lineending type
    then
    false
  restore
    dup false <> if s>d dto$ type s" < this error happened !" type lineending type else drop then
  endtry
  bye ;
