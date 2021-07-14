#analyze
.intel_syntax noprefix
.extern VirtualAlloc
.extern VirtualFree
.extern Sleep
.extern ExitProcess

# xmm0: Value
.global _V15decimal_to_bitsd_rx
_V15decimal_to_bitsd_rx:
movq rax, xmm0
ret

# rcx: Value
.global _V15bits_to_decimalx_rd
_V15bits_to_decimalx_rd:
movq xmm0, rcx
ret

# rcx: Milliseconds
.global _V5sleepx
_V5sleepx:
jmp Sleep

# rcx: Code
.global _V4exitx
_V4exitx:
jmp ExitProcess

# rcx: Bytes
.global _V17internal_allocatex_rPh
_V17internal_allocatex_rPh:

mov rdx, rcx # Amount of bytes to allocate
xor rcx, rcx
mov r8, 0x00001000 | 0x00002000 # MEM_COMMIT | MEM_RESERVE
mov r9, 0x04 # PAGE_READWRITE

sub rsp, 40
call VirtualAlloc
add rsp, 40

ret

# rcx: Address
# rdx: Bytes
.global _V19internal_deallocatePhx
_V19internal_deallocatePhx:

# rcx: Address of the region to be released
# rdx: Size of the region
mov r8, 0x00008000 # MEM_RELEASE

sub rsp, 40
call VirtualFree
add rsp, 40

ret

# rcx: Source
# rdx: Count
# r8: Destination
.global _V4copyPhxS_
_V4copyPhxS_:
push rdi
push rsi

mov rdi, r8 # Destination
mov rsi, rcx # Source
mov rcx, rdx # Count

rep movsb

pop rsi
pop rdi
ret

# rcx: Source
# rdx: Count
# r8: Destination
# r9: Offset
.global _V11offset_copyPhxS_x
_V11offset_copyPhxS_x:
push rdi
push rsi

mov rdi, r8 # Destination
add rdi, r9 # Apply offset
mov rsi, rcx # Source
mov rcx, rdx # Count

rep movsb

pop rsi
pop rdi
ret

# rcx: Destination
# rdx: Count
.global _V4zeroPhx
_V4zeroPhx:
push rdi

mov rdi, rcx # rdi: Destination
mov rcx, rdx # rcx: Count
xor rax, rax # Value used to fill the range

rep stosb

pop rdi
ret

# rcx: Destination
# rdx: Count
# r8: Value
.global _V4fillPhxx
_V4fillPhxx:
push rdi

mov rdi, rcx # rdi: Destination
mov rcx, rdx # rcx: Count
mov rax, r8 # rax: Value

rep stosb

pop rdi
ret

# rcx: Bytes
.global _V14allocate_stackx_rPh
_V14allocate_stackx_rPh:
pop rdx
sub rsp, rcx
mov rax, rsp
jmp rdx

# rcx: Bytes
.global _V16deallocate_stackx
_V16deallocate_stackx:
pop rax
add rsp, rcx
jmp rax
