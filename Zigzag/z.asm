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
mov rdx, 14
sub rdx, rcx
mov r8, rcx
mov rcx, rdx
mov rbx, r8
call function_f
sal rbx, 1
mov rax, 17
sub rax, rbx
add rsp, 48
pop rbx
ret

function_z:
push rbx
sub rsp, 48
mov rbx, 1
add rbx, rcx
mov rcx, rbx
call function_g
mov rcx, rbx
call function_g
mov rcx, rbx
call function_g
mov rcx, rbx
call function_g
mov rcx, rbx
call function_g
add rsp, 48
pop rbx
ret

function_h:
add rcx, rdx
imul rcx, r8
mov rax, rcx
ret

function_j:
mov rax, rcx
imul rax, rdx
imul rcx, rdx
sub rax, rcx
ret

function_k:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
not rbx
mov rdx, rcx
mov rsi, rcx
call function_j
add rbx, rax
not rsi
add rbx, rsi
mov rax, rbx
add rsp, 40
pop rsi
pop rbx
ret

function_run:
sub rsp, 40
mov rcx, 10
call function_g
mov rcx, 10
call function_z
mov rcx, 1
mov rdx, 1
mov r8, 1
call function_h
mov rcx, 1
mov rdx, 1
call function_j
mov rcx, 1
call function_k
add rsp, 40
ret

section .data