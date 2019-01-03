c-library pauses
  \c #include <time.h>

  \c int donanosleep(int sec, int nano) {
  \c struct timespec tim, tim2
  \c tim.tv_sec = (time_t)sec ;
  \c tim.tv_nsec = (long)nano ;
  \c return( nanosleep(&tim, &tim2) );
  \c }

  c-function nanosleep donanosleep n n -- n

end-c-library
