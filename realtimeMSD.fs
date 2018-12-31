\ realtimeMSD.fs
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
\ objects.fs

\ Revisions:
\ 31/12/2018 started coding

require Gforth-Objects/objects.fs

[ifundef] destruction
  interface
     selector destruct ( -- ) \ to free allocated memory in objects that use this
  end-interface destruction
[endif]

object class
  destruction implementation  ( realtimeMSD -- )
  protected

  float% inst-var mean
  float% inst-var previous-mean
  float% inst-var standard-deviation-sample
  float% inst-var standard-deviation-pop
  float% inst-var variance-sample
  float% inst-var variance-pop
  float% inst-var data
  float% inst-var amount

  public

  m: ( realtimeMSD -- )
    0e mean f!
    0e previous-mean f!
    0e standard-deviation-pop f!
    0e standard-deviation-sample f!
    0e variance-pop f!
    0e variance-sample f!
    0e data f!
    0e amount f!
  ;m overrides construct

  m: ( realtimeMSD -- )
    this construct
  ;m overrides destruct

  m: ( ndata realtimeMSD -- )

  ;m method n>data

  m: ( fdata realtimeMSD -- )

  ;m method f>data
end-class realtimeMSD
