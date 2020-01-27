require ~/gforth/unix/pthread.fs
require atomic.fs

c-library task-support
    \c #include <pthread.h>
    \c #include <assert.h>
    \c 
    \c void *gforth_run * t)
    \c {
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
    \c static void *gforth_run_p()
    \c {
    \c   return (void*)&gforth_thread;
    \c }
    \c 
    \c typedef struct {
    \c   volatile int32_t _status;
    \c   pthread_mutex_t _mutex;
    \c   pthread_cond_t _cond;
    \c } task_state_t;
    \c 
    \c // STOP decrements count if > 0, else does a condvar wait.  AWAKEN
    \c // sets count to 1 and signals condvar.  Only one thread ever waits on
    \c // the condvar. Contention seen when trying to stop implies that
    \c // someone is awakening you, so don't wait. And spurious returns are
    \c // fine, so there is no need to track notifications.
    \c 
    \c void task_stop(task_state_t *task) {
    \c 
    \c   // Optional fast-path check:
    \c   // Return immediately if we are in run state.
    \c   // We depend on Atomic::xchg() having full barrier semantics since
    \c   // we are doing a lock-free update to _status.
    \c   if (__atomic_exchange_n(&(task->_status), 0, __ATOMIC_SEQ_CST) > 0)
    \c     return;
    \c 
    \c   // Don't wait if cannot get lock because interference arises from
    \c   // awakening.
    \c   if (pthread_mutex_trylock(&(task->_mutex)) != 0) {
    \c     return;
    \c   }
    \c 
    \c   int retcode;
    \c   if (task->_status > 0)  { // no wait needed
    \c     task->_status = 0;
    \c     retcode = pthread_mutex_unlock(&(task->_mutex));
    \c     assert(retcode == 0);
    \c     // Probably unnecessary: surely pthread mutex operations are full
    \c     // fences?
    \c     __atomic_thread_fence(__ATOMIC_SEQ_CST);
    \c     return;
    \c   }
    \c // // // printf("blocking %p\n", &(task->_cond));
    \c   // Block this thread untill someone awakens us
    \c   retcode = pthread_cond_wait(&(task->_cond), &(task->_mutex));
    \c   assert(retcode == 0);
    \c // // // printf("unblocked\n");
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
    \c   const int s = task->_status;
    \c   task->_status = 1;
    \c 
    \c   if (s < 1) {
    \c     // thread is definitely stopped
    \c // // // printf("unblocking %p\n", &(task->_cond));
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

    c-function task-stop task_stop a -- void ( addr -- )
    c-function task-awaken task_awaken a -- void ( addr -- )
    c-function task-state-size task_state_size -- n (  -- n )
    c-function task-init task_init a -- void ( addr -- )
end-c-library

: abort"   postpone dup  postpone if  postpone bt  postpone then  postpone abort" ; immediate
: assert ( t -- )   0= if  ." backtrace: " bt  1 abort" D'oh!"  then ;

User status   task-state-size cell- uallot  drop
User 'task    \ points to our own task

: his ( task addr1 -- addr2 )   up@ - swap @ + ;

: stop ( -- )   status \ ." ## " dup . 
    task-stop ;
: awaken ( task -- )    status his \ ." ### " dup . 
    task-awaken ;

: task ( "name" -- )    create 0 , ;

: construct ( task -- )
    assert( dup @ 0= )  \ Only construct a task once, lest the gates of
                        \ hell are opened.
    >r  32768 newtask  r@ !
    r@ status his task-init
    r> dup 'task  .s cr his  ! ;

: this-task ( -- task )   'task @ ;

task operator  up@ operator !  operator 'task !  status task-init

: start-task ( xt task -- )   @  1 swap pass  execute ;

