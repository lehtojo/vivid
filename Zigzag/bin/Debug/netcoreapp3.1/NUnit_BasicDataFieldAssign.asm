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
movsd xmm0, qword [rel function_basic_data_field_assign_C0]
movsd qword [rcx+5], xmm0
mov rax, -2718281828459045
mov qword [rcx+13], rax
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

function_basic_data_field_assign_C0 dq 1.414