section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

function_fibonacci:
push rbx
push rsi
push rdi
push rbp
push r12
sub rsp, 48
xor rbx, rbx
xor rsi, rsi
mov rdi, 1
xor rbp, rbp
mov r12, rcx
cmp rbx, r12
jge function_fibonacci_L1
function_fibonacci_L0:
cmp rbx, 1
jg function_fibonacci_L3
mov rsi, rbx
jmp function_fibonacci_L2
function_fibonacci_L3:
lea rax, [rbp+rdi]
mov rcx, rdi
mov rsi, rax
mov rbp, rdi
mov rdi, rax
function_fibonacci_L2:
mov rcx, rsi
call function_to_string
mov rcx, rax
call function_printsln
add rbx, 1
cmp rbx, r12
jl function_fibonacci_L0
function_fibonacci_L1:
add rsp, 48
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

function_run:
sub rsp, 40
mov rcx, 10
call function_fibonacci
xor rax, rax
add rsp, 40
ret

function_to_string:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rdx, rcx
mov rcx, function_to_string_S0
mov rbx, rdx
call type_string_constructor
mov rcx, function_to_string_S1
mov rsi, rax
call type_string_constructor
test rbx, rbx
jge function_to_string_L0
mov rcx, function_to_string_S2
call type_string_constructor
neg rbx
function_to_string_L0:
mov rdi, rax
function_to_string_L2:
mov rax, rbx
xor rdx, rdx
mov rcx, 10
idiv rcx
mov rax, rbx
mov rcx, rdx
xor rdx, rdx
mov r8, 10
idiv r8
mov rdx, 48
add rdx, rcx
mov r8, rcx
mov rcx, rsi
mov r9, r8
mov r8, rdx
xor rdx, rdx
mov rbx, rax
mov rsi, r9
call type_string_function_insert
test rbx, rbx
jne function_to_string_L3
mov rcx, rdi
mov rdx, rax
mov rbp, rax
call type_string_function_combine
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret
mov rax, rbp
function_to_string_L3:
mov rsi, rax
jmp function_to_string_L2
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

function_printsln:
push rbx
sub rsp, 48
mov rdx, 10
mov rbx, rcx
call type_string_function_append
mov rcx, rax
call type_string_function_data
mov rcx, rbx
mov rbx, rax
call type_string_function_length
add rax, 1
mov rcx, rbx
mov rdx, rax
call sys_print
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

type_string_function_combine:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
call type_string_function_length
mov rcx, rsi
mov rdi, rax
call type_string_function_length
add rax, 1
lea rcx, [rdi+rax]
mov rbp, rax
call allocate
mov rcx, [rbx]
mov rdx, rdi
mov r8, rax
mov rbx, rax
call copy
mov rcx, [rsi]
mov rdx, rbp
mov r8, rbx
mov r9, rdi
call offset_copy
mov rcx, rbx
call type_string_constructor
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

type_string_function_append:
push rbx
push rsi
push rdi
sub rsp, 48
mov rbx, rcx
mov rsi, rdx
call type_string_function_length
lea rcx, [rax+2]
mov rdi, rax
call allocate
mov rcx, [rbx]
mov rdx, rdi
mov r8, rax
mov rbx, rax
call copy
mov byte [rbx+rdi], sil
add rdi, 1
mov byte [rbx+rdi], 0
mov rcx, rbx
call type_string_constructor
add rsp, 48
pop rdi
pop rsi
pop rbx
ret

type_string_function_insert:
push rbx
push rsi
push rdi
push rbp
push r12
sub rsp, 48
mov rbx, rcx
mov rsi, rdx
mov rdi, r8
call type_string_function_length
lea rcx, [rax+2]
mov rbp, rax
call allocate
mov rcx, [rbx]
mov rdx, rsi
mov r8, rax
mov r12, rax
call copy
mov rcx, rsi
imul rcx, -1
lea rdx, [rbp+rcx]
lea rcx, [rsi+1]
mov r9, rcx
mov r8, r12
mov rcx, [rbx]
call offset_copy
mov byte [r12+rsi], dil
add rbp, 1
mov byte [r12+rbp], 0
mov rcx, r12
call type_string_constructor
add rsp, 48
pop r12
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
mov r8, [rcx]
movzx rdx, byte [r8+rax]
test dl, dl
jne type_string_function_length_L1
ret
type_string_function_length_L1:
add rax, 1
jmp type_string_function_length_L0
ret

section .data

function_to_string_S0 db '', 0
function_to_string_S1 db '', 0
function_to_string_S2 db '-', 0