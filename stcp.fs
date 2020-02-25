#! /usr/local/bin/gforth
\ stcp.fs
\ sandtable command processor  or stcp

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
  s" stcptest.data" file-status swap drop false = if
    s" stcptest.data" r/w open-file throw
    to datafid
  else
    s" stcptest.data" r/w create-file throw
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

: putstdin-out
  opendata
  httpinput$ $@ addtodata ;

variable tempheader$
: http-header ( -- caddr u )
  s\" HTTP/1.1 200 OK\r\n" tempheader$ $!
  s\" Connection: close\r\n" tempheader$ $+!
  s\" Server: Sandserver 0.1\r\n" tempheader$ $+!
  s\" Accept-Ranges: bytes\r\n" tempheader$ $+!
  s\" Content-type: text/html; charset=utf-8\r\n" tempheader$ $+!
  tempheader$ $@ ;

variable tempresponse$
: http-response ( caddr u -- caddr' u' ) \ caddr u is the message string to send
  { caddr u }
  http-header tempresponse$ $!
  s\" \r\n" tempresponse$ $+!
  s\" \r\n" tempresponse$ $+!
  caddr u tempresponse$ $+!
  s\" \r\n\r\n" tempresponse$ $+!
  tempresponse$ $@ ;

: (doinputread) \ just testing... note i would need to use a wordlist with only the GET command for the real sandtable command
  >in @ . ." >in #1" cr
  source-id . ." source-id #1" cr
  source httpinput$ $!
  httpinput$ $@ dump cr
  source swap drop >in !
  s" done" type cr
  bye ;
