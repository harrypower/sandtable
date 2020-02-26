#! /usr/local/bin/gforth
\ stcp.fs
\ sandtable command processor  or stcp

warnings off

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
  s\" \r" datafid write-line throw
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

\ : GET \ this is the main word to start the command parsing, interpreting, executing and message returning
\   source swap drop >in !
\   source httpinput$ $!
\   httpinput$ $@ addtodata
\   s" Command received" http-response type
\   bye ;

: processhttp
  source httpinput$ $!
  source swap drop >in !
  s" started processhttp" addtodata
  refill drop
  s" after refill" addtodata
  source httpinput$ $!
  source swap drop >in !
\  begin stdin key?-file until
\  stdin slurp-fid httpinput$ $!
  httpinput$ $@ addtodata
  s" got the message" http-response type
  s" sent recept message" addtodata
  bye
  ;

:noname ; is bootmessage
