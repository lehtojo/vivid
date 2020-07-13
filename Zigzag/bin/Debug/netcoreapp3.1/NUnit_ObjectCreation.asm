section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_create_apple
export function_create_apple
function_create_apple:
sub rsp, 40
call type_apple_constructor
add rsp, 40
ret

global function_create_car
export function_create_car
function_create_car:
sub rsp, 40
call type_car_constructor
add rsp, 40
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
call function_create_apple
movsd xmm0, qword [rel function_run_C0]
call function_create_car
ret

type_apple_constructor:
sub rsp, 40
mov rcx, 16
call allocate
mov qword [rax], 100
movsd xmm0, qword [rel type_apple_constructor_C0]
movsd qword [rax+8], xmm0
add rsp, 40
ret

type_car_constructor:
push rbx
sub rsp, 48
mov rcx, 24
movsd qword [rsp+64], xmm0
call allocate
mov qword [rax+8], 2000000
lea rcx, [rel type_car_constructor_S0]
mov rbx, rax
call type_string_constructor
mov qword [rbx+16], rax
movsd xmm0, qword [rsp+64]
movsd qword [rbx], xmm0
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

type_car_constructor_S0 db 'Flash', 0
function_run_C0 dq 0.0
type_apple_constructor_C0 dq 0.1