#! /usr/local/bin/gforth-arm

warnings off
:noname ; is bootmessage

variable query$

: lineending ( -- caddr u )
  s\" <br>\n\n" ;

: return-message ( -- )
  s\" Content-type: text/html; charset=utf-8\n\n" type
  s\" All Ok\n\n" type ;

: get-message@ ( -- )
  s" QUERY_STRING" getenv query$ $! ;
