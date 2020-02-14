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
