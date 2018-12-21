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
  0 { usize ux uy uangle ua }
  ux uy movetoxy .
  uangle s>f fsin usize s>f f*
  90e fsin f/
  ux f>s dup to ua + to ux
  90 uangle - s>f fsin ua s>f f*
  uangle s>f fsin f/
  uy f>s - to uy
  ux uy movetoxy .

  uangle s>f fsin usize s>f f*
  90e fsin f/
  ux f>s dup to ua - to ux
  90 uangle - s>f fsin ua s>f f*
  uangle s>f fsin f/
  uy f>s - to uy
  ux uy movetoxy .

  uangle s>f fsin usize s>f f*
  90e fsin f/
  uy f>s dup to ua + to uy
  90 uangle - s>f fsin ua s>f f*
  uangle s>f fsin f/
  ux f>s - to ux
  ux uy movetoxy .

  uangle s>f fsin usize s>f f*
  90e fsin f/
  ux f>s dup to ua + to ux
  90 uangle - s>f fsin ua s>f f*
  uangle s>f fsin f/
  uy f>s + to uy
  ux uy movetoxy .  
  ;
