c-library atomic-support
    \c #include <stdint.h>
    \c 
    \c void atomic_store(int val, intptr_t *ptr) {
    \c   __atomic_store_n(ptr, val, __ATOMIC_SEQ_CST);
    \c }
    \c 
    \c intptr_t atomic_fetch(intptr_t *ptr) {
    \c   return __atomic_load_n(ptr, __ATOMIC_SEQ_CST);
    \c }
    \c 
    \c intptr_t atomic_xchg(intptr_t val, intptr_t *ptr) {
    \c   return __atomic_exchange_n(ptr, val, __ATOMIC_SEQ_CST);
    \c }
    \c 
    \c intptr_t atomic_cas(intptr_t expected, intptr_t desired,
    \c                     intptr_t *ptr, intptr_t *result_ptr) {
    \c   intptr_t val = expected;
    \c   intptr_t result = - __atomic_compare_exchange_n(ptr, &val, desired, /*weak*/0,
    \c                               __ATOMIC_SEQ_CST, __ATOMIC_SEQ_CST);
    \c   if (result_ptr)  *result_ptr = result;
    \c   return val;
    \c }

    c-function atomic@ atomic_fetch a -- n ( addr -- x )
    c-function atomic! atomic_store n a -- void ( x addr -- )
    c-function atomic-xchg atomic_xchg n a -- n ( x1 addr -- x2 )
    c-function (atomic-cas) atomic_cas n n a a -- n ( x1 x2 addr1 addr2 -- x3 )
end-c-library

: atomic-cas ( x1 x2 addr -- x3 )   0 (atomic-cas) ;
\ : atomic-cas? ( x1 x2 addr -- x3 t )   2>r >r 0 r> 2r> 's 3 cells + (atomic-cas) ;
