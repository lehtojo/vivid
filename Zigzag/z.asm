section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

function_run:
push rbx
push rsi
sub rsp, 40
call type_apples_constructor
mov rbx, rax
mov rcx, 10
call type_apples_constructor
mov rsi, rax
mov rcx, rbx
mov rdx, 7
call type_apples_function_assign_plus
mov rcx, rsi
mov rdx, 2
call type_apples_function_assign_minus
mov rcx, rbx
mov rdx, rsi
call type_apples_function_plus
mov rcx, rax
mov rdx, 10
call type_apples_function_assign_times
xor rax, rax
add rsp, 40
pop rsi
pop rbx
ret

type_apples_constructor:
sub rsp, 40
mov rcx, 8
call allocate
mov qword [rax], 1
add rsp, 40
ret

type_apples_constructor:
push rbx
sub rsp, 48
mov rdx, rcx
mov rcx, 8
mov rbx, rdx
call allocate
mov qword [rax], rbx
add rsp, 48
pop rbx
ret

type_apples_function_plus:
sub rsp, 40
mov rdx, [rcx]
add rdx, [rdx]
mov rcx, rdx
call type_apples_constructor
add rsp, 40
ret

type_apples_function_assign_plus:
add qword [rcx], rdx
ret

type_apples_function_assign_minus:
sub qword [rcx], rdx
ret

type_apples_function_assign_times:
mov rax, [rcx]
imul rax, rdx
mov qword [rcx], rax
ret

section .data