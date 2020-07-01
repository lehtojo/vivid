section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_basic_data_field_assign
export function_basic_data_field_assign
function_basic_data_field_assign:
mov dword [rcx], 3141592653
mov byte [rcx+4], 64
mov rax, [function_basic_data_field_assign_D0]
mov qword [rcx+5], rax
mov qword [rcx+13], -2718281828459045
mov word [rcx+21], 12345
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov rcx, 23
call allocate
mov rcx, rax
call function_basic_data_field_assign
ret

section .data

function_basic_data_field_assign_D0 dq 4653933653213052928