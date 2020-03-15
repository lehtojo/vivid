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


function_run:
lea rcx, [1+3]
push rcx
mov rcx, 2
mov rdx, rcx
imul rdx, 3
push 3
mov r12, 3
mov r13, r12
sub r13, 1
lea r14, [r12+1]
imul r13, r14
push r13
call function_f
add rdx, rax
push rdx
call function_f
push 3
push rax
call type_foo_constructor
push rax
call type_foo_function_sum
lea rdx, [r12+r12]
mov [rax], rdx
lea rdx, [r12+r12]
cmp r12, rdx
jle function_run_L1
push 3
push 3
call function_f
jmp function_run_L0
function_run_L1:
cmp r12, r12
jle function_run_L2
lea rdx, [r12+r12]
push rdx
lea rdx, [r12+r12]
push rdx
call function_f
jmp function_run_L0
function_run_L2:
lea rdx, [r12+r12]
push rdx
lea rdx, [r12+r12]
push rdx
call function_f
function_run_L0:
lea rdx, [1+rcx]
lea r12, [1+rcx]
lea r13, [rdx+r12]
add rdx, r12
add rdx, 1
add rdx, 2
mov rax, rdx


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
mov rdx, [rcx]
add rdx, [rcx+4]
mov rax, rdx


