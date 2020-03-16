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
\ plain Gforth

\ Revisions:
\ 03/15/2020 started coding
warnings off

:noname ; is bootmessage

variable convert$
: udto$ ( ud -- caddr u )  \ convert unsigned double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;
: lineending ( -- caddr u ) \ return a string to produce a line end in html
  s\" <br>\n" ;

: dataout$@ ( - caddr u )
  s" /home/debian/sandtable/stcmdin.data" ;

0 value dataoutfid
: dataout ( caddr u --  ) \ put caddr u string into a file
  { caddr u }
  dataout$@ file-status swap drop false = if
    dataout$@ r/w open-file throw
    to dataoutfid
  else
    dataout$@ r/w create-file throw
    to dataoutfid
  then
  0 s>d dataoutfid resize-file throw
  caddr u dataoutfid write-file throw
  dataoutfid flush-file throw
  dataoutfid close-file throw
  ;

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
variable messagebuffer$
: processhttp ( "ccc" -- ) \ this is called from inetd and will simply get the stdin message sent from inetd and return a message
  try
    getstdin 1- httpinput$ $!
    httpinput$ $@ dataout
    s" < this message was received!" httpinput$ $+! lineending httpinput$ $+!
    \ s" HOME" getenv httpinput$ $+! s" < HOME env " httpinput$ $+! lineending httpinput$ $+!
    httpinput$ $@ type
    bye
    false
  restore
    s>d dto$ type s" < this error happened !" type
  endtry
   ;
