\ squaretest.fs

: nsquare { usize ux uy }
  ux uy movetoxy .
  usize ux + uy movetoxy .
  usize ux + usize uy + movetoxy .
  ux usize uy + movetoxy .
  ux uy movetoxy . ;

: nanglesquare ( usize ux uy uangle ) \ uangle is degrees
  0 0 { usize ux uy uangle ua ub }
  ux uy movetoxy .

  uangle s>f pi 180e f/ f*
  fsin usize s>f f*
  90e pi 180e f/ f*
  fsin f/ f>s to ua
  90 uangle - s>f pi 180e f/ f*
  fsin ua s>f f*
  uangle s>f pi 180e f/ f*
  fsin f/ f>s to ub

  ux ub +
  uy ua -
  movetoxy .

  ux ub + ua -
  uy ua - ub -
  movetoxy .

  ux ua -
  uy ub -
  movetoxy .

  ux uy  movetoxy . ;

: nrotsquare { usize ux uy uangle usteps } \ will make an angle square but then rotate it usteps around 360 degrees for a full circle
  usteps 0 ?do usize ux uy uangle 360 usteps / i * + nanglesquare loop ;

: nsquare2 ( usize ux uy -- ) \ draw usize square using drawline starting at ux uy location
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
    xm-max random
    xm-max random - \ ux
    ym-max random
    ym-max random - \ uy
    .s
    nsquare2
  loop ;
