\ #! /usr/local/bin/gforth-arm
\ testfifosend.fs

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

\ used to test fifo for messaging stuff to server and getting response back from server

\ Requires:
\ sandserver.fs to be running on system
\ Revisions:
\ 25/1/2019 started coding


0 value infid
0 value outfid
variable buffer$
variable message$

: udto$ ( ud -- caddr u )  \ convert double to a string
    swap over dabs <<# #s rot #> #>> buffer$ $! buffer$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> buffer$ $! buffer$ $@ ;

: getmessage ( -- ucaddr u )
  s" /run/sandtablein" r/o open-file throw to infid
  pad pad 80 infid read-file throw
  infid close-file throw ;

: putmessage ( ucaddr u -- )
  s" /run/sandtableout" w/o open-file throw to outfid
  outfid write-file throw
  outfid flush-file throw
  outfid close-file throw ;

s\" testing message from 123\n" message$ $!

: testloop ( -- )
  begin
    message$ $@ putmessage
    getmessage type cr
    500 ms
  again ;
