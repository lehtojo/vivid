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
push r12
sub rsp, 48
call type_list_item_constructor
mov rbx, rax
xor rsi, rsi
mov rcx, 1
mov rdx, 2
call type_item_constructor
mov rdi, rax
mov rcx, 10
call type_adder_item_constructor
mov rbp, rax
mov rcx, rbp
mov rdx, rdi
call type_adder_item_function_set_target
mov rcx, rbp
call type_adder_item_function_add
cmp rsi, 4
jge function_run_L1
function_run_L0:
lea rcx, [rsi+1]
mov rdx, rsi
call type_item_constructor
mov rcx, rbx
mov rdx, rax
call type_list_item_function_add
add rsi, 1
cmp rsi, 4
jl function_run_L0
function_run_L1:
mov rsi, rbx
xor rbx, rbx
mov rcx, rsi
call type_list_item_function_size
cmp rbx, rax
jge function_run_L3
function_run_L2:
mov rcx, rsi
mov rdx, rbx
call type_list_item_function_at
mov rcx, rax
mov rdx, rbx
call type_item_function_add
add rbx, 1
mov rcx, rsi
call type_list_item_function_size
cmp rbx, rax
jl function_run_L2
function_run_L3:
xor rbx, rbx
mov rcx, rsi
call type_list_item_function_size
cmp rbx, rax
jge function_run_L5
function_run_L4:
mov rcx, rsi
mov rdx, rbx
mov r12, rsi
call type_list_item_function_at
mov rsi, rax
mov rcx, [rsi]
call function_to_string
mov rcx, rax
call function_prints
mov rcx, function_run_S0
call function_print
mov rcx, [rsi+8]
call function_to_string
mov rcx, rax
call function_printsln
add rbx, 1
mov rsi, r12
mov rcx, rsi
call type_list_item_function_size
cmp rbx, rax
jl function_run_L4
function_run_L5:
add rsp, 48
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
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
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

function_prints:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, rbx
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

function_print:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, rbx
call function_length_of
mov rcx, rbx
mov rdx, rax
call sys_print
add rsp, 48
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

type_item_constructor:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
mov rcx, 16
call allocate
mov qword [rax], rbx
mov qword [rax+8], rsi
add rsp, 40
pop rsi
pop rbx
ret

type_item_function_add:
add qword [rcx], rdx
add qword [rcx+8], rdx
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
mov rcx, rbx
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
mov rcx, rbx
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
mov rcx, [rbx]
mov rdx, rsi
sub rdx, rbp
lea r9, [rbp+1]
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
mov rdx, [rcx]
movzx r8, byte [rdx+rax]
test r8, r8
jne type_string_function_length_L1
ret
type_string_function_length_L1:
add rax, 1
jmp type_string_function_length_L0
ret

type_list_item_constructor:
push rbx
push rsi
sub rsp, 40
mov rcx, 24
call allocate
mov rbx, rax
mov rcx, 8
call allocate
mov rsi, rax
mov qword [rbx+8], 1
mov qword [rbx+16], 0
add rsp, 40
pop rsi
pop rbx
ret

type_list_item_function_grow:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, [rbx+8]
imul rcx, 2
imul rcx, 8
call allocate
mov rsi, rax
mov rcx, [rbx]
mov rdx, [rbx+8]
mov r8, rsi
call copy
mov rcx, [rbx]
mov rdx, [rbx+8]
call deallocate
mov qword [rbx], rsi
mov rax, [rbx+8]
imul rax, 2
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
mov rcx, rbx
call type_list_item_function_grow
mov rdx, rsi
mov rcx, rbx
type_list_item_function_add_L0:
mov rax, [rcx]
mov r8, [rcx+16]
mov qword [rax+r8*8], rdx
add qword [rcx+16], 1
add rsp, 40
pop rsi
pop rbx
ret

type_list_item_function_at:
mov r8, [rcx]
mov rax, [r8+rdx*8]
ret

type_list_item_function_size:
mov rax, [rcx+16]
ret

type_adder_item_constructor:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, 16
call allocate
mov qword [rax], rbx
add rsp, 48
pop rbx
ret

type_adder_item_function_set_target:
mov qword [rcx+8], rdx
ret

type_adder_item_function_add:
sub rsp, 40
mov rax, rcx
mov rcx, [rax+8]
mov rdx, [rax]
call type_item_function_add
add rsp, 40
ret

section .data

function_run_S0 db ', ', 0
function_to_string_S0 db '', 0
function_to_string_S1 db '', 0
function_to_string_S2 db '-', 0