#! /usr/local/bin/gforth-arm
\ testcgi.fs

warnings off

:noname ; is bootmessage

: lineending ( -- caddr u ) \ return a string to produce a line end in html
  s\" <br>\n" ;

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
  200 0 do
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
    s" < this message was received!" httpinput$ $+! lineending httpinput$ $+!
    s" HOME" getenv httpinput$ $+! s" < HOME env " httpinput$ $+! lineending httpinput$ $+!
    httpinput$ $@ type
    bye
  restore
    s" here with some error" type
  endtry
   ;
