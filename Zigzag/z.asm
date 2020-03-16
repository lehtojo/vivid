function_f:
mov rcx, [rbp]
lea rax, [rcx+rcx]
mov rdx, 1
imul rdx, [rbp+4]
imul rdx, 7
sub rax, rdx
mov rdx, rcx
imul rdx, rax
imul rdx, [rbp+4]
sub rcx, rdx
imul rax, rcx
ret


function_run:
mov rcx, 1
add rcx, 3
push rcx
mov rcx, 2
imul rcx, 3
push 3
mov rdx, 3
mov r12, rdx
sub r12, 1
lea r13, [3+1]
imul r12, r13
push r12
call function_f
add rcx, rax
push rcx
call function_f
push 3
push rax
call type_foo_constructor
push rax
call type_foo_function_sum
lea rcx, [3+3]
mov [rax], rcx
lea rcx, [3+3]
cmp rdx, rcx
jle function_run_L1
push 3
push 3
call function_f
jmp function_run_L0
function_run_L1:
cmp rdx, 3
jle function_run_L2
lea rcx, [3+3]
push rcx
lea rcx, [3+3]
push rcx
call function_f
jmp function_run_L0
function_run_L2:
push 1
push 2
call function_f
push 2
push 3
call function_f
cmp rax, rax
jge function_run_L3
mov rax, 13434
ret
jmp function_run_L0
function_run_L3:
lea rcx, [3+3]
push rcx
lea rcx, [3+3]
push rcx
call function_f
function_run_L0:
call type_list_constructor
push rax
push 10
call type_list_function_create
push rax
push rax
call type_list_function_add
function_run_L4:
push 3
push 2
call function_f
mov rcx, rdx
add rcx, 1
jmp function_run_L4
mov rdx, 1
add rdx, 2
mov rcx, 1
add rcx, 2
mov r12, 1
add r12, 2
mov r13, 1
add r13, 2
mov r14, 1
add r14, 2
mov r15, 1
add r15, 2
mov rbx, 1
add rbx, 2
mov rsi, 1
add rsi, 2
mov rdi, 1
add rdi, 2
mov r8, 1
add r8, 2
mov r9, 1
add r9, 2
mov r10, 1
add r10, 2
mov r11, 1
add r11, 2
mov [rbp-36], rbx
mov rbx, 1
add rbx, 2
mov [rbp-64], rbx
mov rbx, 1
add rbx, 2
mov [rbp-68], rbx
mov rbx, 1
add rbx, 2
mov [rbp-72], rbx
mov rbx, 1
add rbx, 2
mov [rbp-76], rbx
mov rbx, 1
add rbx, 2
lea rax, [rdx+rcx]
add rax, r12
add rax, r13
add rax, r14
add rax, r15
add rax, [rbp-36]
add rax, rsi
add rax, rdi
add rax, r8
add rax, r9
add rax, r10
add rax, r11
add rax, [rbp-64]
add rax, [rbp-68]
add rax, [rbp-72]
add rax, [rbp-76]
add rax, rbx
ret
mov rax, 1
add rax, 2
mov rdx, 1
add rdx, 2
lea rcx, [rax+rdx]
add rax, rdx
add rax, 1
add rax, 2
ret


type_bool_constructor:

type_byte_constructor:

type_link_constructor:

type_long_constructor:

type_num_constructor:

type_decimal_constructor:

type_short_constructor:

type_tiny_constructor:

type_uint_constructor:

type_ulong_constructor:

type_ushort_constructor:

type_foo_constructor:
push 4
call allocate
mov rcx, [rbp+4]
imul rcx, rcx
mov [rax], rcx
mov rcx, [rbp+8]
add rcx, rcx
mov [rax+4], rcx


type_foo_function_sum:
mov rcx, [rbp+8]
mov rax, [rcx]
add rax, [rcx+4]
ret


type_list_constructor:
push 4
call allocate
mov [rax], 0
mov [rax+4], 0
mov [rax+8], 0
mov [rax+12], 0


type_list_function_create:
mov rcx, [rbp+8]
mov rdx, [rbp+4]
mov r12, rdx
imul r12, 4
push r12
call function_allocate
mov [rcx], rax
mov [rcx+4], rdx
mov [rcx+8], rdx
mov [rcx+12], 0


type_list_function_grow:
mov rcx, [rbp+8]
mov rdx, [rcx+8]
cmp rdx, 0
jge type_list_function_grow_L1
mov r12, [rcx+4]
mov r13, r12
imul r13, 2
push r13
call function_allocate
push [rcx]
push r12
push rax
call function_copy
push [rcx]
call function_free
mov [rcx], rax
mov r13, r12
imul r13, 2
mov r12, r13
jmp type_list_function_grow_L0
type_list_function_grow_L1:
lea r14, [r13+rdx]
push r14
call function_allocate
push rax
push r13
push rax
call function_copy
push rax
call function_free
add r13, rdx
mov r13, r13
type_list_function_grow_L0:


type_list_function_add:
mov rcx, [rbp+8]
mov rdx, [rcx+12]
cmp rdx, [rcx+4]
jne type_list_function_add_L0
call type_list_function_grow
type_list_function_add_L0:
mov r12, [rcx]
add rdx, 1
mov rdx, rdx


type_list_function_take:

type_list_function_at:

type_list_function_size:

