function_test:
mov rax, [rbp]
add rax, [rbp+4]
ret


function_run:
mov rax, 5
mov r10, 7
mov rcx, rax
imul rcx, r10
mov rdx, 2
imul rdx, rax
imul rdx, 3
imul rdx, r10
add rcx, rdx
sub rcx, rax
add rcx, r10
mov rdx, r10
imul rdx, 5
add rcx, rdx
mov rdx, rcx
imul rdx, r10
mov r12, rax
imul r12, rcx
sub rdx, r12
mov r12, 3
imul r12, 4
imul r12, r10
imul r12, rax
sub rdx, r12
mov r12, rdx
imul r12, rcx
imul r12, r10
imul r12, rax
mov r13, rax
imul r13, r10
sub r12, r13
add r12, rdx
lea rsi, [rax+1]
add rsi, 2
add rsi, 3
add rsi, 4
add rsi, r10
push rdx
push r12
call function_test
lea r13, [rcx+rdx]
mov r14, r12
sub r14, 1
mov r15, r12
imul r15, r14
lea r14, [r12+rdx]
imul r15, r14
sub r13, r15
lea r14, [rax+rcx]
add r13, r14
mov r14, 3434
imul r14, rax
add r14, r10
mov r15, r12
imul r15, rsi
sub r14, r15
imul rcx, 23465
add r14, rcx
mov rcx, rdx
sub rcx, r13
mov r14, 24
imul r14, r13
add rcx, r14
add rcx, rsi
imul rdx, r12
add rdx, r13
mov rax, rdx
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

