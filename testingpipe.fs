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

0 pipefd readend pipe ." this is pipe returned status at its creation time > " . cr

0 value cpid
variable status

: dochildparent ( -- ) \ this does not work
	fork to cpid
	cpid -1 = if ." fork failed! " cr bye   else ." fork worked " cr then   \ fork did not work so exit

	cpid 0 = if
		5000 ms
	  ." child speaking now!" cr
	  0 pipefd readend @ closeGNU throw
	  ." child writing to pipe!" cr
		0 pipefd writeend @	s\" This is message from child\n"
 		writeGNU . ." < how much was writen in chars!" cr
		." child wrote message and sent it! pipe closing!" cr
	  0 pipefd writeend @ closeGNU throw
	then

	cpid 0 > if
	  ." parent speaking now!" cr
	  0 pipefd writeend @ closeGNU throw
	  ." parent to read pipe " cr
		0 pipefd readend @ pad 80
		readGNU
		pad swap type cr
		." parent closeing pipe now!" cr
		0 pipefd readend @ closeGNU throw
		status wait
		. ." < returned from wait! " cr
 	then
;

dochildparent

bye
