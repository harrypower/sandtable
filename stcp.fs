#! /usr/local/bin/gforth
\ stcp.fs
\ sandtable command processor  or stcp

0 value logfid
variable httpinput$

: getinput source httpinput$ $! \ store current input stream to eof i think ;

: opendata ( -- )
  s" stcptest.data" file-status swap drop false = if
    s" stcptest.data" r/w open-file throw
    to logfid
  else
    s" stcptest.data" r/w create-file throw
    to logfid
  then ;

: addtolog ( caddr u -- )
  openlog
  logfid file-size throw
  logfid write-line throw
  logfid flush-file throw
  logfid close-file throw ;

: putstdin-out
  opendata
  httpinput$ $@ addtolog ;

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

putstdin-out
http-response type

bye
