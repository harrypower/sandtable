\ stdatafiles.fs

\ error constants
s" bbbdatapath or pcdatapath do not exist cannot proceed!" exception constant nopath

variable convert$
: udto$ ( ud -- caddr u )  \ convert unsigned double to a string
    <<# #s  #> #>> convert$ $! convert$ $@ ;
: dto$ ( d -- caddr u )  \ convert double signed to a string
    swap over dabs <<# #s rot sign #> #>> convert$ $! convert$ $@ ;

: pcdatapath$@ ( -- caddr u ) \ return path string to output data for pc testing
  s" /home/pks/sandtable/" ;
: bbbdatapath$@ ( -- caddr u ) \ return path string to output data for BBB sandtable
  s" /home/debian/sandtable/" ;
: fullpath$@ ( -- caddr u ) \ return either pc or bbb path
  0 0 { caddr u }
  pcdatapath$@ file-status swap drop false = if
    pcdatapath$@ to u to caddr then
  bbbdatapath$@ file-status swap drop false = if
    bbbdatapath$@ to u to caddr then
    u 0> = if caddr u else nopath throw then ;
variable temppath$
: path+name ( caddr u -- caddr1 u1 ) \ return the full path and file name
\ caddr u is the file name to add to the path
  fullpath$@ temppath$ $!
  temppath$ $+!
  temppath$ $@ ;
: testoutfile$@ ( -- caddr u ) \ the file name for test output info
  s" sttest.data" path+name ;
: stlastresultfile$@ ( -- caddr u ) \ the file name for the sandtable last result info.
  s" stlastresult.data" path+name ;
: stcalfile$@ ( -- caddr u ) \ the file where the calibration data is
  s" stcalibration.data" path+name ;
: ststatusfile$@ ( -- caddr u ) \ will have the current status of the sandtable
  s" stcurrentstatus.data" path+name ;
: stcmdinfile$@ ( -- caddr u ) \ sandtable commands to be performed
  s" stcmdin" path+name ;
: stcmdoutfile$@ ( -- caddr u ) \ sandtable result of command received via stcmdin
  s" stcmdout" path+name ;
: opendata ( caddr u -- ufid ) \ caddr u is a string for path of file to be opened for writing to .. if it is not present then create that file
  2dup file-status swap drop false = if
    r/w open-file throw
  else
    r/w create-file throw
  then ;
0 value dataoutfid
: testdataout ( caddr u -- ) \ append caddr u string to the testoutfile and put a time stamp with string
  testoutfile$@ opendata to dataoutfid
  dataoutfid file-size throw
  dataoutfid reposition-file throw
  utime udto$ dataoutfid write-line throw
  dataoutfid write-line throw
  dataoutfid flush-file throw
  dataoutfid close-file throw ;
