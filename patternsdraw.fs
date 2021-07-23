\ patternsdraw.fs

\    Copyright (C) 2021  Philip King Smith

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

\ words to read data from vector files made by vectortools.f
\ then this vector data can be drawn on the sandtable with and angle offset and a scale factor  at any x y location

\ Requires:
\ this code runs under Gforth
\ code is closly following patternsdraw.f from sandsim-win32forth but does deviate

\ Revisions:
\ 07/21/2021 started coding
true constant debugging \ true is for testing on pc false is for normal use on BBB sandtable device
debugging [if]
  require c:\users\philip\documents\github\gforth-objects\double-linked-list.fs
\  require c:\users\philip\documents\github\sandtable\sandmotorapi.fs
[else]
  require Gforth-Objects/double-linked-list.fs \ *** this is the final form but for testing use below
  require sandmotorapi.f
[then]

10 set-precision

\ : openvectorfile ( -- )  \ **** this word will change after testing
\  s" c:\Users\Philip\Documents\inkscape-stuff\vector.data" r/o open-file throw to fid ;

[ifundef] destruction
  interface
     selector destruct ( -- ) \ to free allocated memory in objects that use this
  end-interface destruction
[endif]
[ifundef] thesize
  interface
    selector qnt:
  end-interface thesize
[endif]

double-linked-list class
  destruction implementation
  thesize implementation
  struct
    dfloat% field fangle
    dfloat% field fdistance
  end-struct vectordata%
  cell% inst-var fid
  cell% inst-var buffersize
  cell% inst-var adpair$

  m: ( naddr u rawad -- nflag  fs: -- fa ) \ string naddr u if it contains the angle string turn it in floating stack and true
      2dup s"  " search if swap drop - >float if true else false 0.0e then else 2drop 2drop false 0.0e then ;m method getaf
  m: ( naddr u rawad -- nflag  fs: -- fd ) \ string naddr u if it contains the distance string turn it in floating stack and true
  \ if string is not understandable as a floating number then return false and the floating stack contains 0.0e
    s"  " search if 1 /string  -trailing >float if true else false 0.0e then else 2drop false 0.0e then ;m method getdf
  m: ( naddr u rawad -- nflag fs: -- fa fd )
    2dup this getaf rot rot this getdf and ;m method getadf
  m: ( rawad -- nflag fs: -- fx fy )
  \ nflag is true if an xy pair was read in from file
  \ nflag is false if the file has no more lines to read or if the raw.data file is not readable
  \ the floating value of xy pair are returned on floating stack and is only valid if nflag is true
    adpair$ @ buffersize @ fid @ read-line throw
    if
      adpair$ @ swap this getadf
    else
      drop false 0.0e 0.0e
    then ;m method getadpair
  public
  m: ( rawad -- )
    this [parent] construct
    256 buffersize !
    buffersize @ chars allocate throw adpair$ !
  ;m overrides construct
  m: ( rawad -- )
    this [parent] destruct
    adpair$ @ free throw
  ;m overrides destruct
  m: ( rawad -- fs: fangle fdistance -- ) \ store fangle and fdistance in list
    vectordata% %size allocate throw
    dup dup  fdistance f! fangle f!
    vectordata% %size this ll!
  ;m method fad!:
  m: ( rawad -- fs: -- fangle fdistance ) \ retrieve nx ny from list at current link
    this ll@ drop dup
    fangle f@
    fdistance f@
  ;m method fad@:
  m: ( rawad -- usize ) \ return current size of lists
    this ll-size@
  ;m overrides qnt:
  m: ( caddr u rawad -- ) \ opens and reads file with name caddr u and puts the xy data into a rawda linked list
    r/o open-file throw fid !
    this destruct
    this construct
    begin
      this getadpair
      if this fad!: false else fdrop fdrop true then
    until
    fid @ close-file throw ;m method readrawad
end-class rawad

rawad dict-new constant arawadlist

double-linked-list class
  thesize implementation
  selector fxy!:
  selector fxy@:
  struct
    dfloat% field fx
    dfloat% field fy
  end-struct pointdata%
  m:  ( deltaxy -- f: fangle -- fangle1 ) \ convert degrees to radians
    ( fpi 180e f/ ) 0.01745329251e f* ;m method degrees>radians
  m: ( nxscale nyscale nangle deltaxy -- f: fangle fdistance -- fx fy )
  \ given the nangle change and the nxscale and nyscale change calculate the fx and fy of the input fangle and fdistance data
  \ fx and fy is the change assuming the fangle fdistance started at x 0 and y 0
    fswap degrees>radians s>f degrees>radians f+ ( nxscale nyscale f: fdistance fangle1 )
    fswap fdup frot fdup frot fswap ( nxscale nyscale fs: fdistance fangle1 fdistance fangle1 )
    fcos f* ( nxscale nyscale f: fdistance fangle1 fx )
    frot frot fsin f* ( nxscale nyscale f: fx fy )
    s>f 100.0e f/ f* ( nxscale f: fx fy1 )
    fswap s>f 100.0e f/ f* fswap ( f: fx1 fy1 )
  ;m method calcpolar>rect
  m: ( nxscale nyscale nangle deltaxy -- )
    { nxscale nyscale nangle -- } \ read the rawad data and calculate the offsetpoint data given the nxscale nyscale and nangle data then store in deltaxy
    this destruct
    this construct
    arawadlist ll-set-start
    arawadlist qnt: 0 ?do
      arawadlist fad@: nxscale nyscale nangle this calcpolar>rect
      this fxy!:
      arawadlist ll> drop
    loop ;m method calcdeltaxy

  public
  m: ( deltaxy -- fs: fangle fdistance -- ) \ store fangle and fdistance in list
    pointdata% %size allocate throw
    dup dup  fy f! fx f!
    pointdata% %size this ll!
  ;m overrides fxy!:
  m: ( deltaxy -- fs: -- fangle fdistance ) \ retrieve nx ny from list at current link
    this ll@ drop dup
    fx f@
    fy f@
  ;m overrides fxy@:
  m: ( deltaxy -- usize ) \ return current size of lists
    this ll-size@
  ;m overrides qnt:
end-class deltaxy

deltaxy dict-new constant adeltaxy

\\\ testing above code

0.0e fvalue fx1
0.0e fvalue fy1
0.0e fvalue fx2
0.0e fvalue fy2

: drawpattern ( nx ny nxscale nyscale nangle -- ) \ draw the pattern starting at nx ny with nxscale and nyscale with rotation of nangle
  calcdeltaxy
  s>f to fy1 s>f to fx1
  >firstlink: deltaxy
  qnt: deltaxy  0 ?do
    fxy@: deltaxy fy1 f+ to fy2 fx1 f+ to fx2
    >nextlink: deltaxy
    fx1 f>s fy1 f>s fx2 f>s fy2 f>s drawline drop
    fx2 to fx1 fy2 to fy1
  loop
;
