\ CLH (Craig, Landin, and Hagersten) queue locks.
\
\ Each node in the queue has a single cell field.  A free lock has one
\ dummy node, and the head and tail both point to that node.  A node
\ has the possible values 0 (the lock is free) 1 (the lock is owned)
\ or a task address (there is a task waiting in the queue to be
\ awakened).  When a task RELEASEs a lock it looks in the node at the
\ head of the queue: if that node is greater than 1 the task it points
\ to is awakened.

\ Any multi-producer single-consumer queue can be used for a blocking
\ lock.  We use this algorithm because adding and removing queue nodes
\ queue is simple, fast, and doesn't require us to spin even if the
\ lock is contended.

0
    field: lock.tail
    field: lock.head
constant /mutex

\ NB: It might be worth padding this lock-node structure to a cache
\ line size to make sure that when a thread releases a lock, it
\ invalidates only its successor's cache line. This only matters while
\ the lock is spinning.
0
    cell + \ field: lock-node.locked
constant /lock-node

\ Allocate a lock node. Usually this will come from our task-local
\ pool of nodes, but if the pool is empty we'll ALLOCATE one.
: lock-node-allocate ( -- a-addr )
    lock-nodes @
    ?dup if
        dup @  lock-nodes !
    else        
        /lock-node allocate throw
    then ;

\ Recycle a lock node.
: lock-node-free ( a-addr -- )
    lock-nodes @ over !  lock-nodes ! ;

: mutex-init ( addr -- )
    lock-node-allocate  0 over !
    swap  2dup lock.tail !  lock.head ! ;

\ Optional: rather than stopping immediately a lock is held, spin for
\ a little while. This can be a major win if locks are highly
\ contended but only held for a short time. So how long should we spin
\ for? Theoretically, it should be half the time it takes to switch
\ from one task to another; 1000 loops is not a bad guess on a
\ multi-core system with a UNIX-like OS.
\
\ NB: Maybe it's worth this word returning a flag to show if the lock
\ is still blocked.
: spin ( a-addr -- )
    1000 0 do  pause  dup atomic@ 0= if
            unloop drop exit  then  loop
    drop ;

\ Block on a node until it becomes free.
\ a-addr points to a node. While the node is marked as owned, put our
\ own address into it then stop. When the owner of the node releases
\ the lock they should place 0 in the node then awaken us.
: block ( a-addr )
    this-task over atomic-xchg if
        begin  dup atomic@ while  stop  repeat
    then drop ;
    
: get { lock -- }
    \ Allocate a node from our pool
    lock-node-allocate { node }
    1 node ! \ Mark the new node owned
    \ Insert the new node into the queue
    node  lock lock.tail  atomic-xchg ( prev)
    dup atomic@ 1 = if
        \ Someone else owns this lock. Spin for a short while, then
        \ block.
        dup spin  dup block
    then 
    ( prev) lock-node-free \ Recycle the previous lock node
    node lock lock.head ! ;

: release ( lock -- )
    0  over lock.head @ atomic-xchg
    assert( dup )
    dup 1 = if  2drop exit  then  \ There's no-one waiting
    awaken drop ; \ Awaken the next task in the queue
