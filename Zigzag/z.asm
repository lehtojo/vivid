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
lea r13, [rdx+1]
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
lea rcx, [rdx+rdx]
mov [rax], rcx
lea rcx, [rdx+rdx]
cmp rdx, rcx
jle function_run_L1
push 3
push 3
call function_f
jmp function_run_L0
function_run_L1:
cmp rdx, 3
jle function_run_L2
lea rcx, [rdx+rdx]
push rcx
lea rcx, [rdx+rdx]
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
lea rcx, [rdx+rdx]
push rcx
lea rcx, [rdx+rdx]
push rcx
call function_f
function_run_L0:
mov rax, 1
add rax, 2
mov rcx, 1
add rcx, 2
lea rdx, [rax+rcx]
add rax, rcx
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


