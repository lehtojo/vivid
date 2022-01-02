# xmm0: Value
.export _V15decimal_to_bitsd_rx
_V15decimal_to_bitsd_rx:
movq rax, xmm0
ret

# rcx: Value
.export _V15bits_to_decimalx_rd
_V15bits_to_decimalx_rd:
movq xmm0, rcx
ret

# rcx: Source
# rdx: Count
# r8: Destination
.export _V4copyPhxS_
_V4copyPhxS_:
xor rax, rax # Position

# 16-byte copy loop
cmp rdx, 16
jl _V4copyPhxS_L1
_V4copyPhxS_L0:
# Load and store
movups xmm0, xword [rcx+rax]
movups xword [r8+rax], xmm0

# Move to the next slots
add rax, 16

# Continue if there are at least 16 bytes left
lea r9, [rax+16]
cmp r9, rdx
jle _V4copyPhxS_L0
_V4copyPhxS_L1:

# Determine the amount of bytes left
sub rdx, rax

# 1-byte copy loop:
jle _V4copyPhxS_L3 # The subtraction above compares rdx to zero
_V4copyPhxS_L2:
mov r9b, byte [rcx+rax]
mov byte [r8+rax], r9b
add rax, 1
sub rdx, 1
jg _V4copyPhxS_L2 # The subtraction above compares rdx to zero
_V4copyPhxS_L3:
ret

# rcx: Source
# rdx: Count
# r8: Destination
# r9: Offset
.export _V11offset_copyPhxS_x
_V11offset_copyPhxS_x:
add r8, r9
jmp _V4copyPhxS_

# rcx: Destination
# rdx: Count
.export _V4zeroPhx
_V4zeroPhx:
xor rax, rax # Position
pxor xmm0, xmm0 # Value to copy

# 16-byte copy loop
cmp rdx, 16
jl _V4zeroPhx_L1
_V4zeroPhx_L0:
# Load and store
movups xword [rcx+rax], xmm0

# Move to the next slots
add rax, 16

# Continue if there are at least 16 bytes left
lea r9, [rax+16]
cmp r9, rdx
jle _V4zeroPhx_L0
_V4zeroPhx_L1:

# Determine the amount of bytes left
sub rdx, rax

# 1-byte copy loop:
jle _V4zeroPhx_L3 # The subtraction above compares rdx to zero
_V4zeroPhx_L2:
mov byte [rcx+rax], 0
add rax, 1
sub rdx, 1
jg _V4zeroPhx_L2 # The subtraction above compares rdx to zero
_V4zeroPhx_L3:
ret

# rcx: Destination
# rdx: Count
# r8: Value
.export _V4fillPhxx
_V4fillPhxx:
xor rax, rax # Position

# Fill r8 with its first 8 bits
movzx r10, r8b
sal r8, 8
or r8, r10
sal r8, 8
or r8, r10
sal r8, 8
or r8, r10
sal r8, 8
or r8, r10
sal r8, 8
or r8, r10
sal r8, 8
or r8, r10
sal r8, 8
or r8, r10

# 8-byte copy loop
cmp rdx, 8
jl _V4fillPhxx_L1
_V4fillPhxx_L0:
# Load and store
mov qword [rcx+rax], r8

# Move to the next slots
add rax, 8

# Continue if there are at least 8 bytes left
lea r9, [rax+8]
cmp r9, rdx
jle _V4fillPhxx_L0
_V4fillPhxx_L1:

# Determine the amount of bytes left
sub rdx, rax

# 1-byte copy loop:
jle _V4fillPhxx_L3 # The subtraction above compares rdx to zero
_V4fillPhxx_L2:
mov byte [rcx+rax], r8b
add rax, 1
sub rdx, 1
jg _V4fillPhxx_L2 # The subtraction above compares rdx to zero
_V4fillPhxx_L3:
ret

# rcx: Bytes
.export _V14allocate_stackx_rPh
_V14allocate_stackx_rPh:
pop rdx
sub rsp, rcx
mov rax, rsp
jmp rdx

# rcx: Bytes
.export _V16deallocate_stackx
_V16deallocate_stackx:
pop rax
add rsp, rcx
jmp rax
