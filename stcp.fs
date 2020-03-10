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
variable tmppath$
s" echo $HOME" system
s" echo $HOME" sh-get
s\" GFORTHCCPATH=\'" tmppath$ $!
tmppath$ $+! s\" \'" tmppath$ $+!
tmppath$ $@ system
s" export GFORTHCCPATH" system

require sandmotorapi.fs
require Gforth-Objects/stringobj.fs
require unix/libc.fs

:noname ; is bootmessage

0 value dataoutfid
0 value lastoutfid
0 value calibratefid
0 value pidfid
200 value stdinwaittime
variable httpinput$
variable command$
false value http?cmdline?
1 constant *http* \ this is used in http?cmdline? to indicate http has started this code
2 constant *cmdline* \ this is used in http?cmdline? to indicate cmd line has started this code
\ error constants
s" bbbdatapath or pcdatapath do not exist cannot proceed!" exception constant nopath
s" no terminator found in stdin!" exception constant noterm

variable convert$
: udto$ ( ud -- caddr u )  \ convert unsigned double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;
: lineending ( -- caddr u ) \ return a string to produce a line end in html
  s\" <br>\n" ;

: pidfilepath$@ ( -- caddr u ) \ caddr u is string for the file name and path to store programs pid
  s" /run/stcp.fs.pid" ;
: pcdatapath$@ ( -- caddr u ) \ return path string to output data for pc testing
  s" /home/pks/sandtable/" ;
: bbbdatapath$@ ( -- caddr u ) \ return path string to output data for BBB sandtable
  s" /home/debian/sandtable/" ;
: testoutfile$@ ( -- caddr u ) \ the file name for test output info
  s" stcptest.data" ;
: stlastresultfile$@ ( -- caddr u ) \ the file name for the sandtable last result info.
  s" stcplastresult.data" ;
: stcalibration$@ ( -- caddr u ) \ the file where the calibration data is
  s" stcalibration.data" ;
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
  datapath? if pathfile$ $! stlastresultfile$@ pathfile$ $+! else nopath throw then
  pathfile$ $@ file-status swap drop false = if pathfile$ $@ delete-file throw then
  pathfile$ $@ w/o create-file throw to lastoutfid
  lastoutfid write-file throw
  lastoutfid flush-file throw
  lastoutfid close-file throw ;
: stlastresultin ( -- caddr u nflag ) \ read last result data and place in caddr u string ... nflag is true if last result is present .. nflag is false if not present
  datapath? if pathfile$ $! stlastresultfile$@ pathfile$ $+! else nopath throw then
  pathfile$ $@ file-status swap drop false = if pathfile$ $@ r/o open-file throw to lastoutfid else 0 0 false exit then
  lastoutfid slurp-fid true
  lastoutfid close-file throw ;
: stcalibrationout ( ux uy -- ) \ save the calibration data to file
  datapath? if pathfile$ $! stcalibration$@ pathfile$ $+! else nopath throw then
  pathfile$ $@ file-status swap drop false = if pathfile$ $@ delete-file throw then
  pathfile$ $@ w/o create-file throw to calibratefid
  swap s>d udto$ calibratefid write-line throw \ write x
  s>d udto$ calibratefid write-line throw \ write y
  calibratefid flush-file throw
  calibratefid close-file throw ;
: stcalibrationin ( -- ux uy nflag ) \ retreive calibration data from file
  \ nflag is true if calibration data is present and false if there is no calibartion data
  0 0 { ux uy }
  datapath? if pathfile$ $! stcalibration$@ pathfile$ $+! else nopath throw then
  pathfile$ $@ file-status swap drop false = if pathfile$ $@ r/o open-file throw to calibratefid else 0 0 false exit then
  pad 20 calibratefid read-line throw drop pad swap s>unumber? if d>s to ux else 0 0 false exit then
  pad 20 calibratefid read-line throw drop pad swap s>unumber? if d>s to uy else 0 0 false exit then
  calibratefid close-file throw
  ux uy true ;
: pidretrieve ( -- upid nflag ) \ get the pid number of the potential copy of this program running
  \ nflag is true only if upid is the retrieved pid number from saved file .
  \ nflag is false if this saved pid file does not exist or some other error happened in retreiveing it
    pidfilepath$@ file-status swap drop false = if pidfilepath$@ slurp-file else 0 false exit then
    s>unumber? if d>s true else 0 false then ;
: pidstore ( -- ) \ store the pid of this current running program
  pidfilepath$@ w/o create-file throw to pidfid
  (getpid) s>d udto$
  pidfid write-file throw
  pidfid flush-file throw
  pidfid close-file throw ;
: pidfiledelete ( -- ) \ delete the file that stores the pid for this running program
  pidfilepath$@ delete-file throw ;

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
: getstdin ( -- caddr u ) \ recieve the stdin to this code
\ note this will have a terminator in this returned string at the end of the string ... remove this if not used
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

variable messagebuffer$
: processhttp ( "ccc" -- ) \ this is called from inetd and will simply get the stdin message sent from inetd and return a message
  getstdin 1- httpinput$ $!
  httpinput$ $@ testdataout
  s" got the message" http-response type
  s" sent receipt message" testdataout
  *http* to http?cmdline?
  bye ;

: processcmdline ( "ccc" -- ) \ this is called from the command line at time of this code being executed
\ this word will take the command from the stdin and process it !
  try
    getstdin 2dup + 1- @ 255 and 10 = if 1- else  noterm throw then \ remove terminator or throw noterm error
    command$ $!
    command$ $@ messagebuffer$ $! s" < this was received at entry to processcmdline" messagebuffer$ $+! messagebuffer$ $@ testdataout
    (parse-command&submessages)
    (command$@?) if \ true condition means there is a command now process it!
      type ."  < This Command received to processcmdline!" lineending type
      (command$@?) drop . drop ."  < command$ is this long in processcmdline!" lineending type
      *cmdline* to http?cmdline?
      1 submessages$ [bind] strings []@$ drop 2dup type cr
      . ."  < first submessge length" lineending type drop
      (command$@?) drop 2dup swap drop 0 <> if \ command is not null so try and do the command
        2dup commands-instant search-wordlist false <> if \ command is a instant one ... pid saving is not needed
        swap drop swap drop \ remove command string  ( xt )
        (command$@?) drop messagebuffer$ $! s" < this instant command will be executed in processcmdline" messagebuffer$ $+! messagebuffer$ $@ testdataout
        (command$@?) drop type ." < this instant command will be executed" lineending type
        execute \ do the command
        else \ test for slow command
          commands-slow search-wordlist false <> if \ command is a slow one ... pid checking and saving needed
          \ test for running process via pid and only run if nothing else is running
            pidretrieve swap drop false = if \ no other pid running so execute the command
              (getpid) s>d udto$ messagebuffer$ $! s"  < this is the pid that will be stored now during processcmdline!" messagebuffer$ $+! messagebuffer$ $@ testdataout
              pidstore
              (command$@?) drop messagebuffer$ $! s" < this slow command will be executed in processcmdline" messagebuffer$ $+! messagebuffer$ $@ testdataout
              \ execute command here
              execute
              pidfiledelete   \ clean up the pid file that was stored before command to allow other commands to be executed
            else \ another command running so message that info
              drop \ remove the xt found above and on stack
              ." Sandtable is currently busy please wait for it to finish!" lineending type
            then
          else \ the command is not found
            ." Message recieved but the command is not valid!" lineending type
          then
        then
      else \ command is a null string
        2drop \ removed null command string
        ." Message recieved but the command is a null string!" lineending type
      then
    else
      ." Message received but there was no command present in it!"  lineending type
    then
    false
  restore
    dup false <> if
      dup s>d dto$ messagebuffer$ $! s" <this is error on output of processcmdline!" messagebuffer$ $+! messagebuffer$ $@ testdataout
      s>d dto$ type ." <this error occured!" lineending type
      pidretrieve true = if
        (getpid) = if pidfiledelete then  \ clean up pid if it is the same as this running code
      then
    else
      drop \ remove the extra false on stack
    then
    bye
  endtry
;
