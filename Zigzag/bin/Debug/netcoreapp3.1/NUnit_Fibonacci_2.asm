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
xor rbx, rbx ; i
xor rsi, rsi ; next
mov rdi, 1 ; second
xor rbp, rbp ; first
mov r12, rcx
cmp rbx, r12
jge function_fibonacci_L1
function_fibonacci_L0:
cmp rbx, 1
jg function_fibonacci_L3
mov rsi, rbx ; next = i
jmp function_fibonacci_L2
function_fibonacci_L3:
lea rax, [rbp+rdi] ; first + second
mov rcx, rdi ; copy second
mov rsi, rax ; next = ...
mov rbp, rdi ; first = second 
mov rdi, rax ; second = next
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
push r12
sub rsp, 48
mov rbx, rcx
mov rcx, function_to_string_S0
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
function_to_string_L1:
mov rax, rdi
xor rdx, rdx
mov rcx, 10
idiv rcx
mov rax, rdi
mov rcx, rdx
xor rdx, rdx
mov r8, 10
idiv r8
mov rdi, rax
mov rbp, rcx
mov r12, rdx
xor rdx, rdx
mov r8, 48
add r8, rbp
mov rcx, rbx
mov rbx, rdi
call type_string_function_insert
mov rdi, rax
test rbx, rbx
jne function_to_string_L2
mov rcx, rsi
mov rdx, rdi
call type_string_function_combine
add rsp, 48
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret
function_to_string_L2:
xchg rdi, rbx
jmp function_to_string_L1
add rsp, 48
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

function_printsln:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rdx, 10
mov rcx, rbx
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
mov rbx, rcx
mov rcx, 8
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
mov rcx, rbx
mov rdi, rsi
call type_string_function_length
mov rsi, rax
mov rcx, rdi
call type_string_function_length
mov rbp, rax
add rbp, 1
mov rdx, rcx
lea rdx, [rsi+rbp]
mov rcx, rdx
call allocate
mov r12, rax
mov rdx, [rbx]
mov rcx, rdx
mov r8, rdx
mov rdx, rsi
mov r9, r8
mov r8, r12
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
mov rcx, rbx
mov rdi, rsi
call type_string_function_length
mov rsi, rax
mov rdx, rcx
lea rdx, [rsi+2]
mov rcx, rdx
mov rbp, rdi
call allocate
mov rdi, rax
mov rdx, [rbx]
mov rcx, rdx
mov r8, rdx
mov rdx, rsi
mov r9, r8
mov r8, rdi
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
mov rbx, rcx ; rbx = this
mov rsi, rdx ; rsi = index
mov rdi, r8 ; rdi = character 
mov rcx, rbx ; this
mov rbp, rsi ; rbp = index
call type_string_function_length
mov rsi, rax ; save length
mov rdx, rcx ; ?
lea rdx, [rsi+2] ; length + 2
mov rcx, rdx ; 
mov r12, rdi ; save char
call allocate
mov rdi, rax ; rdi = mem
mov rdx, [rbx] ; 
mov rcx, rdx
mov r8, rdx
mov rdx, rbp
mov r9, r8
mov r8, rdi
mov rdx, [rbx]
mov rcx, rdx
mov r8, rsi
sub r8, rbp
mov r10, r9
lea r10, [rbp+1]
mov r9, rdx
mov rdx, r8
mov r8, rdi
mov r11, r9
mov r9, r10
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
mov rdx, [rcx]
mov rax, rdx
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

function_to_string_S0 db '', 0
function_to_string_S1 db '', 0
function_to_string_S2 db '-', 0