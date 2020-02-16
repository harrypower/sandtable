#! /usr/local/bin/gforth
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

0 pipefd readend pipe ." this is pipe returned status > " . cr

0 value cpid

: dochildparent ( -- ) \ this does not work
	fork to cpid
	cpid -1 = if ." fork failed! " cr bye   else ." fork worked" cr then   \ fork did not work so exit

	cpid 0 = if
		5000 ms
	  ." child speaking now!" cr
	  0 pipefd readend @ closeGNU throw
	  ." child writing to pipe!" cr
		s\" This is message from child\n"
		0 pipefd writeend @ write-file
	  0 pipefd writeend @ closeGNU throw
		." child wrote message and sent it!" cr
	  bye
	then

	cpid 0 > if
	  ." parent speaking now!" cr
	  0 pipefd writeend @ closeGNU throw
	  ." parent to read pipe " cr
		pad 80
		0 pipefd readend @ read-file throw
		pad swap type cr 
		." parent closeing pipe now!" cr
		0 pipefd readend @ closeGNU throw
	  bye
	then
;

dochildparent
