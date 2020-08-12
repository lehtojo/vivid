section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

function_run_lambda_0:
add rdx, [rcx]
sub rdx, 1
mov rax, rdx
ret

function_run:
push rbx
sub rsp, 48
mov rcx, 16
call allocate
lea rcx, [rel function_run_lambda_0]
mov qword [rax], rcx
mov qword [rax+8], 3
mov rcx, rax
mov rdx, 10
mov rbx, rax
call qword [rbx]
mov rcx, 12
call type_worker_constructor
add rsp, 48
pop rbx
ret

type_worker_constructor_lambda_0:
mov r9, [rcx]
mov r8, [r9+8]
cmp r8, rdx
jne type_worker_constructor_lambda_0_L1
mov r8, [rcx+8]
cmp r8, 10
jne type_worker_constructor_lambda_0_L1
mov rax, 1
ret
jmp type_worker_constructor_lambda_0_L0
type_worker_constructor_lambda_0_L1:
xor rax, rax
ret
type_worker_constructor_lambda_0_L0:
ret

type_worker_constructor:
push rbx
push rsi
sub rsp, 40
mov rdx, rcx
mov rcx, 16
mov rbx, rdx
call allocate
mov qword [rax+8], rbx
sal rbx, 1
mov rcx, 24
mov rsi, rax
call allocate
lea rcx, [rel type_worker_constructor_lambda_0]
mov qword [rax], rcx
mov qword [rax+8], rsi
mov qword [rax+16], rbx
mov rcx, rax
mov rdx, rbx
mov rbx, rax
call qword [rbx]
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

section .data