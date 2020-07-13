section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

function_change_capacity:
sub rsp, 40
mov qword [rcx+24], 10
lea rcx, [rcx+16]
call type_counter_function_use
add rsp, 40
ret

function_run:
push rbx
sub rsp, 48
call type_contenttoken_constructor
mov qword [rax], 0
lea rcx, [rel function_run_S0]
mov rbx, rax
call type_string_constructor
mov qword [rbx+8], rax
mov qword [rbx+24], 1
lea rcx, [rbx+16]
call type_counter_function_use
mov rcx, rbx
call type_token_function_get_type
mov rcx, rbx
call function_change_capacity
add rsp, 48
pop rbx
ret

type_token_function_get_type:
mov rax, [rcx]
ret

type_counter_function_use:
add qword [rcx], 1
ret

type_contenttoken_constructor:
push rbx
sub rsp, 48
mov rcx, 32
call allocate
mov qword [rax+24], 0
mov qword [rax], 1
lea rcx, [rel type_contenttoken_constructor_S0]
mov rbx, rax
call type_string_constructor
mov qword [rbx+8], rax
mov qword [rbx], 0
mov qword [rbx+24], 0
mov rax, rbx
add rsp, 48
pop rbx
ret

type_string_constructor:
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

section .data

function_run_S0 db 'Content Token', 0
type_contenttoken_constructor_S0 db 'Content', 0