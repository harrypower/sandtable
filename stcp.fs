#! /usr/local/bin/gforth
\ stcp.fs
\ sandtable command processor  or stcp


warnings off

require sandmotorapi.fs  \ note this sandmotorapi.fs stuff is not executed in this code but is used to get sandtable data sandtable-commands.fs will execute the sandtable motors
require Gforth-Objects/stringobj.fs
require unix/libc.fs

:noname ; is bootmessage

0 value dataoutfid
200 value stdinwaittime
variable httpinput$
variable dataoutfile$
s" stcptest.data" dataoutfile$ $!
strings heap-new constant submessages$
strings heap-new constant get-variable-pairs$

variable pathfile$
: pcdatapath ( -- cadd u ) \ return path file string to output data for pc testing
  s" /home/pks/sandtable/" ;
: bbbdatapath ( -- cadd u ) \ return path file string to output data for BBB sandtable
  s" /home/debian/sandtable/" ;
: datapath ( -- caddr u nflag ) \ caddr u is the correct path to use if nflag is true.... if nflag is false caddr u is 0 0
  pcdatapath file-status swap drop false = if
    pcdatapath pathfile$ $!
    dataoutfile$ $@ pathfile$ $+! pathfile$ $@ true
  else
    bbbdatapath file-status swap drop false = if
      bbbdatapath pathfile$ $!
      dataoutfile$ $@ pathfile$ $+! pathfile$ $@ true
    else
      0 0 false
    then
  then ;

: parse-command&submessages ( -- ) \ take command$ and parse command and submessages out of it
  submessages$ [bind] strings destruct
  submessages$ [bind] strings construct
  s" &" command$ $@ submessages$ [bind] strings split$>$s ;

variable convert$
: udto$ ( ud -- caddr u )  \ convert unsigned double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;
: lineending ( -- caddr u ) \ return a string to produce a line end in html
  s\" <br>\n" ;

: opendata ( -- )
  datapath { caddr u nflag }
  nflag true = if
    caddr u file-status swap drop false = if
      caddr u r/w open-file throw
      to dataoutfid
    else
      caddr u r/w create-file throw
      to dataoutfid
    then
  else
    210 abort" the bbbdatapath or the pcdatapath are not present to store the stcp data"
  then ;

: addtodata ( caddr u -- )
  opendata
  dataoutfid file-size throw
  dataoutfid reposition-file throw
  utime udto$ dataoutfid write-line throw
  dataoutfid write-line throw
  dataoutfid flush-file throw
  dataoutfid close-file throw ;

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
  httpinput$ $@ addtodata
  s" got the message" http-response type
  s" sent receipt message" addtodata
  bye ;

: processcmdline ( "ccc" -- ) \ this is called from the command line at time of this code being executed
\ this word will take the command from the stdin and process it !
  getstdin httpinput$ $!
  httpinput$ $@ addtodata
  s\" Message received\r\n" type
  bye ;
