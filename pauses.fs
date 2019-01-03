c-library pauses
  \c #include <unistd.h>

  \c int dousleep(int usecond ) {
  \c return( usleep(usecond) );
  \c }

  c-function usleep dousleep n -- n

end-c-library
