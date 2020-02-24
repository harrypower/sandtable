#! /usr/local/bin/gforth
\ stcp.fs
\ sandtable command processor  or stcp

0 value datafid
variable httpinput$

: getinput source httpinput$ $! ; \ store current input stream to eof i think

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

\ getinput
\ putstdin-out
\ http-response type

\ bye
