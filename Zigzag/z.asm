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
push rsi
push rbx
sub rsp, 40
xor rsi, rsi
cmp rsi, 10
jge function_run_L1
function_run_L0:
mov rcx, function_run_S0
call function_println
add rsi, 1
cmp rsi, 10
jl function_run_L0
function_run_L1:
mov rcx, function_run_S1
call type_string_constructor
mov rbx, rax
mov rsi, rbx
xor rbx, rbx
cmp rbx, 10
jge function_run_L3
function_run_L2:
mov rcx, rsi
call function_printsln
add rbx, 1
cmp rbx, 10
jl function_run_L2
function_run_L3:
add rsp, 40
pop rbx
pop rsi
ret

function_prints:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
call type_string_function_data
mov rsi, rax
mov rcx, rbx
call type_string_function_length
mov rcx, rsi
mov rdx, rax
call sys_print
add rsp, 40
pop rsi
pop rbx
ret

function_printsln:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rdx, 10
call type_string_function_append
mov rcx, rax
call type_string_function_data
mov rsi, rax
mov rcx, rbx
call type_string_function_length
add rax, 1
mov rcx, rsi
mov rdx, rax
call sys_print
add rsp, 40
pop rsi
pop rbx
ret

function_println:
sub rsp, 40
call type_string_constructor
mov rdx, 10
mov rcx, rax
call type_string_function_append
mov rcx, rax
call function_prints
add rsp, 40
ret

type_string_constructor:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, 8
call allocate
mov qword [rax], rbx
add rsp, 48
pop rbx
ret

type_string_function_append:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
mov rdi, rbx
call type_string_function_length
mov rbx, rax
lea rcx, [rbx+2]
mov rbp, rdi
call allocate
mov rdi, rax
mov rcx, [rbp]
mov rdx, rbx
mov r8, rdi
call copy
mov byte [rdi+rbx], sil
add rbx, 1
mov byte [rdi+rbx], 0
mov rcx, rdi
call type_string_constructor
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

type_string_function_data:
mov rax, [rcx]
ret

type_string_function_length:
xor rax, rax
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