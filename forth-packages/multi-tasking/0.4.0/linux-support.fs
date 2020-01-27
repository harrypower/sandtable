\ Forth task support for Linux.
\ STOP and AWAKEN are very thin wrappers around the futex(2) system
\ call.

require unix/libc.fs

c-library posix-task-support
    \c #include <assert.h>
    \c #include <linux/futex.h>
    \c #include <pthread.h>
    \c #include <stddef.h>
    \c #include <stdint.h>
    \c #include <stdio.h>
    \c #include <sys/syscall.h>
    \c #include <sys/time.h>
    \c #include <unistd.h>
    \c
    \c typedef struct {
    \c   volatile int32_t _status;
    \c } task_state_t;
    \c 
    \c void task_stop(task_state_t *task) {
    \c   if (__atomic_exchange_n(&(task->_status), 0, __ATOMIC_SEQ_CST) > 0)
    \c     return;
    \c 
    \c   syscall(SYS_futex, &(task->_status), FUTEX_WAIT, 0, NULL);
    \c 
    \c   __atomic_store_n(&(task->_status), 0, __ATOMIC_SEQ_CST);
    \c }
    \c 
    \c void task_awaken(task_state_t *task) {
    \c   __atomic_store_n(&(task->_status), 1, __ATOMIC_SEQ_CST);
    \c 
    \c   syscall(SYS_futex, &(task->_status), FUTEX_WAKE, 1);
    \c }
    \c 
    \c void task_init(task_state_t *task) {
    \c   __atomic_store_n(&(task->_status), 0, __ATOMIC_SEQ_CST);
    \c }
    \c 
    \c static void *gforth_run_impl(user_area *t) {
    \c   Cell x;
    \c   int throw_code;
    \c   void *ip0=(void*)(t->save_task);
    \c   sigset_t set;
    \c   gforth_UP=t;
    \c   gforth_setstacks(t);
    \c
    \c   *--gforth_SP=(Cell)t;
    \c
    \c   gforth_sigset(&set, SIGINT, SIGQUIT, SIGTERM, SIGWINCH, 0);
    \c   pthread_sigmask(SIG_BLOCK, &set, NULL);
    \c   x=gforth_go(ip0);
    \c   pthread_exit((void*)x);
    \c }
    \c
    \c static void *gforth_run()
    \c {
    \c   return (void*)&gforth_run_impl;
    \c }
    \c
    \c size_t task_state_size() {
    \c   return sizeof (task_state_t);
    \c }
    \c
    \c pthread_attr_t * pthread_detach_attr(void)
    \c {
    \c   static pthread_attr_t attr;
    \c   pthread_attr_init(&attr);
    \c   pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED);
    \c   return &attr;
    \c }

    c-function task-stop task_stop a -- void ( addr -- )
    c-function task-awaken task_awaken a -- void ( addr -- )
    c-function task-state-size task_state_size -- n (  -- n )
    c-function task-init task_init a -- void ( addr -- )

    c-function pthread_create pthread_create a{(pthread_t*)} a a a -- n ( thread attr start arg )
    c-function pthread_detach_attr pthread_detach_attr -- a ( -- addr )
    c-function gforth_stacks gforth_stacks n n n n -- a ( dsize fsize rsize lsize -- task )
    c-function gforth_run gforth_run -- a ( -- addr )

    c-function setbuf setbuf a a -- void ( stream buf -- )
    
end-c-library

