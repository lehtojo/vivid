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
imul rcx, 11
imul rdx, 17
add rcx, rdx
mov rax, rcx
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