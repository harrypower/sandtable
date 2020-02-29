#! /usr/local/bin/gforth
\ stcp.fs
\ sandtable command processor  or stcp


warnings off
:noname ; is bootmessage

0 value dataoutfid
variable httpinput$
variable dataoutfile$
s" stcptest.data" dataoutfile$ $!

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

variable junk$
: getstdin ( -- caddr u )
  junk$ $init
  500 0 do
    1 ms
    begin
      (getstdin) while
      junk$ $+!
    repeat
    2drop
  loop
  junk$ $@ ;

: processhttp ( -- ) \ this is called from inetd and will simply get the stdin message sent from inetd and return a message
  getstdin httpinput$ $!
  httpinput$ $@ addtodata
  s" got the message" http-response type
  s" sent receipt message" addtodata
  bye
  ;
