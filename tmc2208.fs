\ tmc2208.fs
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

\ gforth interface words for tmc2208 trinamic stepper driver
\
\ Requires:
\
\	stringobj.fs
\	syscalls386.fs
\ serial.fs
\ BBB_GPIO_lib.fs
\
\ Revisions:
\ 6/15/2018 started coding
\ 6/18/2018 main structure of object started

require serial.fs
require ./Gforth-Objects/objects.fs
\ require ./Gforth-Objects/stringobj.fs
require ./BBB_Gforth_gpio/BBB_GPIO_lib.fs

[ifundef] destruction
  interface
     selector destruct ( -- ) \ to free allocated memory in objects that use this
  end-interface destruction
[endif]

object class
  destruction implementation
  protected
  0x00      constant GCONF
  %00000101 constant sync
  0x00      constant slave-addr

  inst-value uarthandle
  inst-value buffer
  inst-value enablebank
  inst-value enablebit
  inst-value dirbank
  inst-value dirbit
  inst-value stepbank
  inst-value stepio

  m: ( uaddr u tmc2208 -- ucrc ) \ uaddr u contains string of data to make the crc for
  \ crc calculated and returned note it is only to be 8 bits wide
    0 0 { uaddr u currentByte crc }
    u 0 ?do
      uaddr i + c@ to currentByte
      8 0 do
        crc 7 rshift currentByte 0x01 and xor 0 =
        if
          crc 1 lshift %11111111 and to crc
        else
          crc 1 lshift 0x07 xor %11111111 and to crc
        then
        currentByte 1 rshift %11111111 and to currentByte
      loop
    loop crc ;m method crc8-ATM
\  m: ( ugpiobank ugpiobitmask tmc2208 -- nflag )
\    bbbiosetup false = if
\      BBBiooutput
\      bbbioset
\      bbbiocleanup
\    else
\      true
\    then
\  ;m method out-set
\  m: ( ugpiobank ugpiobitmask tmc2208 -- nflag )
\    bbbiosetup false = if
\      BBBiooutput
\      bbbioclear
\      bbbiocleanup
\    else
\      true
\    then
\  ;m method out-clear
  m: ( ugpiobank ugpiobitmask tmc2208 -- nflag )
    bbbiosetup false = if bbbiooutput bbbiocleanup else true then ;m method gpio-output

  m: ( ugpiobank ugpiobitmask tmc2208 -- nflag )
    bbbiosetup false = if bbbioset bbbiocleanup else true then ;m method gpio-high

  m: ( ugpiobank ugpiobitmask tmc2208 -- nflag )
    bbbiosetup false = if bbbioclear bbbiocleanup else true then ;m method gpio-low

  m: ( uuart tmc2208 -- ) \ configure uart
    serial_open dup 0> if [to-inst] uarthandle else throw then
    uarthandle B115200 serial_setbaud
    uarthandle ONESTOPB serial_setstopbits
    uarthandle PARNONE serial_setparity
    uarthandle serial_flush
  ;m method conf-uart

  public
  m: ( tmc2208 -- )
    enablebank enablebit this [current] gpio-high throw ;m method disable-driver
  m: ( tmc2208 -- )
    enablebank enablebit this [current] gpio-low throw ;m method enable-driver

  m: ( ugb0 uenableio ugb1 udirio ugb2 ustepio uuart tmc2208 -- nflag ) \ constructor
  \ note these banks and pin declarations are done with BBB_GPIO_lib.fs and deal with the BBB hardware from programmers reference manual and such linux is not informed of what you do at that level
  \ ugb0 ugb1 ugb2 are the gpio banks used for the paired gpio pins that follow there declarations.  They can be 0 to 3 only and are parsed accordingly
  \ uenableio is the bit mask for where tmc2208 enable pin is connected to BBB
  \ udirio is the bit mask for where the tmc2208 direction pin is connected to BBB
  \ usetpio is the bit mask for where the tmc2208 step pin is connected to BBB
  \ uuart is for ttyo1 or ttyo2 so values of 1 or 2 are only allowed at this moment
  \ nflag is 0 or false if the tmc2208 driver is present and could be talked to
  \ nflag is -1 if an error happened during gpio port setup
  \ nflag is any other number if uart does not work ( the number should refere to the failure of the uart setup ).. note the uart needs to be turned on in the BBB image and present at linux level used.
  \ nflag is 1 if uuart is not 1 or 2 !
    { ugb0 uenableio ugb1 udirio ugb2 ustepio uuart }
    try
      ugb0 uenableio this [current] gpio-output throw
      ugb0 uenableio this [current] gpio-high throw \ this should turn off power to motor
      ugb0 [to-inst] enablebank uenableio [to-inst] enablebit
      ugb1 udirio this [current] gpio-output throw
      ugb1 udirio this [current] gpio-low throw \ this should setup tmc2208 direction pin to output and low for now!
      ugb1 [to-inst] dirbank udirio [to-inst] dirbit
      ugb2 ustepio this [current] gpio-output throw
      ugb2 ustepio this [current] gpio-low throw \ this should setup tmc2208 step pin to output and low for now!
      ugb2 [to-inst] stepbank ustepio [to-inst] stepio
      uuart case
        1 of 1 this [current] conf-uart endof
        2 of 2 this [current] conf-uart endof
        1 throw \ only ttyo1 or ttyo2 at this moment
      endcase
      12 allocate throw [to-inst] buffer
      false
    restore
    endtry
  ;m overrides construct

  m: ( tmc2208 -- ) \ destructor
    enablebank enablebit this [current] gpio-high drop
    uarthandle serial_close drop
    buffer free drop
  ;m overrides destruct

  m: ( ureg tmc2208 -- uaddr n nflag )
    \ uaddr u is the buffer address with u filled content of bytes returned from tmc2208 if nflag is zero of false
    \ nflag is 2 for a write failed error
    \ nflag is 1 for a reading data from tmc2208 error ... not all data recived
    \ nflag is 3 meaning the crc did not compair with calcuated crc ... note the uaddr and u contains what was returned including the send 4 bytes and the crc byte at end
    uarthandle serial_flush
    sync buffer c!
    0 buffer 1 + c!
    %1111111 and buffer 2 + c! \ place ureg in buffer and set transfer to read register
    buffer 3 this [current] crc8-ATM buffer 3 + c! \ calculate crc and store in buffer
    uarthandle buffer 4 serial_write 4 = if
      buffer 12 0 fill 2 ms
      uarthandle buffer 12 serial_read 12 = if
        buffer 4 + 7 this [current] crc8-ATM buffer 11 + c@ = if
          buffer 4 + 7 0 \ skip the echoed read command and crc and return the response
        else
          buffer 12 3 \ crc did not match calcuated one
        then
      else
        buffer 12 1 \ did not get all the data from read
      then
    else
      0 0 2 \ write failed
   then
  ;m method readreg

end-class tmc2208

1 %10000000000000000 1 %10000000000000 1 %1000000000000 1
tmc2208 heap-new constant mymotorA . cr
0 mymotorA readreg . cr dump

1 %100000000000000000 1 %1000000000000000 1 %100000000000000 2
tmc2208 heap-new constant mymotorB . cr
0 mymotorB readreg . cr dump
