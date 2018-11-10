\ squaretest.fs

: square 140000 movetox 140000 movetoy drop drop
120000 movetox 140000 movetoy drop drop
120000 movetox 120000 movetoy drop drop
140000 movetox 120000 movetoy drop drop ;

: nsquare { usize ux uy }
  ux movetox . uy movetoy .
  usize ux + movetox .
  usize uy + movetoy .
  ux usize - movetox .
  uy usize - movetoy . ;
  
