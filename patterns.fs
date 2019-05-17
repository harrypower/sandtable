\ patterns.fs

\    Copyright (C) 2019  Philip King Smith

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

\ patterns for drawing on sandtable

\ Requires:
\ sandmotorapi.fs
\ random.fs

\ Revisions:
\ 04/06/2019 started coding

require random.fs
require sandmotorapi.fs

: rndstar ( uamount -- ) \ will start at a random board location and draw random length lines from that start point radiating out
  xm-max random ym-max random 0 0 { nx ny nx1 ny1 }
  0 ?do
    xm-max random to nx1
    ym-max random to ny1
    nx ny nx1 ny1 drawline .
    nx1 ny1 nx ny drawline .
  loop ;

: rndstar2 ( uamount nx ny -- )
  0 0 { uamount nx ny nx1 ny1 }
  uamount 0 ?do
    xm-max random to nx1
    ym-max random to ny1
    nx ny nx1 ny1 drawline .
    nx1 ny1 nx ny drawline .
  loop ;

: linestar ( nx ny nangle usize uquant -- ) \ move to nx ny and draw nquant lines of nsize from nx ny location with rotation of nangle
  0 { nx ny nangle usize uquant uintangle }
  xposition yposition nx ny drawline .
  uquant s>f 360e f/ f>s to uintangle
  uquant 0 ?do
    nx ny
    uintangle nangle + s>f fcos usize s>f f* f>s nx +
    uintangle nangle + s>f fsin usize s>f f* f>s ny +
    drawline .
    xposition yposition nx ny drawline .
  loop ;
