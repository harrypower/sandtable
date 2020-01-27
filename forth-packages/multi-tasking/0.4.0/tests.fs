require multi-tasking.fs

\ A few simple smoke tests for locks and atomic memory, plus basic
\ task actions.

task worker
worker construct

variable ticker

: work
    [: begin  stop  1 ticker +!  again ;] worker start-task ;

: kick
    worker awaken  ticker ? ;


\ Example: an array of 10 tasks.
10 value #tasks
create tasks  #tasks /task * allot

cell +user id

: >task ( n - task )   /task * tasks + ;

: construct-all
    #tasks 0 do  i >task construct  i dup >task id his !  loop ;

construct-all

create task-lock /mutex allot
task-lock mutex-init

variable bar

\ Many tasks, each incrementing a counter a fixed number of times.
\ Make sure no increment gets lost.

: bump ( - )
    task-lock get  bar @  1 + bar !  task-lock release  ;

: counting ( - )
    1000000 0 do  bump  loop ;

: zzz ( - )   #tasks 0 do  ['] counting i >task start-task  loop ;


\ The same thing, this time with an atomic counter.

: atomic+! ( delta addr -- )
    >r begin
        dup r@ atomic@  tuck tuck + ( delta n n delta+n )
    r@ atomic-cas = until
    r> 2drop ;

: atomic-bump ( -- n)   1 bar atomic+! ;    
    
: atomic-counting ( - )
    1000000 0 do  atomic-bump  loop ;

: atomic-zzz ( - )   #tasks 0 do  ['] atomic-counting i >task start-task  loop ;


\ Many tasks, all racing to count to 10000000. This is a stress test of
\ blocking and atomic primitives.

: race ( - t )
    begin
        task-lock get
        bar @ 10000000 < dup if
            1 bar +!
        then
        task-lock release
    0= until ;

: racing ( - )
    0 bar !
    #tasks 0 do ['] race i >task start-task loop ;

\ Wait for all tasks to finish.  NB: Nonstandard.

: join-all
    #tasks 0 do  begin i >task thread-id  his @ while  10 ms  repeat loop ;

\ Simple interval timer

: counter ( -- n )   utime 1000 um/mod nip ;
: timer ( d -- )   counter swap - . ;
