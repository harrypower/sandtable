\ tempmotorsetup.fs

require ./BBB_Gforth_gpio/BBB_GPIO_lib.fs
require serial.fs
require ./Gforth-Objects/stringobj.fs

1 constant bank1
%110000000000000000 constant enablexy

: disable-motors ( -- )
  bank1 enablexy BBBiosetup
  BBBiooutput
  BBBioset
  BBBiocleanup ;

disable-motors

: enablemotors ( -- )
  bank1 enablexy BBBiosetup
  BBBiooutput
  BBBioclear
  BBBiocleanup ;

0 value uarthandle
: setupuart1
  1 serial_open to uarthandle
  uarthandle B115200 serial_setbaud
  uarthandle ONESTOPB serial_setstopbits
  uarthandle PARNONE serial_setparity
  uarthandle serial_flush ;

: sendtest ( -- uwriten )
  0xa0 pad c!
  0x00 pad 1 + c!
  0x0f pad 2 + c!
  0xb6 pad 3 + c!
  0x00 pad 4 + c!
  uarthandle pad 4 serial_write ;

: readit
  pad 10 dump ;
  
