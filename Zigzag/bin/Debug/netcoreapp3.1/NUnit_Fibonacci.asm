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
mov rsi, rbx
call type_string_constructor
mov rbx, rax
mov rcx, function_to_string_S1
mov rdi, rsi
call type_string_constructor
mov rsi, rax
test rdi, rdi
jge function_to_string_L0
mov rcx, function_to_string_S2
call type_string_constructor
neg rdi
mov rsi, rax
function_to_string_L0:
function_to_string_L2:
mov rax, rdi
xor rdx, rdx
mov rcx, 10
idiv rcx
mov rax, rdi
mov rcx, rdx
xor rdx, rdx
mov r8, 10
idiv r8
mov rdx, 48
add rdx, rcx
mov r8, rcx
mov rcx, rbx
mov r9, r8
mov r8, rdx
xor rdx, rdx
mov rbx, rax
mov rdi, r9
mov rbp, rbx
call type_string_function_insert
mov rbx, rax
test rbp, rbp
jne function_to_string_L3
mov rcx, rsi
mov rdx, rbx
call type_string_function_combine
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret
function_to_string_L3:
mov rdi, rbp
jmp function_to_string_L2
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

function_printsln:
push rbx
push rsi
sub rsp, 40
mov rdx, 10
mov rbx, rcx
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
push r12
sub rsp, 48
mov rbx, rcx
mov rsi, rdx
mov rdi, rsi
call type_string_function_length
mov rsi, rax
mov rcx, rdi
call type_string_function_length
mov rbp, rax
add rbp, 1
lea rcx, [rsi+rbp]
call allocate
mov r12, rax
mov rcx, [rbx]
mov rdx, rsi
mov r8, r12
call copy
mov rcx, [rdi]
mov rdx, rbp
mov r8, r12
mov r9, rsi
call offset_copy
mov rcx, r12
call type_string_constructor
add rsp, 48
pop r12
pop rbp
pop rdi
pop rsi
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
mov rdi, rsi
call type_string_function_length
mov rsi, rax
lea rcx, [rsi+2]
mov rbp, rdi
call allocate
mov rdi, rax
mov rcx, [rbx]
mov rdx, rsi
mov r8, rdi
call copy
mov byte [rdi+rsi], bpl
add rsi, 1
mov byte [rdi+rsi], 0
mov rcx, rdi
call type_string_constructor
add rsp, 40
pop rbp
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
mov rbp, rsi
call type_string_function_length
mov rsi, rax
lea rcx, [rsi+2]
mov r12, rdi
call allocate
mov rdi, rax
mov rcx, [rbx]
mov rdx, rbp
mov r8, rdi
call copy
mov rcx, rsi
sub rcx, rbp
lea rdx, [rbp+1]
mov r8, rdx
mov rdx, rcx
mov rcx, [rbx]
mov r9, r8
mov r8, rdi
call offset_copy
mov byte [rdi+rbp], r12b
add rsi, 1
mov byte [rdi+rsi], 0
mov rcx, rdi
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