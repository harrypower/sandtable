#! /usr/local/bin/gforth
\ stcp.fs
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
\ sandmotorapi.fs
\ Gforth-Objects/stringobj.fs
\ unix/libc.fs
\
\ Revisions:
\ 03/02/2020 started coding

warnings off

require sandmotorapi.fs
require Gforth-Objects/stringobj.fs
require unix/libc.fs

:noname ; is bootmessage

0 value dataoutfid
0 value lastoutfid
200 value stdinwaittime
variable httpinput$
variable dataoutfile$
s" stcptest.data" dataoutfile$ $!
variable command$
variable instantresult$
false value http?cmdline?
1 constant *http* \ this is used in http?cmdline? to indicate http has started this code
2 constant *cmdline* \ this is used in http?cmdline? to indicate cmd line has started this code
\ error constants
s" bbbdatapath or pcdatapath do not exist cannot proceed!" exception constant nopath

variable convert$
: udto$ ( ud -- caddr u )  \ convert unsigned double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;
: lineending ( -- caddr u ) \ return a string to produce a line end in html
  s\" <br>\n" ;

: pcdatapath$@ ( -- caddr u ) \ return path string to output data for pc testing
  s" /home/pks/sandtable/" ;
: bbbdatapath$@ ( -- caddr u ) \ return path string to output data for BBB sandtable
  s" /home/debian/sandtable/" ;
: testoutfile$@ ( -- caddr u ) \ the file name for test output info
  s" stcptest.data" ;
: stlastresultfile$@ ( -- caddr u ) \ the file name for the sandtable last result info.
  s" stcplastresult.data" ;
: datapath? ( -- caddr u nflag ) \ caddr u is the correct path to use if nflag is true.... if nflag is false caddr u is 0 0
  pcdatapath$@ file-status swap drop false = if
    pcdatapath$@ true
  else
    bbbdatapath$@ file-status swap drop false = if
      bbbdatapath$@ true
    else
      0 0 false
    then
  then ;
: opendata ( caddr u nflag -- ) \ caddr u is a string for path of file to be opened for writing to .. if it is not present then create that file
\ this tcan throw nopath
  { caddr u nflag }
  nflag true = if
    caddr u file-status swap drop false = if
      caddr u r/w open-file throw
      to dataoutfid
    else
      caddr u r/w create-file throw
      to dataoutfid
    then
  else
    nopath throw
  then ;
variable pathfile$
: testdataout ( caddr u -- ) \ append caddr u string to the test data file
  datapath? >r pathfile$ $! testoutfile$@ pathfile$ $+! pathfile$ $@ r> opendata
  dataoutfid file-size throw
  dataoutfid reposition-file throw
  utime udto$ dataoutfid write-line throw
  dataoutfid write-line throw
  dataoutfid flush-file throw
  dataoutfid close-file throw ;
: stlastresultout ( caddr u -- ) \ create last result file using caddr u string
  { caddr u }
  datapath? if pathfile$ $! stlastresultfile$@ pathfile$ $+! else nopath throw then
  pathfile$ $@ file-status swap drop false = if pathfile$ $@ delete-file throw then
  pathfile$ $@ w/o create-file throw to lastoutfid
  caddr u lastoutfid write-file throw
  lastoutfid flush-file throw
  lastoutfid close-file throw ;

require sandcommands.fs

variable tempheader$
: http-header ( -- caddr u ) \ http header string return
  s\" HTTP/1.1 200 OK\r\n" tempheader$ $!
  s\" Connection: close\r\n" tempheader$ $+!
  s\" Server: Sandserver 0.1\r\n" tempheader$ $+!
  s\" Accept-Ranges: bytes\r\n" tempheader$ $+!
  s\" Content-type: text/html; charset=utf-8\r\n" tempheader$ $+!
  tempheader$ $@ ;

variable tempresponse$
: http-response ( caddr u -- caddr' u' ) \ caddr u is the message string to send
  \ caddr' u' is the complete http-response string to return
  { caddr u }
  http-header tempresponse$ $!
  s\" \r\n\r\n" tempresponse$ $+!
  caddr u tempresponse$ $+!
  s\" \r\n\r\n" tempresponse$ $+!
  tempresponse$ $@ ;

: (getstdin)  ( -- caddr u nflag ) \ will return caddr u containing one charcater if nflag is true and caddr u will be empty if stdin can be read from
  stdin key?-file true = if
    pad 1 stdin read-file throw pad swap true
  else
    pad 0 false
  then ;
variable sdtin$
: getstdin ( -- caddr u )
  sdtin$ $init
  stdinwaittime 0 do
    1 ms
    begin
      (getstdin) while
      sdtin$ $+!
    repeat
    2drop
  loop
  sdtin$ $@ ;

: processhttp ( "ccc" -- ) \ this is called from inetd and will simply get the stdin message sent from inetd and return a message
  getstdin httpinput$ $!
  httpinput$ $@ testdataout
  s" got the message" http-response type
  s" sent receipt message" testdataout
  *http* to http?cmdline?
  bye ;

: processcmdline ( "ccc" -- ) \ this is called from the command line at time of this code being executed
\ this word will take the command from the stdin and process it !
  getstdin command$ $!
  command$ $@ testdataout
  command$ $@ remove\r\n 0 junk-buffer$ [bind] strings []@$ drop command$ $! \ removes the \r\n from command$ recieved and puts first string back to command$
  (parse-command&submessages)
  (command$@?) if
    type s\"  < This Command received\r\n" type
  else
    s\" Message received but there was no command present in it!\r\n" type
    bye
  then
  *cmdline* to http?cmdline?

  bye ;
