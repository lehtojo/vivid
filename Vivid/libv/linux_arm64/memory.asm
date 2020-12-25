// x0: Length
.global _V17internal_allocatex_rPh
_V17internal_allocatex_rPh:

// x5: offset
// x4: fd
// x3: flags
// x2: prot
// x1: length
// x0: addr

mov x1, x0 // Length
mov x0, xzr // Address
mov x2, #0x03 // PERMISSION_READ | PERMISSION_WRITE
mov x3, #0x22 // HARDWARE_MEMORY | VISIBILITY_PRIVATE
mov x4, #-1 // FD
mov x5, xzr

mov x8, #222 // mmap
svc #0
ret


// x0: Address
// x1: Length
.global _V10deallocatePhx
_V10deallocatePhx:
mov x8, #215 // munmap
svc #0
ret

// x0: Source
// x1: Count
// x2: Destination
.global _V4copyPhxS_
_V4copyPhxS_:
cmp x1, #1
b.lt _V4copyPhxS__L0
_V4copyPhxS__L1:
ldrb w3, [x0], #1
subs x1, x1, #1
strb w3, [x2], #1
b.ne _V4copyPhxS__L1
_V4copyPhxS__L0:
ret

// x0: Source
// x1: Count
// x2: Destination
// x3: Offset
.global _V11offset_copyPhxS_x
_V11offset_copyPhxS_x:
add x2, x2, x3
cmp x1, #1
b.lt _V11offset_copyPhxS_x_L0
_V11offset_copyPhxS_x_L1:
ldrb w3, [x0], #1
subs x1, x1, #1
strb w3, [x2], #1
b.ne _V11offset_copyPhxS_x_L1
_V11offset_copyPhxS_x_L0:
ret

// x0: Destination
// x1: Count
.global _V4zeroPhx
_V4zeroPhx:
cmp x1, #1
b.lt _V4zeroPhx_L0
_V4zeroPhx_L1:
subs x1, x1, #1
strb wzr, [x0], #1
b.ne _V4zeroPhx_L1
_V4zeroPhx_L0:
ret

// x0: Destination
// x1: Count
// x2: Value
.global _V4fillPhxx
_V4fillPhxx:
cmp x1, #1
b.lt _V4fillPhxx_L0
_V4fillPhxx_L1:
subs x1, x1, #1
strb w2, [x0], #1
b.ne _V4fillPhxx_L1
_V4fillPhxx_L0:
ret

// x0: code
.global _V4exitx
_V4exitx:
mov x8, #60
svc #0
b _V4exitx