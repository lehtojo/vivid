# rdi: Text
# rsi: Length
.export _V14internal_printPhx:
mov rdx, rsi
mov rsi, rdi
mov rdi, 1
mov rax, 1 # System call: sys_write
syscall
ret

# rdi: Buffer
# rsi: Length
.export _V13internal_readPhx_rx:
mov rdx, rsi
mov rsi, rdi
xor rdi, rdi
xor rax, rax # System call: sys_read
syscall
add rax, 1
ret
