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


