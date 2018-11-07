#! /usr/local/bin/gforth-arm

warnings off
:noname ; is bootmessage

variable query$
variable test$
variable apache$s
variable output$
0 value fid
0 value infid
0 value outfid

: getmessage ( -- ucaddr u )
  s" /run/sandtableout" r/o open-file throw to infid
  pad pad 80 infid read-file throw
  infid close-file throw ;

: putmessage ( ucaddr u -- )
  s" /run/sandtablein" w/o open-file throw to outfid
  outfid write-file throw
  outfid flush-file throw
  outfid close-file throw ;

: lineending ( -- caddr u )
  s\" <br>\n\n" ;

: return-message ( -- )
  s\" Content-type: text/html; charset=utf-8\n\n" type
  query$ $@ type
  apache$s $@ type
  test$ $@ type lineending type
  test$ $@ putmessage
  getmessage s" The message recieved is: " type type lineending type
  s\" All Ok\n\n" type ;

: get-get-message ( -- )
  s" QUERY_STRING is:" query$ $! s" QUERY_STRING" getenv query$ $+! lineending query$ $+!
  s" QUERY_STRING" getenv test$ $! ;

: get-apache-stuff ( -- )
  s" REMOTE_ADDR is :" apache$s $! s" REMOTE_ADDR" getenv apache$s $+! lineending apache$s $+!
  s" REQUEST_METHOD is :" apache$s $+! s" REQUEST_METHOD" getenv apache$s $+! lineending apache$s $+!
  s" HTTP_REFERER is:" apache$s $+! s" HTTP_REFERER" getenv apache$s $+! lineending apache$s $+!
  s" HTTP_HOST is:" apache$s $+! s" HTTP_HOST" getenv apache$s $+! lineending apache$s $+!
  s" SERVER_SOFTWARE is:" apache$s $+! s" SERVER_SOFTWARE" getenv  apache$s $+! lineending apache$s $+!
;

: prep-message ( -- )
  get-get-message
  get-apache-stuff ;

prep-message

return-message
bye
