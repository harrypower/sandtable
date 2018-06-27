c-library defines_data
  \c #include <stdio.h>
  \c #include <sys/types.h>
  \c #include <sys/stat.h>
  \c #include <fcntl.h>
  \c #include <unistd.h>
  \c #include <sys/ioctl.h>
  \c #include <linux/spi/spidev.h>

  \c void mydefines(void) { printf("O_NDELAY %d, O_NOCTTY %d, O_RDWR %d , SPI_IOC_WR_MAX_SPEED_HZ %d \n",O_NDELAY , O_NOCTTY , O_RDWR ,SPI_IOC_WR_MAX_SPEED_HZ ) ; }
  \c void defines2(void) { printf("SPI_IOC_WR_BITS_PER_WORD %d, SPI_IOC_WR_MODE %d, SPI_MODE_0 %d\n",SPI_IOC_WR_BITS_PER_WORD ,SPI_IOC_WR_MODE,SPI_MODE_0 );}
  c-function mydefines mydefines void -- void ( -- )
  c-function defines2 defines2   void -- void ( -- )

end-c-library

mydefines cr
defines2
