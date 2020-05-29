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
push rdi
push rbp
sub rsp, 40
mov rcx, 8
call allocate
mov rcx, rax
call type_factory_apple_function_create
mov rbx, rax
call type_largefactory_apple_orange_constructor
mov rsi, rax
mov rcx, rsi
call type_largefactory_apple_orange_function_create_x
mov rdi, rax
mov rcx, rsi
call type_largefactory_apple_orange_function_create_y
mov rsi, rax
mov qword [rdi+8], rsi
mov rcx, [rdi+8]
mov qword [rcx+8], rbx
mov rcx, [rdi+8]
mov rcx, [rcx+8]
mov rdx, [rdi+8]
mov qword [rcx+8], rdx
mov rcx, rbx
call function_to_orange
mov qword [rax], -100
mov rcx, rbx
call type_apple_function_get_weight
mov rbp, rax
mov rcx, rdi
call type_apple_function_get_weight
add rbp, rax
mov rcx, rsi
call type_orange_function_get_sugar_percent
add rbp, rax
mov rax, rbp
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

function_to_orange:
mov rax, rcx
ret

type_apple_constructor:
sub rsp, 40
mov rcx, 16
call allocate
mov qword [rax], 100
add rsp, 40
ret

type_apple_function_get_weight:
mov rdx, [rcx]
mov rax, rdx
ret

type_orange_constructor:
sub rsp, 40
mov rcx, 16
call allocate
mov qword [rax], 70
add rsp, 40
ret

type_orange_function_get_sugar_percent:
mov rdx, [rcx]
mov rax, rdx
ret

type_factory_apple_function_create:
sub rsp, 40
call type_apple_constructor
add rsp, 40
ret

type_largefactory_apple_orange_constructor:
sub rsp, 40
mov rcx, 8
call allocate
mov qword [rax], 100
add rsp, 40
ret

type_largefactory_apple_orange_function_create_x:
sub rsp, 40
call type_apple_constructor
add rsp, 40
ret

type_largefactory_apple_orange_function_create_y:
sub rsp, 40
call type_orange_constructor
add rsp, 40
ret

section .data