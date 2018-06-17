\ crc-8.fs
\
: ACCUMULATE ( oldcrc char -- newcrc)
256 * \ shift char to hi-order byte
XOR \ & xor into previous crc
8 0 DO \ Then for eight repetitions,
DUP 0< IF \ if hi-order bit is "1"
16386 XOR \ xor it with mask and
DUP + \ shift it left one place
1+ \ set lo-order bit to "1"
ELSE \ otherwise, i.e. hi-order bit is "0"
DUP + \ shift it left one place
THEN
LOOP ; \ complete the loop
