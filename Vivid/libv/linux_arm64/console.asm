//analyze
// x0: Text
// x1: Length
.global _V14internal_printPhx
_V14internal_printPhx:
mov x2, x1
mov x1, x0
mov x0, #1
mov x8, #64 // sys_write
svc #0
ret

// x0: Buffer
// x1: Length
.global _V13internal_readPhx_rx
_V13internal_readPhx_rx:
mov x2, x1
mov x1, x0
mov x0, xzr
mov x8, #63 // sys_read
svc #0
ret
