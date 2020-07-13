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
mov r8, rcx
mov r9, rax
cmp r9, 3
jge function_reference_decoy_3_L1
function_reference_decoy_3_L0:
lea rcx, [r9+1]
mov rdx, r9
add rdx, 1
cmp rdx, 3
mov r8, rcx
xchg r9, rdx
jl function_reference_decoy_3_L0
function_reference_decoy_3_L1:
add rdx, r8
mov rax, rdx
ret

global function_reference_decoy_4
export function_reference_decoy_4
function_reference_decoy_4:
xor rax, rax
mov rdx, rcx
mov r8, rcx
mov r9, rcx
mov r10, rcx
cmp rax, 5
jge function_reference_decoy_4_L1
function_reference_decoy_4_L0:
add rdx, 1
add r8, 2
add r9, 4
add r10, 8
add rax, 1
cmp rax, 5
jl function_reference_decoy_4_L0
function_reference_decoy_4_L1:
add rdx, r8
add rdx, r9
add rdx, r10
mov rax, rdx
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
xor rax, rax
mov rcx, rbx
mov rdx, rbx
mov r8, rbx
mov r9, rbx
mov r10, rbx
mov r11, rbx
mov rsi, rbx
mov rdi, rbx
mov rbp, rbx
mov r12, rbx
cmp rax, 5
jge function_reference_decoy_5_L1
function_reference_decoy_5_L0:
add rcx, 1
add rdx, 2
add r8, 3
add r9, 4
add r10, 5
add r11, 6
add rsi, 7
add rdi, 8
add rbp, 9
add r12, 10
add rax, 1
cmp rax, 5
jl function_reference_decoy_5_L0
function_reference_decoy_5_L1:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, r10
add rcx, r11
add rcx, rsi
add rcx, rdi
add rcx, rbp
add rcx, r12
mov rax, rcx
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