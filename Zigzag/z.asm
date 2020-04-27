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

function_run:
push rbx
push rsi
push rdi
xor rbx, rbx
cmp rbx, 10
jge function_run_L1
function_run_L0:
push function_run_S0
call function_println
add rsp, 8
add rbx, 1
cmp rbx, 10
jl function_run_L0
function_run_L1:
push function_run_S1
call type_string_constructor
add rsp, 8
xor rbx, rbx
mov rsi, rax
cmp rbx, 10
jge function_run_L3
function_run_L2:
push rsi
mov rdi, rsi
call function_printsln
add rsp, 8
add rbx, 1
mov rsi, rdi
cmp rbx, 10
jl function_run_L2
function_run_L3:
pop rdi
pop rsi
pop rbx
ret

function_prints:
push rbx
mov rbx, [rsp+16]
push rbx
call type_string_function_length
add rsp, 8
push rax
push rbx
call type_string_function_data
add rsp, 8
push rax
call function_sys_print
pop rbx
ret

function_printsln:
push rbx
mov rbx, [rsp+16]
push rbx
call type_string_function_length
add rsp, 8
add rax, 1
push rax
push 10
push rbx
call type_string_function_append
add rsp, 16
push rax
call type_string_function_data
add rsp, 8
push rax
call function_sys_print
pop rbx
ret

function_println:
push qword [rsp+8]
call type_string_constructor
add rsp, 8
push 10
push rax
call type_string_function_append
add rsp, 16
push rax
call function_prints
add rsp, 8
ret

type_string_constructor:
push 8
call function_allocate
mov rcx, [rsp+8]
mov qword [rax], rcx
ret

type_string_function_append:
push rsi
push rdi
push rbp
mov rsi, [rsp+32]
push rsi
call type_string_function_length
mov rdi, rax
add rsp, 8
lea rcx, [rdi+2]
push rcx
call function_allocate
mov rbp, rax
push rbp
push rdi
push qword [rsi]
call function_copy
mov rcx, [rsp+40]
mov byte [rbp+rdi], cl
add rdi, 1
mov byte [rbp+rdi], 0
push rbp
call type_string_constructor
add rsp, 8
pop rbp
pop rdi
pop rsi
ret

type_string_function_data:
mov rcx, [rsp+8]
mov rax, [rcx]
ret

type_string_function_length:
xor rax, rax
mov rcx, [rsp+8]
type_string_function_length_L0:
mov rdx, [rcx]
movzx r8, byte [rdx+rax]
test r8, r8
jne type_string_function_length_L1
ret
type_string_function_length_L1:
add rax, 1
jmp type_string_function_length_L0
ret

section .data

function_run_S0 db 'Hello World!', 0
function_run_S1 db 'Hello World!', 0