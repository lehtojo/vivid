section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

function_f:
mov rax, rcx
ret

function_g:
push rbx
sub rsp, 48
mov rdx, rcx
imul rdx, -2
mov r8, 15
add r8, rdx
mov rdx, rcx
imul rdx, -1
mov r9, 1
add r9, rdx
sub r8, r9
mov rdx, rcx
mov rcx, r8
mov rbx, rdx
call function_f
mov rax, 1
add rax, rbx
mov rcx, rbx
imul rcx, -1
mov rdx, 1
add rdx, rcx
add rax, rdx
imul rbx, -2
mov rcx, 15
add rcx, rbx
add rax, rcx
add rsp, 48
pop rbx
ret

function_run:
sub rsp, 40
mov rcx, 10
call function_g
add rsp, 40
ret

section .data