section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

function_pidigits:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
push r15
sub rsp, 88
add rcx, 1
mov rbx, rcx
imul rbx, 10
mov rax, rbx
mov r8, 6148914691236517206
mul r8
mov rbx, rdx
sar rbx, 63
add rbx, rdx
add rbx, 2
mov rdx, rcx
mov rcx, rbx
mov rsi, rdx
call type_array_large_constructor
mov rcx, rbx
mov rdi, rax
call type_array_large_constructor
mov rcx, rsi
mov rbp, rax
call type_array_large_constructor
xor r12, r12
cmp r12, rbx
jge function_pidigits_L1
function_pidigits_L0:
mov rcx, rdi
mov rdx, r12
mov r8, 20
mov r13, rax
call type_array_large_function_set
add r12, 1
cmp r12, rbx
mov rax, r13
jl function_pidigits_L0
function_pidigits_L1:
xor r13, r13
mov r14, rax
cmp r13, rsi
jge function_pidigits_L3
function_pidigits_L2:
xor r12, r12
xor r15, r15
cmp r12, rbx
jge function_pidigits_L5
function_pidigits_L4:
mov rcx, rbx
sub rcx, r12
sub rcx, 1
mov rdx, rcx
sal rdx, 1
add rdx, 1
mov r8, rcx
mov rcx, rdi
mov r9, rdx
mov rdx, r12
mov qword [rsp+80], r8
mov qword [rsp+72], r9
call type_array_large_function_get
add rax, r15
mov rcx, rdi
mov rdx, r12
mov r8, rax
call type_array_large_function_set
mov rcx, rdi
mov rdx, r12
call type_array_large_function_get
xor rdx, rdx
mov rcx, [rsp+72]
idiv rcx
mov rdx, rcx
mov rcx, rdi
mov r8, rdx
mov rdx, r12
mov qword [rsp+64], rax
mov qword [rsp+72], r8
call type_array_large_function_get
xor rdx, rdx
idiv qword [rsp+72]
mov rcx, rbp
mov r8, rdx
mov rdx, r12
call type_array_large_function_set
mov rcx, [rsp+64]
imul rcx, [rsp+80]
add r12, 1
cmp r12, rbx
mov r15, rcx
jl function_pidigits_L4
function_pidigits_L5:
mov rcx, rbx
sub rcx, 1
mov rdx, rcx
mov rcx, rdi
call type_array_large_function_get
mov rcx, 1844674407370955162
mul rcx
mov rcx, rdx
sar rcx, 63
add rcx, rdx
mov r8, rcx
mov rdx, r13
mov rcx, r14
call type_array_large_function_set
mov rcx, rbx
sub rcx, 1
mov rdx, rbx
sub rdx, 1
mov r8, rcx
mov rcx, rdi
mov qword [rsp+48], r8
call type_array_large_function_get
xor rdx, rdx
mov rcx, 10
idiv rcx
mov rcx, rbp
mov r8, rdx
mov rdx, [rsp+48]
call type_array_large_function_set
xor r12, r12
cmp r12, rbx
jge function_pidigits_L7
function_pidigits_L6:
mov rcx, rbp
mov rdx, r12
call type_array_large_function_get
imul rax, 10
mov rcx, rdi
mov rdx, r12
mov r8, rax
call type_array_large_function_set
add r12, 1
cmp r12, rbx
jl function_pidigits_L6
function_pidigits_L7:
add r13, 1
cmp r13, rsi
jl function_pidigits_L2
function_pidigits_L3:
mov rcx, rsi
sal rcx, 3
call type_array_u8_constructor
mov rcx, rsi
sub rcx, 1
mov r13, rcx
xor r15, r15
mov qword [rsp+56], rbx
mov rbx, rax
test r13, r13
jl function_pidigits_L9
function_pidigits_L8:
mov rcx, r14
mov rdx, r13
call type_array_large_function_get
add rax, r15
mov rcx, r14
mov rdx, r13
mov r8, rax
call type_array_large_function_set
mov rcx, r14
mov rdx, r13
call type_array_large_function_get
mov rcx, 1844674407370955162
mul rcx
mov r15, rdx
sar r15, 63
add r15, rdx
mov rcx, r14
mov rdx, r13
call type_array_large_function_get
xor rdx, rdx
mov rcx, 10
idiv rcx
mov rcx, 48
add rcx, rdx
mov r8, rcx
mov rdx, r13
mov rcx, rbx
call type_array_u8_function_set
sub r13, 1
test r13, r13
jge function_pidigits_L8
function_pidigits_L9:
mov rax, [rbx]
add rsp, 88
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

function_run:
sub rsp, 40
mov rcx, 3141
call function_pidigits
mov rcx, rax
call function_print
xor rax, rax
add rsp, 40
ret

function_length_of:
xor rax, rax
function_length_of_L0:
movzx rdx, byte [rcx+rax]
test rdx, rdx
jne function_length_of_L1
ret
function_length_of_L1:
add rax, 1
jmp function_length_of_L0
ret

function_print:
push rbx
sub rsp, 48
mov rbx, rcx
call function_length_of
mov rcx, rbx
mov rdx, rax
call sys_print
add rsp, 48
pop rbx
ret

type_array_large_constructor:
push rbx
push rsi
sub rsp, 40
mov rdx, rcx
mov rcx, 16
mov rbx, rdx
call allocate
mov rcx, rbx
sal rcx, 3
mov rsi, rax
call allocate
mov qword [rsi], rax
mov qword [rsi+8], rbx
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

type_array_large_function_set:
sal rdx, 3
mov rax, [rcx]
mov qword [rax+rdx], r8
ret

type_array_large_function_get:
sal rdx, 3
mov r8, [rcx]
mov rax, [r8+rdx]
ret

type_array_u8_constructor:
push rbx
push rsi
sub rsp, 40
mov rdx, rcx
mov rcx, 16
mov rbx, rdx
call allocate
mov rcx, rbx
sal rcx, 0
mov rsi, rax
call allocate
mov qword [rsi], rax
mov qword [rsi+8], rbx
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

type_array_u8_function_set:
sal rdx, 0
mov rax, [rcx]
mov byte [rax+rdx], r8b
ret

section .data