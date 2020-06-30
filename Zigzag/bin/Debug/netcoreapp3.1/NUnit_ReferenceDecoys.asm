section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate
extern large_function

global function_reference_decoy_1
export function_reference_decoy_1
function_reference_decoy_1:
mov rdx, rcx
mov rdx, 1
add rdx, rcx
add rcx, rdx
mov rax, rcx
ret

global function_reference_decoy_2
export function_reference_decoy_2
function_reference_decoy_2:
sal rcx, 1
mov rdx, 1
sal rdx, 1
add rcx, rdx
mov rax, rcx
ret

global function_reference_decoy_3
export function_reference_decoy_3
function_reference_decoy_3:
xor rax, rax
mov rdx, rcx
mov rcx, rdx
mov r8, rdx
mov r9, rax
cmp r9, 3
jge function_reference_decoy_3_L1
function_reference_decoy_3_L0:
lea rcx, [r9+1]
mov rdx, r9
add rdx, 1
cmp rdx, 3
mov r8, rcx
mov rcx, r9
mov r9, rdx
jl function_reference_decoy_3_L0
function_reference_decoy_3_L1:
add rcx, r8
mov rax, rcx
ret

global function_reference_decoy_4
export function_reference_decoy_4
function_reference_decoy_4:
xor rax, rax
mov rdx, rcx
mov r8, rdx
mov r9, r8
mov rcx, r9
mov rdx, r9
mov r8, r9
mov r10, r9
cmp rax, 5
jge function_reference_decoy_4_L1
function_reference_decoy_4_L0:
add rcx, 1
add rdx, 2
add r8, 4
add r10, 8
add rax, 1
cmp rax, 5
jl function_reference_decoy_4_L0
function_reference_decoy_4_L1:
add rcx, rdx
add rcx, r8
add rcx, r10
mov rax, rcx
ret

global function_reference_decoy_5
export function_reference_decoy_5
function_reference_decoy_5:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
sub rsp, 40
mov rbx, rcx
call large_function
xor rcx, rcx
mov rdx, rbx
mov r8, rdx
mov r9, r8
mov r10, r9
mov r11, r10
mov rsi, r11
mov rdi, rsi
mov rbp, rdi
mov r12, rbp
mov rdx, r12
mov r8, r12
mov r9, r12
mov r10, r12
mov r11, r12
mov rbx, r12
mov rsi, r12
mov rdi, r12
mov rbp, r12
mov r13, r12
cmp rcx, 5
jge function_reference_decoy_5_L1
function_reference_decoy_5_L0:
add rdx, 1
add r8, 2
add r9, 3
add r10, 4
add r11, 5
add rbx, 6
add rsi, 7
add rdi, 8
add rbp, 9
add r13, 10
add rcx, 1
cmp rcx, 5
jl function_reference_decoy_5_L0
function_reference_decoy_5_L1:
add rdx, r8
add rdx, r9
add rdx, r10
add rdx, r11
add rdx, rbx
add rdx, rsi
add rdx, rdi
add rdx, rbp
add rdx, r13
mov rax, rdx
add rsp, 40
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov rcx, 10
call function_reference_decoy_1
mov rcx, 10
call function_reference_decoy_2
mov rcx, 10
call function_reference_decoy_3
mov rcx, 10
call function_reference_decoy_4
mov rcx, 10
call function_reference_decoy_5
ret

section .data