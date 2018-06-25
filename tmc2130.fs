\ tmc2130.fs
\    Copyright (C) 2018  Philip K. Smith
\    This program is free software: you can redistribute it and/or modify
\    it under the terms of the GNU General Public License as published by
\    the Free Software Foundation, either version 3 of the License, or
\    (at your option) any later version.

\    This program is distributed in the hope that it will be useful,
\    but WITHOUT ANY WARRANTY; without even the implied warranty of
\    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\    GNU General Public License for more details.

\    You should have received a copy of the GNU General Public License
\    along with this program.  If not, see <http://www.gnu.org/licenses/>.

\ gforth interface words for tmc2210 trinamic stepper driver
\
\ Requires:
\
\	syscalls386.fs
\ BBB_GPIO_lib.fs
\
\ Revisions:
\ 6/24/2018 started coding

require BBB_Gforth_gpio/syscalls386.fs

0x40046B04        constant SPI_IOC_WR_MAX_SPEED_HZ
0x800             constant O_NDELAY
0x100             constant O_NOCTTY
0x002             constant O_RDWR

100000 variable spispeed spispeed !
: openspi ( -- ) \ open spi 2 channel and set to read write with a max speed of 100000 hz
  s\" /dev/spidev2.0\x00" drop O_NDELAY O_NOCTTY or O_RDWR or open value spihandle
  spihandle 0> if
    spihandle SPI_IOC_WR_MAX_SPEED_HZ spispeed ioctl throw \ this should set the speed to 100000 hz as max speed
  else
    spihandle throw
  then ;

\ test with the following

\ spihandle s\" \x23\xf6\x55\x02" write . ." total bytes writen" cr
