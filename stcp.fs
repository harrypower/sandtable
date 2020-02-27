#! /usr/local/bin/gforth
\ stcp.fs
\ sandtable command processor  or stcp

warnings off
:noname ; is bootmessage

0 value datafid
variable httpinput$

variable convert$
: udto$ ( ud -- caddr u )  \ convert unsigned double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;
: lineending ( -- caddr u ) \ return a string to produce a line end in html
  s\" <br>\n" ;

: opendata ( -- )
  s" /home/pks/sandtable/stcptest.data" file-status swap drop false = if
    s" /home/pks/sandtable/stcptest.data" r/w open-file throw
    to datafid
  else
    s" /home/pks/sandtable/stcptest.data" r/w create-file throw
    to datafid
  then ;

: addtodata ( caddr u -- )
  opendata
  datafid file-size throw
  datafid reposition-file throw
  utime udto$ datafid write-line throw
  datafid write-line throw
  datafid flush-file throw
  datafid close-file throw ;

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

: ?cr ( -- )
  #tib @ 1 >= IF  source 1- + c@ #cr = #tib +!  THEN ;
: refill-loop ( -- flag )
  base @ >r base off
  BEGIN  refill ?cr  WHILE  ['] interpret catch drop  >in @ 0=  UNTIL
  true  ELSE  false  THEN  r> base ! ;

variable junk$
variable inbuffer
1024 allocate throw inbuffer !
: processhttp
\  source httpinput$ $!
\  source swap drop >in !
\  s" started processhttp" addtodata
\  refill s>d dto$ junk$ $! s"  < refill" junk$ $+!
\  junk$ $@ addtodata
  infile-id push-file loadfile ! loadline off blk off
  ( need some wordlist stuff here )
  s" after infile-id" addtodata
  ['] refill-loop catch
  s" after refill-loop" addtodata
  ( then use only forth also here )
  \ pop-file
\  source httpinput$ $!
\  source swap drop >in !
\  begin stdin key?-file until
\  stdin slurp-fid httpinput$ $!
  inbuffer @ 1024 stdin read-file throw inbuffer @ swap httpinput$ $!
  httpinput$ $@ addtodata
  s" got the message" http-response type
  s" sent recept message" addtodata
  bye
  ;
