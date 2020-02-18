#! /usr/local/bin/gforth
\ sandtable-commands.fs
\    Copyright (C) 2020  Philip King Smith

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

\ Command processor for sandtable.

\ Requires:
\ sandmotorapi.fs
\ Gforth-Objects/stringobj.fs
\ curl to be installed on OS

\ Revisions:
\ 02/17/2020 started coding

require sandmotorapi.fs
require Gforth-Objects/stringobj.fs


variable argcommand$
argcommand$ $! \ at this point the string is on the stack so put it here for now!
variable buffer$
variable port#$
variable server_addres$


\ s" 192.168.0.59" server_addres$ $!
s" localhost" server_addres$ $!
s" :52222" port#$ $!

: sendcurlmessage ( ucaddr u -- ucaddr1 u1 )
  s\" curl \"" buffer$ $! server_addres$ $@ buffer$ $+! port#$ $@ buffer$ $+! s" /?" buffer$ $+! buffer$ $+! s\" \"" buffer$ $+! buffer$ $@ sh-get
;

: makemessage ( -- caddr u )
  s" command=tryshget" buffer$ $!
  argcommand$ $@ buffer$ $+!
  buffer$ sendcurlmessage 2drop
;

makemessage

get-order get-current \ store order and current on stack
wordlist constant sandtable

sandtable set-current
\ put all outside sandtable commands here



set-current set-order \ restore order and current from stack

\ put all other words here
: do-arg-commands ( -- ) \ will take arg from the OS for this program and run it. There will only be one command in arg but commands can have other args
  next-arg 2dup command$ $!
  swap drop 0 =  abort" There is no command to process ... exiting now!"
  command$ $@ sandtable search-wordlist 0 <> if
    execute \ do the command
  else
    true abort" The command does not exist ... exiting now!"
  then ;

\ do-arg-commands
bye
