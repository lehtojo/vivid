# xmm0: Value
.export _V15decimal_to_bitsd_rx
_V15decimal_to_bitsd_rx:
movq rax, xmm0
ret

# rdi: Value
.export _V15bits_to_decimalx_rd
_V15bits_to_decimalx_rd:
movq xmm0, rdi
ret

# rdi: Source
# rsi: Count
# rdx: Destination
.export _V4copyPhxS_
_V4copyPhxS_:
xor rax, rax # Position

# 16-byte copy loop
cmp rsi, 16
jl _V4copyPhxS_L1
_V4copyPhxS_L0:
# Load and store
movups xmm0, xword [rdi+rax]
movups xword [rdx+rax], xmm0

# Move to the next slots
add rax, 16

# Continue if there are at least 16 bytes left
lea r9, [rax+16]
cmp r9, rsi
jle _V4copyPhxS_L0
_V4copyPhxS_L1:

# Determine the amount of bytes left
sub rsi, rax

# 1-byte copy loop:
jle _V4copyPhxS_L3 # The subtraction above compares rsi to zero
_V4copyPhxS_L2:
mov r9b, byte [rdi+rax]
mov byte [rdx+rax], r9b
add rax, 1
sub rsi, 1
jg _V4copyPhxS_L2 # The subtraction above compares rsi to zero
_V4copyPhxS_L3:
ret

# rdi: Source
# rsi: Count
# rdx: Destination
# rcx: Offset
.export _V11offset_copyPhxS_x
_V11offset_copyPhxS_x:
add rdx, rcx
jmp _V4copyPhxS_

# rdi: Destination
# rsi: Count
.export _V4zeroPhx
_V4zeroPhx:
xor rax, rax # Position
pxor xmm0, xmm0 # Value to copy

# 16-byte copy loop
cmp rsi, 16
jl _V4zeroPhx_L1
_V4zeroPhx_L0:
# Load and store
movups xword [rdi+rax], xmm0

# Move to the next slots
add rax, 16

# Continue if there are at least 16 bytes left
lea r9, [rax+16]
cmp r9, rsi
jle _V4zeroPhx_L0
_V4zeroPhx_L1:

# Determine the amount of bytes left
sub rsi, rax

# 1-byte copy loop:
jle _V4zeroPhx_L3 # The subtraction above compares rsi to zero
_V4zeroPhx_L2:
mov byte [rdi+rax], 0
add rax, 1
sub rsi, 1
jg _V4zeroPhx_L2 # The subtraction above compares rsi to zero
_V4zeroPhx_L3:
ret

# rdi: Destination
# rsi: Count
# rdx: Value
.export _V4fillPhxx
_V4fillPhxx:
xor rax, rax # Position

# Fill rdx with its first 8 bits
movzx r10, dl
sal rdx, 8
or rdx, r10
sal rdx, 8
or rdx, r10
sal rdx, 8
or rdx, r10
sal rdx, 8
or rdx, r10
sal rdx, 8
or rdx, r10
sal rdx, 8
or rdx, r10
sal rdx, 8
or rdx, r10

# 8-byte copy loop
cmp rsi, 8
jl _V4fillPhxx_L1
_V4fillPhxx_L0:
# Load and store
mov qword [rdi+rax], rdx

# Move to the next slots
add rax, 8

# Continue if there are at least 8 bytes left
lea r9, [rax+8]
cmp r9, rsi
jle _V4fillPhxx_L0
_V4fillPhxx_L1:

# Determine the amount of bytes left
sub rsi, rax

# 1-byte copy loop:
jle _V4fillPhxx_L3 # The subtraction above compares rsi to zero
_V4fillPhxx_L2:
mov byte [rdi+rax], dl
add rax, 1
sub rsi, 1
jg _V4fillPhxx_L2 # The subtraction above compares rsi to zero
_V4fillPhxx_L3:
ret

# rdi: Bytes
.export _V14allocate_stackx_rPh
_V14allocate_stackx_rPh:
pop rdx
sub rsp, rdi
mov rax, rsp
jmp rdx

# rdi: Bytes
.export _V16deallocate_stackx
_V16deallocate_stackx:
pop rax
add rsp, rdi
jmp rax