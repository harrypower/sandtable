#! /usr/local/bin/gforth-arm

warnings off
:noname ; is bootmessage
variable query$
variable apache$s
variable output$
0 value fid

: lineending ( -- caddr u )
  s\" <br>\n\n" ;

: return-message ( -- )
  s\" Content-type: text/html; charset=utf-8\n\n" type
  query$ $@ type
  apache$s $@ type
  s\" All Ok\n\n" type ;

: get-get-message ( -- )
  s" QUERY_STRING is:" query$ $! s" QUERY_STRING" getenv query$ $+! lineending query$ $+! ;

: get-apache-stuff ( -- )
  s" REMOTE_ADDR is :" apache$s $! s" REMOTE_ADDR" getenv apache$s $+! lineending apache$s $+!
  s" REQUEST_METHOD is :" apache$s $+! s" REQUEST_METHOD" getenv apache$s $+! lineending apache$s $+!
  s" HTTP_REFERER is:" apache$s $+! s" HTTP_REFERER" getenv apache$s $+! lineending apache$s $+!
  s" HTTP_HOST is:" apache$s $+! s" HTTP_HOST" getenv apache$s $+! lineending apache$s $+!
  s" SERVER_SOFTWARE is:" apache$s $+! s" SERVER_SOFTWARE" getenv  apache$s $+! lineending apache$s $+!
;

: prep-message ( -- )
  get-get-message
  get-apache-stuff
  query$ $@ output$ $! apache$s $@ output$ $+! ;

prep-message

: save-message ( -- )
  s" /run/cgitest.tmp" file-status swap drop false <> if s" touch /run/cgitest.tmp" system then
  \ note the above will never touch the file /run/cgitest.tmp because cgi always runs as nobody so it does not have permision to touch a file
  \ So in order for this cgitest.tmp file to have stuff stored in it the file must exist first and have write permision for nobody !
  s" /run/cgitest.tmp" w/o open-file swap to fid
  false = if
    output$ $@ fid write-file drop
    fid flush-file drop
    fid close-file drop
  then ;

\ save-message
return-message
bye
