\ forkmessaging.fs

\    Copyright (C) 2020  Philip King Smith

\    This program is free software: you can redistribute it and/or modify
\    it under the terms of the GNU General Public License as published by
\    the Free Software Foundation, either version 3 of the License, or
\    (at your option) any later version.

\    This program is distributed in the hope that it will be useful,
\    but WITHOUT ANY WARRANTY; without even the implied warranty of
\    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\    GNU General Public License for more details.

\    You should have received a copy of the GNU General Public License
\    along with this program.  If not, see <http://www.gnu.org/licenses/>.

\ commands used by socket server to control sandtable

\ Requires:

\ Revisions:
\ 02/12/2020 started coding

c-library forkmessaging
  \c #include <unistd.h>
  \c #include <stdlib.h>
  \c #include <sys/wait.h>
  \c #include <sys/types.h>
  \ need to export these types used in functions below to use the functions but this will be later as i need them


  c-function fork     fork    void -- n   ( -- npid ) \ fork process npid is 0 to child forked process and a non zero non repeated pid for the parent process
  c-function getpid   getpid  void -- n   ( -- npid ) \ returns the process ID of the calling process
  c-function getppid  getppid void -- n   ( -- npid ) \ returns the process ID of the parent of the calling process.
  \ If the calling process was created by the fork() function and the parent process still exists at the time of the getppid function call,
  \ this function returns the process ID of the parent process. Otherwise, this function returns a value of 1 which is the process id for init process.
  c-function exit()   exit()  n -- void   ( nstatus -- ) \ exit process
  c-function wait     wait    a -- n      ( a*wstatus -- npid_t )
  c-function pipe     pipe    a -- n      ( apipefd[2] -- n )
\  c-function pipe2    pipe2   a n -- n    ( apipefd[2] nflags -- n )
end-c-library
