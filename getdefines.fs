c-library defines_data
  \c #include <stdio.h>
  \c #include <sys/types.h>
  \c #include <sys/stat.h>
  \c #include <fcntl.h>
  \c #include <unistd.h>
  \c #include <sys/ioctl.h>
  \c #include <linux/spi/spidev.h>

  \c void mydefines(void) { printf("O_NDELAY %d, O_NOCTTY %d, O_RDWR %d , SPI_IOC_WR_MAX_SPEED_HZ %d \n",O_NDELAY , O_NOCTTY , O_RDWR ,SPI_IOC_WR_MAX_SPEED_HZ ) ; }

  c-function mydefines mydefines void -- void ( -- )

end-c-library

mydefines
