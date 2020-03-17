#! /usr/local/bin/gforth-arm
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
require stdatafiles.fs

:noname ; is bootmessage

0 value dataoutfid
0 value lastoutfid
0 value calibratefid
0 value pidfid
100 value stdinwaittime
variable httpinput$
variable command$
\ error constants
s" no terminator found in stdin!" exception constant noterm

variable convert$
: udto$ ( ud -- caddr u )  \ convert unsigned double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;
: lineending ( -- caddr u ) \ return a string to produce a line end in html
  s\" <br>\n" ;

\ ******** this needs to be rewriten to the next *** line
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
\ ****************

require sandcommands.fs

variable messagebuffer$

: processcmdline ( "ccc" -- ) \ this is called from the command line at time of this code being executed
\ this word will take the command from the stdin and process it !
  try
    getstdin 2dup + 1- @ 255 and 10 = if 1- else  noterm throw then \ remove terminator or throw noterm error
    command$ $!
    command$ $@ messagebuffer$ $! s" < this was received at entry to processcmdline in stcp.fs" messagebuffer$ $+! messagebuffer$ $@ testdataout
    (parse-command&submessages)
    (command$@?) if \ true condition means there is a command now process it!
      type ."  < This Command received to processcmdline!" lineending type
      (command$@?) drop . drop ."  < command$ is this long in processcmdline of stcp.fs!" lineending type
      1 submessages$ [bind] strings []@$ drop 2dup type cr
      . ."  < first submessge length" lineending type drop
      (command$@?) drop 2dup swap drop 0 <> if \ command is not null so try and do the command
        2dup commands-instant search-wordlist false <> if \ command is a instant one ...
        swap drop swap drop \ remove command string  ( xt )
        (command$@?) drop messagebuffer$ $! s" < this instant command will be executed in processcmdline of stcp.fs" messagebuffer$ $+! messagebuffer$ $@ testdataout
        (command$@?) drop type ." < this instant command will be executed" lineending type
        execute \ do the command
        else \ test for slow command
          commands-slow search-wordlist false <> if \ command is a slow one ...
            (command$@?) drop messagebuffer$ $! s" < this slow command will be executed in processcmdline of stcp.fs" messagebuffer$ $+! messagebuffer$ $@ testdataout
            \ execute command here
            execute
            pidfiledelete   \ clean up the pid file that was stored before command to allow other commands to be executed
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
      dup s>d dto$ messagebuffer$ $! s" <this is error on output of processcmdline of stcp.fs!" messagebuffer$ $+! messagebuffer$ $@ testdataout
      s>d dto$ messagebuffer$ $! s" <this error occured!" messagebuffer$ $+! lineending messagebuffer$ $+! messagebuffer$ $@ type
    else
      drop \ remove the extra false on stack
    then
    bye
  endtry ;
