c-library defines_data

  \c #include <sys/types.h>
  \c #include <sys/stat.h>
  \c #include <fcntl.h>
  \c #include <unistd.h>
  \c #include <sys/ioctl.h>

  \c void mydefines() { printf("O_NDELAY %d, O_NOCTTY %d, O_RDWR %d \n",O_NDELAY , O_NOCTTY , O_RDWR ) ; }

  c­function mydefines mydefines ­­ void -- void

end-c-library
