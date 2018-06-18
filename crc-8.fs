\ crc-8.fs

: crc8-ATM ( uaddr u -- ucrc ) \ uaddr u contains string of data to make the crc for
\ crc calculated and returned note it is only to be 8 bits wide
  0 0 { uaddr u currentByte crc }
  u 0 ?do
    uaddr i + c@ to currentByte
    8 0 do
      crc 7 rshift currentByte 0x01 and xor 0 =
      if
        crc 1 lshift %11111111 and to crc
      else
        crc 1 lshift 0x07 xor %11111111 and to crc
      then
      currentByte 1 rshift %11111111 and to currentByte
    loop
  loop
  crc ;

\\\
\ following code is c++ code that worked..
\ the 0x05 0x00 0x00 0x00 that generated the following output
\ 5 0 0 72  so in hex that is 0x05 0x00 0x00 0x48 ( this means read gconf)
\ this response is as follows from tmc2208
\ 0x05 0xff 0x00 0x00 0x00 0x01 0x01 00xbb
// Example program
#include <iostream>
#include <string>


typedef unsigned char UCHAR;
void swuart_calcCRC(UCHAR* datagram, UCHAR datagramLength)
{
int i,j;
UCHAR* crc = datagram + (datagramLength-1); // CRC located in last byte of message
UCHAR currentByte;
*crc = 0;
for (i=0; i<(datagramLength-1); i++) { // Execute for all bytes of a message
currentByte = datagram[i]; // Retrieve a byte to be sent from Array
for (j=0; j<8; j++) {
if ((*crc >> 7) ^ (currentByte&0x01)) // update CRC based result of XOR operation
{
*crc = (*crc << 1) ^ 0x07;
}
else
{
*crc = (*crc << 1);
}
currentByte = currentByte >> 1;
} // for CRC bit
} // for message byte
}

int main()
{
  UCHAR stuff[4] = { 0x05, 0x00, 0x00, 0x00 } ;
  std::string name;
  swuart_calcCRC(stuff,4);

  std::cout << (int)stuff[0] << "\n";
  std::cout << (int)stuff[1] << "\n";
  std::cout << (int)stuff[2] << "\n";
  std::cout << (int)stuff[3] << "\n";
}
