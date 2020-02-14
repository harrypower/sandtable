\ testingpipe.fs
\ testing pipe fork and such for messaging issues


require forkmessaging.fs

: allocate-structure: ( usize ustruct "allocated-structure-name" -- )  \ creates a word called allocated-structure-name
  \ usize is the quantity of the indexed array to create
  \ ustruct is the size of the structure that is returned from the name used with end-struct
	%size dup rot swap * allocate throw create , ,
	DOES> ( uindex -- uaddr ) \
		dup >r cell+ @ * r> @ + ;
struct
  cell% field readend
  cell% field writeend
end-struct pipefd%

1 pipefd% allocate-structure: pipefd

0 pipefd readend pipe ." this is the returned message > " . cr

0 value cpid
fork to cpid

cpid -1 [if] -1 exit  [else] ." fork worked" cr [then]  \ fork did not work so exit

: dochildparent
cpid 0 = if
  ." child speaking now! " cr
  0 pipefd readend @ closeGNU throw
  ." child writing to pipe " cr
  0 pipefd writeend @ closeGNU throw
  0 exit
then

cpid 0 > if
  ." parent speaking now!" cr
  0 pipefd writeend @ closeGNU throw
  ." parent to read pipe " cr
  0 pipefd readend @ closeGNU throw
  0 exit
then
;

dochildparent
