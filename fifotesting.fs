#! /usr/local/bin/gforth-arm
\ fifotesting.fs

\    Copyright (C) 2018  Philip King Smith

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

\ testing fifo use for server and client use

\ Requires:

\ Revisions:
\ 11/07/2018 started coding

0 value infid
0 value outfid
0 value logfid
variable buffer$

: dto$ ( ud -- caddr u )  \ convert double to a string
    swap over dabs <<# #s rot #> #>> buffer$ $! buffer$ $@ ;
: dsignto$ ( d -- caddr u )  \ convert double signed to a string
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
  utime dto$ logfid write-line throw
  logfid write-line throw
  logfid flush-file throw
  logfid close-file throw
;

: startfifos ( -- )
  s" mkfifo /run/sandtablein -m666" system
  s" mkfifo /run/sandtableout -m666" system ;

: getmessage ( -- ucaddr u )
  s" /run/sandtablein" r/o open-file throw to infid
  infid slurp-fid
  infid close-file throw ;

: putmessage ( ucaddr u -- )
  s" /run/sandtableout" w/o open-file throw to outfid
  outfid write-file throw
  outfid flush-file throw
  outfid close-file ;

: mainloop ( -- )
  begin
    getmessage
    putmessage
  again ;

: repeatmain ( -- )
  startfifos
  begin
    try
      mainloop
      false
    restore dsignto$ addtolog
    endtry
  again ;

repeatmain

bye
