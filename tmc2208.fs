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


require serial.fs
require ./Gforth_Objects/stringobj.fs
require ./Gforth_Objects/objects.fs

object class
  protected
    22      constant EEprom_size      \ the size of calibration data eeprom on bmp180 device in bytes
    0x77    constant BMP180ADDR       \ I2C address of BMP180 device
    0xF6    constant CMD_READ_VALUE

end-class tmc2208

tmc2208 heap-new constant stepper
