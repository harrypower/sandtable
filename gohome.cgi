#! /usr/local/bin/gforth-arm

warnings off
:noname ; is bootmessage

: lineending ( -- caddr u )
  s\" <br>\n\n" ;

: return-message ( -- )
  s\" Content-type: text/html; charset=utf-8\n\n" type
  s\" hometest.fs starting\n\n" type ;

: rungohome ( -- )
  s" ./home/debian/sandtable/hometest.fs" system ;

return-message
rungohome
lineending type

bye
