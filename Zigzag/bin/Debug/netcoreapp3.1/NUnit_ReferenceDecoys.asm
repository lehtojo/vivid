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
mov rax, rcx
mov rdx, rcx
xor r8, r8
cmp r8, 3
jge function_reference_decoy_3_L1
function_reference_decoy_3_L0:
lea rcx, [r8+1]
mov rdx, r8
add rdx, 1
mov rdx, rcx
mov rax, r8
mov r8, rdx
cmp r8, 3
jl function_reference_decoy_3_L0
function_reference_decoy_3_L1:
add rax, rdx
ret

global function_reference_decoy_4
export function_reference_decoy_4
function_reference_decoy_4:
mov rax, rcx
mov rdx, rcx
mov r8, rcx
mov r9, rcx
xor r10, r10
cmp r10, 5
jge function_reference_decoy_4_L1
function_reference_decoy_4_L0:
add rax, 1
add rdx, 2
add r8, 4
add r9, 8
add r10, 1
cmp r10, 5
jl function_reference_decoy_4_L0
function_reference_decoy_4_L1:
add rax, rdx
add rax, r8
add rax, r9
ret

global function_reference_decoy_5
export function_reference_decoy_5
function_reference_decoy_5:
push rbx
push rsi
push rdi
push rbp
push r12
sub rsp, 48
mov rbx, rcx
call large_function
mov rax, rbx
mov rcx, rbx
mov rdx, rbx
mov r8, rbx
mov r9, rbx
mov r10, rbx
mov r11, rbx
mov rsi, rbx
mov rdi, rbx
mov rbp, rbx
xor r12, r12
cmp r12, 5
jge function_reference_decoy_5_L1
function_reference_decoy_5_L0:
add rax, 1
add rcx, 2
add rdx, 3
add r8, 4
add r9, 5
add r10, 6
add r11, 7
add rsi, 8
add rdi, 9
add rbp, 10
add r12, 1
cmp r12, 5
jl function_reference_decoy_5_L0
function_reference_decoy_5_L1:
add rax, rcx
add rax, rdx
add rax, r8
add rax, r9
add rax, r10
add rax, r11
add rax, rsi
add rax, rdi
add rax, rbp
add rsp, 48
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