\ squaretest.fs

: square 140000 movetox 140000 movetoy drop drop
120000 movetox 140000 movetoy drop drop
120000 movetox 120000 movetoy drop drop
140000 movetox 120000 movetoy drop drop ;

: nsquare { usize ux uy }
  ux uy movetoxy .
  usize ux + uy movetoxy .
  usize ux + usize uy + movetoxy .
  ux usize uy + movetoxy .
  ux uy movetoxy . ;


: nrotsquare ( usize ux uy uangle ) \ uangle is degrees
  0 0 { usize ux uy uangle ua ub }
  ux uy movetoxy .

  uangle s>f 360 s>f f/
  2e pi f* f*
  fsin usize s>f f*
  90e fsin f/ f>s to ua
  90 uangle - s>f 360 s>f f/
  2e pi f* f*
  fsin ua s>f f*
  uangle s>f fsin f/ f>s to ub

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
