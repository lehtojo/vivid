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
mov rax, rcx
sal rax, 1
mov r8, rdx
imul r8, 17
add rax, r8
lea r8, [rcx*8+rcx]
add rax, r8
sar rdx, 2
add rax, rdx
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