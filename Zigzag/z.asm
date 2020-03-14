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
lea r13, [1+rcx]
lea r14, [1+rcx]
lea r15, [1+rcx]
lea rbx, [1+rcx]
lea rsi, [1+rcx]
lea rdi, [1+rcx]
lea rbp, [1+rcx]
lea rsp, [1+rcx]
lea r8, [1+rcx]
lea r9, [1+rcx]
lea r10, [1+rcx]
lea r11, [1+rcx]
mov [rbp-20], rbx
lea rbx, [1+rcx]
mov [rbp-56], rbx
lea rbx, [1+rcx]
mov [rbp-60], rbx
lea rbx, [1+rcx]
mov rcx, 1
add rcx, 2
add rdx, r12
add rdx, r13
add rdx, r14
add rdx, r15
add rdx, [rbp-20]
add rdx, rsi
add rdx, rdi
add rdx, rbp
add rdx, rsp
add rdx, r8
add rdx, r9
add rdx, r10
add rdx, r11
add rdx, [rbp-56]
add rdx, [rbp-60]
add rdx, rbx
add rdx, rcx
mov rax, rdx


