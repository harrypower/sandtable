\ sandcommands.fs

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

\ commands used by socket server to control sandtable

\ Requires:
\ will be included by stcp.fs and not to be used on its own

\ Revisions:
\ 03/02/2020 started coding


variable shjunk$
: sh-stcp ( caddr u -- caddr1 u1)
  { caddr u }
  s\" nohup gforth -e \"s\\\" " shjunk$ $!
  caddr u shjunk$ $+!
  s\" \\\"\" sandtable-commands.fs > sandtable-command.data 2>&1 &" shjunk$ $+! \ note the last & here is to disconnect this new process from the socketserver process
  shjunk$ $@ sh-get ;

strings heap-new constant submessages$
strings heap-new constant get-variable-pairs$
strings heap-new constant junk-buffer$
: remove\r\n ( caddr u -- ) \ remove carrage return and linefeed from caddr u string
\ the output is in junk-buffer$ and is a strings object
\ the junk-buffer$ object can contain all the strings that were split when \r\n was found
\ note the last string in this junk-buffer$ could be a null string or string of zero size
  junk-buffer$ [bind] strings destruct
  junk-buffer$ [bind] strings construct
  s\" \r\n" 2swap
  junk-buffer$ [bind] strings split$>$s ;
: (parse-command&submessages) ( -- ) \ take command$ and parse command and submessages out of it
  submessages$ [bind] strings destruct
  submessages$ [bind] strings construct
  s" &" command$ $@ submessages$ [bind] strings split$>$s ;
: (command$@?) ( -- caddr u nflag ) \ get the command from submessages... nflag is true if command found... nflag is false if no command found
  0 submessages$ [bind] strings []@$ false = if
  s" command=" search true = if
      8 - swap 8 + swap true \ this is the command ... ***note it still can be a null string ***
    else
      false \ no command
    then
  else
    false \ no command
  then ;
: (find-variable-pair$) ( -- ) \ extract variable pairs from submessages$ strings
  0 { nqty }
  submessages$ [bind] strings $qty to nqty
  get-variable-pairs$ [bind] strings destruct
  get-variable-pairs$ [bind] strings construct
  nqty 1 > if \ there are some variable pairs to parse
    nqty 1 do
      s" =" i submessages$ [bind] strings []@$ drop
      get-variable-pairs$ [bind] strings split$>$s
    loop
  then ;
: (variable-pair-value@) ( caddr u - nvalue nflag ) \ look for string caddr u in get-variable-pairs$ and return its value if it is valid ... nflag is true if valid value ... nflag is false if not found or invalid
  0 false { caddr u nvalue nflag }
  get-variable-pairs$ [bind] strings $qty 0 ?do \ find x variable
    i get-variable-pairs$ [bind] strings []@$ drop caddr u compare false = \ caddr u string is the same as found in get-variable-pairs$ string at index i
    if
      i 1+ get-variable-pairs$  [bind] strings []@$ ( n n caddr u nflag )
      false = if ( n n caddr u )
        s>number? true = if ( n n d )
          d>s to nvalue true to nflag
        else ( n n d )
          2drop false to nvalue false to nflag
        then
      else ( n n caddr u )
        2drop false to nflag
      then ( n n )
      leave
    then
  2 +loop \ note variable value pairs are put into get-variable-pairs$ by (find-variable-pair$) word so they should be in groups of two
  nvalue nflag ;

get-order get-current

wordlist constant commands-slow
wordlist constant commands-instant

variable  temp$
commands-instant set-current
\ place instant commands here
\ instant commands are commands that can be run at the same time as sandtable motor commands because they will not only return information
: xmin ( -- ) \ output xm-min
  s" xmin value is " temp$ $!
  xm-min 0 udto$ temp$ $+!
  lineending temp$ $+!
  temp$ $@ type
  temp$ $@ stlastresultout ;

: ymin ( -- ) \ output ym-min
  s" ymin value is " temp$ $!
  ym-min 0 udto$ temp$ $+!
  lineending temp$ $+!
  temp$ $@ type
  temp$ $@ stlastresultout ;

: xmax ( -- ) \ output xm-max
  s" xmax value is " temp$ $!
  xm-max 0 udto$ temp$ $+!
  lineending temp$ $+!
  temp$ $@ type
  temp$ $@ stlastresultout ;

: ymax ( -- ) \ output ym-max
  s" ymax value is " temp$ $!
  ym-max 0 udto$ temp$ $+!
  lineending temp$ $+!
  temp$ $@ type
  temp$ $@ stlastresultout ;

: xnow ( -- )
  stcalibrationin swap drop true = if
    s>d dto$ temp$ $!
    s"  < x is at this value now!" temp$ $+! lineending temp$ $+!
  else
    drop
    s" No x value because calibration not done yet!" temp$ $! lineending temp$ $+!
  then
  temp$ $@ type
  temp$ $@ stlastresultout ;

: ynow ( -- )
  stcalibrationin rot drop true = if
    s>d dto$ temp$ $!
    s"  < y is at this value now!" temp$ $+! lineending temp$ $+!
  else
    drop
    s" No y value because calibration not done yet!" temp$ $! lineending temp$ $+!
  then
  temp$ $@ type
  temp$ $@ stlastresultout ;

: status ( -- )
;

: stopsandserver ( -- )
  ;

: lastresult ( -- )  \ output the last result string
  stlastresultin true = if
    type
  else
    s" There was no last result to display!" type lineending type
  then ;


commands-slow set-current
\ place slower commands-slow sandtable commands here

: fastcalibration ( -- ) \ perform the quickstart function from sandtableapi.fs
http?cmdline? *http* = if
then
http?cmdline? *cmdline* = if
  (find-variable-pair$)
  s" xquick" (variable-pair-value@)
  s" yquick" (variable-pair-value@)
  rot and true = if
    s" Found xquick and yquick for fastcalibration" temp$ $! lineending temp$ $+! temp$ $@ testdataout
    quickstart false = if
        s" xquick" (variable-pair-value@)
        s" yquick" (variable-pair-value@)
        rot 2drop
        stcalibrationout
        s" Fastcalibration done... x & y calibartion data saved" temp$ $! lineending temp$ $+! temp$ $@ testdataout
      else
        s" Fastcalibration failed at quickstart for some reason!" temp$ $! lineending temp$ $+! temp$ $@ testdataout
      then
  else
    s" xquick or yquick values missing!" temp$ $!
    lineending temp$ $+!
    s" fastcalibration not completed!" temp$ $+!
    lineending temp$ $+!
  then
  temp$ $@ type
  temp$ $@ stlastresultout
then ;

: configuresandtable ( -- ) \ perform the configure-stuff and dohome words from sandtableapi.fs
;

: gotoxy ( -- ) \ perform the movetoxy word from
;


set-current set-order
