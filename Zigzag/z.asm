section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

function_run:
sub rsp, 40
mov rcx, 16
call allocate
lea rcx, [rel function_run_lambda_0]
mov qword [rax], rcx
mov qword [rax+8], 3
add rsp, 40
ret

section .data