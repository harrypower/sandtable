#! /usr/local/bin/gforth
\ stcp.fs
\ sandtable command processor  or stcp

0 value datafid
variable httpinput$

: ?cr ( -- ) \ i think this looks for a cr in the input stream to allow refill-loop below to find the exit
  #tib @ 1 >= IF  source 1- + c@ #cr = #tib +!  THEN ;

: refill-loop ( -- flag ) \ this refills the input from source and interprets the words it finds or throws
  base @ >r base off
  BEGIN  refill ?cr  WHILE  ['] interpret catch drop  >in @ 0=  UNTIL
  true  ELSE  false  THEN  r> base ! ;

: getinput ( -- flag ior )  \ need to ajdust some words in here and test it
  infile-id push-file loadfile !
  loadline off  blk off
  ( commands 1 set-order  command? on )  \ this would need to be set up to have a GET command in a wordlist
  ['] refill-loop catch
  ( only forth also )
  pop-file
;

: (doinputread) \ just testing... note i would need to use a wordlist with only the GET command for the real sandtable command
  >in @ . ." >in before" cr
  getinput
  >in @ . ." >in after" cr
  source httpinput$ $!
  httpinput$ $@ dump cr
  s" done" type
  bye ;


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
