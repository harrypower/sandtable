\ tempmotorsetup.fs

require ./BBB_Gforth_gpio/BBB_GPIO_lib.fs
require serial.fs
require ./Gforth-Objects/stringobj.fs

1 constant bank1
%110000000000000000 constant enablexy

: disable-motors ( --  )
  bank1 enablexy BBBiosetup throw
  BBBiooutput
  BBBioset
  BBBiocleanup throw ;

disable-motors

: enablemotors ( -- )
  bank1 enablexy BBBiosetup throw
  BBBiooutput
  BBBioclear
  BBBiocleanup throw ;

0 value uarthandle
: setupuart1
  1 serial_open to uarthandle
  uarthandle B115200 serial_setbaud
  uarthandle ONESTOPB serial_setstopbits
  uarthandle PARNONE serial_setparity
  uarthandle serial_flush ;

: sendit ( -- uwriten )
  uarthandle pad 4 serial_write ;

: sendtest ( -- uwriten )
  0x05 pad c!
  0x00 pad 1 + c!
  0x00 pad 2 + c!
  0x48 pad 3 + c!
  0x00 pad 4 + c!
  sendit ;

: readtest ( -- uread-amount )
  uarthandle serial_lenrx dup dup
  pad swap 0 fill
  uarthandle pad rot serial_read
  pad swap dump ;