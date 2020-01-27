\ Forth task supprt for POSIX. The mutex/condvar logic used here for
\ STOP and AWAKEN is more complex than the simple futex() calls in
\ Linux or _lwp_park() used in BSD Unix, but it's much more portable.

require unix/libc.fs

c-library posix-task-support
    \c #include <pthread.h>
    \c #include <assert.h>
    \c #include <stdio.h>
    \c #include <unistd.h>
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
    \c typedef struct {
    \c   volatile int32_t _status;
    \c   pthread_mutex_t _mutex;
    \c   pthread_cond_t _cond;
    \c } task_state_t;
    \c
    \c // TASK_STOP sets status to 0 and returns if > 0, else waits for
    \c // _cond.  AWAKEN sets count to 1 and signals _cond. _mutex is
    \c // used to avoid a race condition which would lose an AWAKEN.
    \c
    \c void task_stop(task_state_t *task) {
    \c   if (__atomic_exchange_n(&(task->_status), 0, __ATOMIC_SEQ_CST) > 0)
    \c     return;
    \c
    \c   int retcode;
    \c   retcode = pthread_mutex_lock(&(task->_mutex));
    \c   assert(retcode == 0);
    \c
    \c   if (task->_status > 0) {
    \c     task->_status = 0;
    \c     retcode = pthread_mutex_unlock(&(task->_mutex));
    \c     assert(retcode == 0);
    \c     // Probably unnecessary: surely pthread mutex operations are full
    \c     // fences?
    \c     __atomic_thread_fence(__ATOMIC_SEQ_CST);
    \c     return;
    \c   }
    \c
    \c   retcode = pthread_cond_wait(&(task->_cond), &(task->_mutex));
    \c   assert(retcode == 0);
    \c   task->_status = 0;
    \c
    \c   retcode = pthread_mutex_unlock(&(task->_mutex));
    \c   assert(retcode == 0);
    \c
    \c   // Probably unnecessary: surely pthread mutex operations are full
    \c   // fences?
    \c   __atomic_thread_fence(__ATOMIC_SEQ_CST);
    \c }
    \c
    \c void task_awaken(task_state_t *task) {
    \c   int retcode = pthread_mutex_lock(&(task->_mutex));
    \c   assert(retcode == 0);
    \c
    \c   int s = task->_status;
    \c   task->_status = 1;
    \c
    \c   if (s < 1) {
    \c     // thread is definitely stopped
    \c     retcode = pthread_cond_signal(&(task->_cond));
    \c     assert(retcode == 0);
    \c   }
    \c   retcode = pthread_mutex_unlock(&(task->_mutex));
    \c   assert(retcode == 0);
    \c }
    \c
    \c void task_init(task_state_t *task) {
    \c   int retcode = pthread_cond_init(&(task->_cond), NULL);
    \c   assert(retcode == 0);
    \c   retcode = pthread_mutex_init(&(task->_mutex), NULL);
    \c   assert(retcode == 0);
    \c   task->_status = 0;
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

