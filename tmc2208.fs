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

[ifundef] destruction
  interface
     selector destruct ( -- ) \ to free allocated memory in objects that use this
  end-interface destruction
[endif]

require serial.fs
require ./Gforth-Objects/objects.fs
require ./Gforth-Objects/stringobj.fs
require ./BBB_Gforth_gpio/BBB_GPIO_lib.fs


object class
  protected
  0x0F      constant GCONF
  %10100000 constant sync
  0x00      constant slave-addr


  inst-value motors

  public
  m: ( umotors -- ) \ constructor
    dup 1 >= swap 2 <= and if [to-inst] motors else false abort" only 1 or 2 motors at this time" then
  ;m overrides construct

  m: ( -- ) \ destructor

  ;m overrides destruct


end-class tmc2208

tmc2208 heap-new constant mymotors
