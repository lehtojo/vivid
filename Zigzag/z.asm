section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

function_power_of_two:
imul rcx, rcx
mov rax, rcx
ret

function_run:
push rbx
push rsi
push rdi
sub rsp, 48
call type_apples_constructor
mov rbx, rax
mov rcx, 10
call type_apples_constructor
mov rsi, rax
mov rcx, rbx
mov rdx, 7
call type_apples_function_assign_plus
mov rcx, 2
call function_power_of_two
mov rdi, rax
mov rcx, 2
call function_power_of_two
imul rdi, rax
mov rcx, rsi
mov rdx, rdi
call type_apples_function_assign_minus
mov rcx, rbx
mov rdx, rsi
call type_apples_function_plus
mov rcx, rax
mov rdx, 10
call type_apples_function_assign_times
mov rcx, 10
call type_array_large_constructor
mov rdi, rax
mov rcx, rdi
mov rdx, 1
mov r8, [rbx]
call type_array_large_function_set
mov rbx, 2
mov rcx, rdi
mov rdx, 1
call type_array_large_function_get
imul rax, [rsi]
mov rcx, rdi
mov rdx, rbx
mov r8, rax
call type_array_large_function_set
xor rax, rax
add rsp, 48
pop rdi
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

type_array_large_constructor:
push rbx
push rsi
sub rsp, 40
mov rdx, rcx
mov rcx, 16
mov rbx, rdx
mov rsi, rbx
call allocate
mov rbx, rax
mov rcx, rsi
sal rcx, 3
call allocate
mov qword [rbx], rax
mov qword [rbx+8], rsi
mov rax, rbx
add rsp, 40
pop rsi
pop rbx
ret

type_array_large_function_set:
mov rax, [rcx]
mov qword [rax+rdx*8], r8
ret

type_array_large_function_set:
mov rax, [rcx]
mov rdx, [rsp+16]
mov rcx, [rsp+24]
mov qword [rax+rdx*8], rcx
ret

type_array_large_function_get:
mov r8, [rcx]
mov rax, [r8+rdx*8]
ret

section .data