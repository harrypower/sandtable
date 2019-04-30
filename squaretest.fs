\ squaretest.fs

: n>square ( usize ux uy uangle -- ) \ uangle is degrees
  0 0 0 0 { usize ux uy uangle ua ub ux1 uy1 }
  uangle 360 mod to uangle
  uangle 0 <> if
    uangle s>f pi 180e f/ f*
    fsin usize s>f f*
    90e pi 180e f/ f*
    fsin f/ f>s to ua
    90 uangle - s>f pi 180e f/ f*
    fsin ua s>f f*
    uangle s>f pi 180e f/ f*
    fsin f/ f>s to ub
  else
    usize to ub
    0 to ua
  then

  ux uy ux ub + dup to ux1 uy ua - dup to uy1 ( .s ." first " ) drawline . \ testdata
  ux1 uy1 ux ub + ua - dup to ux1 uy ua - ub - dup to uy1 ( .s ." second " ) drawline . \ testdata
  ux1 uy1 ux ua - dup to ux1 uy ub - dup to uy1 ( .s ." third " ) drawline . \ testdata
  ux1 uy1 ux uy ( .s ." last " ) drawline . \ testdata
  ;

: nrotsquare { usize ux uy uangle usteps -- }
  usteps 0 ?do usize ux uy uangle 360 usteps / i * + n>square loop ;

: nsquare ( usize ux uy -- ) \ draw usize square using drawline starting at ux uy location
  { usize ux uy }
  ux uy ux usize + uy drawline .
  ux usize + uy ux usize + uy usize + drawline .
  ux usize + uy usize + ux uy usize + drawline .
  ux uy usize + ux uy drawline . cr ;

require random.fs
: rndsquares ( namount -- )
  seed-init
  0 ?do
    xm-max random \ usize
    xm-max 2 / random \ ux
    dup movetox .
    ym-max 2 / random \ uy
    dup movetoy .
    .s cr
    nsquare
  loop ;

: rndsquares2 ( namount -- )
  seed-init
  0 ?do
    xm-max random
    xm-max random - \ usize
    xposition
    yposition
    nsquare
  loop ;
