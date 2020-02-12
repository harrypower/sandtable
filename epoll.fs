\ epoll.fs

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

\ epoll() to test io with

\ Requires:

\ Revisions:
\ 02/11/2020 started coding

c-library epoll
  \c #include <stdio.h>
  \c #include <unistd.h>
  \c #include <sys/epoll.h>

  c-function epoll_create epoll_create n -- n ( nsize -- nfd )
  c-function epoll_create1 epoll_create1 n -- n ( nflag -- nfd )
  c-function epoll_ctl epoll_ctl n n n a -- n ( nepfd nop nfd astructevent -- nflag )
  c-function epoll_wait epoll_wait n a n n -- n ( nfd astructevent nmaxevents ntimeout -- neventcount )

end-c-library

\ Valid opcodes ( "op" parameter ) to issue to epoll_ctl().
1 constant EPOLL_CTL_ADD \ Add a file decriptor to the interface.
2 constant EPOLL_CTL_DEL \ Remove a file decriptor from the interface.
3 constant EPOLL_CTL_MOD \ Change file decriptor epoll_event structure.
\ EPOLL_EVENTS used in epoll_event struct below for events field
0x001 constant EPOLLIN
0x002 constant EPOLLPRI
0x004 constant EPOLLOUT


struct
  cell%   field *ptr
  cell%   field fd
  cell%   field u32
  double% field u64
end-struct epoll_data%

struct
  cell%       field events
  epoll_data% field data
end-struct epoll_event%
