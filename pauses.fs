c-library pauses
  \c #include <time.h>

  \c int donanosleep(sec, nano) {
  \c struct timespec tim, tim2
  \c tim.tv_sec = sec ;
  \c tim.tv_nsec = nano ;
  \c return( nanosleep(&tim, &tim2) );
  \c }

  c-function nanosleep donanosleep n n -- n

end-c-library
