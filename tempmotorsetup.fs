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
