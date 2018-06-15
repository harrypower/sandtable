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
\
\ Revisions:
\ 6/15/2018 started coding

require serial.fs
require ./Gforth-Objects/objects.fs
require ./Gforth-Objects/stringobj.fs
require ./BBB_Gforth_gpio/BBB_GPIO_lib.fs

[ifundef] destruction
  interface
     selector destruct ( -- ) \ to free allocated memory in objects that use this
  end-interface destruction
[endif]

object class
  destruction implementation
  protected
  0x0F      constant GCONF
  %10100000 constant sync
  0x00      constant slave-addr


  inst-value motors
  inst-value uart-a-handle
  inst-value buffer$
  public
  m: ( umotors -- ) \ constructor
    dup 1 >= swap 2 <= and if [to-inst] motors else false abort" only 1 or 2 motors at this time" then
    string heap-new [to-inst] buffer$
  ;m overrides construct

  m: ( -- ) \ destructor
    buffer$ [bind] string destruct
  ;m overrides destruct

  m: ( -- ) \ configure uart
    1 serial_open dup 0> if [to-inst] uart-a-handle else throw then
    uart-a-handle B115200 serial_setbaud
    uart-a-handle ONESTOPB serial_setstopbits
    uart-a-handle PARNONE serial_setparity
    uart-a-handle serial_flush
  ;m method conf-uart

  m: ( -- uamount-write uaddr uamount-read   ) \ test data recieve
    uart-a-handle serial_flush
    0xa0 pad c! pad 1 buffer$ [bind] string !$
    0x00 pad c! pad 1 buffer$ [bind] string !+$
    0x0F pad c! pad 1 buffer$ [bind] string !+$
    0xb6 pad c! pad 1 buffer$ [bind] string !+$
    uart-a-handle buffer$ [bind] string @$ 4 serial_write
    uart-a-handle pad 8 serial_read pad swap
  ;m method readdata
end-class tmc2208

2 tmc2208 heap-new constant mymotors
mymotors conf-uart
mymotors readdata
