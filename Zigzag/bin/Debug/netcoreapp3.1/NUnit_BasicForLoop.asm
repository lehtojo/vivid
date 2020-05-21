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
mov rax, rcx
xor r8, r8
xor r9, r9
cmp r8, rdx
jge function_basic_for_loop_L1
function_basic_for_loop_L0:
add rax, r9
add r9, 3
add r8, 1
cmp r8, rdx
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