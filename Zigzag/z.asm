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
call type_list_item_constructor
mov rcx, 10
mov rdx, 1
mov rbx, rax
call type_item_constructor
mov rcx, rbx
mov rdx, rax
mov rsi, rax
call type_list_item_function_add
mov rcx, rbx
mov rdx, rsi
call type_list_item_function_assign_plus
mov rcx, rbx
xor rdx, rdx
call type_list_item_function_get
mov rcx, rax
mov rdx, 2
call type_item_function_add
mov rcx, rbx
mov rdx, 1
call type_list_item_function_get
mov rcx, rax
mov rdx, 8
call type_item_function_assign_plus
add rsp, 40
pop rsi
pop rbx
ret

type_item_constructor:
push rbx
push rsi
sub rsp, 40
mov r8, rcx
mov rcx, 16
mov rbx, rdx
mov rsi, r8
call allocate
mov qword [rax], rsi
mov qword [rax+8], rbx
add rsp, 40
pop rsi
pop rbx
ret

type_item_function_add:
add qword [rcx], rdx
add qword [rcx+8], rdx
ret

type_item_function_assign_plus:
sub rsp, 40
call type_item_function_add
add rsp, 40
ret

type_list_item_constructor:
push rbx
sub rsp, 48
mov rcx, 24
call allocate
mov rcx, 8
mov rbx, rax
call allocate
mov qword [rbx], rax
mov qword [rbx+8], 1
mov qword [rbx+16], 0
mov rax, rbx
add rsp, 48
pop rbx
ret

type_list_item_function_grow:
push rbx
push rsi
sub rsp, 40
mov rax, [rcx+8]
sal rax, 4
mov rdx, rcx
mov rcx, rax
mov rbx, rdx
call allocate
mov rcx, [rbx]
mov rdx, [rbx+8]
mov r8, rax
mov rsi, rax
call copy
mov rcx, [rbx]
mov rdx, [rbx+8]
call deallocate
mov qword [rbx], rsi
mov rax, [rbx+8]
sal rax, 1
mov qword [rbx+8], rax
add rsp, 40
pop rsi
pop rbx
ret

type_list_item_function_add:
push rbx
push rsi
sub rsp, 40
mov rax, [rcx+16]
cmp rax, [rcx+8]
jne type_list_item_function_add_L0
mov rbx, rcx
mov rsi, rdx
call type_list_item_function_grow
mov rdx, rsi
mov rcx, rbx
type_list_item_function_add_L0:
mov rax, [rcx+16]
sal rax, 3
mov r8, [rcx]
mov byte [r8+rax], dl
add qword [rcx+16], 1
add rsp, 40
pop rsi
pop rbx
ret

type_list_item_function_get:
sal rdx, 3
mov r8, [rcx]
mov rax, [r8+rdx]
ret

type_list_item_function_assign_plus:
sub rsp, 40
call type_list_item_function_add
add rsp, 40
ret

section .data