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

variable command$
variable messagebuffer$

require sandmotorapi.fs
require Gforth-Objects/stringobj.fs
require stringconvert.fs
require unix/libc.fs
require vectordraw.fs
require squaretest.fs
require patterns.fs
require triangles.fs
require spirograph.fs
require patternsdraw.fs
require stdatafiles.fs
require sandcommands.fs

:noname ; is bootmessage

\ error constants
s" no terminator found in stdin!" exception constant noterm

: processcmd ( -- ) \ caddr u is the string containing the command and possible variables to be processed
\ The command is parsed and other variables parsed then executed if it is a valid command
  try
    cmddatarecieve@ drop
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
          else \ the command is not found
            s" Message recieved but the command is not valid!" messagebuffer$ $! lineending messagebuffer$ $+! messagebuffer$ $@ type
            messagebuffer$ $@ stlastresultout
            messagebuffer$ $@ cmddatasend!
          then
        then
      else \ command is a null string
        2drop \ removed null command string
        s" Message recieved but the command is a null string!" messagebuffer$ $! lineending messagebuffer$ $+! messagebuffer$ $@ type
        messagebuffer$ $@ stlastresultout
        messagebuffer$ $@ cmddatasend!
      then
    else
      s" Message received but there was no command present in it!" messagebuffer$ $! lineending messagebuffer$ $+! messagebuffer$ $@ type
      messagebuffer$ $@ stlastresultout
      messagebuffer$ $@ cmddatasend!
    then
    false
  restore
    dup false <> if
      s>d dto$ messagebuffer$ $! s" <this is error in processcmdline of stcp.fs!" messagebuffer$ $+! messagebuffer$ $@ testdataout
      lineending messagebuffer$ $+! messagebuffer$ $@ type
      messagebuffer$ $@ stlastresultout
      messagebuffer$ $@ cmddatasend!
    else
      drop \ remove the extra false on stack
    then
  endtry ;
: startcmdreception ( -- ) \ set up conditions for recieving commands
  s" starting" putstatus
  stcmdinfile$@ file-status swap drop false =
  if stcmdinfile$@ delete-file throw then ;
: busycmdstatus ( -- ) \ set status to busy
  s" busy" putstatus ;
: readycmdstatus ( -- ) \ set status to ready to recieve
  s" ready" putstatus ;

: cmdloop ( -- ) \ wait for commands then get them then process them then repeat
  startcmdreception
  cmddatasenddelete
  begin
      startcmdreception
      readycmdstatus
      begin
        100 ms
        cmddatarecieve@ true = if
          busycmdstatus
          true
        else
          2drop false
        then
      until
      2drop
      200 ms
      processcmd
      5000 ms \ pause to allow cgi to get the info
      cmddatasenddelete
  again
;

cmdloop
