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
\ objects.fs

\ Revisions:
\ 6/24/2018 started coding

require BBB_Gforth_gpio/syscalls386.fs
require BBB_Gforth_gpio/BBB_GPIO_lib.fs
require Gforth-Objects/objects.fs

0x40046B04        constant SPI_IOC_WR_MAX_SPEED_HZ
0x00              constant SPI_IOC_WR_BITS_PER_WORD  \ need to get this data
\ this is simply a number passed as an address to ioctl as in the above max speed
0x00              constant SPI_IOC_WR_MODE
\ this is a number from 0 to 3 as below that is passed as an address to ioctl
\ 0   \ !< Low at idle, capture on rising clock edge
\ 1   \ !< Low at idle, capture on falling clock edge
\ 2   \ !< High at idle, capture on falling clock edge
\ 3    \ !< High at idle, capture on rising clock edge

\ not sure if these are all needed or if they even do what they do in the uart on this spi but they work to open the channel
0x800             constant O_NDELAY
0x100             constant O_NOCTTY
0x002             constant O_RDWR

100000 variable spispeed spispeed !
0 value spihandle
: openspi ( -- ) \ open spi 2 channel and set to read write with a max speed of 100000 hz
  s\" /dev/spidev2.0\x00" drop O_NDELAY O_NOCTTY or O_RDWR or open to spihandle
  spihandle 0> if
    spihandle SPI_IOC_WR_MAX_SPEED_HZ spispeed ioctl throw \ this should set the speed to 100000 hz as max speed
  else
    spihandle throw
  then ;

\ test with the following
\ openspi
\ spihandle s\" \x23\xf6\x55\x02" write . ." total bytes writen" cr


\ *****************************************************************
\ to configure spi pins use the following at command line
\ config-pin p9.28 spi_cs     ( cs )
\ config-pin p9.29 spi        ( D0 )
\ config-pin p9.30 spi        ( D1 )
\ config-pin p9.31 spi_clock  ( clock )
\ this is spi1 used for x motor

\ config-pin p9.17 spi_cs     ( cs )
\ config-pin p9.18 spi        ( D1 )
\ config-pin p9.21 spi        ( D0 )
\ config-pin p9.22 spi_clock  ( clock )
\ this is spi0 used for y motor

\ this is example code to configure pins on BBB for gpio output
\ s\" config-pin p8.11 output\n" system
\ x stepper dir  ( gpio1_13)
\ s\" config-pin p8.12 output\n" system
\ x stepper step ( gpio1_12)
\ s\" config-pin p9.15 output\n" system
\ x stepper enable ( gpio1_16)

\ s\" config-pin p8.15 output\n" system
\ y stepper dir  ( gpio1_15)
\ s\" config-pin p8.16 output\n" system
\ y stepper step ( gpio1_14)
\ s\" config-pin p9.23 output\n" system
\ y stepper enable ( gpio1_17)
\ *****************************************************************

[ifundef] destruction
  interface
     selector destruct ( -- ) \ to free allocated memory in objects that use this
  end-interface destruction
[endif]

object class
  destruction implementation  ( tmc2130 -- )
  selector enable-motor       ( tmc2130 -- )
  selector disable-motor      ( tmc2130 -- )

  protected
  inst-value spihandle
  inst-value enablebank
  inst-value enablebit
  inst-value dirbank
  inst-value dirbit
  inst-value stepbank
  inst-value stepio
  cell% inst-var spispeed

  m: ( ugpiobank ugpiobitmask tmc2208 -- nflag )
    bbbiosetup false = if bbbiooutput bbbiocleanup else true then ;m method gpio-output

  m: ( ugpiobank ugpiobitmask tmc2208 -- nflag )
    bbbiosetup false = if bbbioset bbbiocleanup else true then ;m method gpio-high

  m: ( ugpiobank ugpiobitmask tmc2208 -- nflag )
    bbbiosetup false = if bbbioclear bbbiocleanup else true then ;m method gpio-low

  public
  m: ( tmc2208 -- ) \ simply make enable pin high on tmc2208 driver board
    enablebank enablebit this [current] gpio-high throw ;m method disable-motor
  m: ( tmc2208 -- ) \ simply make enable pin low on tmc2208 driver board
    enablebank enablebit this [current] gpio-low throw ;m method enable-motor
  m: ( ubankenable uenableio ubankdir udirio ubankstep ustepio uspi tmc2130 -- ) \ constructor
    { ubankenable uenableio ubankdir udirio ubankstep ustepio uspi }
    try
      ubankenable [to-inst] enablebank uenableio [to-inst] enablebit
      enablebank enablebit this gpio-output throw
      this [current] disable-driver \ this should turn off power to motor
      ubankdir [to-inst] dirbank udirio [to-inst] dirbit
      dirbank dirbit this gpio-output throw
      dirbank dirbit this gpio-low throw
      ubankstep [to-inst] stepbank ustepio [to-inst] stepio
      setpbank stepio this gpio-output throw
      setpbank stepio this gpio-low throw
      true [to-inst] spihandle
      100000 spispeed !
      uspi case
        \ this is spi1 on the BBB schematic and mode chart. Linux enumerates the spi starting at 1!
        1 of s\" /dev/spidev2.0\x00" drop O_NDELAY O_NOCTTY or O_RDWR or open [to-inst] spihandle endof
        \ this is spi0 on the BBB schematic and mode chart. Linux enumerates the spi starting at 1!
        0 of s\" /dev/spidev1.0\x00" drop O_NDELAY O_NOCTTY or O_RDWR or open [to-inst] spihandle endof
      endcase
      spihandle 0> if
        spihandle SPI_IOC_WR_MAX_SPEED_HZ spispeed ioctl throw \ set spi speed to 100000 hz
      else
        spihandle throw
      then
      false
    endtry
    restore
  ;m overrides construct
  m: ( tmc2130 -- ) \ destructor
  ;m overrides destruct

end-class tmc2130


1 %10000000000000000 1 %10000000000000 1 %1000000000000 1
tmc2130 heap-new constant mymotorX throw

\ 1 %100000000000000000 1 %1000000000000000 1 %100000000000000 0
\ tmc2130 heap-new constant mymotory throw
