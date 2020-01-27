\ Proposed multi-tasking wordset for Forth, reference implementation.
\ For GForth + GCC, running on a system with POSIX threads.

\ You'll need one or the other of these:
require linux-support.fs
\ require posix-support.fs

require mt.fs
require atomic.fs

\ You'll need one or the other of these:
require clh.fs
\ require spinlock.fs
