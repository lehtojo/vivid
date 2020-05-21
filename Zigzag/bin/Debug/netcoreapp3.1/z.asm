section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_basic_if_statement
export function_basic_if_statement
function_basic_if_statement:
cmp rcx, rdx
jl function_basic_if_statement_L1
mov rax, rcx
ret
jmp function_basic_if_statement_L0
function_basic_if_statement_L1:
mov rax, rdx
ret
function_basic_if_statement_L0:
ret

function_run:
sub rsp, 40
mov rcx, 1
mov rdx, 2
call function_basic_if_statement
add rsp, 40
ret

section .data