
c-library testopen
  \c #include <stdio.h>
  \c #include <sys/types.h>
  \c #include <sys/stat.h>
  \c #include <fcntl.h>
  \c #include <unistd.h>
  \c #include <sys/ioctl.h>
  \c #include <termios.h>

  \c int file ;
  \c int myopen() { if ((file = open("/dev/ttyO1", O_RDWR | O_NOCTTY | O_NDELAY))<0){
  \c  perror("UART: Failed to open the file.\n");
  \c  return -499; }
  \c else { return file ;} }

  c-function myopen myopen void -- n ( -- )

end-c-library
