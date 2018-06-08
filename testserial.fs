require strings.fs
require syscalls386.fs

\ termios structure
create termios 4 4 + 4 + 4 + 2 + 64 + 4 + 4 + allot

0 constant C_IFLAG
4 constant C_OFLAG
8 constant C_CFLAG
12 constant C_LFLAG
16 constant C_LINE
18 constant C__CC
82 constant C_ISPEED
86 constant C_OSPEED

\ c_iflag bits

   1 constant IGNBRK
   2 constant BRKINT
   4 constant IGNPAR
   8 constant PARMRK
  16 constant INPCK
  32 constant ISTRIP
  64 constant INLCR
 128 constant IGNCR
 256 constant ICRNL
 512 constant IUCLC
1024 constant IXON
2048 constant IXANY
4096 constant IXOFF
8192 constant IMAXBEL

\ c_oflag bits

   1 constant OPOST
   2 constant OLCUC
   4 constant ONLCR
   8 constant OCRNL
  16 constant ONOCR
  32 constant ONLRET
  64 constant OFILL
 128 constant OFDEL
 256 constant NLDLY

\ c_cflag bits
			\ baud rates constants
4111 constant CBAUD
   0 constant B0
   7 constant B300
   9 constant B1200
  11 constant B2400
  12 constant B4800
  13 constant B9600
  14 constant B19200
  15 constant B38400
4097 constant B57600
4098 constant B115200
			\ character size constants
  48 constant CSIZE
   0 constant CS5
  16 constant CS6
  32 constant CS7
  48 constant CS8

\ parity constants

768 constant CPARITY
0 constant PARNONE
256 constant PAREVEN
768 constant PARODD

\ stop bits constants

64 constant CSTOPB
0 constant ONESTOPB
64 constant TWOSTOPB

\ c_lflag bits

   1 constant ISIG
   2 constant ICANON
   4 constant XCASE
   8 constant ECHO
  16 constant ECHOE
  32 constant ECHOK
  64 constant ECHONL
 128 constant NOFLSH
 256 constant TOSTOP
 512 constant ECHOCTL
1024 constant ECHOPRT
2048 constant ECHOKE
4096 constant FLUSHO
16384 constant PENDIN
32768 constant IEXTEN

\ com port constants

0 constant COM1
1 constant COM2
2 constant COM3
3 constant COM4


\ ioctl request constants

hex
5401 constant TCGETS
5402 constant TCSETS
540B constant TCFLSH
541B constant FIONREAD
decimal

\ file control constants

hex
800 constant O_NDELAY
100 constant O_NOCTTY
002 constant O_RDWR
decimal

: serial_getoptions ( handle -- | read serial port options into termios )
	TCGETS termios ioctl ( drop )  ;

: serial_setoptions ( handle -- | write termios into serial port options )
	TCSETS termios ioctl ( drop ) ;

: serial_open ( port -- handle | opens the serial port for communcation )
	\ port is the serial port to open
	\ 0 = ttyS0 (COM1)
	\ 1 = ttyS1 (COM2)
	\ 2 = ttyS2 (COM3)
	\ 3 = ttyS3 (COM4)
	\ handle is a handle to the open serial port
	\ if handle < 0 there was an error opening the port
  dup
  0 >=
  if
    s>string count
    s" /dev/ttyO"
    2swap
    strcat
    strpck
    O_RDWR O_NOCTTY  O_NDELAY or or
    open
  \  dup
  \  dup
  \  serial_getoptions \ swap

    \ Disable XON/XOFF flow control and CR to NL mapping

  \  termios C_IFLAG + @
  \  IXON IXOFF or IXANY or ICRNL or ( not) invert and
  \  termios C_IFLAG + !

  \  serial_setoptions
  then ;

: serial_close ( handle -- | closes the port )
	\ handle = serial port handle received from serial_open

	close ;
