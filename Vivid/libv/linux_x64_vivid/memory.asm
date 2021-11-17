# xmm0: Value
.export _V15decimal_to_bitsd_rx
movq rax, xmm0
ret

# rdi: Value
.export _V15bits_to_decimalx_rd
movq xmm0, rdi
ret

# rcx: Nanoseconds
.export _V5sleepx

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
.export _V4exitx
mov rax, 60
syscall
jmp _V4exitx

# rdi: Length
.export _V17internal_allocatex_rPh

mov rsi, rdi # Length
xor rdi, rdi # Address
mov rdx, 3 # 0x03 = PERMISSION_READ | PERMISSION_WRITE
mov r10, 34 # 0x22 = HARDWARE_MEMORY | VISIBILITY_PRIVATE
mov r8, -1
xor r9, r9

mov rax, 9 # System call: sys_mmap
syscall

ret

# rdi: Address
# rsi: Length
.export _V19internal_deallocatePhx

mov rax, 11 # System call: sys_munmap
syscall

ret

# rdi: Source
# rsi: Count
# rdx: Destination
.export _V14seperated_copyPhxS_
cmp rsi, 16
jl _V14seperated_copyPhxS_L1

xor rax, rax
lea rcx, [rsi-16]
_V14seperated_copyPhxS_L0: # 16-byte copy
movups xmm0, xword [rdi+rax]
movups xword [rdx+rax], xmm0
add rax, 16
cmp rax, rcx
jle _V14seperated_copyPhxS_L0

# Apply the copied range to the pointers and to the amount of bytes to copy
add rdi, rax
add rdx, rax
sub rsi, rax

_V14seperated_copyPhxS_L1:

cmp rsi, 4 # Here the amount of bytes to copy must be less than 16
jl _V14seperated_copyPhxS_L3

xor rax, rax
lea rcx, [rsi-4]
_V14seperated_copyPhxS_L2: # 4-byte copy
mov r8d, dword [rdi+rax]
mov dword [rdx+rax], r8d
add rax, 4
cmp rax, rcx
jle _V14seperated_copyPhxS_L2

# Apply the copied range to the pointers and to the amount of bytes to copy
add rdi, rax
add rdx, rax
sub rsi, rax

_V14seperated_copyPhxS_L3:
# Here the amount of bytes to copy must be 3, 2, 1 or 0

test rsi, rsi
jz _V14seperated_copyPhxS_L4 # Return now?

mov cl, byte [rdi+rax]
mov byte [rdx+rax], cl

sub rsi, 1
jz _V14seperated_copyPhxS_L4 # Return now?

mov cl, byte [rdi+rax+1]
mov byte [rdx+rax+1], cl

sub rsi, 1
jz _V14seperated_copyPhxS_L4 # Return now?

mov cl, byte [rdi+rax+2]
mov byte [rdx+rax+2], cl

_V14seperated_copyPhxS_L4:
ret

# rdi: Source
# rsi: Count
# rdx: Destination
.export _V4copyPhxS_

# Ensure there is something to copy
test rsi, rsi
jle _V4copyPhxS_L1

# d = |destination - source|
mov rax, rdx
sub rax, rdi

mov rcx, rax
neg rcx

cmovl rcx, rax

cmp rcx, rsi # Do a seperated copy if the distance between the source and destination is greater or equal to the amount of bytes to copy
jge _V14seperated_copyPhxS_

# Since the source and destination overlap, do a byte by byte copy
xor rax, rax
_V4copyPhxS_L0:
mov cl, byte [rdi+rax]
mov byte [rdx+rax], cl
add rax, 1
cmp rax, rsi
jl _V4copyPhxS_L0

_V4copyPhxS_L1:
ret

# rdi: Source
# rsi: Count
# rdx: Destination
# rcx: Offset
.export _V11offset_copyPhxS_x
add rdx, rcx # Apply the specified offset
jmp _V4copyPhxS_

# rdi: Destination
# rsi: Count
.export _V4zeroPhx
xor rax, rax
idiv rax
ret

# rdi: Destination
# rsi: Count
# rdx: Value
.export _V4fillPhxx
xor rax, rax
idiv rax
ret

# rcx: Bytes
.export _V14allocate_stackx_rPh
pop rdx
sub rsp, rdi
mov rax, rsp
jmp rdx

# rcx: Bytes
.export _V16deallocate_stackx
pop rax
add rsp, rdi
jmp rax

.export _V17get_stack_pointerv_rPh
lea rax, [rsp+8]
ret

.export system_read
mov rax, 0 # System call: sys_read
syscall
ret

.export system_write
mov rax, 1 # System call: sys_write
syscall
ret

.export system_open
mov rax, 2 # System call: sys_open
syscall
ret

.export system_close
mov rax, 3 # System call: sys_close
syscall
ret

.export system_status
mov rax, 4 # System call: sys_stat
syscall
ret

.export system_get_directory_entries
mov rax, 217 # System call: sys_getdents
syscall
ret

.export system_fork
mov rax, 57 # System call: sys_fork
syscall
ret

.export system_change_folder
mov rax, 80 # System call: sys_chdir
syscall
ret

.export system_execute
mov rax, 59 # System call: sys_execve
syscall
ret

.export system_remove_folder
mov rax, 84 # System call: sys_rmdir
syscall
ret

.export system_unlink
mov rax, 87 # System call: sys_unlink
syscall
ret

.export system_wait_id
mov rax, 247 # System call: sys_waitid
mov r10, rcx
syscall
ret

.export system_get_working_folder
mov rax, 79 # System call: sys_getcwd
syscall
ret
