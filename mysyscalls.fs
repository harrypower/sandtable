\ mysyscalls.fs


c-library mysyscalls \ GNU

\c #include <sys/types.h>
\c #include <sys/stat.h>
\c #include <fcntl.h>
\c #include <unistd.h>
\c #include <sys/ioctl.h>

c-function openGNU open a n -- n  ( ^zaddr  flags -- fd )
\   file descriptor is returned
\   Note zaddr points to a buffer containing the filename
\   string terminated with a null character.
c-function closeGNU close n -- n ( fd -- flag )
c-function readGNU read n a n -- n ( fd  buf  count --  n )
\ read count byes into buf from file
c-function writeGNU write n a n -- n ( fd  buf  count  --  n )
\ write count bytes from buf to file
c-function lseekGNU lseek n n n -- n ( fd  offset  type  --  offs )
\ reposition the file ptr
c-function ioctlGNU ioctl n n a -- n ( fd  request argp -- error )

end-c-library
