#! /usr/local/bin/gforth-arm
\ directtesting.cgi
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

\

\ Requires:
\ unix/socket.fs

\ Revisions:
\ 1/26/2019 changes from fifo to socket coding started

warnings off
:noname ; is bootmessage

require unix/socket.fs

5354 value sandtable-port#
1024 value mb-maxsize
variable message-buffer
mb-maxsize allocate throw message-buffer !
0 value userver
0 value usockfd
0 value logfid
variable buffer$

variable query$
variable test$
variable apache$s
variable output$
variable http_host$
variable port#$

: udto$ ( ud -- caddr u )  \ convert double to a string
    <<# #s  #> #>> buffer$ $! buffer$ $@ ;

sandtable-port# s>d udt$ port#$ $!

: getmessage ( -- ucaddr u )

;

: putmessage ( ucaddr u -- )

;

: lineending ( -- caddr u )
  s\" <br>\n\n" ;

: return-message ( -- )
  s\" Content-type: text/html; charset=utf-8\n\n" type
  query$ $@ type
  apache$s $@ type
  test$ $@ type lineending type
  test$ $@ putmessage
  getmessage s" The message recieved is: " type type lineending type
  s\" All Ok\n\n" type ;

: get-get-message ( -- )
  s" QUERY_STRING is:" query$ $! s" QUERY_STRING" getenv query$ $+! lineending query$ $+!
  s" QUERY_STRING" getenv test$ $! ;

: get-apache-stuff ( -- )
  s" REMOTE_ADDR is :" apache$s $! s" REMOTE_ADDR" getenv apache$s $+! lineending apache$s $+!
  s" REQUEST_METHOD is :" apache$s $+! s" REQUEST_METHOD" getenv apache$s $+! lineending apache$s $+!
  s" HTTP_REFERER is:" apache$s $+! s" HTTP_REFERER" getenv apache$s $+! lineending apache$s $+!
  s" HTTP_HOST is:" apache$s $+! s" HTTP_HOST" getenv apache$s $+! lineending apache$s $+!
  s" HTTP_HOST" getenv http_host$ $!
  s" SERVER_SOFTWARE is:" apache$s $+! s" SERVER_SOFTWARE" getenv  apache$s $+! lineending apache$s $+!
;

: prep-message ( -- )
  get-get-message
  get-apache-stuff ;

prep-message

return-message
message-buffer @ free throw
bye
