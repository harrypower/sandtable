#! /usr/local/bin/gforth
\ getinet.fs
\ Copyright (C) 2021  Philip King Smith

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

\ inetd sends message to this code

\ Requires:
\ stdatafiles.fs
\ stringconvert.fs

\ Revisions:
\ 01/26/2021 started coding
warnings off

:noname ; is bootmessage

s" no terminator found in stdin!" exception constant noterm

5000000 value cmdtimeout \ this is about 5 seconds to wait for message to show up

require stringconvert.fs
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
: processcmdline  ( "ccc" -- ) \ this is called from cgi code and will simply get the stdin message sent from cgi and return a message
  try
    getstdin 2dup + 1- @ 255 and 10 = if 1- else  noterm throw then \ remove terminator or throw noterm error
    messagebuff$ $!
    messagebuff$ $@ type
    s" < this message was received!" type lineending type
    getstatus true = if
      s" ready" search swap drop swap drop true = \ status file is present and contains ready
      cmddatarecieve@ swap drop swap drop false = and \ and cmd data recieve file is not present currently
      if
        messagebuff$ $@ cmddatarecieve!
        s" The command has been sent to sandtable!" type lineending type
        100 ms
        utime cmdtimeout s>d d+ timeout 2!
        begin
          100 ms
          cmddatasend@ true = if
            2drop true
          else
            2drop utime timeout 2@  d>
          then
        until
        300 ms
        cmddatasend@ true = if
          type s" < sandtable response to the received command!" type lineending type
        else
          2drop s" Sandtable has not responded to command!"  type lineending type
        then
      else
        s" Sandtable is currently busy with other commands!" type lineending type
        messagebuff$ $@ s" command=stopsand" search >r 2drop r> true = if
          ststopcmd!
          s" Stop sandtable command issued!" type lineending type
        then
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

processcmdline
