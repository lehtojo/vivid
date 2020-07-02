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
mov rax, rcx
sal rax, 1
mov r8, rdx
imul r8, 7
sub rax, r8
mov r8, rcx
imul r8, rax
imul r8, rdx
sub rcx, r8
mov rdx, [rsp+40]
add rcx, rdx
imul rax, rcx
add rax, rdx
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