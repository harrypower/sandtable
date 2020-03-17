#! /usr/local/bin/gforth-arm
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

require stdatafiles.fs

:noname ; is bootmessage

s" no terminator found in stdin!" exception constant noterm

variable convert$
: udto$ ( ud -- caddr u )  \ convert unsigned double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;
: lineending ( -- caddr u ) \ return a string to produce a line end in html
  s\" <br>\n" ;

: dataout ( caddr u -- ) \ put caddr u string into stcmdinfile$@ file
  stcmdinfile$@ opendata to datafid
  0 s>d datafid resize-file throw
  datafid write-file throw
  datafid flush-file throw
  datafid close-file throw ;
: datain ( -- caddr u nflag ) \ get message from sandtable and put that in string caddr u
\ nflag is true if the message from sandtable is present only
\ nflag is false if there is no message yet from sandtable
  stcmdoutfile$@  file-status swap drop false = if
    stcmdoutfile$@ slurp-file true
  else
    0 0 false
  then ;
: getstatus ( -- caddr u nflag ) \ get the status info from sandtable
\ nflag is true if status info file exists
\ nflag is false if no file
  ststatusfile$@ file-status swap drop false = if
    ststatusfile$@ slurp-file true
  else
    0 0 false
  then ;
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

variable httpinput$
: processcmdline  ( "ccc" -- ) \ this is called from inetd and will simply get the stdin message sent from inetd and return a message
  try
    getstdin 2dup + 1- @ 255 and 10 = if 1- else  noterm throw then \ remove terminator or throw noterm error
    messagebuff$ $!
    messagebuff$ $@ type
    s" < this message was received!" type lineending type
    getstatus true = if
      s" ready" search swap drop swap drop true = if
        messagebuff$ $@ dataout
        s" The command has been sent to sandtable!" type
      else 
        s" Sandtable is currently busy with other commands!" type
      then
    else
      2drop
      s" Sandtable is currently not running!"  type
    then
    bye
    false
  restore
    s>d dto$ type s" < this error happened !" type
  endtry
   ;
