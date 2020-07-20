\ stdatafiles.fs

\ error constants
s" bbbdatapath or pcdatapath do not exist cannot proceed!" exception constant nopath

: pcdatapath$@ ( -- caddr u ) \ return path string to output data for pc testing
  s" /home/pks/sandtable/" ;
: bbbdatapath$@ ( -- caddr u ) \ return path string to output data for BBB sandtable
  s" /home/debian/sandtable/" ;
: pc?bbb? ( -- nflag ) \ test if code running on pc or bbb ... nflag is true if on bbb ... nflag is false if on pc
  \ nflag is 1 if niether pc or bbb test works out
  pcdatapath$@ file-status swap drop false = if
    false
  else
    bbbdatapath$@ file-status swap drop false = if true else 1 then
  then ;
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
variable tmpname$
: statuschgowner ( -- ) \ make status file debian:debian
  0 { ntest }
  ststatusfile$@ file-status swap drop false = if
    pc?bbb? to ntest
    ntest true = if s" sudo chown debian:debian " tmpname$ $! then
    ntest false = if s" sudo chown pks:pks " tmpname$ $! then
    ntest 1 = if nopath throw then
    ststatusfile$@ tmpname$ $+!
    tmpname$ $@ system
    \ sudo chown debian:debian filenamepath.data
  then ;
: stcmdoutchgowner ( -- ) \ make stcmdout file debian:debian
  0 { ntest }
  stcmdoutfile$@ file-status swap drop false = if
     pc?bbb? to ntest
     ntest true = if s" sudo chown debian:debian " tmpname$ $! then
     ntest false = if s" sudo chown pks:pks " tmpname$ $! then
     ntest 1 = if nopath throw then
     stcmdoutfile$@ tmpname$ $+!
     tmpname$ $@ system
  then ;
0 value datafid
: shrink-write-file ( caddr u ufid -- ) \ ufid should be valid file that will get resized to 0 size and then caddr u string placed in it and then file is closed
  >r 0 s>d r@ resize-file throw
  r@ write-file throw
  r@ flush-file throw
  r> close-file throw ;
: cmddatarecieve! ( caddr u -- ) \ put caddr u string into stcmdinfile$@ file
  stcmdinfile$@ opendata
  shrink-write-file ;
: cmddatarecieve@ ( -- caddr u nflag ) \ get caddr u string from stcmdinfile$@ file
  stcmdinfile$@ file-status swap drop false =
  if stcmdinfile$@ slurp-file true else 0 0 false then ;
: cmddatasend@ ( -- caddr u nflag ) \ get message from sandtable via stcmoutfile$@ and put that in string caddr u
\ nflag is true if the message from sandtable is present only
\ nflag is false if there is no message yet from sandtable
  stcmdoutfile$@  file-status swap drop false =
  if stcmdoutfile$@ slurp-file true else 0 0 false then ;
: cmddatasend! ( caddr u -- ) \ put caddr u string into stcmdoutfile$@
  stcmdoutfile$@ opendata
  shrink-write-file
  stcmdoutchgowner ;
: cmddatasenddelete ( -- ) \ delete stcmdoutfile
  stcmdoutfile$@ file-status swap drop false =
  if stcmdoutfile$@ delete-file throw then ; \ delete file if it is present.
: testdataout ( caddr u -- ) \ append caddr u string to the testoutfile and put a time stamp with string
  testoutfile$@ opendata to datafid
  datafid file-size throw
  datafid reposition-file throw
  utime udto$ datafid write-line throw
  datafid write-line throw
  datafid flush-file throw
  datafid close-file throw ;
: stlastresultout ( caddr u -- ) \ create last result file using caddr u string
  stlastresultfile$@ opendata
  shrink-write-file ;
: stlastresultin ( -- caddr u nflag ) \ read last result data and place in caddr u string ... nflag is true if last result is present .. nflag is false if not present
  stlastresultfile$@ file-status swap drop false =
  if stlastresultfile$@ slurp-file true else 0 0 false then ;
: stcalibrationout ( ux uy -- ) \ save the calibration data to file
  stcalfile$@ opendata to datafid
  0 s>d datafid resize-file throw
  swap s>d udto$ datafid write-line throw \ write x
  s>d udto$ datafid write-line throw \ write y
  datafid flush-file throw
  datafid close-file throw ;
: stcalibrationin ( -- ux uy nflag ) \ retreive calibration data from file
  \ nflag is true if calibration data is present and false if there is no calibartion data
  0 0 { ux uy }
  stcalfile$@ opendata to datafid
  pad 30 datafid read-line throw drop pad swap s>unumber? if d>s to ux else 0 0 false exit then
  pad 30 datafid read-line throw drop pad swap s>unumber? if d>s to uy else 0 0 false exit then
  datafid close-file throw
  ux uy true ;
: getstatus ( -- caddr u nflag ) \ get the status info from sandtable
\ nflag is true if status info file exists
\ nflag is false if no file
  ststatusfile$@ file-status swap drop false =
  if ststatusfile$@ slurp-file true else 0 0 false then ;
: putstatus ( caddr u -- ) \ put the status message caddr u string out to ststatusfile$@
  ststatusfile$@ opendata
  shrink-write-file
  statuschgowner ;
: cmdstatusdelete ( -- ) \ delete ststatusfile
  ststatusfile$@ file-status swap drop false =
  if ststatusfile$@ delete-file throw then ; \ delete ststatusfile if it is present.
