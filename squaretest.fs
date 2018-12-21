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


: nrotsquare ( usize ux uy uangle -- )
  { usize ux uy uangle }
  ;
