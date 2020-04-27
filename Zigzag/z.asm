section .text
global _start
_start:
call function_run
mov rax, 60
xor rdi, rdi
syscall

extern function_allocate
extern function_integer_power
extern function_sys_print
extern function_sys_read
extern function_copy
extern function_offset_copy
extern function_free

function_test:
push rbx
push rsi
push 10
call function_allocate
mov rsi, rax
mov dword [rsi+28], 7
mov rbx, [rsp+32] ; b
mov rax, [rsp+24] ; a
add rax, rbx
push rax
call function_allocate
mov ecx, [rax+4]
mov dword [rax+rbx*4], ecx
pop rsi
pop rbx
ret

function_run:
push 2
push 1
call function_test
add rsp, 16
ret

section .data