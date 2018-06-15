\ tempmotorsetup.fs

require ./BBB_Gforth_gpio/BBB_GPIO_lib.fs

1 constant bank1
%110000000000000000 constant enablexy

bank1 enablexy BBBiosetup
BBBiooutput
BBBioset
BBBiocleanup
