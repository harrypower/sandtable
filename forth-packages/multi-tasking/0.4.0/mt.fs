\ Proposed multi-tasking wordset for Forth, reference implementation.

\ The core user-visbile wordset

: +user ( n "<spaces>name” -- )
    header reveal  douser,  aligned uallot , ;

task-state-size aligned
     +user status
cell +user 'task    \ points to our own task
cell +user thread-id
cell +user run-xt

cell +user lock-nodes \ See clh.fs

\ The current user pointer
' next-task alias up@ ( -- addr )

: his ( task addr1 -- addr2 )   up@ - swap @ + ;

: stop ( -- )           status task-stop ;
: awaken ( task -- )    status his task-awaken ;

: pause ( -- )   ;

cell constant /task
: task ( "name" -- )    create 0 , ;

\ Return the address of a user VALUE.
: ['uvalue] ( -- addr)
    ' >body @ ( uofs)
    postpone literal  postpone up@  postpone + ;  immediate

\ Give a task unique unbuffered files for stdin and stdout.
: clone-fds ( task -- )  >r
    0 ( stdin)  s" r" fdopen  r@ ['uvalue] infile-id his !
    1 ( stdout) s" w" fdopen  dup 0 setbuf  r> ['uvalue] outfile-id his ! ;

\ The maximum number of user variables that may be allocated
1024 cells constant #user

\ Create the stacks of a task and initialize its user area.
: construct ( task -- )
    assert( dup @ 0= )  \ Only construct a task once, lest the gates of
                        \ hell be opened
    >r
    32768 pagesize min  ( The size of a stack)
    \ Create the stacks and the user area
    dup 2dup  gforth_stacks  r@ !
    \ Create the mutex/condvar pair
    r@ status his task-init
    \ Copy most user variables from this task
    throw-entry r@ over his
    udp @  throw-entry up@ -  -  move
    \ Create the HOLD buffer above the user area
    r@ @ #user + word-pno-size +
    dup r@ holdend his !  dup r@ holdptr his !
    word-pno-size - r@ holdbufptr his !
    \ Optional: create files for debugging output
    r@ clone-fds
    \ Clear the pool of lock nodes
    0 r@ lock-nodes his !
    \ Point 'task at the pointer to this task
    r> dup 'task his  ! ;

: this-task ( -- task )   'task @ ;

\ The main task is called OPERATOR for historic reasons.
task operator  up@ operator !  operator 'task !  status task-init

\ The first code executed when a task starts.  The user variable
\ RUN-XT contains the XT of a word to execute.
: start-code ( addr -- )
    up!  run-xt @ catch  0 thread-id !  (bye) ;

: start-task ( xt task -- )
    >r
    r@ thread-id his @ abort" thread seems already to be running"
    ['] start-code >body r@ save-task his !
    r@ run-xt his !
    r@ thread-id his  pthread_detach_attr  gforth_run  r> @ ( user area)
    pthread_create if  -1 throw  then ;

warnings @  warnings off

\ Redefine a couple of standard words to work correctly when
\ multi-tasking.

: marker ( "<spaces> name" -- )
    marker,
    Create up@ , A,
  does>
    dup @ up!  cell + @ marker! ;

: pad ( -- c-addr )
    this-task operator = if  pad
    else  holdbuf-end  then ;

warnings !
