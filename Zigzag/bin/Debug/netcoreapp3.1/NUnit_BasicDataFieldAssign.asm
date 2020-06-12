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
mov dword [rcx], 314159265
mov byte [rcx+4], 64
mov qword [rcx+5], 1.414
mov qword [rcx+13], -2718281828459045
mov word [rcx+21], 12345
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov ecx, 23
call allocate
mov rcx, rax
call function_basic_data_field_assign
ret

section .data