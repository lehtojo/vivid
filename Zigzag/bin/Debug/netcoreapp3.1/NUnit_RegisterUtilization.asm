section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_register_utilization
export function_register_utilization
function_register_utilization:
mov r8, rax
lea r8, [rcx+rcx]
mov r9, rdx
sal r9, 0
imul r9, 7
sub r8, r9
mov r9, rcx
imul r9, r8
imul r9, rdx
sub rcx, r9
mov rdx, [rsp+40]
add rcx, rdx
imul r8, rcx
add r8, rdx
mov rax, r8
ret

function_run:
sub rsp, 40
mov rcx, 1
mov rdx, 1
mov r8, 1
mov r9, 1
mov qword [rsp+32], 1
call function_register_utilization
mov rax, 1
add rsp, 40
ret

section .data