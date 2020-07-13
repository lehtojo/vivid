section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

function_f:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, [rsp+40]
add rcx, [rsp+48]
mov rax, rcx
ret

global function_g
export function_g
function_g:
sub rsp, 56
lea r8, [rcx+1]
mov r9, rcx
sar r9, 1
sal rcx, 2
lea r10, [rdx+1]
mov r11, rdx
sal r11, 1
sar rdx, 2
mov qword [rsp+40], rdx
mov rdx, r9
mov r9, r10
mov qword [rsp+32], r11
xchg r8, rcx
call function_f
add rsp, 56
ret

function_run:
sub rsp, 40
mov rcx, 1
mov rdx, 1
call function_g
mov rax, 1
add rsp, 40
ret

section .data