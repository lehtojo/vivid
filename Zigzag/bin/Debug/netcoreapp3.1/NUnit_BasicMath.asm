section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_basic_math
export function_basic_math
function_basic_math:
mov r9, rcx
imul r9, r8
add r9, rcx
add r9, r8
imul rdx, rcx
add r8, 1
imul rdx, r8
imul rdx, 100
add r9, rdx
mov rax, r9
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov rcx, 1
mov rdx, 2
mov r8, 3
call function_basic_math
ret

section .data