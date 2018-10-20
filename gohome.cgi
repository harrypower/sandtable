#! /usr/local/bin/gforth-arm

warnings off
:noname ; is bootmessage

: return-message ( -- )
  s\" Content-type: text/html; charset=utf-8\n\n" type
  s\" hometest.fs starting\n\n" type ;

\ send message to server here to start the home procedure 

return-message

bye
