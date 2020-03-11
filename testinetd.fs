#! /usr/local/bin/gforth
\ testinetd.fs

warnings off

:noname ; is bootmessage

variable tmppath$
\ s" echo $HOME" sh-get type cr
\ s" echo $HOME" sh-get
\ s\" GFORTHCCPATH=\'" tmppath$ $!
\ tmppath$ $+! s\" \'" tmppath$ $+!
\ tmppath$ $@ system
\ s\" GFORTHCCPATH=\'/root\'" sh-get 2drop
\ s" export GFORTHCCPATH" sh-get 2drop
\ s" printenv GFORTHCCPATH" sh-get type cr
\ s" GFORTHCCPATH" getenv dump cr

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
: getstdin ( -- caddr u ) \ recieve the stdin to this code
\ note this will have a terminator in this returned string at the end of the string ... remove this if not used
  sdtin$ $init
  400 0 do
    1 ms
    begin
      (getstdin) while
      sdtin$ $+!
    repeat
    2drop
  loop
  sdtin$ $@ ;

variable httpinput$
variable messagebuffer$
: processhttp ( "ccc" -- ) \ this is called from inetd and will simply get the stdin message sent from inetd and return a message
  try
    getstdin 1- httpinput$ $!
    s" got the message" http-response type
    bye
  restore
    s" here with some error" http-response type 
  endtry
   ;
