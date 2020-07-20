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

strings heap-new constant submessages$
strings heap-new constant get-variable-pairs$
strings heap-new constant junk-buffer$
strings heap-new constant othercmds$
string heap-new constant atemp$

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

: (variable-pair-string@) ( caddr u -- caddr1 u1 nflag ) \ look for string caddr u in get-variable-pairs$ and return the string that is paired with it ... nflag is true if caddr u string found and caddr1 u1 is string returned
  \ note caddr1 u1 can still be a null string or empty string if nflag is true
  \ nflag is false if caddr u string is not found in get-variable-pairs$
  0 0 { caddr u caddr1 u1 }
  get-variable-pairs$ [bind] strings $qty 0 ?do \ find caddr1 u1 string that goes with caddr u pair name
    i get-variable-pairs$ [bind] strings []@$ drop caddr u compare false = \ caddr u string is the same as found in get-variable-pairs$ string at index i
    if
      i 1+ get-variable-pairs$  [bind] strings []@$ ( caddr u nflag )
      invert unloop exit \ string pair found and exiting
    then
  2 +loop \ note variable string or value pairs are put into get-variable-pairs$ by (find-variable-pair$) word so they are in groups of two
  0 0 false \ no string pair found
;

: (variable-pair-value@) ( caddr u -- nvalue nflag ) \ look for string caddr u in get-variable-pairs$ and return its value if it is valid ... nflag is true if valid value ... nflag is false if not found or invalid
  0 0 { caddr u caddr1 u1 }
  caddr u (variable-pair-string@) swap to u1 swap to caddr1
  true = if
    u1 0 > if
      caddr1 u1 s>number? true = if \ caddr u found and its number pair is returned!
        d>s true
      else \ caddr u found but its number pair is not understandable as a number!
        2drop 0 false
      then
    else \ caddr u found but its number pair is null or empty!
      0 false
    then
  else
    0 false \ caddr u  string not found so no number to get from it !
  then
;

: lastresultdatasend ( caddr u -- ) \ takes string caddr u and sends it to stlastresultout and cmddatasend!
  2dup stlastresultout cmddatasend! ;
: lastresulttestsend ( caddr u -- ) \ send string caddr u to stlastresultout and testdataout
  2dup stlastresultout testdataout ;

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
  temp$ $@ lastresultdatasend ;

: ymin ( -- ) \ output ym-min
  s" ymin value is " temp$ $!
  ym-min 0 udto$ temp$ $+!
  lineending temp$ $+!
  temp$ $@ lastresultdatasend ;

: xmax ( -- ) \ output xm-max
  s" xmax value is " temp$ $!
  xm-max 0 udto$ temp$ $+!
  lineending temp$ $+!
  temp$ $@ lastresultdatasend ;

: ymax ( -- ) \ output ym-max
  s" ymax value is " temp$ $!
  ym-max 0 udto$ temp$ $+!
  lineending temp$ $+!
  temp$ $@ lastresultdatasend ;

: xnow ( -- )
  sandtableready? true = if
    xposition s>d dto$ temp$ $!
    s"  < x is at this value now!" temp$ $+! lineending temp$ $+!
  else
    s" No x value because calibration not done yet!" temp$ $! lineending temp$ $+!
  then
  temp$ $@ lastresultdatasend ;

: ynow ( -- )
  sandtableready? true = if
    yposition s>d dto$ temp$ $!
    s"  < y is at this value now!" temp$ $+! lineending temp$ $+!
  else
    s" No y value because calibration not done yet!" temp$ $! lineending temp$ $+!
  then
  temp$ $@ lastresultdatasend ;

: status ( -- )
  sandtableready? true = if
    xposition S>d dto$ temp$ $!
    s" < x current location!" temp$ $+! lineending temp$ $+!
    yposition s>d dto$ temp$ $+!
    s" < y current location!" temp$ $+! lineending temp$ $+!
    s" Sandtable is calibrated and is ready to receive commands!" temp$ $+! lineending temp$ $+!
  else
    s" Sandtable is not calibrated yet but is ready to receive commands!" temp$ $+! lineending temp$ $+!
  then
  temp$ $@ lastresultdatasend ;

: poweroffsandtable ( -- ) \ this will save current x and y positions and power down sandtable
  xposition yposition stcalibrationout
  s" the sandtable will now be powered off!" temp$ $! lineending temp$ $+!
  temp$ $@ lastresultdatasend
  s" shutdown +1" system \ shutdown in 1 min.
  5000 ms
  cmdstatusdelete
  cmddatasenddelete
  bye ;

: stopsand ( -- ) \ this simply returns a message saying the sandtable is stoped
\ the reason this only returns a message is because if this command runs that means the sandtable is already stopped and not drawing
  s" The sandtable is not drawing now and waiting for commands!" temp$ $! lineending temp$ $+!
  temp$ $@ lastresultdatasend ;

: stopsandserver ( -- ) \ this will save current x and y positions and stop this sand command processing service
  xposition yposition stcalibrationout
  s" the sandtable command processor will now be terminated!"  temp$ $! lineending temp$ $+!
  temp$ $@ lastresultdatasend
  5000 ms \ pause to allow cgi to get the info
  cmdstatusdelete
  cmddatasenddelete
  bye ;

: lastresult ( -- )  \ output the last result string
  stlastresultin true = if
    temp$ $! lineending temp$ $+!
  else
    2drop
    s" There was no last result to display!" temp$ $! lineending temp$ $+!
  then
  temp$ $@ cmddatasend! ;



commands-slow set-current
\ place slower commands-slow sandtable commands here
: testslowcmd ( -- ) \ for testing slow commands
  s" at testslowcmd" temp$ $! lineending temp$ $+! temp$ $@ testdataout
  temp$ $@ lastresultdatasend ;

: fastcalibration ( -- ) \ perform the quickstart function from sandtableapi.fs
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
  temp$ $@ lastresultdatasend ;

: retrieve-fastcalibration ( -- ) \ perform the quickstart function from sandtableapi.fs
  (find-variable-pair$)
  stcalibrationin ( xposition yposition nflag )
  true = if
    s" Got x and y postions from storage for fastcalibration" temp$ $! lineending temp$ $+! temp$ $@ testdataout
    2dup quickstart false = if ( xposition yposition )
        stcalibrationout ( )
        s" Fastcalibration done... x & y calibartion data saved" temp$ $! lineending temp$ $+! temp$ $@ testdataout
      else
        s" Fastcalibration failed at quickstart for some reason!" temp$ $! lineending temp$ $+! temp$ $@ testdataout
      then
  else
    s" xposition and yposition not retrieved from storage!" temp$ $!
    lineending temp$ $+!
    s" fastcalibration not completed!" temp$ $+!
    lineending temp$ $+!
  then
  temp$ $@ lastresultdatasend ;

: fullcalibration ( -- ) \ perform the configure-stuff and dohome words from sandtableapi.fs
  s" Sandtable will be configured now please observer the table for correct operation!"  temp$ $! lineending temp$ $+!
  s" Note this may take several minutes!" temp$ $+! lineending temp$ $+!
  temp$ $@ lastresultdatasend
  configure-stuff false = if
    s" Configuration done and ok" temp$ $! lineending temp$ $+!
    dohome true = if
      0 0 stcalibrationout
      s" Home finding routine dohome finished and sandtable is now ready to use!" temp$ $+! lineending temp$ $+!
    else
      s" dohome home finding routine failed for some reason!" temp$ $+! lineending temp$ $+!
    then
  else
    s" Configuration failed for some reason during fullcalibration!" temp$ $!
  then
  temp$ $@ lastresultdatasend ;

: drawaline ( -- ) \ perform the drawline word on sandtable
  0 0 0 0 0 { x1 y1 x2 y2 nflag }
  (find-variable-pair$)
  s" x1" (variable-pair-value@) to nflag to x1
  s" y1" (variable-pair-value@) nflag and to nflag to y1
  s" x2" (variable-pair-value@) nflag and to nflag to x2
  s" y2" (variable-pair-value@) nflag and to nflag to y2
  nflag true = if
    s" X1,Y1 X2,Y2 received so drawaline will procede with drawing!" temp$ $! lineending temp$ $+!
    temp$ $@ testdataout
    temp$ $@ lastresultdatasend
    x1 y1 x2 y2 drawline
    case
      200 of s" drawline performed correctly without any errors!" temp$ $+! lineending temp$ $+!
      endof
      201 of  s" drawline has calculated sandtable quardinates incorrectly so error 201 was issued at movetoxy when it normaly never will throw this error!" temp$ $+! lineending temp$ $+!
      endof
      202 of s" Sandtable not configures or calibrated yet!  Sandtable did nothing as a result!" temp$ $+! lineending temp$ $+!
      endof
    endcase
  else
    s" A variable missing so drawaline will do nothing at this time!" temp$ $! lineending temp$ $+!
  then
  temp$ $@ lastresultdatasend ;

: manylines ( -- ) \ perform the lines word on sandtable
  0 0 0 0 0 { x y angle quantity nflag }
  (find-variable-pair$)
  s" x" (variable-pair-value@) to nflag to x
  s" y" (variable-pair-value@) nflag and to nflag to y
  s" angle" (variable-pair-value@) nflag and to nflag to angle
  s" quantity" (variable-pair-value@) nflag and to nflag to quantity
  sandtableready? true = if
    nflag true = if
      s" X,Y,Angle,Quantity received so manylines will procede with drawing!" temp$ $! lineending temp$ $+!
      temp$ $@ testdataout
      temp$ $@ lastresultdatasend
      x y angle quantity lines
      s" lines performed correctly without any errors!" temp$ $+! lineending temp$ $+!
    else
      s" A variable missing so manylines will do nothing at this time!" temp$ $! lineending temp$ $+!
    then
  else
    s" Sandtable not calibrated yet so manylines will not execute!" temp$ $! lineending temp$ $+!
  then
  temp$ $@ lastresultdatasend ;

: zigzag ( -- ) \ perform zigzag-line
  0 0 0 { steps hv nflag }
  (find-variable-pair$)
  s" steps" (variable-pair-value@) to nflag to steps
  s" hv" (variable-pair-value@) nflag and to nflag to hv
  sandtableready? true = if
    nflag true = if
      s" steps and hv variables received so zigzag will procede with drawing!" temp$ $! lineending temp$ $+!
      temp$ $@ testdataout
      temp$ $@ lastresultdatasend
      hv 0 >= hv 1 <= and if
        steps hv zigzag-line
      else
        304
      then
      case
        300 of s" zigzag performed correctly without any errors!" temp$ $+! lineending temp$ $+!
        endof
        301 of  s" zigzag failed drawing at first part of drawing loop!" temp$ $+! lineending temp$ $+!
          closedown
        endof
        302 of s" zigzag failed drawing at second part of drawing loop!" temp$ $+! lineending temp$ $+!
          closedown
        endof
        303 of s" zigzag had some error with border word and as a result bailed at some unknow part of the drawing" temp$ $+! lineending temp$ $+!
          closedown
        endof
        304 of s" Only 0 or 1 values can be used for hv! Sandtable will not draw anything!" temp$ $+! lineending temp$ $+!
        endof
      endcase
    else
      s" A variable missing so zigzag will do nothing at this time!" temp$ $! lineending temp$ $+!
    then
  else
    s" Sandtable not calibrated yet so zigzag will not execute!" temp$ $! lineending temp$ $+!
  then
  temp$ $@ lastresultdatasend ;

: gotoxy ( -- ) \ perform the movetoxy word on sandtable
  (find-variable-pair$)
  s" x" (variable-pair-value@)
  s" y" (variable-pair-value@)
  rot and true = if
    s" Received x and y values.... performing gotoxy now!" temp$ $! lineending temp$ $+!
    temp$ $@ testdataout
    temp$ $@ lastresultdatasend
    movetoxy
    case
      200 of s" Gotoxy performed correctly without any errors!" temp$ $+! lineending temp$ $+!
      endof
      201 of  s" X or Y values are not on the sandtable so sandtable did nothing!" temp$ $+! lineending temp$ $+!
      endof
      202 of s" Sandtable not configures or calibrated yet!  Sandtable did nothing as a result!" temp$ $+! lineending temp$ $+!
      endof
    endcase
  else
    2drop
    s" Did not receive x or y value with gotoxy command! Sandtable will stay put for now!" temp$ $! lineending temp$ $+! temp$ $@ testdataout
  then
  temp$ $@ lastresultdatasend ;

: gotox ( -- ) \ perform the movetox word on sandtable
  (find-variable-pair$)
  s" x" (variable-pair-value@)
  true = if
    s" Recieved the X value and will now move the sandtable to the absolute X location now!" temp$ $! lineending temp$ $+!
    temp$ $@ lastresultdatasend
    movetox
    case
      200 of s" Gotox performed correctly without any errors!" temp$ $+! lineending temp$ $+!
      endof
      201 of  s" X value not on the sandtable so sandtable did nothing!" temp$ $+! lineending temp$ $+!
      endof
      202 of s" Sandtable not configures or calibrated yet!  Sandtable did nothing as a result!" temp$ $+! lineending temp$ $+!
      endof
    endcase
  else
    drop
    s" Did not receive x value so sandtable will stay put for now!" temp$ $! lineending temp$ $+!
  then
  temp$ $@ lastresultdatasend ;

: gotoy ( -- ) \ perform the movetoy word on sandtable
  (find-variable-pair$)
  s" y" (variable-pair-value@)
  true = if
    s" Recieved the y value and will now move the sandtable to the absolute Y location now!" temp$ $! lineending temp$ $+!
    temp$ $@ lastresultdatasend
    movetoy
    case
      200 of s" GotoY performed correctly without any errors!" temp$ $+! lineending temp$ $+!
      endof
      201 of  s" Y value not on the sandtable so sandtable did nothing!" temp$ $+! lineending temp$ $+!
      endof
      202 of s" Sandtable not configures or calibrated yet!  Sandtable did nothing as a result!" temp$ $+! lineending temp$ $+!
      endof
    endcase
  else
    drop
    s" Did not receive Y value so sandtable will stay put for now!" temp$ $! lineending temp$ $+!
  then
  temp$ $@ lastresultdatasend ;

: drawavector ( -- ) \ perform the drawvector word on sandtable
  0 0 0 0 0 0 0 { x y angle distance x1 y1 nflag }
  (find-variable-pair$)
  s" x" (variable-pair-value@) to nflag to x
  s" y" (variable-pair-value@) nflag and to nflag to y
  s" angle" (variable-pair-value@) nflag and to nflag to angle
  s" distance" (variable-pair-value@) nflag and to nflag to distance
  sandtableready? true = if
    nflag true = if
      s" Drawing vector starting at x y with angle and distance given!" temp$ $! lineending temp$ $+!
      temp$ $@ lastresultdatasend
      x y angle distance drawvector to nflag to y1 to x1
      nflag
      case
        200 of s" Drawvector performed correctly without any errors!" temp$ $+! lineending temp$ $+!
          s" Current x is " temp$ $+! x1 s>d dto$ temp$ $+! lineending temp$ $+!
          s" Current y is " temp$ $+! y1 s>d dto$ temp$ $+! lineending temp$ $+!
        endof
        201 of  s" drawline has calculated sandtable quardinates incorrectly at drawvector command so error 201 was issued at movetoxy when it normaly never will throw this error!" temp$ $+! lineending temp$ $+!
        endof
        202 of s" Sandtable not configures or calibrated yet!  Sandtable did nothing as a result!" temp$ $+! lineending temp$ $+!
        endof
      endcase
    else
      s" x,y,angle,distance values missing!" temp$ $!
      lineending temp$ $+!
      s" Sandtable will do nothing!" temp$ $+!
      lineending temp$ $+!
    then
  else
    s" Sandtable not configures or calibrated yet!  Sandtable did nothing as a result!" temp$ $!
    lineending temp$ $+!
  then
  temp$ $@ lastresultdatasend ;

\ these commands are from squaretest.fs
s" nsquare" othercmds$ bind strings !$x
s" nrotsquare" othercmds$ bind strings !$x
s" nnrotsquare" othercmds$ bind strings !$x
s" nsquare2" othercmds$ bind strings !$x
s" rndsquares" othercmds$ bind strings !$x
s" rndsquares2" othercmds$ bind strings !$x
\ these commands are from triangles.fs
s" triangle" othercmds$ bind strings !$x
s" triangle2" othercmds$ bind strings !$x
s" trianglecenter" othercmds$ bind strings !$x
s" ntrianglecenter" othercmds$ bind strings !$x
s" equaltrianglecenter" othercmds$ bind strings !$x
s" nequaltrianglecenter" othercmds$ bind strings !$x
\ these commands are from patterns.fs
s" rndstar" othercmds$ bind strings !$x
s" rndstar2" othercmds$ bind strings !$x
s" linestar" othercmds$ bind strings !$x
s" circle" othercmds$ bind strings !$x
s" arc" othercmds$ bind strings !$x
s" circle2" othercmds$ bind strings !$x
s" concentric-circles" othercmds$ bind strings !$x
s" circle-circles" othercmds$ bind strings !$x
s" circle-spin" othercmds$ bind strings !$x
s" ncircle-spin" othercmds$ bind strings !$x
\ these commands from pirograph.fs
s" threeleggedspiral" othercmds$ bind strings !$x
\ place other commands here!

: othercmds ( -- ) \ this will parse thecmd for a sub command to execute.  s0 to s11 are passed with this command for stack values that this sub command needs
  0 0 false { caddr u nflag }
  (find-variable-pair$)
  sandtableready? true = if
    s" thecmd" (variable-pair-string@) swap to u swap to caddr
    true = if
      s" Command is " temp$ $!
      caddr u temp$ $+! lineending temp$ $+!
      othercmds$ [bind] strings $qty 0 ?do
        i othercmds$ [bind] strings []@$ drop caddr u compare false = if
          true to nflag
          leave
        then
      loop
      nflag if
        caddr u temp$ $+! s"  is a valid command and will be executed!" temp$ $+! lineending temp$ $+!
        temp$ $@ lastresultdatasend
        try
          12 0 ?do
            s" s" atemp$ [bind] string !$
            i s>d dto$ atemp$ [bind] string !+$
            atemp$ [bind] string @$ (variable-pair-value@) false = if drop then
          loop
          caddr u find-name name>interpret execute
          false
        restore
          dup false = if
            drop caddr u temp$ $+! s"  command was executed and finished with no errors!" temp$ $+! lineending temp$ $+!
          else
            caddr u temp$ $+! s"  command had the following error: " temp$ $+! s>d dto$ temp$ $+! lineending temp$ $+!
          then
        endtry
      else
        caddr u temp$ $+! s"  is not a valid command nothing will be executed!" temp$ $+! lineending temp$ $+!
      then
    else
      s" thecmd variable not found so nothing is executed!" temp$ $! lineending temp$ $+!
    then

    s" s0" (variable-pair-value@)
    true = if
      s" s0 is " temp$ $+! s>d dto$ temp$ $+! lineending temp$ $+!
    else
      drop s" s0 is null!" temp$ $+! lineending temp$ $+!
    then
    s" s0" (variable-pair-string@)
    true = if
      s" s0 as string is " temp$ $+!
      temp$ $+! lineending temp$ $+!
    else
      s" s0 not found as string " temp$ $+! lineending temp$ $+!
      2drop
    then
  else
    s" Sandtable not configures or calibrated yet!  Sandtable did nothing as a result!" temp$ $!
    lineending temp$ $+!
  then

  temp$ $@ lastresultdatasend ;

set-current set-order
