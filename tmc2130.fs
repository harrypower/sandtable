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
\ mdca-obj.fs
\ pauses.fs

\ Revisions:
\ 6/24/2018 started coding
\ 01/05/2019 added some constants and changed throws to abort"
\ requires pauses.fs but may not use this addition

require BBB_Gforth_gpio/syscalls386.fs
require Gforth-Objects/objects.fs
require BBB_Gforth_gpio/BBB_GPIO_lib.fs
require Gforth-Objects/mdca-obj.fs
require pauses.fs

0x40046B04        constant SPI_IOC_WR_MAX_SPEED_HZ
0x40016B03        constant SPI_IOC_WR_BITS_PER_WORD
\ this is simply a number passed as an address to ioctl as in the above max speed
0x40016B01        constant SPI_IOC_WR_MODE
\ this is a number from 0 to 3 as below that is passed as an address to ioctl
\ 0  Low at idle, capture on rising clock edge
\ 1  Low at idle, capture on falling clock edge
\ 2  High at idle, capture on falling clock edge
\ 3  High at idle, capture on rising clock edge

\ not sure if these are all needed or if they even do what they do in the uart on this spi but they work to open the channel
0x800             constant O_NDELAY
0x100             constant O_NOCTTY
0x002             constant O_RDWR

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

\ the following are the settings to then make the objects mymotorx and mymotory based on above system pin config's used
\ 1 %10000000000000000 1 %10000000000000 1 %1000000000000 1
\ tmc2130 heap-new constant mymotorX

\ 1 %100000000000000000 1 %1000000000000000 1 %100000000000000 0
\ tmc2130 heap-new constant mymotory
\ *****************************************************************
\ some of the tmc2130 registers
0x00 constant GCONF
0x01 constant GSTAT
0x10 constant IHOLD_IRUN
0x11 constant TPOWERDOWN
0x12 constant TSTEPS
0x13 constant TPWMTHRS
0x14 constant TCOOLTHRS
0x15 constant THIGH
0x6c constant CHOPCONF
0x6d constant COOLCONF
0x6F constant DRV_STATUS
0x70 constant PWMCONF
0x71 constant PWM_SCALE
0x6b constant MSCURACT
0x73 constant LOST_STEPS
0x6A constant MSCNT

[ifundef] destruction
  interface
     selector destruct ( -- ) \ to free allocated memory in objects that use this
  end-interface destruction
[endif]

object class
  destruction implementation  ( tmc2130 -- )
  selector enable-motor       ( tmc2130 -- )
  selector disable-motor      ( tmc2130 -- )
  selector putreg             ( ureg udata tmc2130 -- utmc2130_status nflag )
  selector getreg             ( ureg tmc2130 -- utmc2130_status udata nflag )
  protected

  inst-value spihandle
  inst-value enablebank
  inst-value enablebit
  inst-value dirbank
  inst-value dirbit
  inst-value stepbank
  inst-value stepio
  cell% inst-var u32data
  char% inst-var bytedata
  inst-value bufferA
  inst-value bufferB
  inst-value quickreg
  inst-value currentqr

  m: ( ugpiobank ugpiobitmask tmc2130 -- nflag ) \ will set bitmask of bank to output mode ... nflag is false for all good other value for some failure
    bbbiosetup false = if bbbiooutput bbbiocleanup else bbbiocleanup drop true then ;m method gpio-output

  m: ( ugpiobank ugpiobitmask tmc2130 -- nflag ) \ will set bitmask of bank to high output ... nflag is false for all good other value for some failure
    bbbiosetup false = if bbbioset bbbiocleanup else bbbiocleanup drop true then ;m method gpio-high

  m: ( ugpiobank ugpiobitmask tmc2130 -- nflag ) \ will set bitmask of bank to low output ... nflag is false for all good other value for some failure
    bbbiosetup false = if bbbioclear bbbiocleanup else bbbiocleanup drop true then ;m method gpio-low

  m: ( uaddr tmc2130 -- ndata ) \ takes string of 4 bytes and converts to 32 bit ndata
  \ uaddr is the buffer location for the string
  \ the string is always 4 bytes long
    0 { uaddr data }
    3 0 do uaddr i + c@ data or 8 lshift to data loop
    uaddr 3 + c@ data or
  ;m method $-data
  m: ( ndata tmc2130 -- uaddr ) \ takes ndata a 32 bit number and makes a string  and returns that string with uaddr
  \ note the string returned is always 4 bytes long
    { ndata }
    4 0 do ndata 0xff000000 and 24 rshift bufferB i + c! ndata 8 lshift to ndata loop  bufferB
  ;m method data-$
  public
  m: ( tmc2130 -- ) \ simply make enable pin high on tmc2130 driver board
    enablebank enablebit this gpio-high abort" disable-motor failure" ;m overrides disable-motor
  m: ( tmc2130 -- ) \ simply make enable pin low on tmc2130 driver board
    enablebank enablebit this gpio-low abort" enable-motor failure" ;m overrides enable-motor
  m: ( udirection tmc2130 -- ) \ udirection is 0 for left and 1 for right
    case
      0 of dirbank dirbit this gpio-low abort" gpio-low of setdirection failure" endof
      1 of dirbank dirbit this gpio-high abort" gpio-high of setdirection failure" endof
    endcase ;m method setdirection
  m: ( usteps tmc2130 -- ) \ fast step the motor usteps times ... this steps around 30 khz on BBB
    stepbank stepio bbbiosetup abort" bbbiosetup of faststeps failure"
    0 ?do
      bbbioset
      1000 0 do loop
      bbbioclear
      1000 0 do loop
    loop
    BBBiocleanup abort" bbbiocleanup of faststeps failure"
  ;m method faststeps
  m: ( utime usteps tmc2130 -- ) \ step the motor ustep times with utime between each step... utime of 1000 is around 30 khz on BBB
    { utime usteps }
    stepbank stepio bbbiosetup abort" bbbiosetup of timedsteps failure"
    usteps 0 ?do
      bbbioset
      utime 0 ?do loop
      bbbioclear
      utime 0 ?do loop
    loop
    BBBiocleanup abort" bbbiocleanup of timedsteps failure"
  ;m method timedsteps
  m: ( useconds usteps tmc2130 -- ) \ steps with usleep
    { utime usteps }
    stepbank stepio bbbiosetup abort" bbbiosetup of steps-usleep failure"
    usteps 0 ?do
      bbbioset
      utime usleep drop
      bbbioclear
      utime usleep drop
    loop
    BBBiocleanup abort" bbbiocleanup of steps-usleep failure"
  ;m method steps-usleep
  m: ( ubankenable uenableio ubankdir udirio ubankstep ustepio uspi tmc2130 -- nflag ) \ constructor
  \ nflag is false for configuration ok
  \ nflag is any other number meaning something did not work to configure this motor driver
    { ubankenable uenableio ubankdir udirio ubankstep ustepio uspi }
    try
      ubankenable [to-inst] enablebank uenableio [to-inst] enablebit
      enablebank enablebit this gpio-output abort" construct of tmc2130 failure a"
      this disable-motor \ this should turn off power to motor
      ubankdir [to-inst] dirbank udirio [to-inst] dirbit
      dirbank dirbit this gpio-output abort" construct of tmc2130 failure b"
      dirbank dirbit this gpio-low abort" construct of tmc2130 failure c"
      ubankstep [to-inst] stepbank ustepio [to-inst] stepio
      stepbank stepio this gpio-output abort" construct of tmc2130 failure d"
      stepbank stepio this gpio-low abort" construct of tmc2130 failure e"
      true [to-inst] spihandle
      uspi case
        \ this is spi1 on the BBB schematic and mode chart. Linux enumerates the spi starting at 1!
        1 of s\" /dev/spidev2.0\x00" drop O_NDELAY O_NOCTTY or O_RDWR or open [to-inst] spihandle endof
        \ this is spi0 on the BBB schematic and mode chart. Linux enumerates the spi starting at 1!
        0 of s\" /dev/spidev1.0\x00" drop O_NDELAY O_NOCTTY or O_RDWR or open [to-inst] spihandle endof
      endcase
      spihandle 0> if
        200000 u32data !
        spihandle SPI_IOC_WR_MAX_SPEED_HZ u32data ioctl abort" construct of tmc2130 failure f" \ set spi speed to 100000 hz
        8 bytedata c!
        spihandle SPI_IOC_WR_BITS_PER_WORD bytedata ioctl abort" construct of tmc2130 failure g" \ set bits per word to 8
        0 bytedata c!
        spihandle SPI_IOC_WR_MODE bytedata ioctl abort" construct of tmc2130 failure h" \ set to low on idle and capture on rising of clock
      else
        spihandle abort" construct of tmc2130 failure i"
      then
      6 allocate abort" construct of tmc2130 failure j" [to-inst] bufferA
      6 allocate abort" construct of tmc2130 failure k" [to-inst] bufferB
      9 10 2 multi-cell-array heap-new [to-inst] quickreg
      10 0 do 9 0 do 0 i j quickreg [bind] multi-cell-array cell-array! loop loop \ set quickreg's to 0
      0 [to-inst] currentqr
      false
    endtry
    restore
  ;m overrides construct
  m: ( tmc2130 -- ) \ destructor
    spihandle 0> if
      this disable-motor
      spihandle close abort" destruct of tmc2130 failure a"
      bufferA free abort" destruct of tmc2130 failure b"
      bufferB free abort" destruct of tmc2130 failure c"
      quickreg [bind] multi-cell-array destruct
      quickreg free abort" destruct of tmc2130 failure d"
    then ;m overrides destruct

  m: ( ureg tmc2130 -- utmc2130_status udata nflag ) \ read a register from tmc2130 device
    \ nflag is false if no apparent errors in spi communication aka the correct bytes sent and recieved
    \ nflag is true if the incorrect amount of bytes were sent or recieved and uspi_status and udata are returned as 0
    \ uspi_status is the spi_staus data returned from tmc2130 data transfer and only the lower 4 bits are valid
    \ udata is the data returned from tmc2130 for the ureg requested and is potenaly 32 bits of data
    bufferA 6 0 fill bufferB 6 0 fill
    %1111111 and
    bufferA c!
    spihandle bufferA 5 write 5 = if
      spihandle bufferB 5 read 5 = if
        bufferB c@ %00001111 and \ note only the lower 4 bits are used
        bufferB 1 + this $-data
        0
      else
        0 0 true
      then
    else
      0 0 true
    then
  ;m overrides getreg
  m: ( ureg udata tmc2130 -- utmc2130_status nflag ) \ write to ureg register udata value in the tmc2130 device
  \ nflag is false if no apparent errors in spi communication aka the correct bytes sent and recieved
  \ nflag is true if the incorrect amount of bytes were sent or recieved and uspi_status and udata are returned as 0
  \ uspi_status is the spi_staus data returned from tmc2130 data transfer and only the lower 4 bits are valid
    bufferA 6 0 fill bufferB 6 0 fill
    this data-$ bufferA 1 + 4 cmove
    %1111111 and %10000000 or bufferA c!
    spihandle bufferA 5 write 5 = if
      spihandle bufferB 5 read 5 = if
        bufferB c@ %00001111 and \ note only the lower 4 bits are used
        false
      else
        0 true
      then
    else
      0 true
    then
  ;m overrides putreg
  m: ( uGCONF uIHOLD_IRUN uTPOWERDOWN uTPWMTHRS uTCOOLTHRS uTHIGH uCHOPCONF uCOOLCONF uPWMCONF uqrindex tmc2130 -- )
  \ set values for quickreg at uqrindex ... note there are only 0 to 9 uqrindex settings ( 10 total quickreg )
  \ uqrindex is the quickreg that is being set and the other stack items are the values that are put in this quickreg
    { uqrindex }
    uqrindex 0>= uqrindex 10 < and if
      9 0 do 8 i - uqrindex quickreg [bind] multi-cell-array cell-array! loop
    then
  ;m method quickreg!
  m: ( uqrindex tmc2130 -- ) \ the previous values stored in uqrindex quickreg will be transfered to the tmc2130 device via spi interface
    { uqrindex }
    uqrindex 0>= uqrindex 10 < and if
      GCONF      0 uqrindex quickreg [bind] multi-cell-array cell-array@ this putreg abort" usequickreg failure a" drop
      IHOLD_IRUN 1 uqrindex quickreg [bind] multi-cell-array cell-array@ this putreg abort" usequickreg failure b" drop
      TPOWERDOWN 2 uqrindex quickreg [bind] multi-cell-array cell-array@ this putreg abort" usequickreg failure c" drop
      TPWMTHRS   3 uqrindex quickreg [bind] multi-cell-array cell-array@ this putreg abort" usequickreg failure d" drop
      TCOOLTHRS  4 uqrindex quickreg [bind] multi-cell-array cell-array@ this putreg abort" usequickreg failure e" drop
      THIGH      5 uqrindex quickreg [bind] multi-cell-array cell-array@ this putreg abort" usequickreg failure f" drop
      CHOPCONF   6 uqrindex quickreg [bind] multi-cell-array cell-array@ this putreg abort" usequickreg failure g" drop
      COOLCONF   7 uqrindex quickreg [bind] multi-cell-array cell-array@ this putreg abort" usequickreg failure h" drop
      PWMCONF    8 uqrindex quickreg [bind] multi-cell-array cell-array@ this putreg abort" usequickreg failure i" drop
      uqrindex [to-inst] currentqr
    then
  ;m method usequickreg
  m: ( tmc2130 -- ) \ print some stuff
    this [parent] print cr
    ." spihandle " spihandle . cr
    ." TSTEPS " TSTEPS this getreg abort" print failure a" . cr
    ." standstill " dup %1000 and 0= if ." no" else ." yes" then cr
    ." stallguard flag " dup %100 and 0= if ." off" else ." on" then cr
    ." driver error " dup %10 and 0= if ." no" else ." yes" then cr
    ." tmp2130 reset " %1 and 0= if ." no" else ." yes" then cr
    ." GSTAT " GSTAT this getreg abort" print failure b" . drop cr
    ." DRV_STATUS " DRV_STATUS this getreg abort" print failure c" dup u. swap drop cr
    ." PWM_SCALE " PWM_SCALE this getreg abort" print failure d" . drop cr
    ." SG_RESULT " dup %1111111111 and . cr
    ." fsactive " dup %1000000000000000 and 0= if ." microstep" else ." fullstep" then cr
    ." CS ACTUAL " dup %111110000000000000000 and 16 rshift . cr
    %10000000000000000000000000 and 0= if ." normal temperature!" else ." over temperature!" then cr
    ." quickreg that is being used " currentqr . cr
 ;m overrides print
end-class tmc2130

\ *****************************************************************
\\\
1 %10000000000000000 1 %10000000000000 1 %1000000000000 1
tmc2130 heap-new constant mymotorX

1 %100000000000000000 1 %1000000000000000 1 %100000000000000 0
tmc2130 heap-new constant mymotory
