#analyze
.intel_syntax noprefix
# rcx: Nanoseconds
.global _V5sleepx
_V5sleepx:

# Nanoseconds / 1 000 000 000 = Seconds
mov rax, rdi
mov rcx, 1237940039285380275
imul rcx
mov rax, rdx
shr rax, 63
sar rdx, 26
add rax, rdx

# Create an instance of the following type into stack:
# Time {
#   seconds: large
#   nanoseconds: large
# }

push rax # Set the value of the member Time.seconds

# Remaining nanoseconds
# = Total nanoseconds - Seconds * 1 000 000 000
imul rax, 1000000000
sub rdi, rax

push rdi # Set the value of the member Time.nanoseconds

mov rdi, rsp
xor rsi, rsi

mov rax, 35 # System call: sys_nanosleep
syscall

add rsp, 16
ret

# rdi: Code
.global _V4exitx
_V4exitx:
mov rax, 60
syscall
jmp _V4exitx

# rdi: Length
.global _V17internal_allocatex_rPh
_V17internal_allocatex_rPh:

mov rsi, rdi # Length
xor rdi, rdi # Address
mov rdx, 0x03 # PERMISSION_READ | PERMISSION_WRITE
mov r10, 0x22 # HARDWARE_MEMORY | VISIBILITY_PRIVATE
mov r8, -1
xor r9, r9

mov rax, 9 # System call: sys_mmap
syscall

ret

# rdi: Address
# rsi: Length
.global _V19internal_deallocatePhx
_V19internal_deallocatePhx:

mov rax, 11 # System call: sys_munmap
syscall

ret

# rdi: Source
# rsi: Count
# rdx: Destination
.global _V4copyPhxS_
_V4copyPhxS_:

xchg rdi, rdx # rdi: Destination, rdx: Source
mov rcx, rsi # rcx: Count
mov rsi, rdx # rsi: Source

rep movsb

ret

# rdi: Source
# rsi: Count
# rdx: Destination
# rcx: Offset
.global _V11offset_copyPhxS_x
_V11offset_copyPhxS_x:

add rdx, rcx # Apply offset

xchg rdi, rdx # rdi: Destination, rdx: Source
mov rcx, rsi # rcx: Count
mov rsi, rdx # rsi: Source

rep movsb

ret

# rdi: Destination
# rsi: Count
.global _V4zeroPhx
_V4zeroPhx:

mov rcx, rsi # rcx: Count
xor rax, rax # Value used to fill the range

rep stosb

ret

# rdi: Destination
# rsi: Count
# rdx: Value
.global _V4fillPhxx
_V4fillPhxx:

mov rcx, rsi # rcx: Count
mov rax, rdx # rax: Value

rep stosb

ret

# rcx: Bytes
.global _V14allocate_stackx_rPh
_V14allocate_stackx_rPh:
pop rdx
sub rsp, rdi
mov rax, rsp
jmp rdx

# rcx: Bytes
.global _V16deallocate_stackx
_V16deallocate_stackx:
pop rax
add rsp, rdi
jmp rax