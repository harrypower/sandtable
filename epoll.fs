c-library epoll
  \c #include <stdio.h>
  \c #include <unistd.h>
  \c #include <sys/epoll.h>

  c-function epoll_create epoll_create n -- n ( nsize -- nfd )
  c-function epoll_create epoll_create1 n -- n ( nflag -- nfd )
  c-function epoll_ctl epoll_ctl n n n a -- n ( nepfd nop nfd astructevent -- nflag )
  c-function epoll_wait epoll_wait n a n n -- n ( nfd astructevent nmaxevents ntimeout -- neventcount )

end-c-library

1 constant EPOLL_CTL_ADD \ Add a file decriptor to the interface.
2 constant EPOLL_CTL_DEL \ Remove a file decriptor from the interface.
3 constant EPOLL_CTL_MOD \ Change file decriptor epoll_event structure.

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
end-struct epoll_event
