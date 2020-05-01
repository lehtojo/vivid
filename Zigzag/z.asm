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
push rbp
push r12
mov rbx, 1000
mov rax, rbx
imul rax, 8
push rax
call function_allocate
mov rdi, rax
mov qword [rdi], 2
mov rsi, 1
mov rbp, 2
cmp rbp, rbx
jg function_run_L1
function_run_L0:
xor rax, rax
xor rcx, rcx
cmp rcx, rsi
jge function_run_L3
function_run_L2:
mov rdx, rax
mov rax, rbp
mov r8, rdx
xor rdx, rdx
idiv qword [rdi+rcx*8]
test rdx, rdx
jne function_run_L4
add r8, 1
function_run_L4:
test r8, r8
jne function_run_L6
mov rdx, [rdi+rcx*8]
mov r9, rdx
imul r9, rdx
cmp r9, rbp
jl function_run_L8
mov qword [rdi+rsi*8], rbp
add rsi, 1
mov rax, r8
jmp function_run_L3
jmp function_run_L7
function_run_L8:
test r8, r8
jle function_run_L7
mov rax, r8
jmp function_run_L3
function_run_L7:
mov qword [rdi+rcx*8], rdx
jmp function_run_L5
function_run_L6:
test r8, r8
jle function_run_L5
mov rax, r8
jmp function_run_L3
function_run_L5:
add rcx, 1
mov rax, r8
cmp rcx, rsi
jl function_run_L2
function_run_L3:
add rbp, 1
cmp rbp, rbx
jle function_run_L0
function_run_L1:
push function_run_S0
call function_println
add rsp, 8
mov r12, rbx
xor rbx, rbx
cmp rbx, rsi
jge function_run_L10
function_run_L9:
push qword [rdi+rbx*8]
call function_to_string
add rsp, 8
push rax
call function_printsln
add rsp, 8
add rbx, 1
cmp rbx, rsi
jl function_run_L9
function_run_L10:
push function_run_S1
call function_println
add rsp, 8
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

function_to_string:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push function_to_string_S0
call type_string_constructor
mov rsi, rax
add rsp, 8
push function_to_string_S1
call type_string_constructor
mov rdi, rax
add rsp, 8
mov rbx, [rsp+56]
test rbx, rbx
jge function_to_string_L0
push function_to_string_S2
call type_string_constructor
add rsp, 8
mov rcx, rbx
neg rcx
mov rbx, rcx
mov rdi, rax
function_to_string_L0:
function_to_string_L1:
mov rax, rbx
xor rdx, rdx
mov rcx, 10
idiv rcx
mov rcx, rbx
mov rax, rcx
mov r8, rdx
xor rdx, rdx
mov r9, 10
idiv r9
mov rcx, 48
add rcx, r8
push rcx
push 0
push rsi
mov rbx, rax
mov rbp, rdx
mov r12, r8
mov r13, rdi
call type_string_function_insert
mov rdi, rax
add rsp, 24
test rbx, rbx
jne function_to_string_L2
push rdi
push r13
call type_string_function_combine
add rsp, 16
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret
function_to_string_L2:
mov rsi, rdi
mov rdi, r13
jmp function_to_string_L1
pop r13
pop r12
pop rbp
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

type_string_function_combine:
push rsi
push rdi
push rbp
push r12
push r13
mov rsi, [rsp+48]
push rsi
call type_string_function_length
mov rdi, rax
add rsp, 8
mov rbp, [rsp+56]
push rbp
call type_string_function_length
mov r12, rax
add rsp, 8
add r12, 1
lea rcx, [rdi+r12]
push rcx
call function_allocate
mov r13, rax
push r13
push rdi
push qword [rsi]
call function_copy
push rdi
push r13
push r12
push qword [rbp]
call function_offset_copy
push r13
call type_string_constructor
add rsp, 8
pop r13
pop r12
pop rbp
pop rdi
pop rsi
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

type_string_function_insert:
push rbx
push rsi
push rdi
push rbp
mov rsi, [rsp+40]
push rsi
call type_string_function_length
mov rdi, rax
add rsp, 8
lea rcx, [rdi+2]
push rcx
call function_allocate
mov rbp, rax
push rbp
mov rbx, [rsp+56]
push rbx
push qword [rsi]
call function_copy
lea rcx, [rbx+1]
push rcx
push rbp
mov rcx, rdi
sub rcx, rbx
push rcx
push qword [rsi]
call function_offset_copy
mov rcx, [rsp+56]
mov byte [rbp+rbx], cl
add rdi, 1
mov byte [rbp+rdi], 0
push rbp
call type_string_constructor
add rsp, 8
pop rbp
pop rdi
pop rsi
pop rbx
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

function_run_S0 db '-------------------', 0
function_run_S1 db '-------------------', 0
function_to_string_S0 db '', 0
function_to_string_S1 db '', 0
function_to_string_S2 db '-', 0