section .text

global sys_print
sys_print:
push rdi
push rsi
push rbx
mov rax, 0x01 ; System call: sys_write
mov rdi, 1 ; Output mode
mov rsi, [rsp+32] ; Parameter: Text
mov rdx, [rsp+40] ; Parameter: Length
mov rbx, rsp
syscall
mov rsp, rbx
pop rbx
pop rsi
pop rdi
ret 16

global sys_read
sys_read:
push rdi
push rsi
push rbx
mov rax, 0 ; System call: sys_read
mov rdi, 0 ; Input mode
mov rsi, [rsp+32] ; Parameter: Buffer
mov rdx, [rsp+40] ; Parameter: Length
mov rbx, rsp
syscall
mov rsp, rbx
pop rbx
pop rsi
pop rdi
ret 16