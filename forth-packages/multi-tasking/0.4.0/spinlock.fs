\ Simple spin locks
\
\ These are best on single-processor round-robin multi-tasking
\ systems.

cell constant /mutex

: mutex-init ( lock -- )   0 swap atomic! ;

: get ( lock -- )
    >r
    assert( r@ @ this-task <> )
    begin  pause  0 this-task r@ atomic-cas  0= until
    assert( r@ @ this-task = )
    r> drop
;

: release ( lock -- )
    assert( dup @ this-task = )
    0 swap atomic! ;

