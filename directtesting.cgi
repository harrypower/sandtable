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
\ script.fs
\ curl to be installed on BBB

\ Revisions:
\ 1/26/2019 changes from fifo to socket coding started
\ 2/1/2019 using curl to send message to server

warnings off
:noname ; is bootmessage

require script.fs

0 value userver
0 value usockfd

variable query$
variable thequery$
variable apache$s
variable output$
variable http_host$
variable port#$
variable server_addres$

variable tmpmake$
: udto$ ( ud -- caddr u )  \ convert double to a string
    <<# #s  #> #>> tmpmake$ $! tmpmake$ $@ ;

\ s" http://mysandtable.local" server_addres$ $!  \ this works if the BBB has host setup and is called mysandtable
s" http://mysandtable" server_addres$ $!
\ s" http://192.168.0.59" server_addres$ $!
s" :52222/" port#$ $!  \ the / is need at end of port number

variable curl$
: sandtablemessage ( ucaddr u -- ucaddr1 u1 ) \ send ucaddr u string to sandtable via curl in cmd line and return caddr1 u1 from the sandtable
{ ucaddr u }
s\" curl --get --data \"" curl$ $!
ucaddr u curl$ $+!
s\" \" " curl$ $+! \ note the space after the last " is needed to separate
server_addres$ $@ curl$ $+!
port#$ $@ curl$ $+!
curl$ $@ sh-get ;

: sandtablemessagecmdline ( ucaddr u -- caddr1 u1 )
  2drop \ just testing now
  s\" echo \"command=fromcgi\" | /home/debian/sandtable/stcp.fs -e \"processhttp\"" sh-get
;

: lineending ( -- caddr u )
  s\" <br>\n" ;

: return-message ( -- )
  \ s\" Content-type: text/html; charset=utf-8\n\n" type
  s\" <!DOCTYPE html>\n" type
  s\" <html>\n" type
  s\" <head><title>CGI return</title></head>\n" type
  s\" <body>\n" type
  query$ $@ type
  apache$s $@ type
  s" CGI got this message: " type thequery$ $@ type lineending type
\  thequery$ $@ sandtablemessage
  thequery$ $@ sandtablemessagecmdline
  s" Server message recieved is: " type type lineending type
  s\" </body></html>\n" type
;

: get-get-message ( -- )
  s" QUERY_STRING is:" query$ $! s" QUERY_STRING" getenv query$ $+! lineending query$ $+!
  s" QUERY_STRING" getenv thequery$ $! ;

: get-apache-stuff ( -- )
  s" REMOTE_ADDR is :" apache$s $! s" REMOTE_ADDR" getenv apache$s $+! lineending apache$s $+!
  s" REQUEST_METHOD is :" apache$s $+! s" REQUEST_METHOD" getenv apache$s $+! lineending apache$s $+!
  s" HTTP_REFERER is:" apache$s $+! s" HTTP_REFERER" getenv apache$s $+! lineending apache$s $+!
  s" HTTP_HOST is:" apache$s $+! s" HTTP_HOST" getenv apache$s $+! lineending apache$s $+!
  s" SERVER_SOFTWARE is:" apache$s $+! s" SERVER_SOFTWARE" getenv  apache$s $+! lineending apache$s $+!
  s" HTTP_HOST" getenv http_host$ $!
;

: prep-message ( -- )
  get-get-message
  get-apache-stuff ;

prep-message

return-message
bye
