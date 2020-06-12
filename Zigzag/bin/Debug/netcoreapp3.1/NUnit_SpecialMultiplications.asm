section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_special_multiplications
export function_special_multiplications
function_special_multiplications:
mov r8, rcx
sal r8, 1
mov r9, rdx
imul r9, 17
add r8, r9
lea r9, [rcx*8+rcx]
add r8, r9
sar rdx, 2
add r8, rdx
mov rax, r8
ret

function_run:
sub rsp, 40
mov rcx, 1
mov rdx, 1
call function_special_multiplications
mov rax, 1
add rsp, 40
ret

section .data