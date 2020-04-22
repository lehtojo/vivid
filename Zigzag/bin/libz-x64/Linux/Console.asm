section .text

global function_sys_print
function_sys_print:
push rdi
push rsi
mov rax, 0x01 ; System call: sys_write
mov rdi, 1 ; Output mode
mov rsi, [rsp+24] ; Parameter: Text
mov rdx, [rsp+32] ; Parameter: Length
syscall
pop rsi
pop rdi
ret 16

global function_sys_read
function_sys_read:
push rdi
push rsi
mov rax, 0x03 ; System call: sys_write
mov rdi, 0 ; Input mode
mov rsi, [rsp+24] ; Parameter: Buffer
mov rdx, [rsp+32] ; Parameter: Length
syscall
pop rsi
pop rdi
ret 16