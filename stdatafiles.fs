\ stdatafiles.fs

\ error constants
s" bbbdatapath or pcdatapath do not exist cannot proceed!" exception constant nopath

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
    u 0> if caddr u else nopath throw then ;
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
  s" stcmdin.data" path+name ;
: stcmdoutfile$@ ( -- caddr u ) \ sandtable result of command received via stcmdin
  s" stcmdout.data" path+name ;
: opendata ( caddr u -- ufid ) \ caddr u is a string for path of file to be opened for writing to .. if it is not present then create that file
  2dup file-status swap drop false = if
    r/w open-file throw
  else
    r/w create-file throw
  then ;
0 value datafid
: testdataout ( caddr u -- ) \ append caddr u string to the testoutfile and put a time stamp with string
  testoutfile$@ opendata to datafid
  datafid file-size throw
  datafid reposition-file throw
  utime udto$ datafid write-line throw
  datafid write-line throw
  datafid flush-file throw
  datafid close-file throw ;
: stlastresultout ( caddr u -- ) \ create last result file using caddr u string
  stlastresultfile$@ file-status swap drop false = if
    stlastresultfile$@ w/o open-file throw to datafid
    0 0 datafid resize-file throw
  else
    pathfile$ $@ w/o create-file throw to datafid
  then
  datafid write-file throw
  datafid flush-file throw
  datafid close-file throw ;
: stlastresultin ( -- caddr u nflag ) \ read last result data and place in caddr u string ... nflag is true if last result is present .. nflag is false if not present
  stlastresultfile$@ file-status swap drop false =
    if stlastresultfile$@ slurp-file else 0 0 false exit then ;
: stcalibrationout ( ux uy -- ) \ save the calibration data to file
  datapath? if pathfile$ $! stcalibration$@ pathfile$ $+! else nopath throw then
  pathfile$ $@ file-status swap drop false = if pathfile$ $@ delete-file throw then
  pathfile$ $@ w/o create-file throw to calibratefid
  swap s>d udto$ calibratefid write-line throw \ write x
  s>d udto$ calibratefid write-line throw \ write y
  calibratefid flush-file throw
  calibratefid close-file throw ;
: stcalibrationin ( -- ux uy nflag ) \ retreive calibration data from file
  \ nflag is true if calibration data is present and false if there is no calibartion data
  0 0 { ux uy }
  datapath? if pathfile$ $! stcalibration$@ pathfile$ $+! else nopath throw then
  pathfile$ $@ file-status swap drop false = if pathfile$ $@ r/o open-file throw to calibratefid else 0 0 false exit then
  pad 20 calibratefid read-line throw drop pad swap s>unumber? if d>s to ux else 0 0 false exit then
  pad 20 calibratefid read-line throw drop pad swap s>unumber? if d>s to uy else 0 0 false exit then
  calibratefid close-file throw
  ux uy true ;
