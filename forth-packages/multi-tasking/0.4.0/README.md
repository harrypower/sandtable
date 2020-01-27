Multi-tasking
=============

Multiprogramming is a common facility in Forth systems.  The wordset
which supports this is known as the multi-tasking wordset, and the
entity executing a program sequence is known as a task.

Rationale
---------

The earliest Forth systems developed by Charles Moore supported
multiprogramming, in that the computer could execute muliple
concurrent program sequences.  Many of today's Forths support
multiprogramming, but it has never been part of any standard.  Forth
has been multi-tasking for almost 50 years. It's time to standardize
it.

Interactions
------------

The most complex part of this proposal is not the wordset itself but
the way in which it interacts with the rest of the system.  It is
necessary to go through every line of the standard to determine which
parts will be affected and then to decide what to do.

Round-robin versus pre-emptive task scheduling
----------------------------------------------

Originally, Forth used a co-operative (sometimes called "round robin")
task scheduler to share time between tasks on a single processor. Such
a scheduler only switches tasks voluntarily, i.e. when a task
relinquishes control. This is done either by executing PAUSE or an I/O
word such as TYPE. More recent systems sometimes use a "pre-emptive"
scheduler which may switch tasks at any time, and some systems have
multiple processors sharing the same memory. This wordset doesn't
address such differences except to note that you may have to execute
PAUSE in a long-running computation to allow other tasks to run. From
th epoint of view of this standard it does not matter: as long as it
follows the rules of the Memory Model, a Standard Forth program will
execute correctly on either kind of system.

The Standard Forth Memory Model
-------------------------------

Multiprocessor systems vary in the degree to which processors operate
asynchronously.  In order to allow systems to operate as quickly as
possible while minimizing power consumption, memory operations
performed by one task may appear to other tasks in a different order,
stores may appear to be delayed, and in some cases incomplete updates
to cells in memory may be visible. On some systems values may be read
that have never been written by any task.

A Data Race occurs when two or more tasks access the same memory
location concurrently, and at least one of the accesses is some kind
of non-atomic store, and the tasks are not using any MUTEXes to
guarantee exclusive access to that memory location.  This standard
does not specify what values result from data races, either when
those values are read from memory or when they are written.

ATOMIC accesses and MUTEX operations are sequentially consistent.
Sequential consistency means that there appears to be a total order of
all atomic operations, i.e. every task observes atomic operations in
the same order.  More formally, the result of any execution is the same
as if

 >   the operations of all the tasks had been executed in some
 >   sequential order, and

 >   the operations of each processor appear in this total ordering in
 >   the same order as was specified in its program.

Therefore, a well-defined program may store values into a buffer by using
ordinary (non-atomic) stores then publish the address of that buffer
to other tasks using either a region protected by a MUTEX or by using
an atomic store. Another task may use a MUTEX or an atomic fetch to
read the address of the buffer, then use ordinary (non-atomic) fetches
to load the values from it.

[ As long as a program has no data races, the entire program is
sequentially consistent, even though it uses plain fetches and stores
for most of its operations. This is an important property that greatly
aids intuitive understanding of programs. ]

Task creation
-------------

TASK ( "<spaces>name” -- )

 >   Skip leading space delimiters. Parse name delimited by a
 >   space. Create a definition for name with the execution semantics
 >   defined below. Reserve /TASK bytes of data space at a suitably
 >   aligned address.

 >   name is referred to as a "task".

name Execution: ( –– a-addr )

 >   a-addr is the address of the task. The task may not be used until
 >   it has been CONSTRUCTed.

/TASK ( -- n)     "per task"

 >   n is the size of a task.

 >   [This word allows arrays of tasks to be created without having
 >   to name each one.]

CONSTRUCT ( a-addr )

 >   Instantiate the task at a-addr. a-addr must be a memory region
 >   of at least /TASK bytes in size. CONSTRUCT shall be executed at
 >   most once on a task. Once CONSTRUCT has been executed, the
 >   task's USER variables may be accessed.

START&ndash;TASK ( xt a-addr -- )

 >   Start the task at addr asynchronously executing the word whose
 >   execution token is xt. If the word terminates by executing EXIT,
 >   reaching the end of the word, or throwing an execption, the task
 >   terminates.  The task may be restarted after this by executing
 >   START-TASK again.  The task shall not be restarted until it has
 >   terminated.

User variables
--------------

+USER ( n "<spaces>name” -- )

 >   Skip leading space delimiters. Parse name delimited by a
 >   space. Create a definition for name with the execution semantics
 >   defined below. Reserve n bytes of data space, suitably aligned, in
 >   every task.

 >   name is referred to as a "user variable".

[ I can't find any common practice for defining user variables.
Traditional forth defines n USER to have a fixed offset in the uder
area, but that is of no use unless once can discover the highest
already-defined user variable. Suggestions welcome. ]

name Execution: ( –– a-addr )

 >   a-addr is the address of the variable in the currently-running
 >   task.

[ I think we need to say that a child of MARKER deallocates all user
variables defined after it. ]

HIS ( addr1 addr2 -- addr3 )

 >   Given a task at address addr1 and a user varible address in the
 >   current task, return the address of the corresponding user
 >   variable in that task.

 >   [ Usage: <task-name> <user-variable-name> HIS
 >   e.g.  TYPIST BASE HIS @ ]

Starting, stopping, and pausing tasks
-------------------------------------

STOP ( -- )

 >   Block the currently-running task unless or until AWAKEN has been
 >   issued. Calls to AWAKEN are not counted, so multiple AWAKENs
 >   before a STOP can unblock only a single STOP.

AWAKEN ( a-addr -- )

 >   a-addr is the address of a task. If that task is blocked, unblock
 >   it so that it resumes execution. If that task is not blocked, the
 >   next time it executes STOP it will continue rather than blocking.

PAUSE ( -- )

 >   May cause the execuiting task temporarily to relinquish the CPU.

[ In a Forth system which uses round-robin scheduling this can be used
to allow other tasks to run However, this isn't usually needed because
any I/O causes a task to block. All words which perform I/O (TYPE,
ACCEPT, READ­FILE, etc.) should (unless they are extremely high
priority?) execute PAUSE . ]

Atomic memory words
-------------------

ATOMIC@ ( a-addr - x )

 >   x is the value stored at a-addr.  ATOMIC@ is sequentially consistent.

See: Sequential consistency

ATOMIC! ( x a-addr –– )                “atomic-store”

 >   Store x at a-addr.  ATOMIC! is sequentially consistent.

See: Sequential consistency

ATOMIC&ndash;XCHG ( x1 a-addr – x2)          "atomic-exchange"

 >   Atomically replace the value at a-addr with x1. x2 is the value
 >   previously at a-addr. This is an atomic read-modify-write
 >   operation. ATOMIC-XCHG has the memory effect of an ATOMIC@
 >   followed by an ATOMIC! .
 >   
See: Sequential consistency

ATOMIC&ndash;CAS ( x-expected x-desired a-addr -– x-prev)

 >   Atomically compare the value at a-addr with expected, and if
 >   equal, replace the value at a-addr with desired. prev is the value
 >   at a-addr immediately before this operation. This is an atomic
 >   read-modify-write operation.  ATOMIC&ndash;CAS has the memory effect of
 >   an ATOMIC@ followed by (if the store happens) an ATOMIC! .

See: Sequential consistency

[ Do we need ATOMIC+! ? It can be written by using ATOMIC&ndash;CAS but it's rather clumsy to do so. ]

Mutual Exclusion
----------------

/MUTEX ( –- n)            "per-mutex"

 >   n is the number of bytes in a mutex.  

MUTEX&ndash;­INIT ( a-addr -- )

 >   Initialize a mutex. Set its state to released.

GET ( a-addr --)

 >   Obtain control of the mutex at a-addr. If the mutex is owned by
 >   another task, the task executing GET will block until the mutex is
 >   available.  This operation is part of the total order, and happens
 >   after another task releases the mutex. 

[ In a round-robin scheduler, this word executes PAUSE before attempting
to acquire the mutex. ]

RELEASE ( a-addr –- )

 >   Release the mutex at addr.

 >   This operation is part of the total order, and happens before
 >   another task can GET the mutex.
