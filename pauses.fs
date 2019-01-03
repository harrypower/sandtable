c-library pauses
  \c #include <time.h>

  \c struct timespec  tim, tim2;
  \c int donanosleep(&tim, &tim2) {
  \c return( nanosleep(&tim, &tim2) );
  \c }

  c-function nanosleep donanosleep n n -- n

end-c-library
