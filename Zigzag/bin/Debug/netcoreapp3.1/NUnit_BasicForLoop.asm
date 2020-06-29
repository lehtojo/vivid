section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_basic_for_loop
export function_basic_for_loop
function_basic_for_loop:
xor rax, rax
xor r8, r8
mov r9, rax
mov rax, rcx
cmp r9, rdx
jge function_basic_for_loop_L1
function_basic_for_loop_L0:
add rax, r8
add r8, 3
add r9, 1
cmp r9, rdx
jl function_basic_for_loop_L0
function_basic_for_loop_L1:
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov rcx, 1
mov rdx, 1
call function_basic_for_loop
ret

section .data